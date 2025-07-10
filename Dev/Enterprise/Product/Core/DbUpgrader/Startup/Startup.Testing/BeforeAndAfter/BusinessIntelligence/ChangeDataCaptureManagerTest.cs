using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class ChangeDataCaptureManagerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestDropDbWhenStatusRestore()
		{
			var mainDbName = "TestDb";
			var auditDbName = mainDbName + Db.AuditDatabaseSuffix;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				try
				{
					InitialiseCdc(mainDbConnection);

					var testLogger = new DummyLoggerForTest();
					var cdcManager = new ChangeDataCaptureManagerForTesting(mainDbConnection, mainDbConnection, mainDbConnection, testLogger);
					cdcManager.MainDbName_Exposed = mainDbName;
					cdcManager.AuditDbName_Exposed = auditDbName;

					mainDbConnection.CreateDatabase(auditDbName);

					Assert("Audit DB should exist", mainDbConnection.DatabaseExists(auditDbName));

					cdcManager.Test_AlwaysOnReplicaNamesList = new List<string>() { "replicaName3" };
					AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
					cdcManager.Test_RemoveDatabaseFromAlwaysOnSetup = true;

					cdcManager.DisableCdcAndCleanupBiDatabases();

					Assert("Audit DB should NOT exist", !mainDbConnection.DatabaseExists(auditDbName));
				}
				finally
				{
					DropDatabase(mainDbConnection, auditDbName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestShouldDropWhenDbIsNotAlwaysOn()
		{
			var mainDbName = "TestDb";
			var auditDbName = mainDbName + Db.AuditDatabaseSuffix;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				try
				{
					InitialiseCdc(mainDbConnection);

					var testLogger = new DummyLoggerForTest();
					var cdcManager = new ChangeDataCaptureManagerForTesting(mainDbConnection, mainDbConnection, mainDbConnection, testLogger);
					cdcManager.MainDbName_Exposed = mainDbName;
					cdcManager.AuditDbName_Exposed = auditDbName;

					mainDbConnection.CreateDatabase(auditDbName);

					Assert("Audit DB should exist", mainDbConnection.DatabaseExists(auditDbName));

					AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;
					cdcManager.DisableCdcAndCleanupBiDatabases();

					Assert("Audit DB should NOT exist", !mainDbConnection.DatabaseExists(auditDbName));
				}
				finally
				{
					DropDatabase(mainDbConnection, auditDbName);
				}
			}
		}

		public void TestDropBiDatabaseSQLText()
		{
			AssertEquals("When Dropping BI Database, Set SINGLE_USER and DROP in one transaction", @"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}];", ChangeDataCaptureManager.DropBiDatabaseCmdText);
		}

		[UseSnapshotProtection]
		public void TestDisableCdcIfRequired()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				InitialiseCdc(mainDbConnection);

				AssertEquals("Is Cdc enabled?", true, CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName));
				AssertEquals("BiAuditServer", "TestAuditServer", GetStringRegistryItemValue(mainDbConnection, "BiAuditServer"));
				AssertEquals("BiDataWarehouseServer", "TestDataWarehouseServer", GetStringRegistryItemValue(mainDbConnection, "BiDataWarehouseServer"));
				AssertEquals("BiAnalysisServer", "TestAnalysisServer", GetStringRegistryItemValue(mainDbConnection, "BiAnalysisServer"));
				AssertEquals("BiPowerBiWebPortalUrl", "TestPowerBiUrl", GetStringRegistryItemValue(mainDbConnection, "BiPowerBiWebPortalUrl"));
				AssertEquals("BiAuditAPI", true, GetBooleanRegistryItemValue(mainDbConnection, "BiAuditAPI"));
				AssertEquals("BiResetChangeDataCapture", true, GetBooleanRegistryItemValue(mainDbConnection, "BiResetChangeDataCapture"));

				var testLogger = new DummyLoggerForTest();
				var cdcManager = new ChangeDataCaptureManager(mainDbConnection, null, null, testLogger);
				cdcManager.DisableCdcAndCleanupBiDatabases();

				AssertEquals("Is Cdc enabled?", false, CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName));
				AssertEquals("BiAuditServer", "TestAuditServer", GetStringRegistryItemValue(mainDbConnection, "BiAuditServer"));
				AssertEquals("BiDataWarehouseServer", "TestDataWarehouseServer", GetStringRegistryItemValue(mainDbConnection, "BiDataWarehouseServer"));
				AssertEquals("BiAnalysisServer", "TestAnalysisServer", GetStringRegistryItemValue(mainDbConnection, "BiAnalysisServer"));
				AssertEquals("BiPowerBiWebPortalUrl", "TestPowerBiUrl", GetStringRegistryItemValue(mainDbConnection, "BiPowerBiWebPortalUrl"));
				AssertEquals("BiAuditAPI", true, GetBooleanRegistryItemValue(mainDbConnection, "BiAuditAPI"));
				AssertEquals("BiResetChangeDataCapture", false, GetBooleanRegistryItemValue(mainDbConnection, "BiResetChangeDataCapture"));
			}
		}

		[UseSnapshotProtection]
		public void TestDisableCdcIfRequired_BiDatabasesDontExist()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				InitialiseCdc(mainDbConnection);
				SetRegistryItemValue(mainDbConnection, "BiAuditServer", Db.ServerName);
				SetRegistryItemValue(mainDbConnection, "BiDataWarehouseServer", Db.ServerName);

				AssertEquals("Is Cdc enabled?", true, CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName));
				AssertEquals("BiAuditServer", Db.ServerName, GetStringRegistryItemValue(mainDbConnection, "BiAuditServer"));
				AssertEquals("BiDataWarehouseServer", Db.ServerName, GetStringRegistryItemValue(mainDbConnection, "BiDataWarehouseServer"));
				AssertEquals("BiResetChangeDataCapture", true, GetBooleanRegistryItemValue(mainDbConnection, "BiResetChangeDataCapture"));

				var testLogger = new DummyLoggerForTest();
				var cdcManager = new ChangeDataCaptureManagerForTesting(mainDbConnection, mainDbConnection, mainDbConnection, testLogger);
				cdcManager.MainDbName_Exposed = Db.DatabaseName;
				cdcManager.AuditDbName_Exposed = "TestDb_Audit";
				cdcManager.EdwDbName_Exposed = "TestDb_EDW";

				AssertNoExceptionThrown("Exception should not be thrown if BI database does not exist.", () =>
				{
					cdcManager.DisableCdcAndCleanupBiDatabases();
				});

				AssertEquals("Is Cdc enabled?", false, CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName));
				AssertEquals("BiAuditServer", Db.ServerName, GetStringRegistryItemValue(mainDbConnection, "BiAuditServer"));
				AssertEquals("BiDataWarehouseServer", Db.ServerName, GetStringRegistryItemValue(mainDbConnection, "BiDataWarehouseServer"));
				AssertEquals("BiResetChangeDataCapture", false, GetBooleanRegistryItemValue(mainDbConnection, "BiResetChangeDataCapture"));
			}
		}

		[UseSnapshotProtection]
		public void TestDisableCdcIfRequired_BiDatabasesInUse()
		{
			var mainDbName = "TestDb";
			var auditDbName = mainDbName + Db.AuditDatabaseSuffix;
			var edwDbName = mainDbName + Db.EdwDatabaseSuffix;
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var testLogger = new DummyLoggerForTest();
				var cdcManager = new ChangeDataCaptureManagerForTesting(mainDbConnection, mainDbConnection, mainDbConnection, testLogger);
				cdcManager.MainDbName_Exposed = mainDbName;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;

				try
				{
					mainDbConnection.CreateDatabase(auditDbName);
					mainDbConnection.CreateDatabase(edwDbName);

					using (var auditConnection = Db.NewAdminConnection(auditDbName))
					using (var edwConnection = Db.NewAdminConnection(edwDbName))
					{
						auditConnection.EnsureIsOpen();
						edwConnection.EnsureIsOpen();

						InitialiseCdc(mainDbConnection);

						AssertEquals("Audit db exists?", true, mainDbConnection.DatabaseExists(auditDbName));
						AssertEquals("EDW db exists?", true, mainDbConnection.DatabaseExists(edwDbName));

						cdcManager.DisableCdcAndCleanupBiDatabases();

						AssertEquals("Audit db exists?", false, mainDbConnection.DatabaseExists(auditDbName));
						AssertEquals("EDW db exists?", false, mainDbConnection.DatabaseExists(edwDbName));
					}
				}
				finally
				{
					DropDatabase(mainDbConnection, auditDbName);
					DropDatabase(mainDbConnection, edwDbName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDropDbIsAttemptedOnAuditAlwaysOnReplicas()
		{
			var mainDbName = "TestDb";
			var auditDbName = mainDbName + Db.AuditDatabaseSuffix;
			var edwDbName = mainDbName + Db.EdwDatabaseSuffix;

			var mockDb_Main = "MockDbTestDropAuditDatabase";
			var replicaName = Db.ServerName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var testLogger = new DummyLoggerForTest();
				var cdcManager = new ChangeDataCaptureManagerForTesting(mainDbConnection, mainDbConnection, mainDbConnection, testLogger);
				cdcManager.MainDbName_Exposed = mainDbName;
				cdcManager.Test_AlwaysOnReplicaNamesList = new List<string>() { "replicaName1", "replicaName2", "replicaName3", "replicaName4", "replicaName5", "replicaName6" };
				cdcManager.Test_RemoveDatabaseFromAlwaysOnSetup = true;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;

				try
				{
					mainDbConnection.CreateDatabase(auditDbName);
					mainDbConnection.CreateDatabase(edwDbName);

					using (var auditConnection = Db.NewAdminConnection(auditDbName))
					using (var edwConnection = Db.NewAdminConnection(edwDbName))
					{
						auditConnection.EnsureIsOpen();
						edwConnection.EnsureIsOpen();

						InitialiseCdc(mainDbConnection);

						try
						{
							AdoTestUtils.CreateDbIfNotExists(mainDbConnection, mockDb_Main);
							using (var connection = Db.NewAdminConnection(Db.ServerName, mockDb_Main))
							{
								AssertNoExceptionThrown(() => cdcManager.DisableCdcAndCleanupBiDatabases());

								CombineAssertions("Drop Db is attempted on audit Always-On replicas", () =>
								{
									AssertCollectionContains($"Database [{auditDbName}] cannot be dropped on server [replicaName1] (1/6). The database state is 'ONLINE'", testLogger.Logs);
									AssertCollectionContains($"Database [{auditDbName}] cannot be dropped on server [replicaName2] (2/6). The database state is 'RECOVERING'", testLogger.Logs);
									AssertCollectionContains($"Database [{auditDbName}] has been dropped on server [replicaName3] (3/6).", testLogger.Logs);
									AssertCollectionContains($"Database [{auditDbName}] cannot be dropped on server [replicaName4] (4/6). The server is unavailable", testLogger.Logs);
									AssertCollectionContains($"Database [{auditDbName}] cannot be dropped on server [replicaName5] (5/6). The database state is 'WhateverState'", testLogger.Logs);
									AssertCollectionContains($"Database [{auditDbName}] has been dropped on server [replicaName6] (6/6).", testLogger.Logs);
								});
							}
						}
						finally
						{
							AdoTestUtils.DropDbIfExists(mainDbConnection, mockDb_Main);
						}
					}
				}
				finally
				{
					DropDatabase(mainDbConnection, auditDbName);
					DropDatabase(mainDbConnection, edwDbName);
				}
			}
		}

		void DropDatabase(DbConnection connection, string dbName)
		{
			var rawDropCmdText = "IF EXISTS(SELECT null FROM sys.databases WHERE name='{0}') DROP DATABASE [{0}]";
			connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, rawDropCmdText, dbName));
		}

		void InitialiseCdc(AdminConnection mainDbConnection)
		{
			if (!CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName))
			{
				CdcDatabase.Enable(mainDbConnection, Db.DatabaseName);
			}

			SetRegistryItemValue(mainDbConnection, "BiAuditServer", "TestAuditServer");
			SetRegistryItemValue(mainDbConnection, "BiDataWarehouseServer", "TestDataWarehouseServer");
			SetRegistryItemValue(mainDbConnection, "BiAnalysisServer", "TestAnalysisServer");
			SetRegistryItemValue(mainDbConnection, "BiPowerBiWebPortalUrl", "TestPowerBiUrl");
			SetRegistryItemValue(mainDbConnection, "BiAuditAPI", true);
			SetRegistryItemValue(mainDbConnection, "BiResetChangeDataCapture", true);
		}

		void SetRegistryItemValue(DbConnection mainDbConnection, string registryName, bool registryBoolValue)
		{
			SetRegistryItemValue(mainDbConnection, registryName, registryBoolValue ? bool.TrueString : bool.FalseString);
		}

		void SetRegistryItemValue(DbConnection mainDbConnection, string registryName, string registryStringValue)
		{
			var sqlText = @"
IF EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = @RegistryName)
	UPDATE dbo.StmData SET SD_BinaryValue = CONVERT(VARBINARY(MAX), @RegistryStringValue) WHERE SD_Name = @RegistryName
