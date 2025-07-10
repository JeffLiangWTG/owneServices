using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Reflection;
#endif
using System.Xml;
using CargoWise.Definitions;

namespace Enterprise.Upgrades.Preinstall.Starter
{
	class Program
	{
		public const string preInstallAssembly = "Enterprise.Upgrades.Preinstall.exe";

		public static int Main(string[] args)
		{
			try
			{
#if NETFRAMEWORK
#pragma warning disable RS0030 // TODO: WI00814924 - Use Assembly.Location when project has migrated to .NET 6+
				var folder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
#pragma warning restore RS0030
#else
				var folder = AppContext.BaseDirectory;
#endif
				CheckDotNetFrameworkVersion(folder);
				var process = StartPreinstallProcess(folder, args);
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					throw new Exception("Installation failed");
				}
				return ExitCodes.Success;
			}
			catch (NotSupportedException notSupportedEx)
			{
				Console.Error.WriteLine(notSupportedEx);
				return ExitCodes.DotNetVersionNotSupported;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				return ExitCodes.Unspecified;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		internal static Process StartPreinstallProcess(string preInstallExePath, string[] args)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = Path.Combine(preInstallExePath, preInstallAssembly),
				Arguments = string.Join(" ", args.Select(s => s.IndexOf(' ') >= 0 ? $"\"{s}\"" : s)),
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = preInstallExePath,
			};

			return Process.Start(startInfo);
		}

		static void CheckDotNetFrameworkVersion(string folderPath)
		{
			var targetVersion = GetTargetDotNetversion(folderPath);

			var highestRunnableVersion = new ClientDotNetRetriever().GetDotNetVersion().Version;
			if (targetVersion > highestRunnableVersion)
			{
				throw new NotSupportedException($".Net version {targetVersion} is not yet installed on the machine. Please install correct .Net version or higher first.");
			}

			Version GetTargetDotNetversion(string folder)
			{
				const string configPath = preInstallAssembly + ".config";

				var doc = new XmlDocument();
				doc.Load(Path.Combine(folder, configPath));
				var fullVersionInfo = doc
					.DocumentElement
					.SelectSingleNode("/configuration/startup/supportedRuntime[@version='v4.0']")
					.Attributes["sku"]
					.Value;

				var dotNetVersion = fullVersionInfo.Split('v').Last();
				return new Version(dotNetVersion);
			}
		}
	}
}
