using System;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class EdwDatabasePopulatorTransactionedTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			BiAutomationConfigLoader.Instance.ResetConfiguration();
		}

		public void TestMasterStateIsEmpty()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				TestConnection.ExecuteNonQuery($"INSERT INTO [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[MasterState](ParamName, ParamValue) SELECT 'TestParam', 'TestValue'");
				var sqlText = $"IF EXISTS (SELECT NULL FROM [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[MasterState]) SELECT 1 ELSE SELECT 0";

				Assert("MasterState should not be empty.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));

				var populator = new EdwDatabasePopulatorForTest(TestConnection);
				populator.FlagCdcReEnabledTablesAsRequiringInitialLoad();

				Assert("MasterState should be empty.", !Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
			}
		}

		public void TestNoTablesTriggeredForInitialLoad()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var populator = new EdwDatabasePopulatorForTest(TestConnection);
				populator.Run();

				Assert(string.Format("The following tables were triggered for initial load:\r\n{0}", string.Join("\r\n", populator.TablesForInitialLoad_Exposed)), !populator.TablesForInitialLoad_Exposed.Any());
			}
		}

		public void TestTriggerInitialLoadForAllDenormalizedTables()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = string.Format(@"UPDATE [{0}].ModelTableState SET InitialLoadRequired = 0, CurrentState = 'New'", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				foreach (var denormTable in BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig)
				{
					sqlText = string.Format(@"
ALTER VIEW [{0}].[vw_{1}]
	AS
SELECT
	[Test Column] = 1", denormTable.Schema, denormTable.Name);
					TestConnection.ExecuteNonQuery(sqlText);
				}

				var populator = new EdwDatabasePopulatorForTest(TestConnection);
				populator.Run();

				sqlText = string.Format(@"
IF NOT EXISTS (SELECT NULL FROM [{0}].ModelTableState WHERE InitialLoadRequired = 0 AND CurrentState = 'New')
	SELECT 1
ELSE
	SELECT 0", BiConstants.BiAdminSchemaName);
				Assert("All EDW Denormalized Tables should have been triggered for initial load.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
			}
		}

		public void TestTriggerInitialLoadOnlyForSpecificDenormalizedTable()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = string.Format(@"UPDATE [{0}].ModelTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				var denormTable = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig.FirstOrDefault();
				if (denormTable != null)
				{
					sqlText = string.Format(@"
ALTER VIEW [{0}].[vw_{1}]
	AS
SELECT
	[Test Column] = 1", denormTable.Schema, denormTable.Name);
					TestConnection.ExecuteNonQuery(sqlText);

					var populator = new EdwDatabasePopulatorForTest(TestConnection);
					populator.Run();

					sqlText = string.Format(@"
IF EXISTS (SELECT NULL FROM [{2}].ModelTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND ModelSchemaName = '{0}' AND ModelTableName = '{1}') AND
	NOT EXISTS (SELECT NULL FROM [{2}].ModelTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND (ModelSchemaName <> '{0}' OR ModelTableName <> '{1}'))
	SELECT 1
ELSE
	SELECT 0", denormTable.Schema, denormTable.Name, BiConstants.BiAdminSchemaName);
					Assert("Only one denormalized table should have been triggered for initial load.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
				}
				else
				{
					Fail("No denormalized tables to test");
				}
			}
		}

		public void TestNoCustomTableTriggeredForInitialLoad()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = string.Format(@"UPDATE [{0}].CustomTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				var populator = new EdwDatabasePopulatorForTest(TestConnection);
				populator.Run();

				sqlText = string.Format(@"
IF EXISTS (SELECT NULL FROM [{0}].CustomTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New')
	SELECT 1
ELSE
	SELECT 0", BiConstants.BiAdminSchemaName);
				Assert("Custom tables should not have been triggered for initial load.", !Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
			}
		}

		public void TestTriggerInitialLoadOnlyForCustomTableViewChange()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = string.Format(@"UPDATE [{0}].CustomTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				var customTable = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig.FirstOrDefault(t => !string.IsNullOrEmpty(t.ViewName));
				if (customTable != null)
				{
					sqlText = string.Format(@"
ALTER VIEW [{0}].[{1}]
	AS
SELECT
	[Test Column] = 1", customTable.Schema, customTable.ViewName);
					TestConnection.ExecuteNonQuery(sqlText);

					var populator = new EdwDatabasePopulatorForTest(TestConnection);
					populator.Run();

					sqlText = string.Format(@"
IF EXISTS (SELECT NULL FROM [{2}].CustomTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND ModelSchemaName = '{0}' AND ModelTableName = '{1}') AND
	NOT EXISTS (SELECT NULL FROM [{2}].CustomTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND (ModelSchemaName <> '{0}' OR ModelTableName <> '{1}'))
	SELECT 1
ELSE
	SELECT 0", customTable.Schema, customTable.Name, BiConstants.BiAdminSchemaName);
					Assert("Only one custom table should have been triggered for initial load.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
				}
				else
				{
					Fail("No custom tables to test");
				}
			}
		}

		public void TestTriggerInitialLoadOnlyForCustomTableInitialLoadChange()
		{
			try
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
				{
					var sqlText = string.Format(@"UPDATE [{0}].CustomTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'", BiConstants.BiAdminSchemaName);
					TestConnection.ExecuteNonQuery(sqlText);

					var customTable = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig.FirstOrDefault(t => !string.IsNullOrEmpty(t.InitialLoadQuery));
					if (customTable != null)
					{
						customTable.InitialLoadQuery = "// Modified script";

						var populator = new EdwDatabasePopulatorForTest(TestConnection);
						populator.Run();

						sqlText = string.Format(@"
IF EXISTS (SELECT NULL FROM [{2}].CustomTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND ModelSchemaName = '{0}' AND ModelTableName = '{1}') AND
	NOT EXISTS (SELECT NULL FROM [{2}].CustomTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND (ModelSchemaName <> '{0}' OR ModelTableName <> '{1}'))
	SELECT 1
ELSE
	SELECT 0", customTable.Schema, customTable.Name, BiConstants.BiAdminSchemaName);
						Assert("Only one custom table should have been triggered for initial load.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
					}
					else
					{
						Fail("No custom tables to test");
					}
				}
			}
			finally
			{
				BiAutomationConfigLoader.Instance.ResetConfiguration();
			}
		}

		public void TestTriggerInitialLoadOnlyForCustomTableIncrementalLoadChange()
		{
			try
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
				{
					var sqlText = string.Format(@"UPDATE [{0}].CustomTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'", BiConstants.BiAdminSchemaName);
					TestConnection.ExecuteNonQuery(sqlText);

					var customTable = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig.FirstOrDefault(t => !string.IsNullOrEmpty(t.IncrementalLoadQuery));
					if (customTable != null)
					{
						customTable.IncrementalLoadQuery = "// Modified script";

						var populator = new EdwDatabasePopulatorForTest(TestConnection);
						populator.Run();

						sqlText = string.Format(@"
IF EXISTS (SELECT NULL FROM [{2}].CustomTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND ModelSchemaName = '{0}' AND ModelTableName = '{1}') AND
	NOT EXISTS (SELECT NULL FROM [{2}].CustomTableState WHERE InitialLoadRequired = 1 AND CurrentState = 'New' AND (ModelSchemaName <> '{0}' OR ModelTableName <> '{1}'))
	SELECT 1
ELSE
	SELECT 0", customTable.Schema, customTable.Name, BiConstants.BiAdminSchemaName);
						Assert("Only one custom table should have been triggered for initial load.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
					}
					else
					{
						Fail("No custom tables to test");
					}
				}
			}
			finally
			{
				BiAutomationConfigLoader.Instance.ResetConfiguration();
			}
		}

		public void TestStagingTableConfigurationNotUpdated()
		{
			try
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
				{
					var edwTable = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.First();
					edwTable.StagingTable = "TestTable";

					var populator = new EdwDatabasePopulatorForTest(TestConnection);
					populator.Run();
					Fail("Test should have thrown an OdysseyDataException.");
				}
			}
			catch (OdysseyDataException ex)
			{
				AssertEquals("Exception message", "Could not find staging table [TestTable]. Make sure that CDC configuration is updated and try again.", ex.Message);
			}
			finally
			{
				BiAutomationConfigLoader.Instance.ResetConfiguration();
			}
		}

		public void TestContainsTrackingComments()
		{
			var stagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => t.SourceTable);
			var edwTables = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => stagingTables.Contains(t.StagingTable));
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				new EdwDatabasePopulatorForTest(TestConnection).Run();

				foreach (var edwTable in edwTables)
				{
					var fullTableName = $"{edwTable.Schema}.{edwTable.Name}";
					var checkQuery = $"FROM biadmin.TransformTableConfiguration WHERE ModelTableName = '{edwTable.Name}' AND InitialLoadQuery LIKE '%Table:Ini_{fullTableName}%' AND IncrementalInsertQuery LIKE '%Table:Inc_{fullTableName}%' AND CustomIndexScript LIKE '%Table:Ini_{fullTableName}%'";
					Assert(TestConnection.Exists(checkQuery));
				}
			}
		}
	}
}
