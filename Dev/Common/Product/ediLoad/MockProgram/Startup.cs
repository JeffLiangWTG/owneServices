using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MockProgram
{
	// Used by various unit tests that need to test interaction with an external program
	public static class Startup
	{
		public static int Main(string[] args)
		{
			if (args != null && args.Length == 1 && args[0] == "-WaitTwoSecondsAndStartMessageLoop")
			{
				Thread.Sleep(2000);
				Application.Run(new Form());
				// Unit test will kill this program.
			}
			else if (args != null && args.Length == 1 && args[0] == "-WaitTwoSecondsAndExit")
			{
				Thread.Sleep(2000);
			}
			else if (args != null && args.Length == 2 && args[0] == "-Return")
			{
				return int.Parse(args[1]);
			}
			else if (args != null && args.Length == 3 && args[0] == "-Return" && args[2] == "-RelaunchedElevated")
			{
				return int.Parse(args[1]) * 2;
			}
			else if (args != null && args.Length == 1 && args[0] == "-LookForGenownFont")
			{
				foreach (FontFamily family in new InstalledFontCollection().Families)
				{
					if (family.Name == "genown_v01")
					{
						return 42;
					}
				}
				return -1;
			}
			else
			{
				using (StreamWriter streamWriter = File.CreateText(args[0]))
				{
					streamWriter.WriteLine(Environment.CurrentDirectory + Path.DirectorySeparatorChar);
					streamWriter.WriteLine(Process.GetCurrentProcess().MainModule.FileName);
					foreach (string arg in args)
					{
						streamWriter.WriteLine(arg);
					}

					// There is a race condition between when this program creates the file and when the unit test tries reading from the file.
					// We sleep for a second here to try and force the race condition to happen every time. That way we can be sure that the
					// test handles it properly. (Otherwise the test might fail intermittently.)
					Thread.Sleep(1000);
				}

				for (; ; )
				{
					Thread.Sleep(Timeout.Infinite); // Unit test will kill this program.
				}
			}

			return 0;
		}
	}
}
