using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.IO;
using CargoWise.Loader.Common;

namespace Enterprise.Loader
{
	class ProgramLocationChecker : InstallationItem
	{
		public ProgramLocationChecker(Installation installation)
			: base(installation)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		protected override bool NeedsToInstallCore()
		{
			return !((EnterpriseConfiguration)Installation.Configuration).RunFromThisLocation
				&& !Assembly.GetEntryAssembly().Location.StartsWith(Installation.Configuration.BaseTargetPath, StringComparison.OrdinalIgnoreCase);
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			string commandLine = Installation.Configuration.AllArguments;
			if (!commandLine.Contains(EnterpriseConfiguration.StartupPathArgumentPrefix))
			{
				commandLine = InsertCommand(commandLine, EnterpriseConfiguration.StartupPathArgumentPrefix + "\"" + Installation.Configuration.StartupPath + "\"");
			}

			string installedExe = Path.Combine(Installation.Configuration.BaseTargetPath, EnterpriseConfiguration.StartupExeFileName);
			if (File.Exists(installedExe) && new Version(FileVersionInfo.GetVersionInfo(installedExe).FileVersion) >= new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion))
			{
				StartAndExit(installedExe, commandLine);
			}
			else
			{
				commandLine = InsertCommand(commandLine, EnterpriseConfiguration.RunFromThisLocationArgument);

				string sourceExe = Assembly.GetExecutingAssembly().Location;
				string targetExe = Path.Combine(Temp.TempPath, EnterpriseConfiguration.StartupExeFileName);
				File.Copy(sourceExe, targetExe);

				if (File.Exists(sourceExe + ".config"))
				{
					string targetExeConfig = Path.Combine(Temp.TempPath, EnterpriseConfiguration.StartupExeConfigFileName);
					File.Copy(sourceExe + ".config", targetExeConfig);
				}

				var sourceLogConfig = Path.Combine(Path.GetDirectoryName(sourceExe)!, EnterpriseConfiguration.ApplicationLoggingConfigFileName);
				if (File.Exists(sourceLogConfig))
				{
					string targetLogConfig = Path.Combine(Temp.TempPath, EnterpriseConfiguration.ApplicationLoggingConfigFileName);
					File.Copy(sourceLogConfig, targetLogConfig);
				}

				StartAndExit(targetExe, commandLine);
			}
			return InstallationResult.OK();
		}

		/// <summary>
		/// Command line might have the special <see cref="EnterpriseConfiguration.CommandArgument"/> in it, which means everything
		/// after that gets passed to the program being launched. Therefore we can't simply append to the 
		/// end of the string because we want to pass this argument to CargoWise.Start.exe, not to the program being launched.
		/// </summary>
		static string InsertCommand(string sourceCommandLine, string additionalCommandToInsert)
		{
			//Match '-Cmd', but not within the quoted string
			var matchExpression = $@"(?<=^([^\""]|\""[^\""]*\"")*)(\s\{EnterpriseConfiguration.CommandArgument}\s)";
			int cmdIndex = Regex.Match(sourceCommandLine, matchExpression)?.Index ?? -1;
			if (cmdIndex > 0)
			{
				return sourceCommandLine.Insert(cmdIndex, " " + additionalCommandToInsert);
			}
			else
			{
				return sourceCommandLine + " " + additionalCommandToInsert;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		internal virtual void StartAndExit(string exe, string commandLine)
		{
			Process.Start(new ProcessStartInfo(exe, commandLine) { WorkingDirectory = Path.GetDirectoryName(exe) });
			Environment.Exit(0);
		}
	}
}
