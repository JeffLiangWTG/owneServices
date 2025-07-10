using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Reflection;
#endif

namespace Enterprise.Upgrades.Postinstall.Starter
{
	class Program
	{
		public const string postInstallAssembly = "Enterprise.Upgrades.Postinstall.exe";

		static void Main(string[] args)
		{
#if NETFRAMEWORK
#pragma warning disable RS0030 // TODO: WI00814924 - Use Assembly.Location when project has migrated to .NET 6+
			var folder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
#pragma warning restore RS0030
#else
			var folder = AppContext.BaseDirectory;
#endif
			var process = StartPostinstallProcess(Path.Combine(folder, postInstallAssembly), args);
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				throw new Exception("Installation failed");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		internal static Process StartPostinstallProcess(string postInstallExePath, string[] args)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = postInstallExePath,
				Arguments = string.Join(" ", args.Select(s => s.IndexOf(' ') >= 0 ? $"\"{s}\"" : s)),
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = Path.GetDirectoryName(postInstallExePath),
			};

			return Process.Start(startInfo);
		}
	}
}
