using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using Microsoft.Win32;
using static System.FormattableString;
using static Enterprise.Client.Common.LogHelper;

namespace Enterprise.Client.Common
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
	public class OldVersionsRemover : InstallationItem
	{
#if DEBUG
		protected
#endif
		readonly List<Exception> exceptionLog = new List<Exception>();

		/// <param name="silentlyContinueOnError">
		/// When running this on application launch, it's not worth crashing and failing to launch the application
		/// merely because we couldn't clean up disk space.
		/// When running this on uninstall, it is worth reporting the error because uninstalling is the task the user
		/// is trying to accomplish.
		/// </param>
		public OldVersionsRemover(Installation installation, bool uninstallAll = false, bool silentlyContinueOnError = false)
			: this(installation, new CurrentVersionCleanerFactory(), new CurrentVersionCleanerConfigManager(), uninstallAll, silentlyContinueOnError)
		{
		}

#if DEBUG
		public
#endif
		OldVersionsRemover(Installation installation, ICurrentVersionCleanerFactory configFactory, ICurrentVersionCleanerConfigManager configManager, bool uninstallAll, bool silentlyContinueOnError)
			: base(installation)
		{
			Argument.NotNull(configFactory, nameof(configFactory));
			Argument.NotNull(configManager, nameof(configManager));

			this.configFactory = configFactory;
			this.configManager = configManager;
			this.uninstallAll = uninstallAll;
			this.silentlyContinueOnError = silentlyContinueOnError;
		}

		readonly ICurrentVersionCleanerFactory configFactory;
		readonly ICurrentVersionCleanerConfigManager configManager;
		readonly bool uninstallAll;
		readonly bool silentlyContinueOnError;

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		public static void Run(string baseInstallationPath, Version targetVersion)
		{
			var configuration = NewConfiguration();
			configuration.BaseTargetPath = baseInstallationPath;
			configuration.TargetVersion = targetVersion;
			var result = new OldVersionsRemover(new Installation(configuration)).InstallExcludingDependencies();
			if (result.IsError || result.IsWarning)
			{
				WriteEventLog(result.Message, EventLogEntryType.Warning);
			}
		}

		static Configuration NewConfiguration()
		{
			return new Configuration();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		protected override InstallationResult InstallExcludingDependencies()
		{
			const string name = "Removing old installations";
			ChangeCurrentTaskDescription(name);
			try
			{
				InstallationResult cleanupResult;
				using (var mutex = new UpgraderMutex($"Global\\CargoWiseOne{nameof(OldVersionsRemover)}"))
				{
					if (uninstallAll && mutex.WaitOne()
						|| mutex.IsCreatedNew && mutex.WaitOne(TimeSpan.FromMilliseconds(100)))
					{
						cleanupResult = CleanupCurrentVersionAndOldVersionFolders();
					}
					else
					{
						return InstallationResult.OK($"{name} skipped, due to the other instance is running");
					}
				}

				try
				{
					CleanupNGenRoots();
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					exceptionLog.Add(e);
				}

				if (silentlyContinueOnError && !cleanupResult.IsOK)
				{
					WriteEventLog(cleanupResult.Message, EventLogEntryType.Warning);
					return InstallationResult.OK(cleanupResult.Message);
				}
				return cleanupResult;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = ex.ToString();
				if (silentlyContinueOnError)
				{
					WriteEventLog(message, EventLogEntryType.Error);
					return InstallationResult.OK(message);
				}
				else
				{
					return InstallationResult.Error(message);
				}
			}

			InstallationResult CleanupCurrentVersionAndOldVersionFolders()
			{
				var baseInstallationPath = Installation.Configuration.BaseTargetPath;
				if (Directory.Exists(baseInstallationPath))
				{
					var config = configManager.LoadConfiguration(baseInstallationPath);
					if (DateTime.UtcNow.Subtract(config.LastStartTime) < config.CurrentVersionCleanupIntervalInDays)
					{
						var skipCleanerMessage = Invariant($"Skipped cleanup as the latest cleanup was performed on: {config.LastStartTime} less than {config.CurrentVersionCleanupIntervalInDays.TotalDays} day(s) of the configured intervals.");
						return InstallationResult.OK(skipCleanerMessage);
					}

					CleanCurrentVersion(baseInstallationPath, config);

					var currentVersionFile = Path.Combine(baseInstallationPath, "CurrentVersion");
					var versionsInUse = new CurrentVersionFile(currentVersionFile).RetrieveAllVersionNumbers();
					foreach (var directory in Directory.GetDirectories(baseInstallationPath).Where(directory => !versionsInUse.Contains(Path.GetFileName(directory))))
					{
						if (!uninstallAll && DateTime.UtcNow.Subtract(Directory.GetCreationTimeUtc(directory)) < TimeSpan.FromHours(24))
						{
							continue;
						}

						string versionString = Path.GetFileName(directory);
						if (versionString.Length == 0)
						{
							throw new InvalidOperationException("Path.GetFileName(directory) could not get the path.");
						}

						if (Version.TryParse(versionString, out var version)
							&& (Installation.Configuration.TargetVersion == null
								|| Installation.Configuration.TargetVersion != version))
						{
							try
							{
								using (var mutex = UpgraderMutex.GetVersionedFolderMutex(version))
								{
									if (mutex.IsCreatedNew && mutex.WaitOne(0))
									{
										UpgradeManager.SafeDeleteInstallationDirectory(directory);
									}
								}
							}
							catch (Exception e) when (!e.IsCriticalException())
							{
								exceptionLog.Add(e);
							}
						}
					}
				}

				UpdateLastSuccessTimeAfterCleanUp();

				if (exceptionLog.Count == 0)
				{
					return InstallationResult.OK();
				}

				var builder = new StringBuilder("Completed, but with some caught exceptions: ");
				var i = 0;
				var allFileInUseExceptions = true;

				foreach (Exception e in exceptionLog)
				{
					if (allFileInUseExceptions && (e.Message == null || !e.Message.StartsWith("Could not delete as there are files in use:")))
					{
						allFileInUseExceptions = false;
					}

					builder.Append("\r\n" + ++i + ": " + e);
				}

				var message = builder.ToString();

				message = message.Length > 20000
					? message.Substring(0, 20000) + "\u2026"
					: message;

				return allFileInUseExceptions ? InstallationResult.NotAffectCargoWiseFunctionsWarning(message) : InstallationResult.Warning(message);

				void UpdateLastSuccessTimeAfterCleanUp()
				{
					if (!Directory.Exists(baseInstallationPath))
					{
						return;
					}

					var config = configManager.LoadConfiguration(baseInstallationPath);
					config.LastSuccessTime = DateTime.UtcNow;
					var savingConfigResult = configManager.SaveConfigurationViaAppManager(baseInstallationPath, config);
					if (savingConfigResult.Status != AppManagerResultStatus.Success)
					{
						throw new SaveCurrentVersionCleanerConfigException(baseInstallationPath, savingConfigResult);
					}
				}
			}
		}

#if DEBUG
		public virtual
#endif
		void CleanCurrentVersion(string baseInstallationPath, ICurrentVersionCleanerConfigWithLogs config)
		{
			var cleaner = configFactory.GetCurrentVersionCleaner(config);

			UpdateDateTimesBeforeCleanCurrentVersion();
			cleaner.CleanCurrentVersion();

			void UpdateDateTimesBeforeCleanCurrentVersion()
			{
				var utcNow = DateTime.UtcNow;
				config.LastStartTime = utcNow;
				config.NextRuntime = utcNow + config.CurrentVersionCleanupIntervalInDays;

				var savingConfigResult = configManager.SaveConfigurationViaAppManager(baseInstallationPath, config);
				if (savingConfigResult.Status != AppManagerResultStatus.Success)
				{
					exceptionLog.Add(new SaveCurrentVersionCleanerConfigException(baseInstallationPath, savingConfigResult));
				}
			}
		}

#if DEBUG
		public virtual
#endif
		void CleanupNGenRoots()
		{
			var mutexName = "Global\\CargoWiseOneCleanupNgenRoots";
			using (var mutex = new UpgraderMutex(mutexName))
			{
				if (mutex.IsCreatedNew && mutex.WaitOne(0))
				{
					CleanupNGenRoots(RegistryView.Registry32);
					CleanupNGenRoots(RegistryView.Registry64);
				}
			}
		}

		void CleanupNGenRoots(RegistryView view)
		{
			using (var hklmReg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
			{
				IEnumerable<string> binariesList;

				using (var roots = hklmReg.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework\v2.0.50727\NGenService\Roots", false))
				{
					if (roots == null)
					{
						throw new InvalidOperationException("OpenSubKey operation failed.");
					}

					binariesList = FilterBinariesList(roots.GetSubKeyNames());
				}

				foreach (var root in binariesList)
				{
					NGenUninstall(root);
				}
			}
		}

		protected virtual IEnumerable<string> FilterBinariesList(string[] subKeyNames)
		{
			return subKeyNames
				.Select(name => name.Replace("/", @"\"))
				.Where(path =>
					(
						path.StartsWith(CargoWiseEdiPath, StringComparison.OrdinalIgnoreCase)
						|| path.StartsWith(LegacyWiseTechPath, StringComparison.OrdinalIgnoreCase)
						|| path.StartsWith(WiseTechPath, StringComparison.OrdinalIgnoreCase)
					)
					&& !File.Exists(path));
		}

		static string CargoWiseEdiPath
		{
			get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "CargoWise edi");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		internal static string WiseTechPath
		{
			get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WiseTech Global", "CargoWise");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		internal static string LegacyWiseTechPath
		{
			get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WiseTech Global", "CargoWise One");
		}

		void NGenUninstall(string file)
		{
			try
			{
				if (!File.Exists(file))
				{
					var results = new InstallationResultCollection();
					new NGenInstaller(Installation, Installation.Configuration.CurrentPackage, file, NGenInstaller.Action.Uninstall).Install(results);
					if (results.ErrorCount > 0 || results.WarningCount > 0)
					{
						throw new InvalidOperationException($@"Errors occurred during un-installation of NGen:
{string.Join(Environment.NewLine, new[] { results.GetErrorMessages(), results.GetWarningMessages() })}");
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				exceptionLog.Add(e);
			}
		}
	}
}
