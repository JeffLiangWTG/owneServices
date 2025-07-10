using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	abstract public class PowerBiDeployerTest<T> : TestCase where T : PowerBiDeployer
	{
		readonly char AltDirectorySeparatorChar = System.IO.Path.AltDirectorySeparatorChar;
		protected override void SetUp()
		{
			base.SetUp();
			if (TestingState.IsRunningOnDAT)
			{
				Name = "SkipTest";
			}
		}

		public void SkipTest()
		{
			Assert(true);
		}

		public static string PowerBiWebPortalUrl
		{
			get
			{
				return SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value;
			}
		}

		protected abstract T TestDeployerInstance { get; }

		protected LoggerForTest TestLogger => (testLogger = testLogger ?? new LoggerForTest());
		LoggerForTest testLogger;

		abstract public void TestPowerBiFolderStructure();

		abstract public void TestPowerBiCleanRootFolder();

		abstract public void TestIsDataSourceConfigured();

		[UseSnapshotProtection]
		public void TestUnauthorizedExceptionWarning()
		{
			var reportCredentials = new BiReportCredential();
			reportCredentials.Domain = System.Environment.MachineName;
			reportCredentials.UserName = "NonExistingUser";
			reportCredentials.Password = "Test123456";
			string errorMessage;

			using (CreateLocalUser(reportCredentials.UserName, reportCredentials.Password))
			{
				var deployer = TestDeployerInstance;

				AssertNoExceptionThrown(() =>
				{
					deployer.IsPowerBiConfigured(out errorMessage);
				});
				var logger = (LoggerForTest)deployer.Logger;
				AssertCollectionContains(logger.LogEntries, deployer.UnauthorizedExceptionMessage);
			}
		}

		public void TestPowerBIServerVersion()
		{
			var deployer = TestDeployerInstance;
			var validVersions = new List<string> { "1.5", "1.6", "2.1" };

			foreach (var version in validVersions)
			{
				((IExposedDeployer)deployer).PowerBiServerVersion_Exposed = version;
				deployer.CheckPowerBiHasCorrectServerVersion();
				AssertCollectionNotContains("The report server needs to be upgraded to the September 2019 Version'", TestLogger.LogEntries);
			}

			var invalidVersions = new List<string> { "0.5", "1.1", "1.4" };

			foreach (var version in invalidVersions)
			{
				((IExposedDeployer)deployer).PowerBiServerVersion_Exposed = version;
				deployer.CheckPowerBiHasCorrectServerVersion();
				AssertCollectionContains("The report server needs to be upgraded to the September 2019 Version'", TestLogger.LogEntries);
			}
		}

		public void TestDeployReports()
		{
			var deployer = TestDeployerInstance;

			if (CheckEnvironment(deployer))
			{
				try
				{
					deployer.DeployReportFiles();
					AssertReportsDeployed(deployer);
				}
				finally
				{
					deployer.DeleteCatalogItem(deployer.ClientFolderPath);
				}
			}
		}

		public void TestDeployDatasets()
		{
			var deployer = TestDeployerInstance;
			if (CheckEnvironment(deployer))
			{
				try
				{
					TestLogger.ClearLog();
					deployer.DeploySharedDatasets();
					AssertCollectionContains("Expected message: 'Deploying Shared Datasets'", "Deploying Shared Datasets:", TestLogger.LogEntries);
					TestLogger.ClearLog();
				}
				finally
				{
					deployer.DeleteCatalogItem(deployer.ClientFolderPath);
				}
			}
		}

		public void TestCheckPolicies()
		{
			var deployer = TestDeployerInstance;

			if (CheckEnvironment(deployer))
			{
				try
				{
					deployer.DeployReportFiles();
					AssertReportsDeployed(deployer);

					TestLogger.ClearLog();
					deployer.CheckPowerBiServerHealthStatus();
					//AssertEquals(true, TestLogger.LogEntries.Count() == 0);
					Assert(string.Join("\r\n", TestLogger.LogEntries), TestLogger.LogEntries.IsNullOrEmpty());

					TestLogger.ClearLog();
					((IExposedDeployer)deployer).SetInheritParentPolicy(deployer.ClientFolderPath, true);
					deployer.CheckPowerBiServerHealthStatus();
					Assert(string.Join("\r\n", TestLogger.LogEntries), TestLogger.LogEntries.Contains(string.Format(CultureInfo.InvariantCulture, "Folder path '{0}' should not inherit parent policy.", deployer.ClientFolderPath)));
				}
				finally
				{
					deployer.DeleteCatalogItem(deployer.ClientFolderPath);
				}
			}
		}

		public void TestClientSystemGroupDoesNotExist()
		{
			var deployer = TestDeployerInstance;
			((IExposedDeployer)deployer).ClientSystemExposed = string.Empty;
			if (CheckEnvironment(deployer))
			{
				try
				{
					deployer.DeployReportFiles();
					AssertReportsDeployed(deployer);
					AssertCollectionContains("Expected warning message: 'Client system group cannot be determined. Make sure that roles are correctly assigned in the Power BI web browser.'",
						"Client system group cannot be determined. Make sure that roles are correctly assigned in the Power BI web browser.",
						TestLogger.LogEntries);
				}
				finally
				{
					deployer.DeleteCatalogItem(deployer.ClientFolderPath);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestBiReportUserDoesNotExist()
		{
			var deployer = TestDeployerInstance;
			if (CheckEnvironment(deployer))
			{
				try
				{
					var biReportCredential = new BiReportCredential();
					biReportCredential.Domain = "CORP";
					biReportCredential.UserName = "NonExistingUser";
					biReportCredential.Password = "Test123456";
					SystemDataRegistry.Instance.BiReportUserCredential.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, biReportCredential);

					deployer.DeployReportFiles();
				}
				catch (PowerBiException ex)
				{
					AssertEquals("Exception message", @"The user or group name 'CORP\NonExistingUser' is not recognized.", ex.Message);
				}
				finally
				{
					deployer.DeleteCatalogItem(deployer.ClientFolderPath);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestLocalUserHasRightsThroughImpersonation()
		{
			var reportCredentials = new BiReportCredential();
			reportCredentials.Domain = System.Environment.MachineName;
			reportCredentials.UserName = "NonExistingUser";
			reportCredentials.Password = "Test123456";

			using (CreateLocalUser(reportCredentials.UserName, reportCredentials.Password))
			{
				var deployer = TestDeployerInstance;

				if (CheckEnvironment(deployer))
				{
					AssertNoExceptionThrown(() =>
					{
						deployer.DeployReportFiles();
					});
				}
			}
		}

		IDisposable CreateLocalUser(string userName, string password)
		{
			bool userAlreadyExists = false;

			using (var context = new PrincipalContext(ContextType.Machine))
			{
				var user = UserPrincipal.FindByIdentity(context, userName);
				if (user == null)
				{
					user = new UserPrincipal(context);
					user.Name = userName;
					user.Enabled = true;
					user.SetPassword(password);
					user.Save();
				}
				else
				{
					userAlreadyExists = true;
				}
			}

			return userAlreadyExists ? null :
				new DisposableAction(() =>
				{
					using (var context = new PrincipalContext(ContextType.Machine))
					{
						var user = UserPrincipal.FindByIdentity(context, userName);
						if (user != null)
						{
							user.Delete();
						}
					}
				});
		}

		public void TestPowerBiWebPortalUrlRemovesSlash()
		{
			var powerBiBrowserNoSlash = @"http://" + System.Environment.MachineName + @":80/PBIReportServer";
			var powerBiBrowser = powerBiBrowserNoSlash + AltDirectorySeparatorChar;

			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, powerBiBrowser))
			{
				AssertEquals("PowerBi Web Portal Url should have no trailing slash", powerBiBrowserNoSlash, PowerBiWebPortalUrl);
			}
		}

		public void TestPowerBiWebPortalUrlNoSlash()
		{
			var powerBiBrowser = @"http://" + System.Environment.MachineName + @":80/PBIReportServer";

			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, powerBiBrowser))
			{
				AssertEquals("PowerBi Web Portal Url should have no trailing slash", powerBiBrowser, PowerBiWebPortalUrl);
			}
		}

		[UseSnapshotProtection]
		public void TestGetAnalysisServerForDataSource()
		{
			var analysisServer = System.Environment.MachineName + @"\Instance, 80";
			var powerBiBrowser = @"http://" + System.Environment.MachineName + @":80/PBIReportServer";

			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, analysisServer))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, powerBiBrowser))
			{
				var deployer = TestDeployerInstance;
				var analysisServerValueForDataSource = ((IExposedDeployer)deployer).GetAnalysisServerForDataSource_Exposed(analysisServer);
				AssertEquals("Analysis server for data source", @".\Instance, 80", analysisServerValueForDataSource);
			}
		}
		public bool CheckEnvironment(PowerBiDeployer deployer)
		{
			if (string.IsNullOrEmpty(PowerBiWebPortalUrl))
			{
				Assert("This test requires PowerBiReportsUrl registry to contain a value!", false);
				return false;
			}
			string errorMessage;
			if (!deployer.IsPowerBiConfigured(out errorMessage))
			{
				Assert("This test requires Power BI to be configured!", false);
				return false;
			}
			return true;
		}

		abstract public void AssertReportsDeployed(PowerBiDeployer deployer);
		[UseSnapshotProtection]
		public void TestExceptionMessageContainsPBIURLForIsPowerBiConfigured()
		{
			var analysisServer = System.Environment.MachineName + @"\Instance, 80";
			var powerBiBrowser = @"http://" + System.Environment.MachineName + @"/PBIReportServer";
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, analysisServer))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, powerBiBrowser))
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				try
				{
					registration.KeyForTest.IsInternalSystemForTest = false;
					var deployer = new PowerBiDeployerTestConcrete(TestLogger);
					string errorMessage;
					deployer.IsPowerBiConfigured(out errorMessage);
					Fail("Verify IsPowerBiConfigured() exception message should have Power BI Report Server URL.");
				}
				catch (PowerBiException ex)
				{
					bool containsURL = ex.Message.ToLower().Contains(powerBiBrowser.ToLower());
					Assert("IsPowerBiConfigured() exception message contains PBI Report Server URL", containsURL);
				}
				finally
				{
					registration.KeyForTest.IsInternalSystemForTest = true;
				}
			}
		}

		public void TestErrorGettingLoggedInTestEnvironment()
		{
			try
			{
				var logger = new LoggerForTest();
				var deployer = new PowerBiDeployerTestConcrete(logger);
				deployer.IsPowerBiConfigured(out string errorMessage);
				bool logContainsErrorMessage = false;
				foreach (var item in logger.LogEntries.ToList())
				{
					logContainsErrorMessage = item.Contains("Could not connect to Power BI Report URL");
					if (logContainsErrorMessage)
					{
						break;
					}
				}
				Assert("Logger should contain error message in IsPowerBiConfigured()", logContainsErrorMessage);
			}
			catch (PowerBiException)
			{
				Fail("IsPowerBiConfigured() should not throw exception in Test/UAT environment");
			}
		}
		public void TestErrorGettingLoggedInProductionEnvironment()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			try
			{
				registration.KeyForTest.IsInternalSystemForTest = false;
				var deployer = new PowerBiDeployerTestConcrete(TestLogger);
				deployer.IsPowerBiConfigured(out string errorMessage);
				Fail("IsPowerBiConfigured() should throw exception in production environment");
			}
			catch (PowerBiException ex)
			{
				bool exceptionContainsErrorMessage = ex.Message.Contains("Could not connect to Power BI Report URL");
				Assert("IsPowerBiConfigured() exception message contains error message.", exceptionContainsErrorMessage);
			}
			finally
			{
				registration.KeyForTest.IsInternalSystemForTest = true;
			}
		}

		public void VerifyBuildURL(string url, string resourcePath, string expectedUrl)
		{
			var deployer = new PowerBiDeployerTestConcrete(TestLogger);
			string actualUrl = deployer.BuildURL(url, resourcePath);
			AssertEquals("Verify build URL logic ", expectedUrl, actualUrl);
		}
		public void TestBuildURLWithDifferentCombinations()
		{
			var powerBiBrowser = @"http://" + System.Environment.MachineName + @":80/PBIReportServer";
			var resourcePath = @"Folder";
			var expectedUrl = powerBiBrowser + AltDirectorySeparatorChar + resourcePath;
			VerifyBuildURL(powerBiBrowser, resourcePath, expectedUrl);
			VerifyBuildURL(powerBiBrowser + AltDirectorySeparatorChar, resourcePath, expectedUrl);
			VerifyBuildURL(powerBiBrowser, AltDirectorySeparatorChar + resourcePath, expectedUrl);
			VerifyBuildURL(powerBiBrowser + AltDirectorySeparatorChar, AltDirectorySeparatorChar + resourcePath, expectedUrl);
			VerifyBuildURL(powerBiBrowser + AltDirectorySeparatorChar + AltDirectorySeparatorChar, AltDirectorySeparatorChar + resourcePath, expectedUrl);
			VerifyBuildURL(powerBiBrowser + AltDirectorySeparatorChar + AltDirectorySeparatorChar, "" + AltDirectorySeparatorChar + AltDirectorySeparatorChar + resourcePath, expectedUrl);
		}
		[UseSnapshotProtection]
		public void TestPowerBiApiUrl()
		{
			var analysisServer = System.Environment.MachineName + @"\Instance, 80";
			var powerBiBrowser = @"http://" + System.Environment.MachineName + @"/PBIReportServer";
			string expectedUrl = powerBiBrowser + @"/api/v2.0/";
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, analysisServer))
			using (SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, powerBiBrowser))
			{
				var deployer = new PowerBiDeployerTestConcrete(TestLogger);
				deployer.SetPowerBiApiUrl("");
				AssertEquals("Verify building PowerBiApiUrl", expectedUrl, deployer.PowerBiApiUrl);
			}
		}
	}
}
