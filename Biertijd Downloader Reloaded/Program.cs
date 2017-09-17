using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biertijd_Downloader_Reloaded
{
    //TODO: add pat's comment, maybe. contains info about the girl

    internal class Program
    {
        public const int ALBUMS_PER_PAGE = 20;
        public const int PICS_PER_ALBUM = 20;
        public const string SITE_URL = "http://biertijd.xxx/";

        public static int CurrentPage = 0;
        public static Regex RegexAlbumLink = new Regex(@"(?<full>index\.php\?itemid=(?<id>\d+)"")", RegexOptions.Compiled);   //<a href="index.php?itemid=67798">
        public static Regex RegexAlbumImage = new Regex(@"(?<full>http://media01\.biertijd\.com/(?<path>galleries/(?<gallery>[^/]+)/(?<album>[^/]+)/(?<num>\d{2})\.jpg))", RegexOptions.Compiled);    //media01.biertijd.com/galleries/gal_name/index_name/num.jpg


        private static void Main(string[] args)
        {
            Console.WriteLine("Biertijd Downloader Reloaded");
            Console.WriteLine(" - by HoLLy");
            Console.WriteLine("\nYes, the interface is kinda downgraded. This should be faster though.");
            Console.WriteLine("\n");

            while (true) {
                string curLink = SITE_URL + "?startpos=" + (ALBUMS_PER_PAGE * CurrentPage);
                var albums = ExtractMatches(curLink, RegexAlbumLink);

                foreach (Match album in albums) {
                    //we have every album on this page
                    Console.Write("Downloading album " + album.Groups["id"].Value + ": ");

                    //get all links for this album
                    var pics = ExtractMatches(SITE_URL + album.Groups["full"].Value, RegexAlbumImage);

                    Console.Write('[' + new string(' ', PICS_PER_ALBUM) + ']' + new string('\b', PICS_PER_ALBUM + 1));  //make fancy (pt1)

                    //download every pic in this album, threaded
                    Parallel.ForEach(pics, pic => {
                        string relPath = pic.Groups["path"].Value;
                        if (!File.Exists(relPath)) {
                            PreparePath(relPath);
                            new WebClient().DownloadFile(pic.Groups["full"].Value, Directory.GetCurrentDirectory() + "/" + relPath);
                        }
                        Console.Write('.');     //fill fancy (pt2)
                    });

                    Console.WriteLine(']');     //clear fancy (pt3)
                }

                CurrentPage++;
            }
        }

        private static IEnumerable<Match> ExtractMatches(string url, Regex reg) => reg.Matches(new WebClient().DownloadString(url)).OfType<Match>().Distinct();
        private static void PreparePath(string path) => Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));
    }
}
