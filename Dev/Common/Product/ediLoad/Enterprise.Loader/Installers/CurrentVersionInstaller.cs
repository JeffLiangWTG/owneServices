using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;

namespace Enterprise.Loader
{
	public class CurrentVersionInstaller : InstallationItem
	{
		public CurrentVersionInstaller(Installation installation, InstallationProgramFromVersionedFile enterpriseLaunch)
			: base(installation)
		{
			AddDependency(new VersionInfoInitializer(installation));
			this.enterpriseLaunch = enterpriseLaunch;
		}

		readonly InstallationProgramFromVersionedFile enterpriseLaunch;

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			ChangeCurrentTaskDescription("Installing current version");
			try
			{
				EnterpriseConfiguration configuration = (EnterpriseConfiguration)Installation.Configuration;
				if (configuration.SelectCurrentVersion)
				{
					return ShowNoCurrentVersionDialog(configuration);
				}
				else if (VersionInfoInitializer.CurrentVersion != null)
				{
					if (VersionInfoInitializer.CurrentVersion.Version.Equals(configuration.TargetVersion))
					{
						try
						{
							ValidateInstallation(configuration.TargetVersion, configuration.CurrentPackage);
							return InstallationResult.OK();
						}
						catch (InvalidPackageException)
						{
							// The current package is invalid, so we need to upgrade
						}
					}

					var upgradeManager = configuration.NewUpgradeManager();
					var targetInstallationPath = Path.Combine(Installation.Configuration.BaseTargetPath, VersionInfoInitializer.CurrentVersion.Version.ToString());
					var commandLineArgs = Installation.Configuration.AllArguments.Split(' ');

					using (upgradeManager.InstallUpgradePackage(VersionInfoInitializer.CurrentVersion, targetInstallationPath, null, commandLineArgs))
					{
						upgradeManager.RunCurrentVersionWriter(targetInstallationPath);
						Installation.Configuration.TargetVersion = VersionInfoInitializer.CurrentVersion.Version;
						return InstallationResult.OK();
					}
				}
				else
				{
					return ShowNoCurrentVersionDialog(configuration);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return InstallationResult.Error(ex.ToString());
			}
		}

		protected virtual void ValidateInstallation(Version targetVersion, string installationPath)
		{
			UpgradeManager.ValidateInstallation(targetVersion, installationPath);
		}

		InstallationResult ShowNoCurrentVersionDialog(EnterpriseConfiguration configuration)
		{
			if (configuration.UILevel != UILevel.AutomatedWithNoUI)
			{
				UpgradeManager upgradeManager = configuration.NewUpgradeManager();
				var upgradePackages = upgradeManager.QueryRunnablePackages();
				if (upgradePackages.Count > 0)
				{
					using (NoCurrentVersionDialog noCurrentVersionDialog = new NoCurrentVersionDialog(upgradePackages))
					{
						noCurrentVersionDialog.ShowDialog();
						if (noCurrentVersionDialog.UpgradeToRun != null)
						{
							using (upgradeManager.InstallUpgradePackage(noCurrentVersionDialog.UpgradeToRun, Path.Combine(Installation.Configuration.BaseTargetPath, noCurrentVersionDialog.UpgradeToRun.Version.ToString())))
							{
								if (enterpriseLaunch != null)
								{
									enterpriseLaunch.Arguments += " -Upgrade:" + noCurrentVersionDialog.UpgradeToRun.PK;
								}

								Installation.Configuration.TargetVersion = noCurrentVersionDialog.UpgradeToRun.Version;
								return InstallationResult.OK();
							}
						}
					}
				}
				else
				{
					return InstallationResult.Error("No compatible software version is available to install.");
				}
			}

			return InstallationResult.Error("No current software version configured.");
		}
	}
}
