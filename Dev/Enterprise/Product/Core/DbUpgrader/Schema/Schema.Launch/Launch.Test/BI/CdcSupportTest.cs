using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing.BI;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class CdcSupportTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSynchroniseCdcSchema()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, Db.DatabaseName);
					BiAutomationConfigLoader.Instance.ResetConfiguration();

					SetupTestCdcConfig();

					// GlbBranch => To be enabled (selected cdc table)
					CdcTableTest.AssertCdcIsEnabled(testConnection, GlbBranchSchema.Constants.TableName, false);
					CdcTableTest.DisableCdcIfEnabled(testConnection, GlbBranchSchema.Constants.TableName);

					// GlbDepartment => Modified a column (to be disabled + re-enabled)
					var depRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.Constants.GE_GE);
					depRemover.DropRelateObjects(testConnection);
					ReEnableCdc(testConnection, GlbDepartmentSchema.Constants.TableName);
					testConnection.ExecuteNonQuery("ALTER TABLE [GlbDepartment] DROP COLUMN [GE_GE]; ALTER TABLE [GlbDepartment] ADD [GE_GE] int null;");

					// GlbGroup => No changes (to stay enabled)
					ReEnableCdc(testConnection, GlbGroupSchema.Constants.TableName);

					// JobContainer => Non-eligible columns must be removed from CDC (to be disabled + re-enabled)
					EnableCdcWithAllColumns(testConnection, JobContainerSchema.Constants.TableName);

					// RefCountry => Added a non-eligible column, which will be ignored (to stay enabled)
					ReEnableCdc(testConnection, RefCountrySchema.Constants.TableName);
					testConnection.ExecuteNonQuery("ALTER TABLE [RefCountry] ADD Col1 xml");

					// StmEvent => Not in our CDC configuration (to be disabled)
					CdcTableTest.EnableCdcTable(testConnection, StmReportRunSchema.Constants.TableName);

					// GlbCompany => No changes (to stay enabled)
					ReEnableCdc(testConnection, GlbCompanySchema.Constants.TableName);

					// GlbCompany (another capture instance) => Not in our CDC configuration (to be disabled)
					CdcTableTest.EnableCdcTable(testConnection, GlbCompanySchema.Constants.TableName, "TEST-CDC-GlbCompany");

					// Pre-condition checks
					testConnection.ExecuteNonQuery(@"
					INSERT dbo.GlbBranch (GB_PK, GB_GC) SELECT TOP (1) newid(), GC_PK FROM dbo.GlbCompany;
					INSERT dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc) VALUES (newid(), '!@#', '!@#');
					INSERT dbo.GlbGroup (GG_PK, GG_Code) VALUES (newid(), '!@#');
					INSERT dbo.JobContainer (JC_PK) VALUES (newid());
					INSERT dbo.RefCountry (RN_PK, RN_Code, Col1) VALUES (newid(), 'XX', '<xml/>');
					INSERT dbo.StmReportRun (RRI_PK, RRI_ReportName, RRI_StartTimeUtc, RRI_SystemCreateTimeUtc, RRI_SystemCreateUser) VALUES (newid(), '!@#', GETUTCDATE(), GETUTCDATE(), '~BP');
					INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name) VALUES (newid(), 'DAN', '!@#');");

					CdcDatabase.DisableAndStopJobsAndTrigger(testConnection, Db.DatabaseName);
					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					AssertCdcTableRowCount(testConnection, GlbDepartmentSchema.Constants.SqlSchemaName, GlbDepartmentSchema.Constants.TableName, 1);
					AssertCdcTableRowCount(testConnection, GlbGroupSchema.Constants.SqlSchemaName, GlbGroupSchema.Constants.TableName, 1);
					AssertCdcTableRowCount(testConnection, JobContainerSchema.Constants.SqlSchemaName, JobContainerSchema.Constants.TableName, 1);
					AssertCdcTableRowCount(testConnection, RefCountrySchema.Constants.SqlSchemaName, RefCountrySchema.Constants.TableName, 1);
					AssertCdcTableRowCount(testConnection, StmReportRunSchema.Constants.SqlSchemaName, StmReportRunSchema.Constants.TableName, 1);
					AssertCdcTableRowCount(testConnection, GlbCompanySchema.Constants.SqlSchemaName, GlbCompanySchema.Constants.TableName, 1);
					AssertCdcCaptureInstanceTableRowCount(testConnection, "TEST-CDC-GlbCompany", 1);

					//
					// SYNC CDC
					var cdc = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
					cdc.SynchroniseCdcSchema();

					// Assert Results
					CombineAssertions(() =>
					{
						CdcTableTest.AssertCdcIsEnabled(testConnection, GlbBranchSchema.Constants.TableName, true);
						CdcTableTest.AssertCdcIsEnabled(testConnection, GlbDepartmentSchema.Constants.TableName, true);
						CdcTableTest.AssertCdcIsEnabled(testConnection, GlbGroupSchema.Constants.TableName, true);
						CdcTableTest.AssertCdcIsEnabled(testConnection, JobContainerSchema.Constants.TableName, true);
						CdcTableTest.AssertCdcIsEnabled(testConnection, RefCountrySchema.Constants.TableName, true);
						CdcTableTest.AssertCdcIsEnabled(testConnection, StmReportRunSchema.Constants.TableName, true);

						AssertCdcInstanceIsEnabled(testConnection, GlbCompanySchema.Constants.SqlSchemaName, GlbCompanySchema.Constants.TableName, GlbCompanySchema.Constants.SqlSchemaName + "_" + GlbCompanySchema.Constants.TableName, true);
						AssertCdcInstanceIsEnabled(testConnection, GlbCompanySchema.Constants.SqlSchemaName, GlbCompanySchema.Constants.TableName, "TEST-CDC-GlbCompany", false);
					});

					// Assert row count (re-enabled tables should have no rows)
					CombineAssertions(() =>
					{
						AssertCdcTableRowCount(testConnection, GlbBranchSchema.Constants.SqlSchemaName, GlbBranchSchema.Constants.TableName, 0);
						AssertCdcTableRowCount(testConnection, GlbDepartmentSchema.Constants.SqlSchemaName, GlbDepartmentSchema.Constants.TableName, 0);
						AssertCdcTableRowCount(testConnection, GlbGroupSchema.Constants.SqlSchemaName, GlbGroupSchema.Constants.TableName, 1);
						AssertCdcTableRowCount(testConnection, JobContainerSchema.Constants.SqlSchemaName, JobContainerSchema.Constants.TableName, 0);
						AssertCdcTableRowCount(testConnection, RefCountrySchema.Constants.SqlSchemaName, RefCountrySchema.Constants.TableName, 1);
						AssertCdcTableRowCount(testConnection, GlbCompanySchema.Constants.SqlSchemaName, GlbCompanySchema.Constants.TableName, 1);
					});
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestSynchroniseCdcSchemaIfColumnIdsDontMatch()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, Db.DatabaseName);
					BiAutomationConfigLoader.Instance.ResetConfiguration();

					SetupTestCdcConfig();

					ReEnableCdc(testConnection, GlbCompanySchema.Constants.TableName);
					CdcTableTest.AssertCdcIsEnabled(testConnection, GlbCompanySchema.Constants.TableName, true);

					testConnection.ExecuteNonQuery(@"ALTER TABLE GlbCompany ADD GC_Code2 char(3)");

					var oldColumnId = GetColumnId(testConnection, GlbCompanySchema.Constants.TableName, "GC_Code");
					var newColumnId = GetColumnId(testConnection, GlbCompanySchema.Constants.TableName, "GC_Code2");
					AssertNotEquals("column_id's should be different.", oldColumnId, newColumnId);

					var dependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, GlbCompanySchema.Constants.TableName, GlbCompanySchema.Constants.GC_Code);
					dependencyRemover.DropRelateObjects(testConnection);
					testConnection.ExecuteNonQuery("ALTER TABLE GlbCompany DROP COLUMN GC_Code");

					DbObjectCreator.RenameColumn(testConnection, GlbCompanySchema.Constants.TableName, "GC_Code2", "GC_Code");

					var renamedColumnId = GetColumnId(testConnection, GlbCompanySchema.Constants.TableName, "GC_Code");

					AssertEquals("Renamed column should have the same column_id as the added column.", newColumnId, renamedColumnId);

					CdcTableTest.AssertCdcIsEnabled(testConnection, GlbCompanySchema.Constants.TableName, true);

					var cdc = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
					cdc.SynchroniseCdcSchema();

					CdcTableTest.AssertCdcIsEnabled(testConnection, GlbCompanySchema.Constants.TableName, true);

					testConnection.ExecuteNonQuery("INSERT INTO dbo.GlbCompany([GC_PK] ,[GC_Code], [GC_Name]) VALUES (newid(), 'ABC', 'AU company')");
					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					Assert("Change exists for GC_Code?", testConnection.Exists("FROM cdc.dbo_GlbCompany_CT WHERE __$operation=2 AND GC_Code = 'ABC'"));
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		int GetColumnId(DbConnection connection, string tableName, string columnName)
		{
			return Convert.ToInt32(connection.ExecuteScalar($"SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}') AND name = '{columnName}'"));
		}

		[UseSnapshotProtection]
		public void TestAreTablesTrackedByCdc()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();

					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", false, true, "", "", "Test_PK", false);
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");

					testConnection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[TestTable]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
)");
					CdcTableTest.EnableCdcTable(testConnection, "TestTable");

					var cdcSupport = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
					cdcSupport.SynchroniseCdcSchema();
					AssertEquals(true, cdcSupport.AreTablesTrackedByCdc);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAreTablesTrackedByCdc_ShouldBeFalse_WhenCdcDisabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
					{
						CdcDatabase.Disable(testConnection, Db.DatabaseName);
					}

					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();

					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", false, true, "", "", "Test_PK", false);
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");

					testConnection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[TestTable]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
)");

					var cdcSupport = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
					AssertEquals(false, cdcSupport.AreTablesTrackedByCdc);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestAreTablesTrackedByCdc_ShouldBeFalse_WhenATableIsCdcDisabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();

					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", false, true, "", "", "Test_PK", false);
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");

					testConnection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[TestTable]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
)");
					var cdcSupport = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
					cdcSupport.SynchroniseCdcSchema();
					var tableName = CdcConfigurationInfo.GetEligibleCdcTables(testConnection).First().TableName;
					CdcTableTest.DisableCdcIfEnabled(testConnection, tableName);
					AssertEquals(false, cdcSupport.AreTablesTrackedByCdc);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnablingCdcDoesNotCreateCdcJobs()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					EnsureCdcIsEnabled(adminConnection, Db.DatabaseName);
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();

					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", false, true, "", "", "Test_PK", false);
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");

					adminConnection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[TestTable]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
)");
					CdcTableTest.EnableCdcTable(adminConnection, "TestTable");
					DisableCdcJob(adminConnection, "capture");
					DisableCdcJob(adminConnection, "cleanup");

					CombineAssertions(() =>
					{
						AssertCdcJobExists(adminConnection, "capture", exists: false);
						AssertCdcJobExists(adminConnection, "cleanup", exists: false);
					});

					var cdcSupport = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, adminConnection);
					cdcSupport.SynchroniseCdcSchema();

					CombineAssertions(() =>
					{
						AssertCdcJobExists(adminConnection, "capture", exists: false);
						AssertCdcJobExists(adminConnection, "cleanup", exists: false);
					});
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		void EnsureCdcIsEnabled(AdminConnection testConnection, string dbName)
		{
			if (CdcDatabase.IsEnabled(testConnection, dbName))
			{
				CdcDatabase.Disable(testConnection, dbName);
			}
			CdcDatabase.Enable(testConnection, dbName);
		}

		void DisableCdcJob(AdminConnection connection, string jobName)
		{
			var sqlText = $"EXEC sys.sp_cdc_drop_job @job_type = N'{jobName}'";
			connection.ExecuteNonQuery(sqlText);
		}

		void AssertCdcJobExists(AdminConnection connection, string jobName, bool exists)
		{
			var sqlText = $"IF EXISTS (SELECT * FROM msdb.dbo.sysjobs_view WHERE name = N'cdc.{Db.DatabaseName}_{jobName}') SELECT 1 ELSE SELECT 0";
			AssertEquals($"DB name: {Db.DatabaseName}, Job type: {jobName} exists?", exists, Convert.ToBoolean(connection.ExecuteScalar(sqlText)));
		}

		[UseSnapshotProtection]
		public void TestSupportsNetChangesFlagSetCorrectly()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					var registration = ObjectFactory.Get<IProductRegistration>();
					registration.KeyForTest.IsInternalSystemForTest = false;

					EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();

					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", false, true, "", "", "Test_PK", false);
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");

					testConnection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[TestTable]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
)");
					CdcTableTest.EnableCdcTable(testConnection, "TestTable");
					var cdcSupport = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
					cdcSupport.SynchroniseCdcSchema();
					AssertEquals(CdcConfigurationInfo.SupportsNetChangesFlag, !string.IsNullOrEmpty(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(testConnection)));
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		void ReEnableCdc(AdminConnection testConnection, string tableName)
		{
			CdcTableTest.DisableCdcIfEnabled(testConnection, tableName);
			CdcTableTest.EnableCdcTable(testConnection, tableName);
			CdcTableTest.AssertCdcIsEnabled(testConnection, tableName, true);
		}

		void EnableCdcWithAllColumns(DbConnection testConnection, string tableName)
		{
			CdcTableTest.DisableCdcIfEnabled(testConnection, tableName);
			var cdcTable = new CdcTable(Db.SqlDbOwnerSchema, tableName);
			cdcTable.EnableCdc(testConnection, includeAllColumns: true);

			CdcTableTest.AssertCdcIsEnabled(testConnection, tableName, true);
		}

		void AssertCdcTableRowCount(DbConnection testConnection, string schemaName, string sourceTable, int expectedCount)
		{
			string captureInstance = schemaName + "_" + sourceTable;
			AssertCdcCaptureInstanceTableRowCount(testConnection, captureInstance, expectedCount);
		}

		void AssertCdcCaptureInstanceTableRowCount(DbConnection testConnection, string captureInstance, int expectedCount)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, "SELECT count(*) FROM cdc.[{0}_CT];", captureInstance);
			AssertEquals(captureInstance + " CDC row count", expectedCount, testConnection.ExecuteScalar(sqlText));
		}

		void AssertCdcInstanceIsEnabled(DbConnection conn, string schemaName, string tableName, string captureInstance, bool expected)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS (
					SELECT 1
					FROM
						cdc.change_tables ct
						INNER JOIN sys.tables t ON t.object_id = ct.source_object_id
						INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
					WHERE
						s.name = '{0}'
						AND t.name = '{1}'
						AND ct.capture_instance = '{2}')
					SELECT 1
				ELSE
					SELECT 0",
				schemaName,
				tableName,
				captureInstance);
			bool actual = Convert.ToBoolean(conn.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			AssertEquals(
				String.Format(CultureInfo.InvariantCulture, "Capture instance [{0}.{1}] - [{2}] exists", schemaName, tableName, captureInstance),
				expected, actual);
		}

		public static T CastDataRowTypeToConfigRow<T>(object val)
		{
			return (val == null || val == DBNull.Value) ? default(T) : (T)val;
		}

		public void SetupTestCdcConfig()
		{
			var cdcTableConfig = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig;
			var cdcColumnConfig = BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig;

			var cdcTablesAsDataTable = cdcTableConfig.CopyToDataTable();
			var cdcColumnsAsDataTable = cdcColumnConfig.CopyToDataTable();

			var cdcTablesToTest = cdcTablesAsDataTable.Select(@"
	SourceTable='GlbBranch' or
	SourceTable='GlbDepartment' or
	SourceTable='GlbGroup' or
	SourceTable='JobContainer' or
	SourceTable='RefCountry' or
	SourceTable='StmReportRun' or
	SourceTable='GlbCompany'");

			cdcColumnConfig.Clear();
			cdcTableConfig.Clear();

			foreach (var cdcTable in cdcTablesToTest)
			{
				var cdcTableAdded = cdcTableConfig.AddCdcTableConfigRow(
					CastDataRowTypeToConfigRow<string>(cdcTable[0]),
					CastDataRowTypeToConfigRow<string>(cdcTable[1]),
					CastDataRowTypeToConfigRow<string>(cdcTable[2]),
					CastDataRowTypeToConfigRow<string>(cdcTable[3]),
					CastDataRowTypeToConfigRow<string>(cdcTable[4]),
					CastDataRowTypeToConfigRow<bool>(cdcTable[5]),
					CastDataRowTypeToConfigRow<bool>(cdcTable[6]),
					CastDataRowTypeToConfigRow<string>(cdcTable[7]),
					CastDataRowTypeToConfigRow<string>(cdcTable[8]),
					CastDataRowTypeToConfigRow<string>(cdcTable[9]),
					CastDataRowTypeToConfigRow<bool>(cdcTable[10])
				);

				var cdcColumnsToAdd = cdcColumnsAsDataTable.Select($"SourceTable='{(string)cdcTable[1]}'");
				foreach (var cdcColumn in cdcColumnsToAdd)
				{
					cdcColumnConfig.AddCdcColumnConfigRow(
						cdcTableAdded,
						CastDataRowTypeToConfigRow<string>(cdcColumn[1]),
						CastDataRowTypeToConfigRow<string>(cdcColumn[2]),
						CastDataRowTypeToConfigRow<int>(cdcColumn[3]),
						CastDataRowTypeToConfigRow<int>(cdcColumn[4]),
						CastDataRowTypeToConfigRow<int>(cdcColumn[5]),
						CastDataRowTypeToConfigRow<bool>(cdcColumn[6]),
						CastDataRowTypeToConfigRow<bool>(cdcColumn[7]),
						CastDataRowTypeToConfigRow<string>(cdcColumn[8]),
						CastDataRowTypeToConfigRow<string>(cdcColumn[9]),
						CastDataRowTypeToConfigRow<bool>(cdcColumn[10]),
						CastDataRowTypeToConfigRow<string>(cdcColumn[11]),
						CastDataRowTypeToConfigRow<string>(cdcColumn[12]),
						CastDataRowTypeToConfigRow<bool>(cdcColumn[13]),
						CastDataRowTypeToConfigRow<bool>(cdcColumn[14]),
						CastDataRowTypeToConfigRow<string>(cdcColumn[15])
					);
				}
			}
		}

		[RequiresLargeDatabase]
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDisableCdcDatabases()
		{
			var auditDbName = Db.AuditDatabaseName;
			const string testTableName = "GlbStaff";

			using (var connection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Disable(connection, Db.DatabaseName);
				}
				CdcDatabase.Enable(connection, Db.DatabaseName);
				EnableCdcOnTable(connection, testTableName);

				AssertEquals("Database enabled for CDC?", true, CdcDatabase.IsEnabled(connection, Db.DatabaseName));
				AssertEquals("Table enabled for CDC?", true, new CdcTable("dbo", testTableName).IsCdcEnabled(connection));

				var cdcSupport = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, connection);
				cdcSupport.DisableCdcForChangedTables();

				AssertEquals("Database enabled for CDC?", true, CdcDatabase.IsEnabled(connection, Db.DatabaseName));
				AssertEquals("Table enabled for CDC?", false, new CdcTable("dbo", testTableName).IsCdcEnabled(connection));

				cdcSupport.SynchroniseCdcSchema();

				AssertEquals("Database enabled for CDC?", true, CdcDatabase.IsEnabled(connection, Db.DatabaseName));
				AssertEquals("Table enabled for CDC?", true, new CdcTable("dbo", testTableName).IsCdcEnabled(connection));
			}
		}

		[UseSnapshotProtection]
		public void TestMandatoryTraceFlagMissingException_IsHostedWithCargowise()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var testCdc = new CdcSupportForTest(new DummyUpgradeManager(), Db.DatabaseName, testConnection);

				try
				{
					EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

					EnvProxy.SetHostedLocationForTest("SYD");
					AssertEquals("Is WiseCloud hosted?", true, EnvProxy.IsHostedWithCargowise);
					testCdc.SynchroniseCdcSchema();
				}
				catch (OdysseyDataException ex)
				{
					var expectedMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
					AssertContains("Exception message", expectedMessage, ex.Message);
				}
				finally
				{
					EnvProxy.SetHostedLocationForTest(null);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestMandatoryTraceFlagMissingException_IsNotHostedWithCargowise()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var testCdc = new CdcSupportForTest(new DummyUpgradeManager(), Db.DatabaseName, testConnection);

				try
				{
					EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

					EnvProxy.SetHostedLocationForTest("NCW");
					AssertEquals("Is WiseCloud hosted?", false, EnvProxy.IsHostedWithCargowise);
					testCdc.SynchroniseCdcSchema();
				}
				catch (OdysseyDataException ex)
				{
					var expectedMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
					AssertContains("Exception message", expectedMessage, ex.Message);
				}
				finally
				{
					EnvProxy.SetHostedLocationForTest(null);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnsureAllowAlterCdcMetaObjectsTriggerExists()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

				AssertTriggerExists(testConnection, expectedExists: true);

				testConnection.ExecuteNonQuery("DROP TRIGGER TG_AllowAlterCDCMetaObjects ON DATABASE");
				AssertTriggerExists(testConnection, expectedExists: false);

				var testCdc = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
				testCdc.CheckAlterCdcMetaObjectsTrigger();

				AssertTriggerExists(testConnection, expectedExists: true);
			}
		}

		void AssertTriggerExists(AdminConnection connection, bool expectedExists)
		{
			var actualExists = connection.Exists("FROM sys.triggers WHERE name = 'TG_AllowAlterCDCMetaObjects'");
			AssertEquals("TG_AllowAlterCDCMetaObjects exists?", expectedExists, actualExists);
		}

		[UseSnapshotProtection]
		public void TestEnsureAllowAlterCdcMetaObjectsTriggerIsEnabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

				AssertTriggerExists(testConnection, expectedExists: true);
				AssertTriggerIsEnabled(testConnection, expectedEnabled: true);

				testConnection.ExecuteNonQuery("DISABLE TRIGGER TG_AllowAlterCDCMetaObjects ON DATABASE");
				AssertTriggerIsEnabled(testConnection, expectedEnabled: false);

				var testCdc = new CdcSupport(new DummyUpgradeManager(), Db.DatabaseName, testConnection);
				testCdc.CheckAlterCdcMetaObjectsTrigger();

				AssertTriggerExists(testConnection, expectedExists: true);
				AssertTriggerIsEnabled(testConnection, expectedEnabled: true);
			}
		}

		void AssertTriggerIsEnabled(AdminConnection connection, bool expectedEnabled)
		{
			var actualEnabled = connection.Exists("FROM sys.triggers WHERE name = 'TG_AllowAlterCDCMetaObjects' AND is_disabled = 0");
			AssertEquals("TG_AllowAlterCDCMetaObjects is enabled?", expectedEnabled, actualEnabled);
		}

		void EnableCdcOnTable(AdminConnection connection, string testTableName)
		{
			connection.ExecuteNonQuery($"EXEC sys.sp_cdc_enable_table @source_schema = 'dbo', @source_name = '{testTableName}', @role_name = null");
		}
	}
}
