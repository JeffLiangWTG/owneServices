using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure.ErrorReporting;
using CargoWiseOne.WebInfrastructure.TestFramework;
using Moq;
using NUnit.Framework;

namespace CargoWiseOne.WebInfrastructure.Integration.Test
{
	[UseSnapshotProtection]
	class WebUpgradeManagerLockoutTest : TestCase
	{
		public void TestUpgradeCheckDuringLockoutAndSchemaVersionChange()
		{
			var applicationName = "Test";

			var initialVersion = new Version(1, 0, 0, 0);
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			CreateSelfUpgradingWebsite(packageMaker, PackageMaker.EnterpriseWebDeployZipName, applicationName);
			var initialPackage = packageMaker.UploadPackage(initialVersion, "APL");

			using var sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => ((IDbConnectionInternals)Db.Connection).ADOConnection);
			using var upgradeManager = new WebUpgradeManagerForTest(testConfiguration, new Version(0, 0, 0, 1), sqlContext, errorReporterMock.Object, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			var localPath = upgradeManager.InstallWebFilesForTest(initialPackage);
			var site = testManager.AddWebSite((string name) => Path.Combine(localPath, applicationName));
			testManager.ServerManager.CommitChanges();
			WebDbConfiguration.SaveConfiguration(new WebDbConfigurationInfo() { ApplicationPath = WebAppPath.For(site), ServerName = Db.ServerName, DatabaseName = Db.DatabaseName });
			AssertEquals(Path.Combine(localPath, applicationName), testManager.GetWebPage(site, "ServerPath.aspx").TrimEnd('\\'));
			testManager.GetWebPage(site, "NotifyUpgradeRequired.aspx");
			Thread.Sleep(TimeSpan.FromSeconds(2));
			AssertEquals(Path.Combine(localPath, applicationName), testManager.GetWebPage(site, "ServerPath.aspx").TrimEnd('\\'));

			var finalVersion = new Version(1, 0, 0, 1);
			packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			CreateSelfUpgradingWebsite(packageMaker, PackageMaker.EnterpriseWebDeployZipName, applicationName);
			packageMaker.UploadPackage(finalVersion, "CUR");

			var initialDbMajorSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection);
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					try
					{
						DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(initialDbMajorSchemaVersion + 1, adminConnection);
						testManager.GetWebPage(site, "NotifyUpgradeRequired.aspx");
					}
					finally
					{
						adminConnection.ResetLockout();
					}
				}

				var expectedPath = Path.Combine(localPath, applicationName);
				var updatedPath = testManager.WaitForPageUpdate(site, expectedPath, "ServerPath.aspx");

