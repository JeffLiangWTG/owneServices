using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	public class CdcTableTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestChangeTableInfo()
		{
			var ct = new CdcTableForTesting("dbo", "GlbStaff");
			using (var testConnection = Db.NewAdminConnection())
			{
				EnsureCdcIsEnabled(testConnection, Db.DatabaseName);
				if (!ct.IsCdcEnabled(testConnection))
				{
					ct.EnableCdc(testConnection);
				}
				var captureInstance = ct.CaptureInstance;
				AssertEquals("dbo_GlbStaff", captureInstance);
			}
		}

		[UseSnapshotProtection]
		public void TestIsCdcEnabled()
		{
			string testRefDb = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ");
			const string testTable = "CdcTableTest$Table";

			using (var testConnection = Db.NewAdminConnection(testRefDb))
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, testRefDb);

					testConnection.BeginTransaction();

					testConnection.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS [{0}]; CREATE TABLE [{0}] (Col1 int PRIMARY KEY)", testTable));
					AssertCdcIsEnabled(testConnection, testTable, expected: false);
					EnableCdcTable(testConnection, testTable);
					AssertCdcIsEnabled(testConnection, testTable, expected: true);
				}
				finally
				{
					testConnection.RollbackTransaction();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnableCdc()
		{
			string testRefDb = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ");
			const string testTable = "CdcTableTest$Table";

			string testTableCreateSql = string.Format(@"
				DROP TABLE IF EXISTS [{0}];
				CREATE TABLE [{0}]
				(
					ColPk int PRIMARY KEY,
					ColVarchar100 varchar(100),
					ColVarcharMax varchar(max),
					ColNVarcharMax nvarchar(max),
					ColVarbinaryMax varbinary(max),
					ColSqlVariant sql_variant,
					ColTimestamp timestamp,
					ColXml xml,
					ColDatetime datetime,
				)", testTable);

			using (var testConnection = Db.NewAdminConnection(testRefDb))
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, testRefDb);

					testConnection.BeginTransaction();

					testConnection.ExecuteNonQuery(testTableCreateSql);
					EnableCdcTable(testConnection, testTable);
					AssertCdcIsEnabled(testConnection, testTable, expected: true);
					AssertCaptureColumnList(testConnection, testTable, "ColPk,ColVarchar100,ColDatetime");
				}
				finally
				{
					testConnection.RollbackTransaction();
				}
			}
		}

		void AssertCaptureColumnList(DbConnection conn, string tableName, string expected)
		{
			string sqlText = string.Format(
				"SELECT column_name FROM cdc.captured_columns WHERE object_id = object_id('cdc.{0}_{1}_CT')",
				Db.SqlDbOwnerSchema, tableName);

			AssertEquals(
				string.Format("[{0}] capture columns", tableName),
				expected,
				string.Join(",", DataUtils.GetListOfValuesFromQuery(conn, sqlText)));
		}

		public void TestGetTablesFromCdcConfigurationXml()
		{
			AssertEquals("Are there tables to be enabled for CDC?", true, CdcConfigTables.Any());
			AssertEquals("Is GlbBranch in the list?", true, CdcConfigTables.Where(t => t.SourceTable == GlbBranchSchema.Constants.TableName).Count() == 1);
			AssertEquals("Is JobHeader in the list?", true, CdcConfigTables.Where(t => t.SourceTable == JobHeaderSchema.Constants.TableName).Count() == 1);
			AssertEquals("Is dbo schema in the list?", true, CdcConfigTables.Where(t => t.SourceSchema == Db.SqlDbOwnerSchema).Any());
		}

		public void TestGetEligibleColumnsFromCdcConfigurationXml()
		{
			var cdcTable = new CdcTable(
						JobConShipLinkSchema.Constants.SqlSchemaName,
						JobConShipLinkSchema.Constants.TableName);

			AssertEquals("JobConShipLink eligible CDC column predicate",
				"JN_JK, JN_JS, JN_PK, JN_SystemCreateTimeUtc, JN_SystemCreateUser, JN_SystemLastEditTimeUtc, JN_SystemLastEditUser",
				string.Join(", ", cdcTable.GetEligibleCdcColumns())
			);

			cdcTable = new CdcTable(
						UNDGAttributeSchema.Constants.SqlSchemaName,
						UNDGAttributeSchema.Constants.TableName);

			AssertEquals("UNDGAttribute eligible CDC column predicate",
				"DA_DG, DA_Index, DA_IsSystem, DA_Language, DA_PK, DA_Standard, DA_SystemCreateTimeUtc, DA_SystemCreateUser, DA_SystemLastEditTimeUtc, DA_SystemLastEditUser, DA_Type, DA_UNNO, DA_Variant",
				string.Join(", ", cdcTable.GetEligibleCdcColumns())
			);

			cdcTable = new CdcTable("!@#", "#@!");
			AssertEquals("Non-existing table eligible CDC column predicate",
				"",
				string.Join(", ", cdcTable.GetEligibleCdcColumns())
			);
		}

		[UseSnapshotProtection]
		public void TestDisableCdc()
		{
			string testRefDb = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ");
			const string testTable = "CdcTableTest$Table";
			var testCdcTable = new CdcTable(Db.SqlDbOwnerSchema, testTable);

			using (var testConnection = Db.NewAdminConnection(testRefDb))
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, testRefDb);

					testConnection.BeginTransaction();

					testConnection.ExecuteNonQuery($"DROP TABLE IF EXISTS [{testTable}]; CREATE TABLE [{testTable}] (Col1 int PRIMARY KEY)");
					EnableCdcTable(testConnection, testTable);
					AssertCdcIsEnabled(testConnection, testTable, expected: true);

					testCdcTable.DisableCdc(testConnection, string.Format("{0}_{1}", Db.SqlDbOwnerSchema, testTable));
					AssertCdcIsEnabled(testConnection, testTable, expected: false);
				}
				finally
				{
					testConnection.RollbackTransaction();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDisabledCdcInstances()
		{
			string testRefDb = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ");
			const string testTable = "CdcTableTest$Table";
			var testCdcTable = new CdcTable(Db.SqlDbOwnerSchema, testTable);

			using (var testConnection = Db.NewAdminConnection(testRefDb))
			{
				try
				{
					EnsureCdcIsEnabled(testConnection, testRefDb);

					testConnection.BeginTransaction();

					testConnection.ExecuteNonQuery($"DROP TABLE IF EXISTS [{testTable}]; CREATE TABLE [{testTable}] (Col1 int PRIMARY KEY)");
					EnableCdcTable(testConnection, testTable, captureInstance: "INST1");
					EnableCdcTable(testConnection, testTable, captureInstance: "INST2");
					AssertCdcIsEnabled(testConnection, testTable, expected: true);
					AssertCdcInstanceCount(testConnection, testTable, 2);

					testCdcTable.DisableCdcInstances(testConnection);
					AssertCdcIsEnabled(testConnection, testTable, expected: false);
					AssertCdcInstanceCount(testConnection, testTable, 0);
				}
				finally
				{
					testConnection.RollbackTransaction();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnsureSelectedCdcTablesAreEnabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

				DisableCdcIfEnabled(testConnection, GlbBranchSchema.Constants.TableName);
				AssertCdcIsEnabled(testConnection, GlbBranchSchema.Constants.TableName, false);

				DisableCdcIfEnabled(testConnection, GlbCompanySchema.Constants.TableName);
				AssertCdcIsEnabled(testConnection, GlbCompanySchema.Constants.TableName, false);

				var tablesToEnableCdc = CdcTable.GetTablesToEnableCdc(testConnection);

				var glbBranchInfo = tablesToEnableCdc.FirstOrDefault(t => t.SchemaName == GlbBranchSchema.Constants.SqlSchemaName && t.TableName == GlbBranchSchema.Constants.TableName);
				AssertEquals("Was GlbBranch included in tables to be enabled?", true, glbBranchInfo != null);

				if (glbBranchInfo != null)
				{
					var cdcglbBranchTable = new CdcTableForTesting(glbBranchInfo.SchemaName, glbBranchInfo.TableName);
					cdcglbBranchTable.EnableCdc(testConnection);
					AssertCdcIsEnabled(testConnection, GlbBranchSchema.Constants.TableName, true);

					var cdcTableGlbCompany = new CdcTableForTesting(GlbCompanySchema.Constants.SqlSchemaName, GlbCompanySchema.Constants.TableName);
					cdcTableGlbCompany.EnableCdc(testConnection);
					AssertCdcIsEnabled(testConnection, GlbCompanySchema.Constants.TableName, true);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecreateCdcTriggersWithAuditAndEdwFilters()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();
					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", true, true, "Test_Value = 'Audit'", "Test_Value = 'EDW'", "Test_PK", false);

					var cdcColumnConfig = new CdcColumnConfigDataTable();
					var pkColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");
					var valueColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_Value", "varchar", 100, 0, 0, true, false, "", "", true, "", "", true, true, "");

					CreateTestTableAndEnableCdc(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					CdcTable.RecreateCdcTriggers(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: true);

					InsertIntoTable(testConnection, "dbo", "TestTable", "Audit");
					InsertIntoTable(testConnection, "dbo", "TestTable", "EDW");
					InsertIntoTable(testConnection, "dbo", "TestTable", "XXX");

					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'Audit'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'EDW'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'XXX'", exists: false);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecreateCdcTriggersWithAuditFilter()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();
					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", true, false, "Test_Value = 'Audit'", "", "Test_PK", false);

					var cdcColumnConfig = new CdcColumnConfigDataTable();
					var pkColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");
					var valueColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_Value", "varchar", 100, 0, 0, true, false, "", "", true, "", "", true, true, "");

					CreateTestTableAndEnableCdc(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					CdcTable.RecreateCdcTriggers(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: true);

					InsertIntoTable(testConnection, "dbo", "TestTable", "Audit");
					InsertIntoTable(testConnection, "dbo", "TestTable", "EDW");
					InsertIntoTable(testConnection, "dbo", "TestTable", "XXX");

					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'Audit'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'EDW'", exists: false);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'XXX'", exists: false);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecreateCdcTriggersWithEdwFilter()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();
					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", false, true, "", "Test_Value = 'EDW'", "Test_PK", false);

					var cdcColumnConfig = new CdcColumnConfigDataTable();
					var pkColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");
					var valueColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_Value", "varchar", 100, 0, 0, true, false, "", "", true, "", "", true, true, "");

					CreateTestTableAndEnableCdc(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					CdcTable.RecreateCdcTriggers(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: true);

					InsertIntoTable(testConnection, "dbo", "TestTable", "Audit");
					InsertIntoTable(testConnection, "dbo", "TestTable", "EDW");
					InsertIntoTable(testConnection, "dbo", "TestTable", "XXX");

					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'Audit'", exists: false);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'EDW'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'XXX'", exists: false);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecreateCdcTriggersWithSameFilters()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();
					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", true, true, "Test_Value = 'XXX'", "Test_Value = 'XXX'", "Test_PK", false);

					var cdcColumnConfig = new CdcColumnConfigDataTable();
					var pkColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");
					var valueColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_Value", "varchar", 100, 0, 0, true, false, "", "", true, "", "", true, true, "");

					CreateTestTableAndEnableCdc(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					CdcTable.RecreateCdcTriggers(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: true);

					InsertIntoTable(testConnection, "dbo", "TestTable", "Audit");
					InsertIntoTable(testConnection, "dbo", "TestTable", "EDW");
					InsertIntoTable(testConnection, "dbo", "TestTable", "XXX");

					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'Audit'", exists: false);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'EDW'", exists: false);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'XXX'", exists: true);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecreateCdcTriggersWithEmptyAuditFilter()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();
					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", true, true, "", "Test_Value = 'XXX'", "Test_PK", false);

					var cdcColumnConfig = new CdcColumnConfigDataTable();
					var pkColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");
					var valueColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_Value", "varchar", 100, 0, 0, true, false, "", "", true, "dbo", "TestTable", true, true, "");

					CreateTestTableAndEnableCdc(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					CdcTable.RecreateCdcTriggers(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					InsertIntoTable(testConnection, "dbo", "TestTable", "Audit");
					InsertIntoTable(testConnection, "dbo", "TestTable", "EDW");
					InsertIntoTable(testConnection, "dbo", "TestTable", "XXX");

					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'Audit'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'EDW'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'XXX'", exists: true);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecreateCdcTriggersWithEmptyEdwFilter()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				try
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Clear();
					BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Clear();
					var cdcTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("dbo", "TestTable", "", "", "", true, true, "Test_Value = 'XXX'", "", "Test_PK", false);

					var cdcColumnConfig = new CdcColumnConfigDataTable();
					var pkColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");
					var valueColumn = cdcColumnConfig.AddCdcColumnConfigRow(cdcTable, "Test_Value", "varchar", 100, 0, 0, true, false, "", "", true, "", "", true, true, "");

					CreateTestTableAndEnableCdc(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					CdcTable.RecreateCdcTriggers(testConnection);
					AssertTriggerExists(testConnection, "TR_II_dbo_TestTable_CT", exists: false);

					InsertIntoTable(testConnection, "dbo", "TestTable", "Audit");
					InsertIntoTable(testConnection, "dbo", "TestTable", "EDW");
					InsertIntoTable(testConnection, "dbo", "TestTable", "XXX");

					new CdcScannerForTest().ScanUntilNoTransactionsToProcess();

					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'Audit'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'EDW'", exists: true);
					AssertChangeExists(testConnection, "cdc", "dbo_TestTable_CT", "Test_Value = 'XXX'", exists: true);
				}
				finally
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCdcTableList()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnsureCdcIsEnabled(testConnection, Db.DatabaseName);
				var sqlText = @"
					CREATE TABLE [dbo].[TestTable]
					(
						Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
						Test_Value varchar(100)
					)";
				testConnection.ExecuteNonQuery(sqlText);

				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(testConnection);

				sqlText = "IF EXISTS (select * from cdc.CdcTables where captureinstance = 'dbo_TestTable') SELECT 1 ELSE SELECT 0";
				AssertEquals("CDC table exists in list", true, Convert.ToBoolean(testConnection.ExecuteScalar(sqlText)));
			}
		}

		[UseSnapshotProtection]
		public void TestCdcColumns()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

				var expectedColumns = new HashSet<string>
				{
					"CaptureInstance",
					"Lsn",
					"LastDUProcessedLsn",
					"PkColumnList"
				};

				var sqlText = @"
SELECT name
FROM sys.columns
WHERE object_id = OBJECT_ID('cdc.CdcTables')";

				var actualColumns = new HashSet<string>(DataUtils.GetListOfValuesFromQuery(testConnection, sqlText));

				AssertContainsExactElementsInAnyOrder("Expected CDC to have all of the columns needed to support DUC", expectedColumns, actualColumns);
			}
		}

		[UseSnapshotProtection]
		public void TestCdcInstanceIsLockDown()
		{
			TestCdcInstanceLockDown("dbo", "TestTable");
			TestCdcInstanceLockDown("hrm", "TestTable");
		}

		void TestCdcInstanceLockDown(string schemaName, string tableName)
		{
			var dbName = Db.DatabaseName;
			using (var adminConnection = Db.NewAdminConnection())
			using (var unRestrictedWriter = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, dbName))
			using (var restrictedWriter = Db.NewExtraRestrictedWriterConnection(Db.ServerName, dbName))
			{
				CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				PrepareLockedTable(adminConnection, schemaName, tableName);
				var cdctable = new CdcTableForTesting(schemaName, tableName);
				cdctable.EnableCdc(adminConnection);

				var cdcScanner = new CdcScannerForTest(new CdcScannerLoggerForTest());
				cdcScanner.ScanUntilNoTransactionsToProcess();

				InsertIntoLockedTable(adminConnection, schemaName, tableName, "insert1");
				InsertIntoLockedTable(adminConnection, schemaName, tableName, "insert2");
				cdcScanner.ScanUntilNoTransactionsToProcess();

				var unRestrictedWriterResult = ReadCdcResult(unRestrictedWriter, schemaName, tableName, "str");
				SqlException ex = null;
				try
				{
					ReadCdcResult(restrictedWriter, schemaName, tableName, "str");
				}
				catch (SqlException e)
				{
					ex = e;
				}

				CombineAssertions(() =>
				{
					AssertEquals("Only UnRestrictedWriter can read cdc scan result", 2, unRestrictedWriterResult.Count());
					AssertContainsExactElementsInAnyOrder("Only UnRestrictedWriter can read cdc scan result", new string[] { "insert1", "insert2" }, unRestrictedWriterResult);
					AssertNotNull("Expecting restrictedWriter can not access the cdc instance.", ex);
					AssertEquals(DbErrorType.PermissionDeniedOnObject, new DbErrorMatch(ex).ExceptionType);
				});
			}
		}

		void CreateTestTableAndEnableCdc(AdminConnection testConnection)
		{
			var sqlText = @"
DROP TABLE IF EXISTS [dbo].[TestTable];
CREATE TABLE [dbo].[TestTable]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
	Test_Value varchar(100)
)";

			EnsureCdcIsEnabled(testConnection, Db.DatabaseName);

			testConnection.ExecuteNonQuery(sqlText);

			foreach (var tableInfo in CdcTable.GetTablesToEnableCdc(testConnection))
			{
				var cdcTable = new CdcTableForTesting(tableInfo.SchemaName, tableInfo.TableName);
				if (cdcTable.IsCdcEnabled(testConnection))
				{
					cdcTable.DisableCdc(testConnection, $"{tableInfo.SchemaName}_{tableInfo.TableName}");
				}
				cdcTable.EnableCdc(testConnection);
			}
		}

		void InsertIntoTable(DbConnection conn, string schemaName, string tableName, string value)
		{
			var sqlText = $"insert into [{schemaName}].[{tableName}] select newid(), '{value}'";
			conn.ExecuteNonQuery(sqlText);
		}

		void AssertChangeExists(DbConnection conn, string schemaName, string tableName, string condition, bool exists)
		{
			var sqlText = $"if exists (select null from [{schemaName}].[{tableName}] where {condition}) select 1 else select 0";
			AssertEquals($"Change [{condition}] exists?", exists, Convert.ToBoolean(conn.ExecuteScalar(sqlText)));
		}

		void AssertTriggerExists(DbConnection conn, string triggerName, bool exists)
		{
			var sqlText = $"if exists (select null from sys.triggers where name = '{triggerName}') select 1 else select 0";
			AssertEquals($"Trigger [{triggerName}] exists?", exists, Convert.ToBoolean(conn.ExecuteScalar(sqlText)));
		}

		void AssertCdcInstanceCount(DbConnection conn, string tableName, int expected)
		{
			string sqlText = string.Format(@"
				SELECT count(*)
				FROM
					cdc.change_tables ChgTab
					INNER JOIN sys.tables SrcTab ON SrcTab.object_id = ChgTab.source_object_id
				WHERE
					SrcTab.name = '{0}'",
				tableName);
			AssertEquals(string.Format("CDC capture instance count [{0}]", tableName),
				expected,
				(int)conn.ExecuteScalar(sqlText));
		}

		void PrepareLockedTable(AdminConnection adminConn, string schemaName, string tableName)
		{
			FormattableString sql = $@"IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'{schemaName}' )
    EXEC('CREATE SCHEMA [{schemaName}]');";
			adminConn.ExecuteNonQuery(sql.ToString(CultureInfo.InvariantCulture));

			sql = $"create table [{schemaName}].[{tableName}] (id uniqueidentifier NOT NULL PRIMARY KEY, str varchar(20))";
			adminConn.ExecuteNonQuery(sql.ToString(CultureInfo.InvariantCulture));
		}

		void InsertIntoLockedTable(AdminConnection adminConnection, string schemaName, string tableName, string value)
		{
			FormattableString sqlText = $"insert into [{schemaName}].[{tableName}] select newid(), '{value}'";
			adminConnection.ExecuteNonQuery(sqlText.ToString(CultureInfo.InvariantCulture));
		}

		IEnumerable<string> ReadCdcResult(DbConnection conn, string schemaName, string tableName, string columnName)
		{
			FormattableString sql = $@"
DECLARE @from_lsn binary(10), @to_lsn binary(10);
SET @from_lsn = sys.fn_cdc_get_min_lsn('{schemaName}_{tableName}');
SET @to_lsn   = sys.fn_cdc_get_max_lsn();
SELECT * FROM [cdc].[fn_cdc_get_net_changes_{schemaName}_{tableName}]
(@from_lsn, @to_lsn, N'all');";
			var list = new List<string>();
			conn.ExecuteReader(sql.ToString(CultureInfo.InvariantCulture),
				record =>
				{
					list.Add(record[columnName] as string);
				});
			return list;
		}

		public static void EnableCdcTable(DbConnection conn, string tableName, string captureInstance = null)
		{
			var testCdcTable = new CdcTableForTesting(conn, Db.SqlDbOwnerSchema, tableName);
			testCdcTable.EnableCdc(conn, captureInstance);
		}

		public static void AssertCdcIsEnabled(DbConnection conn, string tableName, bool expected)
		{
			AssertEquals(string.Format("Is [{0}] CDC enabled?", tableName), expected, IsTableCdcEnabled(conn, tableName));
		}

		public static bool IsTableCdcEnabled(DbConnection conn, string tableName)
		{
			return new CdcTable(Db.SqlDbOwnerSchema, tableName).IsCdcEnabled(conn);
		}

		public static void DisableCdcIfEnabled(DbConnection conn, string tableName)
		{
			var testCdcTable = new CdcTable(Db.SqlDbOwnerSchema, tableName);
			testCdcTable.DisableCdcInstances(conn);
		}

		CdcTableConfigDataTable CdcConfigTables => cdcTables ?? (cdcTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig);
		CdcTableConfigDataTable cdcTables;

		void EnsureCdcIsEnabled(AdminConnection testConnection, string dbName)
		{
			if (CdcDatabase.IsEnabled(testConnection, dbName))
			{
				CdcDatabase.Disable(testConnection, dbName);
			}
			CdcDatabase.Enable(testConnection, dbName);
		}
	}
}
