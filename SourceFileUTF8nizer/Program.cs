using System;
using System.IO;
using System.Text;

namespace SourceFileUTF8nizer
{
    public class Program
    { 
        private static Encoding GetFileEncoding(string srcFile)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.GetEncoding("windows-1252");

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[10];
            FileStream file = new FileStream(srcFile, FileMode.Open);
            file.Read(buffer, 0, 10);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;
            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                enc = Encoding.UTF7;
            else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                // 1201 unicodeFFFE Unicode (Big-Endian)
                enc = Encoding.GetEncoding(1201);
            else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                // 1200 utf-16 Unicode
                enc = Encoding.GetEncoding(1200);
            else if (validateUtf8whitBOM(srcFile))

                enc = new UTF8Encoding(false);
            return enc;
        }

        private static bool validateUtf8whitBOM(string FileSource)

        {

            bool bReturn = false;

            string TextANSI;

            //lread the file as utf8

            StreamReader srFileWhitBOM = new StreamReader(FileSource);

            srFileWhitBOM.Close();


            //lread the file as  ANSI

            srFileWhitBOM = new StreamReader(FileSource, Encoding.Default, false);

            TextANSI = srFileWhitBOM.ReadToEnd();

            srFileWhitBOM.Close();

            // if the file contains special characters is UTF8 text read ansi show signs

            if (TextANSI.Contains("Ã") || TextANSI.Contains("±"))

                     bReturn = true;

            return bReturn;

        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Falta informar a pasta de fontes.");
                return;
            }

            Console.WriteLine("Verificando pasta: " + args[0]);

            try
            {
                int utf8WithBom = 0;
                int utf8WithoutBom = 0;
                int windows1252 = 0;
                int under = 0;
                int converted = 0;
                int total = 0;

                foreach (var f in new DirectoryInfo(args[0]).GetFiles("*.cs", SearchOption.AllDirectories))
                {
                    if (f.FullName.IndexOf(@"\TestResults\") >= 0)
                        continue;

                    total++;

                    var utf8NoBom = new UTF8Encoding(false);
                    using (var sw = new StreamReader(f.FullName, utf8NoBom))
                    {
                        sw.Peek();
                        if (sw.CurrentEncoding == Encoding.UTF8)
                        {
                            utf8WithBom++;
                            Console.WriteLine("Já é UTF-8 com BOM: " + f.FullName);
                            continue;
                        }
                    }

                    string textAsUTF8 = File.ReadAllText(f.FullName, Encoding.UTF8);
                    string textAsNative = File.ReadAllText(f.FullName, Encoding.GetEncoding("windows-1252"));

                    if (textAsNative == textAsUTF8)
                    {
                        under++;
                        Console.WriteLine("Não faz diferença: " + f.FullName);
                        File.WriteAllText(f.FullName, textAsNative, Encoding.UTF8);
                        Console.WriteLine("Convertido para UTF8 com BOM: " + f.FullName);
                        converted++;
                        continue;
                    }

                    var enc = GetFileEncoding(f.FullName);

                    if (enc is UTF8Encoding)
                    {
                        utf8WithoutBom++;
                        Console.WriteLine("Já é um UTF-8, Mas sem BOM");
                        File.WriteAllText(f.FullName, textAsUTF8, Encoding.UTF8);
                        Console.WriteLine("Convertido para UTF8 com BOM: " + f.FullName);
                        converted++;
                        continue;
                    }

                    if (enc == Encoding.GetEncoding("windows-1252"))
                    {
                        windows1252++;
                        File.WriteAllText(f.FullName, textAsNative, Encoding.UTF8);
                        Console.WriteLine("Convertido para UTF8 com BOM: " + f.FullName);
                        converted++;
                        continue;
                    }
                }
                Console.WriteLine();
                Console.WriteLine("   UTF8 c/ BOM:\t{0}", utf8WithBom);
                Console.WriteLine("   UTF8 s/ BOM:\t{0}", utf8WithoutBom);
                Console.WriteLine("  Windows 1252:\t{0}", windows1252);
                Console.WriteLine(" Sem diferença:\t{0}", under);
                Console.WriteLine("         Total:\t{0}", total);
                Console.WriteLine("Não analisados:\t{0}", total - (utf8WithBom + utf8WithoutBom + windows1252 + under));
                Console.WriteLine("   Convertidos:\t{0}", converted);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("*** ERRO FATAL ***");
                Console.WriteLine(e.ToString());
            }
        }
    }
}
