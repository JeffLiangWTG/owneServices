using System;
using System.Runtime.InteropServices;

namespace Enterprise.RemotePrinting.Client
#if WixCustomAction
.CustomAction // Different namespace for file copy in Setup project (to prevent error 'same symbol name defined in multiple project').
#endif
{
		public static class CommandLineHelper
		{
			[DllImport("shell32.dll", SetLastError = true)]
			static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

			public static string[] CommandLineToArgs(string commandLine)
			{
				if (string.IsNullOrWhiteSpace(commandLine))
				{
					return Array.Empty<string>();
				}

				var argv = CommandLineToArgvW(commandLine, out var argc);
				if (argv == IntPtr.Zero)
				{
					throw new System.ComponentModel.Win32Exception();
				}

				try
				{
					var args = new string[argc];
					for (var i = 0; i < args.Length; i++)
					{
						var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
						args[i] = Marshal.PtrToStringUni(p);
					}

					return args;
				}
				finally
				{
					Marshal.FreeHGlobal(argv);
				}
			}

			public static string GetArgument(string commandLine, string argumentPrefix)
			{
				var args = CommandLineToArgs(commandLine);

				return GetArgument(args, argumentPrefix);
			}

			public static string GetArgument(string[] args, string argumentPrefix)
			{
				foreach (var argument in args)
				{
					if (argument.StartsWith(argumentPrefix))
					{
						return argument;
					}
				}

				return string.Empty;
			}

			public static string GetExeFileName(string commandLine)
			{
				if (string.IsNullOrWhiteSpace(commandLine))
				{
					return string.Empty;
				}

				if (commandLine.StartsWith("\""))
				{
					var closingQuoteIndex = commandLine.IndexOf('"', 1);
					return closingQuoteIndex > 1 ? commandLine.Substring(1, closingQuoteIndex - 1) : commandLine.Substring(1);
				}
				else
				{
					int spaceIndex = commandLine.IndexOf(' ');
					if (spaceIndex >= 0)
					{
						return commandLine.Substring(0, spaceIndex);
					}
					else
					{
						return commandLine;
					}
				}
			}

			public static bool MatchesExeFileName(string commandLine, string exeFileName)
			{
				if (string.IsNullOrWhiteSpace(commandLine) || string.IsNullOrWhiteSpace(exeFileName))
				{
					return false;
				}

				// File name characters case does not matter in Windows
				return string.Equals(GetExeFileName(commandLine), exeFileName, StringComparison.OrdinalIgnoreCase);
			}
		}
}
