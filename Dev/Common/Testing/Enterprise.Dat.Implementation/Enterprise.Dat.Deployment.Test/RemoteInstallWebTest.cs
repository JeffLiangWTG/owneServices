using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using CargoWiseOne.WebInfrastructure.TestFramework;
using Dat.Integration;
using Dat.Integration.Deployment;
using Enterprise.DbUpgrader.Resource;
using Microsoft.Web.Administration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class RemoteInstallWebTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Install Web Sites")]
		public void TestWebSiteCanBeInstalled()
		{
			// Arrange
			var databaseServer = Db.ServerName;
			var webServer = "localhost";
			var workItemNumber = "TestWebSiteCanBeInstalled";
			var databaseName = $"SH0{workItemNumber}";

			var logs = new StringBuilder("Install logs are:\r\n\r\n");
			var logger = new Mock<ITaskLogger>();
			logger.Setup(x => x.RecordTask(It.IsAny<string>())).Callback<string>(x => logs.AppendLine(x)).Returns(DisposableAction.NoAction);
			logger.Setup(x => x.RecordInfo(It.IsAny<string>())).Callback<string>(x => logs.AppendLine(x));

			var serviceHostSite = InstallSiteItem.GetSitesToInstall().Single(x => x.FolderName == "Services");

			var taskInfo = new TaskInfo("shelf", "owner", $@"
TestRigRestoreFromBackup: noBackupFileNeededForUninstall
TestRigIconName: {workItemNumber}
TestRigSqlServer: {databaseServer}
TestRigDatabaseName: {databaseName}
TestRigWebDomain: {workItemNumber}
TestRigWebSites: {serviceHostSite.FolderName}
TestRigWebServer: {webServer}
TestRigWebServerInstallPath: {portableWebServer.InstallPath}
TestRigCreateIcon: false
TestRigRegister: false
TestRigLaunchDbUpgrade: false
TestRigVerbose: true
");
			var options = new TestedShelfDeploymentOptions(taskInfo);
			var webAddress = $"{options.WebDomain}/{serviceHostSite.DefaultApplicationPath}";
			var expectedAppPoolName = GetAppPoolName(webAddress);

			// Note, this version is below than `InstallationConfiguration.MinimumVersionOfUsingDirectoryLink`,
			// which will make the installation use simple path structure rather than directory link so that the cleanup would be simpler
			const string AppVersion = "21.1.11.111";
			using var db = new OdysseyDbHelper(databaseName, Version.Parse(AppVersion));
			using var cleanup = new DisposableAction(
				() => CleanUp(workItemNumber, expectedAppPoolName, AppVersion),
				() => CleanUp(workItemNumber, expectedAppPoolName, AppVersion));

			// Act
			new RemoteInstallWeb().Invoke(options.WebServer, options.WebServerInstallPath, options.SqlServer, options.DatabaseName, options.WebDomain, options.WebSites, !options.PermitDuplicateWebSites, options.ClientCode, logger.Object, options.Verbose);

			// Assert
			AssertSiteAndAppPoolExist(logs.ToString(), workItemNumber, expectedAppPoolName);
		}

		static string GetAppPoolName(string webAddress)
		{
			return webAddress.Replace('/', '_');
		}

		static void AssertSiteAndAppPoolExist(string logs, string siteName, string appPoolName)
		{
			using (var serverManager = new ServerManager())
			{
				AssertEquals(logs, true, serverManager.Sites.Any(x => x.Name.StartsWith(siteName)));
				AssertEquals(logs, true, serverManager.ApplicationPools.Any(ap => ap.Name == appPoolName));
			}
		}

		static void CleanUp(string siteName, string appPoolName, string versionPath)
		{
			RemoveSiteAndPool(siteName, appPoolName);

			var rootPath = new InstallationConfiguration(Guid.NewGuid()).RootDirectoryPath;
			AssertEndsWith(
				"The root path returned by RootDirectoryPath should be real root instead of that containing version",
				"CargoWiseOneWeb",
				rootPath);
			WebSiteTestManager.DeleteApplicationRootDirectory(Path.Combine(rootPath, versionPath));
		}

		static void RemoveSiteAndPool(string siteName, string appPoolName)
		{
			using (var serverManager = new ServerManager())
			{
				var appPool = serverManager.ApplicationPools.SingleOrDefault(ap => ap.Name == appPoolName);
				if (appPool is not null)
				{
					appPool.Stop();
				}

				var site = serverManager.Sites.SingleOrDefault(s => s.Name == siteName);
				if (site is not null)
				{
					WebDbConfiguration.DeleteAllConfigurations(WebAppPath.For(site));
					serverManager.Sites.Remove(site);
				}

				if (appPool is not null)
				{
					serverManager.ApplicationPools.Remove(appPool);
				}

				serverManager.CommitChanges();
			}
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

		sealed class OdysseyDbHelper : IDisposable
		{
			readonly string dbName;
			readonly Version currentVersion;
			readonly AdminConnection connection;

			public OdysseyDbHelper(string dbName, Version currentVersion)
			{
				this.dbName = dbName;
				this.currentVersion = currentVersion;

				connection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb);

				AdoTestUtils.CreateDbDropExisting(connection, dbName);
				using var useMaster = ((ICurrentDbControl)connection).UseDatabase(dbName);

				CreateLogins();
				PrepareDbSchema();
				UploadCurrentPackage();
			}

			void CreateLogins()
			{
				foreach (var login in connection.Logins)
				{
					login.EnableLogin(_ => { });
					login.EnsureLoginHasRightsToCurrentDatabase();
				}
			}

			void PrepareDbSchema()
			{
				var scriptManager = new ScriptManager();
				foreach (var schema in scriptManager.MainDbSchemas)
				{
					connection.ExecuteNonQuery("CREATE SCHEMA [" + schema + "]");
				}

				connection.ExecuteNonQuery(scriptManager.MaindDbXmlSchemaScript);
				connection.ExecuteNonQuery(scriptManager.MaindDbSchemaScript);
			}

			void UploadCurrentPackage(Version version = null)
			{
				var packageMaker = new PackageMaker(((IDbConnectionInternals)connection).ADOConnection);

				foreach (var installSite in InstallSiteItem.GetSitesToInstall())
				{
					packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, installSite.FolderName, "Web.Config", "");
					packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, installSite.FolderName, "Global.asax", "");
				}

				packageMaker.UploadPackage(currentVersion, "CUR");
			}

			public void Dispose()
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					_ = DbConnectionKiller.KillOtherConnections(adminConnection, dbName);
				}

				DropLogins();
				AdoTestUtils.DropDbIfExists(connection, dbName);
			}

			void DropLogins()
			{
				var logins = new List<string>();
				connection.ExecuteReader(
					"SELECT name FROM sys.server_principals WHERE name LIKE @loginName",
					command => command.AddParameter("@loginName", SqlDbType.NVarChar, 128, $"{dbName}[_]%"),
					record => logins.Add(record["name"].ToString()));

				foreach (var login in logins)
				{
					connection.ExecuteNonQuery($"DROP LOGIN [{login}]");
				}
			}
		}
	}
}
