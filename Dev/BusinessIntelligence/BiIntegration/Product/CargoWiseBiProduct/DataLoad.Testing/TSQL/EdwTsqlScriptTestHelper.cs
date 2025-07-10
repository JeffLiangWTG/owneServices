using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Integration;

namespace CargoWise.Bi.Product.DataLoad.Testing
{
	public class EdwTsqlScriptTestHelper
	{
		IEnumerable<(string, string)> StagingTableList => stagingTableList ?? (stagingTableList = CdcConfigTables.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => (t.SourceSchema, t.SourceTable)).ToArray());
		IEnumerable<(string, string)> stagingTableList;

		IEnumerable<BiAutomationConfigDataSet.CdcTableConfigRow> CdcConfigTables => cdcTables ?? (cdcTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient));
		IEnumerable<BiAutomationConfigDataSet.CdcTableConfigRow> cdcTables;

		IEnumerable<BiAutomationConfigDataSet.EdwTableConfigRow> EdwBaseTableList
		{
			get
			{
				return edwBaseTableList ?? (edwBaseTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => StagingTableList.Contains((t.SourceSchema, t.StagingTable))));
			}
		}
		IEnumerable<BiAutomationConfigDataSet.EdwTableConfigRow> edwBaseTableList;

		BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable EdwAggregateTableList
		{
			get
			{
				return edwAggregateTableList ?? (edwAggregateTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig);
			}
		}
		BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable edwAggregateTableList;

		string EdwDbName
		{
			get
			{
				return Db.EdwDatabaseName;
			}
		}

		public void ETLRunTransformationForTest(List<string> schemaTable, Action insert, AdminConnection connection)
		{
			var mainDbTableList = EdwBaseTableList.Where(t =>
			{
				return t.SourceSchema == "dbo" && schemaTable.Contains(t.StagingTable);
			}).Distinct();

			TruncateTables(connection);
			DisableCdc(connection);
			EnableCdcForMainDBTables(connection, mainDbTableList.Select(t => (t.SourceSchema, t.StagingTable)));

			#region Initial Load

			var scanner = new EdwTsqlScriptRunnerTest.CdcScannerForEdwTest();
			scanner.ScanUntilNoTransactionsToProcessSafe();

			var logger = new LoggerForTest();
			var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
			edwTsqlScriptRunner.Run();

			insert();

			scanner.ScanUntilNoTransactionsToProcessSafe();
			edwTsqlScriptRunner.Run();

			#endregion
		}

		public void TruncateTables(DbConnection connection)
		{
			foreach (var (schema, table) in StagingTableList)
			{
				connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[Staging].[{1}]", EdwDbName, table));
			}

			foreach (var table in EdwBaseTableList)
			{
				connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[{2}]", EdwDbName, table.Schema, table.Name));
			}

			foreach (var table in EdwAggregateTableList)
			{
				connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[{2}]", EdwDbName, table.Schema, table.Name));
			}

			connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[MasterState]", EdwDbName, BiConstants.BiAdminSchemaName));
			connection.ExecuteNonQuery(string.Format(@"
UPDATE [{0}].[{1}].[StagingTableState]
SET
	CurrentState = 'New',
	InitialLoadRequired = 1,
	CurrentMaxLsn = 0x0,
	InitialLoadRecordCount = NULL,
	InitialLoadDurationMs = NULL,
	IncrementalLoadRecordCount = NULL,
	IncrementalLoadDurationMs = NULL,
	SqlErrorMessage = NULL,
	StateModifiedTimestamp = GETDATE()

UPDATE [{0}].[{1}].[TransformTableState]
SET
	CurrentState = 'New',
	InitialLoadRequired = 1,
	InitialTransformRecordCount = NULL,
	InitialTransformDurationMs = NULL,
	MergeTransformInsertRecordCount = NULL,
	MergeTransformInsertDurationMs = NULL,
	MergeTransformDeleteRecordCount = NULL,
	MergeTransformDeleteDurationMs = NULL,
	IndexReorganizeDurationMs = NULL,
	SqlErrorMessage = NULL,
	StateModifiedTimestamp = GETDATE()", EdwDbName, BiConstants.BiAdminSchemaName));
		}

		public void EnableCdcForMainDBTables(AdminConnection connection, IEnumerable<(string, string)> mainDbTableList)
		{
			ShrinkLogFile(connection);
			if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcTestHelper.TruncateLog(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);
			}

			foreach (var (schema, table) in mainDbTableList)
			{
				var cdcTable = new CdcTableForTesting(schema, table);
				if (!cdcTable.IsCdcEnabled(connection))
				{
					cdcTable.EnableCdc(connection);
				}
			}
		}

		void DisableCdc(AdminConnection connection)
		{
			ShrinkLogFile(connection);

			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Disable(connection, Db.DatabaseName);
			}
		}

		void ShrinkLogFile(DbConnection connection)
		{
			var logFiles = DataUtils.GetListOfValuesFromQuery(connection, "SELECT name FROM sys.database_files WHERE [type] = 1");
			string sqlText = "CHECKPOINT;" + string.Join("", logFiles.Select(lf => string.Format("DBCC SHRINKFILE ({0}, 1);", lf)));
			connection.ExecuteNonQuery(sqlText);
		}
	}
}
