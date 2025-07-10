using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Loader.Common;
using CargoWise.RemoteDesktopServices.Upgrader.Common;
using Enterprise.RemoteDesktopServices;

namespace CargoWise.CitrixServices.Upgrader
{
	class UpgraderStartupDirector : StartupDirector
	{
		internal const string PluginProductName = ClientCitrixVersion.ProductName;

		protected override Configuration GetNewConfiguration()
		{
			return new UpgraderConfiguration();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override bool InitializeInstallationItems()
		{
			var installation = new Installation(Configuration);

			var mutexObtainedByInstallation = new Mutex(false, "Global\\CargoWiseOneCitrixServicesUpgrader");
			var upgradeInstallerCitrixAsync = new UpgradeInstallerAsync(installation, PluginProductName, ClientCitrixVersion.InstallerExeName);

			TopLevelItem = new InstallationItemContainer(installation, true);
			TopLevelItem.AddDependency(new RunningInstanceChecker(installation, mutexObtainedByInstallation, "CargoWise.CitrixServices.Upgrader"));
			TopLevelItem.AddDependency(new SetUpProcessRunningInstanceChecker(installation, ClientCitrixVersion.InstallerName));
			TopLevelItem.AddDependency(new WindowsRegistryChecker(installation, ["CargoWiseCitrixServices", "CargoWiseOneCitrixServices"], PluginProductName));
			TopLevelItem.AddDependency(new SessionKillerWarning(installation, "wfica32", PluginProductName));
			TopLevelItem.AddDependency(upgradeInstallerCitrixAsync);
			TopLevelItem.AddDependency(new VersionChecker(installation, ClientCitrixVersion.Version.ToString(), ClientCitrixVersion.EncryptedUpgradeCode, TimeSpan.FromMinutes(15), PluginProductName, "https://wisetechacademy.com/search?quickstart=0eb901d6-bad7-4bc7-9d21-2845e2014f95"));
			TopLevelItem.AddDependency(new ResourceCleaner(installation, upgradeInstallerCitrixAsync.CleanUpSetupFiles));
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
