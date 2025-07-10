using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.IO;
using Enterprise.LogShipping.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	sealed class DbManagerTest : LogShippingTestFixture
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		public void TestRunDbManagerTestsWhichRequireConfiguredLS()
		{
			TempDirectory tempDir = null;

			try
			{
				using (var dbTestHelper = new DbTestHelper())
				{
					tempDir = new TempDirectory();
					SetupTestLSInfo(dbTestHelper);
					testSetupInfo.BackupLocalCopyDirectory = tempDir.DirectoryName;
					testSetupInfo.MainDatabase.ShouldInitialise = true;
					try
					{
						RestoreManager manager = new RestoreManager();
						manager.OnShowMessage += new NotificationDelegate(Manager_OnShowMessage);
						bool restoreResult = manager.Restore(testSetupInfo);
						AssertEquals("Restore Database failed. \r\nOutput text:\r\n" + restoreMangagerOutput.ToString(), true, restoreResult);

						LogShippingConfigurator configurator = new LogShippingConfigurator();
						configurator.OnShowMessage += new NotificationDelegate(Configurator_OnShowMessage);

						try
						{
							AssertEquals("LS configuration failed. \r\nOutput text:\r\n" + configuratorOutput.ToString(), true, configurator.Configure(testSetupInfo));
							AssertGetSecondaryDatabasesInfo();
							AssertGetEnterpriseDatabases(dbTestHelper);
							AssertIsDatabaseInStandByMode();
						}
						finally
						{
							testSetupInfo.SetupAction = SetupAction.Remove;
							testSetupInfo.ShouldDropDatabaseAfterLSRemoving = true;
							configurator.Configure(testSetupInfo);
						}
					}
					finally
					{
						dbTestHelper.DropDatabase(testDatabaseInfo.SecondaryDatabaseName);
					}
				}
			}
			finally
			{
				if (tempDir != null)
				{
					string tempDirPath = tempDir.DirectoryName;
					try
					{ tempDir.Dispose(); }
					catch { DirectoryHelper.RetryDeletingTempDirectory(tempDirPath); }
				}
			}
		}

		[SnailTest]
		public void TestGetPrimaryLinkedEDocsDatabasesDoesNotHideException()
		{
			var dbManager = new DbManager();
			var lsInfo = new LogShippingInfo();
			lsInfo.PrimaryServer = new SqlServerInfo(Db.ServerName, "~ThisGotToBeAnInvalidInstanceName~", "1.0.0.0");
			lsInfo.MainDatabase = new MainDatabaseInfo(new LogShippingInfo(), "WhatEverDbName");
			string error = null;

			var eDocsList = dbManager.GetPrimaryLinkedEDocsDatabases(lsInfo, new Action<string>(s => error = s));
			AssertNull("eDocsList", eDocsList);

			var expectedErrorStart = "A network-related or instance-specific error occurred while establishing a connection to SQL Server";
			Assert("Unexpected error message: " + error, error.StartsWith(expectedErrorStart, StringComparison.OrdinalIgnoreCase));
			AssertEquals("LastErrorMessage", error, dbManager.LastErrorMessage);
		}

		void AssertGetSecondaryDatabasesInfo()
		{
			var configuredLSInfo = new DbManager().GetSecondaryDatabasesInfo(testSetupInfo.SecondaryServer);
			Assert("Should be at least test LS configured", configuredLSInfo.Length > 0);

			var info = Array.Find(configuredLSInfo, info1 => info1.MainDatabase.DatabaseName == testSetupInfo.MainDatabase.DatabaseName);
			AssertEquals("PrimaryServerFromLSMetadata", testSetupInfo.PrimaryServer.FullInstanceName, info.PrimaryServerFromLSMetadata);
			AssertEquals("PrimaryServer", testSetupInfo.PrimaryServer.FullInstanceName, info.PrimaryServer.FullInstanceName);
			AssertEquals("PrimaryDatabase", testDatabaseInfo.DatabaseName, info.MainDatabase.DatabaseName);
			AssertEquals("SecondaryServer", testSetupInfo.SecondaryServer.FullInstanceName, info.SecondaryServer.FullInstanceName);
			AssertEquals("BackupSourceDirectory", testSetupInfo.BackupSourceDirectory, info.BackupSourceDirectory);
			AssertEquals("BackupLocalCopyDirectory", testSetupInfo.BackupLocalCopyDirectory, info.BackupLocalCopyDirectory);
			AssertEquals("OriginalBackupSourceDirectory", testSetupInfo.BackupSourceDirectory, info.OriginalBackupSourceDirectory);
			AssertEquals("OriginalBackupLocalCopyDirectory", testSetupInfo.BackupLocalCopyDirectory, info.OriginalBackupLocalCopyDirectory);

			AssertEquals("ConfigurationChanged", false, info.ConfigurationChanged);

			AssertEquals("CopyJobUid", testSetupInfo.MainDatabase.CopyJobId, info.MainDatabase.CopyJobId);
			AssertEquals("RestoreJobUid", testSetupInfo.MainDatabase.RestoreJobId, info.MainDatabase.RestoreJobId);
		}

		void AssertIsDatabaseInStandByMode()
		{
			var dbManager = new DbManager();
			Assert("Secondary database should be in standby mode",
				dbManager.IsDatabaseInStandByMode(testSetupInfo.SecondaryServer.FullInstanceName, testSetupInfo.MainDatabase.SecondaryDatabaseName));
			Assert("Primary database shouldn't be in standby mode",
				!dbManager.IsDatabaseInStandByMode(testSetupInfo.PrimaryServer.FullInstanceName, testDatabaseInfo.DatabaseName));
		}

		void AssertGetEnterpriseDatabases(DbTestHelper dbTestHelper)
		{
			var tempDatabase = "TestTempDatabase" + Guid.NewGuid();
			try
			{
				dbTestHelper.CreateDatabase(tempDatabase);
				var dbManager = new DbManager();
				var databases = new List<string>(dbManager.GetEnterpriseDatabases(Db.ServerName));
				Assert("Should be at least current enterprise database", databases.Count > 0);
				Assert("Shouldn't contains system database",
					!databases.Contains("master")
					&& !databases.Contains("tempdb")
					&& !databases.Contains("model")
					&& !databases.Contains("msdb"));
				Assert("Temp database is not enterprise db", !databases.Contains(tempDatabase));
				Assert("Current database is enterprise db", databases.Contains(Db.DatabaseName));
				Assert("Secondary database shouldn't be in primary db list", !databases.Contains(testSetupInfo.MainDatabase.SecondaryDatabaseName));
			}
			finally
			{
				dbTestHelper.DropDatabase(tempDatabase);
			}
		}

		void SetupTestLSInfo(DbTestHelper dbTestHelper)
		{
			testSetupInfo = new LogShippingInfo();
			testSetupInfo.PrimaryServer = new SqlServerInfoForTesting(Db.Connection.ServerNameWithoutInstance, Db.Connection.ServerInstanceName, Db.Connection.ServerVersionNumber.ToString());
			testDatabaseInfo = new MainDatabaseInfoForTesting(testSetupInfo, "TestDatabase" + Guid.NewGuid());
			testSetupInfo.MainDatabase = testDatabaseInfo;
			testSetupInfo.SecondaryServer = testSetupInfo.PrimaryServer;
			((MainDatabaseInfoForTesting)testDatabaseInfo).SecondaryDatabaseNameValue = testDatabaseInfo.DatabaseName + "__Secondary";
			testSetupInfo.BackupSourceDirectory = Path.Combine(BaseSourcePath, DbTestHelper.TestBackupFilePath);
			testSetupInfo.SetupAction = SetupAction.Setup;
			testDatabaseInfo.BackupFileName = dbTestHelper.GetTestBackupFile(new SqlServerVersionNumber($"{testSetupInfo.PrimaryServer.Version.Major}.{testSetupInfo.PrimaryServer.Version.Minor}"));
		}

		void Manager_OnShowMessage(string message)
		{
			restoreMangagerOutput.AppendLine(message);
		}

		void Configurator_OnShowMessage(string message)
		{
			configuratorOutput.AppendLine(message);
		}

		readonly StringBuilder restoreMangagerOutput = new StringBuilder();
		readonly StringBuilder configuratorOutput = new StringBuilder();

		LogShippingInfo testSetupInfo;
		MainDatabaseInfo testDatabaseInfo;
	}

	class DbManagerServerInitTest : LogShippingTestFixture
	{
		readonly Mock<ISqlServerSecurityInitializationService> initServiceMock = new Mock<ISqlServerSecurityInitializationService>(MockBehavior.Strict);
		readonly Mock<ISqlConnectionProvider> connectionProviderMock = new Mock<ISqlConnectionProvider>(MockBehavior.Strict);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Explicit generic parameters when setting up a service collection")]
		public override void ConfigureTestServices(IServiceCollection services)
		{
			base.ConfigureTestServices(services);

			services.RemoveAll<ISqlServerSecurityInitializationService>();
			services.RemoveAll<ISqlConnectionProvider>();
			services.AddSingleton<ISqlServerSecurityInitializationService>(sp => initServiceMock.Object);
			services.AddSingleton<ISqlConnectionProvider>(sp => connectionProviderMock.Object);
		}

		public void TestCheckConnection_InitializeServer()
		{
			// Arrange
			var serverName = Db.ServerName;
			var dbManager = new DbManager();

			connectionProviderMock.SetupSequence(x => x.OpenNewSqlConnection<OdysseyAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Throws(() => new Exception("Login failed for user 'Some User'."))
				.Throws(() => new Exception("After Init"));

			initServiceMock.Setup(x => x.IsServerInitialized(serverName, "master")).Returns(false).Verifiable(Times.Once);
			initServiceMock.Setup(x => x.InitializeServer(serverName, "master")).Verifiable(Times.Once());

			// ACT
			var result = dbManager.CheckConnection(serverName);

			// Assert
			connectionProviderMock.VerifyAll();
			initServiceMock.VerifyAll();
			AssertEquals(false, result);
			AssertEquals("After Init",dbManager.LastErrorMessage);
		}
	}
}