ELSE
	INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_BinaryValue) VALUES (NEWID(), @RegistryName, CONVERT(VARBINARY(MAX), @RegistryStringValue))";

			using (var cmd = mainDbConnection.Command(sqlText))
			{
				cmd.AddParameter("@RegistryName", SqlDbType.NVarChar, registryName);
				cmd.AddParameter("@RegistryStringValue", SqlDbType.NVarChar, registryStringValue);
				cmd.ExecuteNonQuery();
			}
		}

		string GetStringRegistryItemValue(DbConnection mainDbConnection, string registryName)
		{
			return GetRegistryItemValue(mainDbConnection, registryName)?.ToString();
		}

		bool GetBooleanRegistryItemValue(DbConnection mainDbConnection, string registryName)
		{
			var registryValue = GetRegistryItemValue(mainDbConnection, registryName);
			if (registryValue != null)
			{
				return Convert.ToBoolean(registryValue, CultureInfo.InvariantCulture);
			}
			else
			{
				return false;
			}
		}

		object GetRegistryItemValue(DbConnection mainDbConnection, string registryName)
		{
			var sqlText = @"SELECT @RegistryValue = CONVERT(NVARCHAR(MAX), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = @RegistryName";
			using (var cmd = mainDbConnection.Command(sqlText))
			{
				cmd.AddParameter("@RegistryName", SqlDbType.NVarChar, registryName);
				cmd.AddOutputParameter("@RegistryValue", SqlDbType.NVarChar, -1, 0, 0, null);

				cmd.ExecuteNonQuery();
				var registryValue = cmd.GetParameterValue("@RegistryValue");
				if (registryValue != DBNull.Value)
				{
					return registryValue;
				}
				else
				{
					return null;
				}
			}
		}

		sealed class ChangeDataCaptureManagerForTesting : ChangeDataCaptureManager
		{
			public ChangeDataCaptureManagerForTesting(AdminConnection mainDbConnection, AdminConnection auditConnection, AdminConnection dataWarehouseConnection, IUpgradeTaskWorkflowLogger logger)
				: base(mainDbConnection, auditConnection, dataWarehouseConnection, logger)
			{
				TryCount = 1;
			}

			public string MainDbName_Exposed { get; set; }
			protected override string MainDbName => MainDbName_Exposed ?? base.MainDbName;

			public string AuditDbName_Exposed { get; set; }
			protected override string AuditDbName => AuditDbName_Exposed ?? base.AuditDbName;

			public string EdwDbName_Exposed { get; set; }
			protected override string EdwDbName => EdwDbName_Exposed ?? base.EdwDbName;

			public List<string> Test_AlwaysOnReplicaNamesList;
			public bool? Test_RemoveDatabaseFromAlwaysOnSetup;
			string replicaBeingProcessed = string.Empty;

			protected override List<string> GetAlwaysOnSecondaryReplicaNamesList(string dbName)
			{
				if (Test_AlwaysOnReplicaNamesList != null)
				{
					return Test_AlwaysOnReplicaNamesList;
				}
				else
				{
					return base.GetAlwaysOnSecondaryReplicaNamesList(dbName);
				}
			}

			protected override bool RemoveDatabaseFromAlwaysOnSetup(string dbName)
			{
				if (Test_RemoveDatabaseFromAlwaysOnSetup.HasValue)
				{
					return Test_RemoveDatabaseFromAlwaysOnSetup.Value;
				}
				else
				{
					return base.RemoveDatabaseFromAlwaysOnSetup(dbName);
				}
			}

			protected override void DropDb(AdminConnection replicaConnection, string dbName)
			{
				if (replicaBeingProcessed.StartsWith("replicaName", StringComparison.OrdinalIgnoreCase))
				{
					replicaBeingProcessed = string.Empty;
				}
				else
				{
					base.DropDb(replicaConnection, dbName);
				}
			}

			protected override AdminConnection GetAdminConnectionToReplica(string replicaName)
			{
				replicaBeingProcessed = replicaName;
				if (replicaName.Equals("replicaName4", StringComparison.OrdinalIgnoreCase))
				{
					var error = SqlExceptionBuilder.CreateSqlError(11001, byte.MaxValue, byte.MinValue, replicaName, "Unreachable server", "", 0);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					throw SqlExceptionBuilder.CreateSqlException(errors);
				}
				else
				{
					return Db.NewAdminConnection();
				}
			}

			protected override string GetDatabaseStateDescription(AdminConnection replicaConnection, string dbName)
			{
				switch (replicaBeingProcessed)
				{
					case "replicaName1":
						return "ONLINE";
					case "replicaName2":
						return "RECOVERING";
					case "replicaName3":
						return "RESTORING";
					case "replicaName4":
						return "ONLINE";
					case "replicaName5":
						return "WhateverState";
					case "replicaName6":
						return "RESTORING";
					default:
						return base.GetDatabaseStateDescription(replicaConnection, dbName);
				}
			}
		}
	}
}
