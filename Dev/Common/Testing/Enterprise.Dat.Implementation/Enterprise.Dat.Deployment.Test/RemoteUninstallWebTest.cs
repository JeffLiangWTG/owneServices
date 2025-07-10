using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.IO;
using CargoWiseOne.WebInfrastructure;
using Dat.Integration;
using Dat.Integration.Deployment;
using Microsoft.Web.Administration;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	class RemoteUninstallWebTest : TestCase
	{
		public void TestWebSitesAreUninstalledForTheSameWebDbConfig()
		{
			var databaseServer = Db.ServerName;
			var webServer = "localhost";
			var workItemNumber = "WI00644263";
			var testDatabaseName = $"SH0{workItemNumber}";

			using (var tempDir = new TempDirectory())
			{
				// Arrange
				var testWebDomain = AddNewTestSite(databaseServer, testDatabaseName, tempDir);
				AssertEquals(true, AnyWebSiteExistsHavingNameStartsWith(testWebDomain));

				// Act
				var taskInfo = new TaskInfo("shelf", "owner", $@"
TestRigRestoreFromBackup: noBackupFileNeededForUninstall
TestRigIconName: {workItemNumber}
TestRigSqlServer: {databaseServer}
TestRigDatabaseName: {testDatabaseName}
TestRigWebDomain: {workItemNumber}
TestRigWebSites: {string.Join(",", InstallSiteItem.GetSitesToInstall().Select(x => x.FolderName))}
TestRigWebServer: {webServer}
TestRigWebServerInstallPath: {portableWebServer.InstallPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
");
				var options = new TestedShelfDeploymentOptions(taskInfo);
				var logger = new TaskLogger();
				new RemoteUninstallWeb().Invoke(options.WebServer, options.WebServerInstallPath, options.SqlServer, options.DatabaseName, options.WebDomain, !options.PermitDuplicateWebSites, logger, options.Verbose);

				// Assert
				AssertEquals(false, AnyWebSiteExistsHavingNameStartsWith(testWebDomain));
			}
		}

		static bool AnyWebSiteExistsHavingNameStartsWith(string siteName)
		{
			using (var serverManager = new ServerManager())
			{
				return serverManager.Sites.Any(x => x.Name.StartsWith(siteName));
			}
		}

		static string AddNewTestSite(string serverName, string databaseName, string applicationPhysicalPath)
		{
			var testSitePrefix = $"TestSite_{Guid.NewGuid():N}";

			using (var serverManager = new ServerManager())
			{
				for (var i = 0; i < 3; i++)
				{
					var testSiteName = $"{testSitePrefix}_{i}";
					_ = serverManager.ApplicationPools.Add(testSiteName);
					var testSite = serverManager.Sites.Add(testSiteName, "https", $"*:443:{testSiteName}/", applicationPhysicalPath);

					testSite.Applications[0].ApplicationPoolName = testSiteName;
					serverManager.CommitChanges();

					WebDbConfiguration.SaveConfiguration(new WebDbConfigurationInfo()
					{
						ServerName = serverName,
						DatabaseName = databaseName,
						ApplicationPath = WebAppPath.For(testSite)
					});
				}
			}

			return testSitePrefix;
		}

		PortableWebServer portableWebServer;
		protected override void SetUp()
		{
			base.SetUp();
			portableWebServer = new PortableWebServer();
		}

		protected override void TearDown()
		{
			portableWebServer.Dispose();
			base.TearDown();
		}
	}
}
