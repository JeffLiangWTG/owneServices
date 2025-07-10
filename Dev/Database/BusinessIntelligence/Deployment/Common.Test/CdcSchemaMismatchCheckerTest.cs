using System;
using System.Data;
using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	class CdcSchemaMismatchCheckerTest : TestCase
	{
		[RequiresLargeLogFile]
		public void TestGetCdcSchemaMismatches()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				AssertNull("CDC is not enabled on database", CdcSchemaMismatchChecker.GetCdcSchemaMismatchLogs(testConnection));

				EnableCdcTables(testConnection);
				AssertNull("CDC is enabled on database.", CdcSchemaMismatchChecker.GetCdcSchemaMismatchLogs(testConnection));

				// Expected message: "The following tables should be enabled for CDC"
				// Mock Action: Disable CDC on a table currently enabled for CDC.
				var tableWithEnabledCdc = CdcConfigTables.First(t => t.TableInAudit);
				new CdcTable(tableWithEnabledCdc.SourceSchema, tableWithEnabledCdc.SourceTable).DisableCdcInstances(testConnection);

				// Expected message: "The following capture instances should be disabled for CDC"
				// Mock Action: Enable CDC on a table currently not enabled for CDC.
				Assert(MailDBItemsSchema.Constants.TableName + " is enabled for CDC. Need to pick another table for this test.", !CdcConfigTables.Any(t => t.SourceTable == MailDBItemsSchema.Constants.TableName));
				new CdcTableForTesting(MailDBItemsSchema.Constants.SqlSchemaName, MailDBItemsSchema.Constants.TableName).EnableCdc(testConnection);

				// Expected message: "The following columns should be enabled for CDC"
				// Expected message: "The following columns should be disabled for CDC:"
				// Find another CDC enabled table with an excluded XML field.
				var excludedColumnFromAnotherCdcTable = GetExcludedColumnFromCdcEnabledTable(testConnection, tableWithEnabledCdc.SourceTable);
				var anotherCdcTable = CdcConfigTables.Where(t => t.SourceSchema == excludedColumnFromAnotherCdcTable.SchemaName && t.SourceTable == excludedColumnFromAnotherCdcTable.TableName).FirstOrDefault();
				Assert(excludedColumnFromAnotherCdcTable.TableName + " is not enabled for CDC. Need to pick another table for this test.", anotherCdcTable != null);

				// Mock Action 1: Disable CDC on the table.
				new CdcTable(excludedColumnFromAnotherCdcTable.SchemaName, excludedColumnFromAnotherCdcTable.TableName).DisableCdcInstances(testConnection);
				// Mock Action 2: Re-enable CDC with only the PK and a column not enabled in the CDC config in the captured column list
				var pkColumn = anotherCdcTable.GetCdcColumnConfigRows().Single(c => c.IsPrimaryKey);

				string testCapturedColumnList = "[" + pkColumn.SourceColumn + "],[" + excludedColumnFromAnotherCdcTable.ColumnName + "]";
				// Run sp_cdc_enable_table
				using (var cmd = testConnection.Command("sys.sp_cdc_enable_table"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@source_schema", SqlDbType.NVarChar, excludedColumnFromAnotherCdcTable.SchemaName);
					cmd.AddParameter("@source_name", SqlDbType.NVarChar, excludedColumnFromAnotherCdcTable.TableName);
					cmd.AddParameter("@role_name", SqlDbType.NVarChar, DBNull.Value);
					cmd.AddParameter("@captured_column_list", SqlDbType.NVarChar, testCapturedColumnList);
					cmd.ExecuteNonQuery();
				}

				// Expected message: "The following CDC configured tables were not found in the database"
				// Expected message: "The following CDC configured columns were not found in the database"
				// Mock Action: Add a non-existing table to the CDC configuration.
				var testTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestTable", "", "", "", true, true, "", "", "TestColumn1", false);
				BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(testTable, "TestColumn1", "uniqueidentifier", 16, 0, 0, false, true, "", "", true, "", "", true, true, "");

				var errorMsg = CdcSchemaMismatchChecker.GetCdcSchemaMismatchLogs(testConnection);
				AssertNotNull("CDC schema mismatches", errorMsg);

				CombineAssertions(errorMsg, () =>
				{
					Assert("Expected message: \"The following CDC configured tables were not found in the database\"", errorMsg.Contains("The following CDC configured tables were not found in the database:"));
					Assert("Expected message: \"The following tables should be enabled for CDC\"", errorMsg.Contains("The following tables should be enabled for CDC:"));
					Assert("Expected message: \"The following capture instances should be disabled for CDC\"", errorMsg.Contains("The following capture instances should be disabled for CDC:"));
					Assert("Expected message: \"The following CDC configured columns were not found in the database\"", errorMsg.Contains("The following CDC configured tables were not found in the database:"));
					Assert("Expected message: \"The following columns should be enabled for CDC\"", errorMsg.Contains("The following columns should be enabled for CDC:"));
					Assert("Expected message: \"The following columns should be disabled for CDC:\"", errorMsg.Contains("The following columns should be disabled for CDC:"));
				});
			}
		}

		CdcConfigurationInfo GetExcludedColumnFromCdcEnabledTable(DbConnection testConnection, string tableNameNotEqualTo)
		{
			string sqlText = @"
				SELECT TOP (1) s.name, t.name, ct.capture_instance, c.name
				FROM
					sys.schemas s
					INNER JOIN sys.tables t ON s.schema_id = t.schema_id
					INNER JOIN sys.columns c ON t.object_id = c.object_id
					INNER JOIN sys.types dt ON c.user_type_id = dt.user_type_id
					INNER JOIN cdc.change_tables ct ON t.object_id = ct.source_object_id
					LEFT JOIN cdc.captured_columns cc ON ct.object_id = cc.object_id AND c.name = cc.column_name
				WHERE
					t.name != @NameNotEqualTo
					AND dt.name = 'xml'
					AND cc.column_name is null";

			using (var cmd = testConnection.Command(sqlText))
			{
				cmd.AddParameter("@NameNotEqualTo", SqlDbType.NVarChar, 128, tableNameNotEqualTo);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var schemaName = reader[0].ToString();
						var tableName = reader[1].ToString();
						var captureInstance = reader[2].ToString();
						var columnName = reader[3].ToString();

						return new CdcConfigurationInfo(schemaName, tableName, captureInstance, columnName);
					}
					else
					{
						throw new InvalidOperationException("No CDC enabled table has excluded XML fields. Need to pick another excluded type for this test.");
					}
				}
			}
		}

		[RequiresLargeLogFile]
		public void TestGetCdcSchemaMismatchesIsCaseInsensitive()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				AssertNull("Logs when CDC is not enabled on database", CdcSchemaMismatchChecker.GetCdcSchemaMismatchLogs(testConnection));

				EnableCdcTables(testConnection);
				AssertNull("Logs when CDC is enabled on database", CdcSchemaMismatchChecker.GetCdcSchemaMismatchLogs(testConnection));

				var cdcTable = new CdcTableForTesting("dbo", "GlbStaff");
				if (cdcTable.IsCdcEnabled(testConnection))
				{
					cdcTable.DisableCdcInstances(testConnection);
				}
				cdcTable.EnableCdc(testConnection, "dbo_glbstaff");

				AssertNull("Logs when GlbStaff has capture instance = 'dbo_glbstaff'", CdcSchemaMismatchChecker.GetCdcSchemaMismatchLogs(testConnection));
			}
		}

		#region Implementation

		void EnableCdcTables(AdminConnection connection)
		{
			CdcDatabase.Enable(connection, Db.DatabaseName);
			var cdcConfigTables = CdcConfigurationInfo.GetEligibleCdcTables(connection);
			foreach (var cdcTable in cdcConfigTables.Select(t => new CdcTableForTesting(t.SchemaName, t.TableName)))
			{
				if (!cdcTable.IsCdcEnabled(connection))
				{
					cdcTable.EnableCdc(connection);
				}
			}
		}

		CdcTableConfigDataTable CdcConfigTables => cdcTables ?? (cdcTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig);
		CdcTableConfigDataTable cdcTables;

		#endregion
	}
}
