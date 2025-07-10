using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Loader.Common;
using CargoWise.RemoteDesktopServices.Upgrader.Common;
using Enterprise.RemoteDesktopServices;

namespace CargoWise.RemoteDesktopServices.Upgrader
{
	class UpgraderStartupDirector : StartupDirector
	{
		internal const string PluginProductName = ClientVersion.ProductName;

		protected override Configuration GetNewConfiguration()
		{
			return new UpgraderConfiguration();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override bool InitializeInstallationItems()
		{
			var installation = new Installation(Configuration);

			var mutexObtainedByInstallation = new Mutex(false, "Global\\CargoWiseOneRemoteDesktopServicesUpgrader");
			var upgradeInstallerAsync = new UpgradeInstallerAsync(installation, PluginProductName, ClientVersion.InstallerExeName);

			TopLevelItem = new InstallationItemContainer(installation, true);
			TopLevelItem.AddDependency(new RunningInstanceChecker(installation, mutexObtainedByInstallation, "CargoWise.RemoteDesktopServices.Upgrader"));
			TopLevelItem.AddDependency(new SetUpProcessRunningInstanceChecker(installation, ClientVersion.InstallerName));
			TopLevelItem.AddDependency(new WindowsRegistryChecker(installation, ["CargoWiseRemoteDesktopServices", "CargoWiseOneRemoteDesktopServices"], PluginProductName));
			TopLevelItem.AddDependency(new SessionKillerWarning(installation, "mstsc", PluginProductName));
			TopLevelItem.AddDependency(upgradeInstallerAsync);
			TopLevelItem.AddDependency(new VersionChecker(installation, ClientVersion.Version.ToString(), ClientVersion.EncryptedUpgradeCode, TimeSpan.FromMinutes(15), PluginProductName, "https://wisetechacademy.com/search?quickstart=0eb901d6-bad7-4bc7-9d21-2845e2014f95"));
			TopLevelItem.AddDependency(new ResourceCleaner(installation, upgradeInstallerAsync.CleanUpSetupFiles));
			TopLevelItem.AddDependency(new InstallationDoneNotifier(installation, PluginProductName));
			TopLevelItem.AddDependency(new ResourceCleaner(installation, mutexObtainedByInstallation.ReleaseMutex)); // Release the mutex after the installation is done

			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override void StartUserInterface()
		{
			base.StartUserInterface();
			TopLevelItem.ChangeCurrentTaskDescription($"Upgrading {PluginProductName}");
		}
	}
}
