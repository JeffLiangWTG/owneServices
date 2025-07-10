using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWiseOne.WebInfrastructure.Integration.Strong.Test.Helpers;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	public sealed class WithSitesInstaller : IDisposable
	{
		public WithSitesInstaller(IInstallationConfiguration installationConfiguration, string siteName = "")
		{
			SiteName = string.IsNullOrWhiteSpace(siteName) ? "web.localhost" : siteName;
			this.installationConfiguration = installationConfiguration;
		}

		public string SiteName { get; }

		public List<InstallSiteItem> InstalledSites { get; private set; }

		public string InstallSites(string serverName, string databaseName)
		{
			var currentVersionDir = GetCurrentVersionDir();

			if (!string.IsNullOrWhiteSpace(currentVersionDir))
			{
				UninstallCurrentVersion();
				SiteInstallerViaAppManager.ExecuteOrInvoke(serverName, databaseName, SiteName);
				UpdateInstalledSites();
			}

			return currentVersionDir;
		}

		string GetCurrentVersionDir()
		{
			var currentVersion = DbVersionsHelper.QueryCurrentVersion()?.Version;
			return currentVersion == null ? string.Empty : Path.Combine(installationConfiguration.RootDirectoryPath, currentVersion.ToString());
		}

		void UpdateInstalledSites()
		{
			InstalledSites = Strong.Test.InstallSites.LoadSitesFromConfig().InstallSiteItems.Where(x => x.Install).ToList();
			InstalledSites.ForEach(x => x.Install(SiteName));
		}

		public void Dispose()
		{
			UninstallCurrentVersion();
		}

		public void UninstallCurrentVersion()
		{
			var currentVersionDir = GetCurrentVersionDir();
			if (string.IsNullOrEmpty(currentVersionDir) || !Directory.Exists(currentVersionDir))
			{
				return;
			}

			SiteUninstallerViaAppManager.ExecuteOrInvoke(currentVersionDir);
		}

		readonly IInstallationConfiguration installationConfiguration;
	}
}