				AssertEquals(expectedPath, updatedPath);
				AssertEquals(Db.ServerName + ";" + Db.DatabaseName, testManager.GetWebPage(site, "ServerAndDatabase.aspx").TrimEnd('\\'));
			}
			finally
			{
				using (Db.DisableSchemaVersionCheck())
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(initialDbMajorSchemaVersion, Db.Connection);
				}
			}
		}

		public void TestUpgradeCheckAfterLockoutAndSchemaVersionChange()
		{
			var applicationName = "Test";

			var initialVersion = new Version(1, 0, 0, 0);
			var packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			CreateSelfUpgradingWebsite(packageMaker, PackageMaker.EnterpriseWebDeployZipName, applicationName);
			var initialPackage = packageMaker.UploadPackage(initialVersion, "APL");

			using var sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => ((IDbConnectionInternals)Db.Connection).ADOConnection);
			using var upgradeManager = new WebUpgradeManagerForTest(testConfiguration, new Version(0, 0, 0, 1), sqlContext, errorReporterMock.Object, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			var localPath = upgradeManager.InstallWebFilesForTest(initialPackage);
			var site = testManager.AddWebSite((string name) => Path.Combine(localPath, applicationName));
			testManager.ServerManager.CommitChanges();
			WebDbConfiguration.SaveConfiguration(new WebDbConfigurationInfo() { ApplicationPath = WebAppPath.For(site), ServerName = Db.ServerName, DatabaseName = Db.DatabaseName });
			AssertEquals(Path.Combine(localPath, applicationName), testManager.GetWebPage(site, "ServerPath.aspx").TrimEnd('\\'));
			testManager.GetWebPage(site, "NotifyUpgradeRequired.aspx");
			Thread.Sleep(TimeSpan.FromSeconds(2));
			AssertEquals(Path.Combine(localPath, applicationName), testManager.GetWebPage(site, "ServerPath.aspx").TrimEnd('\\'));

			var finalVersion = new Version(1, 0, 0, 1);
			packageMaker = new PackageMaker(((IDbConnectionInternals)Db.Connection).ADOConnection);
			CreateSelfUpgradingWebsite(packageMaker, PackageMaker.EnterpriseWebDeployZipName, applicationName);
			packageMaker.UploadPackage(finalVersion, "CUR");

			var initialDbMajorSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection);
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					try
					{
						DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(initialDbMajorSchemaVersion + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}
				}

				testManager.GetWebPage(site, "NotifyUpgradeRequired.aspx");
				var expectedPath = Path.Combine(localPath, applicationName);
				var updatedPath = testManager.WaitForPageUpdate(site, expectedPath, "ServerPath.aspx");

				AssertEquals(expectedPath, updatedPath);
				AssertEquals(Db.ServerName + ";" + Db.DatabaseName, testManager.GetWebPage(site, "ServerAndDatabase.aspx").TrimEnd('\\'));
			}
			finally
			{
				using (Db.DisableSchemaVersionCheck())
				{
					DbRegistry.DatabaseMajorSchemaVersion.SaveValue(initialDbMajorSchemaVersion, Db.Connection);
				}
			}
		}

		void CreateSelfUpgradingWebsite(PackageMaker packageMaker, string deploymentZip, string applicationName, bool writeTestFile = true)
		{
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, applicationName, "Web.Config", GetWebConfig(writeTestFile));
			packageMaker.AddWebFile(deploymentZip, applicationName, "Global.asax", @"<%@ Application Inherits=""CargoWiseOne.WebInfrastructure.TestFramework.GlobalForTest"" %>");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, applicationName, "ServerPath.aspx", "<%=System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath%>");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, applicationName, "NotifyUpgradeRequired.aspx", "<%CargoWiseOne.WebInfrastructure.WebUpgradeManager.NotifyUpgradeRequired()%>");
			packageMaker.AddWebFile(PackageMaker.EnterpriseWebDeployZipName, applicationName, "ServerAndDatabase.aspx", "<%=\"" + Db.ServerName + ";" + Db.DatabaseName + "\"%>");
		}

		string GetWebConfig(bool writeTestFile)
		{
			var testConfig = GlobalForTest.GetWebConfig(writeTestFile, testConfiguration.RunKey);

			var testXmlDoc = new XmlDocument();
			testXmlDoc.LoadXml(testConfig);

			var baseXmlDoc = new XmlDocument();
			baseXmlDoc.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Web.Base.Config"));

			var runtimeElement = baseXmlDoc.SelectSingleNode("configuration/runtime");
			var newRuntime = testXmlDoc.ImportNode(runtimeElement, true);
			testXmlDoc.DocumentElement.AppendChild(newRuntime);

			using var ms = new MemoryStream();
			using var xmlTextWriter = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented };
			testXmlDoc.Save(xmlTextWriter);
			var result = Encoding.UTF8.GetString(ms.ToArray());
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			testManager = new WebSiteTestManager(HtmlFail);
			Db.Connection.EnsureIsOpen();
			errorReporterMock = new Mock<IErrorReporter>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			testManager.Dispose();

			try
			{
				WebSiteTestManager.DeleteApplicationRootDirectory(testConfiguration.RootDirectoryPath);
			}
			catch
			{
				// ignored because some site been notified to clean up old version
			}
		}

		readonly InstallationConfigurationForTest testConfiguration = new InstallationConfigurationForTest(Guid.NewGuid());
		WebSiteTestManager testManager;
		Mock<IErrorReporter> errorReporterMock;
	}
}
