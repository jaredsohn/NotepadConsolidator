//Consolidate Notepads
//Written by Jared Sohn (jared dot sohn at gmail.com).  Code found at github.com/jaredsohn.
//September 2010
//Written in C# in Visual Studio 2005
//
// If you want to reuse this code, do whatever you want with it but give me credit.
//
// Use this program to gather up the text of all open untitled instances of notepad.exe and consolidate them into one file.
// Really useful if you have the annoying habit of using Windows (insert joke here) and going Start->Run->notepad whenever you want to record an idea that pops into your head.
//
// Warning: You will lose data if any untitled notepad file is over MaxFileSize (1 MB), although I would expect this would be a rare occurrence; it doesn't affect me in my usage.
//
// Future enhancement ideas:
//
// * have an option to send an e-mail or scp instead (useful when running on other peoples' computers.)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace ConsolidateNotepads
{
    static class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, StringBuilder lParam);

        public const int MaxFileSize = 1024 * 1024;

        static void ShowUsage()
        {
            System.Console.WriteLine("\nUsage: NotepadConsolidator [path]|desktop");
            System.Console.WriteLine("\nConsolidate the contents of all open notepads into a text file within path.");
            System.Console.WriteLine("Specify 'desktop' to have it write to your desktop folder.");        
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                ShowUsage();
                return;
            }

            if ((args[0].ToLower() != "desktop") && (!System.IO.Directory.Exists(args[0])))
            {
                System.Console.WriteLine("Error: path '" + args[0] + "' does not exist.");
                return;
            }

            string destFolder = args[0];
            if (args[0].ToLower() == "desktop")
            {
                destFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            try
            {
                int untitledNotepadCount = 0;
                Process[] processes = Process.GetProcesses();
                Process[] notepads = Process.GetProcessesByName("notepad");
                if (notepads.Length == 0) return;
                string fullStr = "";
                for (int i = 0; i < notepads.Length; i++)
                {
                    if (notepads[i] != null)
                    {
                        if (notepads[i].MainWindowTitle == "Untitled - Notepad")
                        {
                            untitledNotepadCount++;

                            IntPtr child = FindWindowEx(notepads[i].MainWindowHandle, new IntPtr(0), "Edit", null);

                            StringBuilder sb = new StringBuilder(MaxFileSize);
                            SendMessage(child, 0x000D, MaxFileSize - 1, sb);

                            fullStr += sb.ToString() + "\r\n----------------------------------------------\r\n\r\n\r\n";
                        }
                    }
                }

                if ((fullStr != null) && (fullStr != ""))
                {
                    fullStr = fullStr.Trim();
                    string fullFileName = System.IO.Path.Combine(destFolder, "notepads" + String.Format("{0:_yyyy_MM_dd_hh_mm_ss}" + ".txt", DateTime.Now));
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(fullFileName);
                    sw.Write(fullStr); 
                    sw.Close();

                    System.Console.WriteLine("Consolidated " + untitledNotepadCount + " notepads to file '" + fullFileName + "'.");
                }
                else
                {
                    System.Console.WriteLine("No untitled notepads were found.");
                } 

                // Kill existing notepads now.  (Not earlier, in case a bug would cause us to crash and lose all of the information.)
                for (int i = 0; i < notepads.Length; i++)
                {
                    if (notepads[i] != null)
                    {
                        if (notepads[i].MainWindowTitle == "Untitled - Notepad")
                        {
                            notepads[i].Kill();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("An error occured: " + ex.Message);
                System.Console.Write(ex.StackTrace);
            }
        }
    }
}