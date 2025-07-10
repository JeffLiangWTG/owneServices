using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace CargoWise.Loader.Client
{
	public class AppManagerInstaller : InstallationItem
	{
		public const string RegistryKeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\CargoWise edi\ediAppMgr";
		public const string RegistryValueName = "CurrentVersion";
		public const string MSIPackageGuid = "5541A8A0_B996_490D_873F_E13C0D732642"; //needs underscores due to .MSI format

		public static Version CurrentVersion
		{
			get
			{
				return new Version(2, 3, 0);
			}
		}

		public AppManagerInstaller(Installation installation)
			: this(installation, installation.Configuration.AppManagerDirectoryName)
		{
			Argument.NotNull(installation, nameof(installation)); // Suggested By ReviewBot 
		}

		public AppManagerInstaller(Installation installation, string appManagerDirectoryName)
			: base(installation)
		{
			Argument.NotNull(installation, nameof(installation));

			this.appManagerDirectoryName = appManagerDirectoryName;

			var value = Installation.Configuration.Services.Registry.GetValue(RegistryKeyName, RegistryValueName, null) as string;

			if (string.IsNullOrEmpty(value))
			{
				install = true;
			}
			else
			{
				try
				{
					Version version = new Version(value);
					upgrade = version < CurrentVersion;
				}
				catch (ArgumentException)
				{
					install = true;
				}
				catch (FormatException)
				{
					install = true;
				}
			}

			if (install)
			{
				AddDependency(CreateSetupTask(installation, appManagerDirectoryName));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		WindowsInstallerProgram CreateSetupTask(Installation installation, string appManagerDirectoryName)
		{
			Argument.NotNull(installation, nameof(installation)); // Suggested By ReviewBot 
			WindowsInstallerProgram setup = new WindowsInstallerProgram(this, "CargoWise Application Manager");

			setup.AddTerminalServerInstallModeDependency();
			setup.SetFullPathOfProgramToRun(Path.Combine(installation.Configuration.GetApplicationPathInCurrentPackage(), "CargoWiseAppManagerSetup.msi"));
			setup.SilentlyIgnoreRebootRequired = true;
			if (!string.IsNullOrEmpty(appManagerDirectoryName))
			{
				setup.Arguments += string.Format(CultureInfo.InvariantCulture, " INSTALLLOCATION.{0}=\"{1}\"", MSIPackageGuid, installation.Configuration.GetTargetPath(appManagerDirectoryName));
			}
			setup.Arguments += (installation.Configuration.UILevel == UILevel.AutomatedWithNoUI) ? " /qn" : " /qb";

			return setup;
		}

		protected virtual string AppManagerUpgradeDllPath
		{
			get
			{
				return Path.Combine(Installation.Configuration.GetApplicationPathInCurrentPackage(), "CargoWise.AppManagerUpgrade.dll");
			}
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			if (upgrade)
			{
				try
				{
					ChangeCurrentTaskDescription("Upgrading CargoWise Application Manager");
					AppManagerResult result;
					using (var appManagerClient = ((ClientConfiguration)Installation.Configuration).GetNewAppManagerClient())
					{
						result = appManagerClient.Upgrade(AppManagerUpgradeDllPath, "CargoWise.AppManagerUpgrade.AppManagerUpgradeDetailsProvider", null, 0);
						if (result.Status == AppManagerResultStatus.Retry)
						{
							Thread.Sleep(1000);
							result = appManagerClient.Upgrade(AppManagerUpgradeDllPath, "CargoWise.AppManagerUpgrade.AppManagerUpgradeDetailsProvider", null, 0);
						}
					}
					switch (result.Status)
					{
						case AppManagerResultStatus.Success:
							Version runningVersion = new Version(0, 0);
							int waitCount = 0;
							do
							{
								if (waitCount > 60) // 60 * 5 seconds = 5 minutes 
								{
									return ManualUpgrade("Timeout waiting for CargoWise Application Manager to upgrade");
								}
								Thread.Sleep(5000);
								using (var appManagerClient = ((ClientConfiguration)Installation.Configuration).GetNewAppManagerClient())
								{
									try
									{
										runningVersion = appManagerClient.GetVersionNumber();
									}
									catch (Exception e) when (!e.IsCriticalException()) { }
								}
								waitCount++;
							} while (runningVersion < CurrentVersion);
							return InstallationResult.OK();
						default:
							return ManualUpgrade(result.Message);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					return ManualUpgrade(e.Message);
				}
			}
			else
			{
				return InstallationResult.OK();
			}
		}

		InstallationResult ManualUpgrade(string errorMessage)
		{
			var results = new InstallationResultCollection();
			CreateSetupTask(this.Installation, appManagerDirectoryName).Install(results);
			return results.ErrorCount == 0 ? InstallationResult.OK() : InstallationResult.Error(results.GetErrorMessages());
		}

		protected override bool NeedsToInstallCore()
		{
			return install || upgrade;
		}

		readonly bool install;

		readonly bool upgrade;
		readonly string appManagerDirectoryName;
	}
}
