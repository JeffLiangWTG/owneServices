using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	/// <summary>
	/// Checks if the executing application needs to be upgraded and installs the upgrade.
	/// </summary>
	public class InstallCurrentVersionTask : AbstractApplicationStartupTask, IApplicationStartupTaskProgress
	{
		public override string TaskDescription => Res.GetString("46D4B528-A46D-4f8a-9A5E-97A8035E5765", "Checking for current software version");

		public override int FailureExitCode => ExitCodes.InstallCurrentVersionTaskExit;

		public event ZArchitecture.Core.Progress Progress;

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return arguments[ApplicationArguments.OptionUpgrade] == null && (bool)arguments[ApplicationArguments.OptionRunWithoutLoader] && !(bool)arguments[ApplicationArguments.OptionSkipVersionCheck];
		}

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			var currentVersionInDatabase = upgradeManager.QueryCurrentVersion();
			if (currentVersionInDatabase != null && !ReleaseInfo.Instance.VersionNumber.ToVersion().Equals(currentVersionInDatabase.Version))
			{
				if (currentVersionInDatabase.Version < UpgradeManager.MinimumRunnableVersion)
				{
					StartupNotification.Show(Res.GetString("108a8646-dfa0-4bea-8de6-4a13ba1307d6", "The current version marked in the database is too old to be run with the current version of ediLoad.\r\nRestore ediLoad.exe.old to ediLoad.exe on the server directory if you really want to run the old version."), BrandingFactory.Instance.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}
				try
				{
					var targetInstallationPath = InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber(currentVersionInDatabase.Version));
					using (upgradeManager.InstallUpgradePackage(currentVersionInDatabase, targetInstallationPath, new string[] { "-NoUI" }))
					{
						upgradeManager.RunCurrentVersionWriter(targetInstallationPath);
					}
					new Upgrader().LaunchInstalledPackage(new VersionNumber(currentVersionInDatabase.Version));
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					StartupNotification.ShowError(Res.GetString("71672435-cfd0-4389-8076-dbd952aa3d38", "An error occurred installing the current software version.") + "\r\n\r\n" + e.Message, BrandingFactory.Instance.ProductName);
				}
				return false;
			}
			return true;
		}

		public bool OnProgress(string status, int percentComplete)
		{
			if (Progress != null)
			{
				status = Res.GetString("5A38AE45-C739-4001-8F96-F04E04612A82", "Installing current version...\r\n{0}", status);
				Progress(status, percentComplete);
			}
			return true;
		}
	}
}
