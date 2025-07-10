using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.DbUpgrader.Schema;
using Enterprise.Integration;
using NUnit.Framework;

namespace CargoWise.Bi.Product.DataLoad.Testing
{
	[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
	public abstract class EdwEtlExecutionTest : TestCase
	{
		#region Setup and Teardown

		protected override void SetUp()
		{
			base.SetUp();
			BiAutomationConfigLoader.Instance.ResetConfiguration();

			mainDbConnection = Db.NewAdminConnection();
			biConnection = Db.NewAdminConnection(edwDbName);
			Db.ConnectionOverrideForTest = mainDbConnection;

			factory = new BusinessObjectFactory(mainDbConnection);

			logger = new LoggerForTest();
			edwTsqlScriptRunner = new EdwTsqlScriptRunner(biConnection, logger);

			PopulateEdwDatabaseBiAdminTables();
			TruncateTables();

			InitialiseCdc();
		}

		protected LoggerForTest logger { get; private set; }
		protected virtual IEnumerable<string> MainDbTableList { get; }

		IEnumerable<string> TablesToEnableCdc
		{
			get
			{
				var tableList = new List<string>();
				if (!MainDbTableList.Contains("GlbStaff"))
				{
					tableList.Add("GlbStaff");
				}
				tableList.AddRange(MainDbTableList);
				return tableList;
			}
		}

		protected AdminConnection mainDbConnection { get; private set; }
		protected AdminConnection biConnection { get; private set; }
		protected BusinessObjectFactory factory { get; private set; }

		readonly string edwDbName = Db.EdwDatabaseName;
		EdwTsqlScriptRunner edwTsqlScriptRunner;

		protected override void TearDown()
		{
			biConnection?.Dispose();
			mainDbConnection?.Dispose();

			BiAutomationConfigLoader.Instance.ResetConfiguration();
			base.TearDown();
		}

		#endregion

		#region CDC

		void InitialiseCdc()
		{
			DisableCdc();
			EnableCdc();
		}

		protected void DisableCdc()
		{
			ShrinkLogFile();

			if (CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName))
			{
				CdcDatabase.Disable(mainDbConnection, Db.DatabaseName);
			}
		}

		void ShrinkLogFile()
		{
			var logFiles = DataUtils.GetListOfValuesFromQuery(mainDbConnection, "SELECT name FROM sys.database_files WHERE [type] = 1");
			string sqlText = "CHECKPOINT;" + string.Join("", logFiles.Select(lf => string.Format("DBCC SHRINKFILE ({0}, 1);", lf)));
			mainDbConnection.ExecuteNonQuery(sqlText);
		}

		protected void EnableCdc()
		{
			ShrinkLogFile();
			if (!CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName))
			{
				CdcDatabase.Enable(mainDbConnection, Db.DatabaseName);
			}

			foreach (var table in TablesToEnableCdc)
			{
				CdcTable cdcTable;
				if (table.Contains("."))
				{
					var tableValue = table.Split('.');
					cdcTable = new CdcTableForTesting(tableValue[0], tableValue[1]);
				}
				else
				{
					cdcTable = new CdcTableForTesting(Db.SqlDbOwnerSchema, table);
				}

				if (!cdcTable.IsCdcEnabled(mainDbConnection))
				{
					cdcTable.EnableCdc(mainDbConnection);
				}
			}

			AssertEquals("Database is enabled for CDC?", true, CdcDatabase.IsEnabled(mainDbConnection, Db.DatabaseName));
		}

		IEnumerable<string> StagingTableList => stagingTableList ?? (stagingTableList = CdcConfigTables.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => t.SourceTable).ToArray());
		IEnumerable<string> stagingTableList;

		protected void ScanCdc()
		{
			var scanner = new CdcScannerForTest();
			scanner.ScanUntilNoTransactionsToProcessSafe();
		}

		public class CdcScannerForTest : CdcScanner
		{
			public CdcScannerForTest()
			{
				CdcScannerLogger = new CdcScannerLoggerForTest();
			}

			public void ScanUntilNoTransactionsToProcessSafe()
			{
				try
				{
					ScanUntilNoTransactionsToProcess();
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.AnotherConnectionIsRunningSpReplcmdsForCdc)
				{
				}
			}
			public class CdcScannerLoggerForTest : CdcScannerLogger
			{
				public CdcScannerLoggerForTest()
				{
					Logs = new List<string>();
				}

				public List<string> Logs;

				public override void Log(string logMessage)
				{
					Logs.Add(logMessage);
				}

				public override void Debug(string logMessage)
				{
					Logs.Add(logMessage);
				}

				public override void Error(string logMessage)
				{
					Logs.Add(logMessage);
				}

				public override void Warning(string logMessage)
				{
					Logs.Add(logMessage);
				}
			}
		}

		#endregion

		#region Initialise Master tables

		void PopulateEdwDatabaseBiAdminTables()
		{
			var edwDbPopulator = new EdwDatabasePopulator(biConnection);
			edwDbPopulator.Run();
		}

		public void TruncateTables()
		{
			foreach (var table in StagingTableList)
			{
				biConnection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[Staging].[{1}]", edwDbName, table));
			}

			foreach (var table in EdwBaseTableList)
			{
				biConnection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[{2}]", edwDbName, table.Schema, table.Name));
			}

			foreach (var table in EdwAggregateTableList)
			{
				biConnection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[{2}]", edwDbName, table.Schema, table.Name));
			}

			foreach (var table in EdwCustomTableList)
			{
				biConnection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[{2}]", edwDbName, table.Schema, table.Name));
			}

			biConnection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[MasterState]", edwDbName, BiConstants.BiAdminSchemaName));
			biConnection.ExecuteNonQuery(string.Format(@"
UPDATE [{0}].[{1}].[StagingTableState]
SET
	CurrentState = 'Idle',
	InitialLoadRequired = NULL,
	CurrentMaxLsn = 0x0,
	InitialLoadRecordCount = NULL,
	InitialLoadDurationMs = NULL,
	IncrementalLoadRecordCount = NULL,
	IncrementalLoadDurationMs = NULL,
	SqlErrorMessage = NULL,
	StateModifiedTimestamp = GETDATE()

UPDATE [{0}].[{1}].[TransformTableState]
SET
	CurrentState = 'Idle',
	InitialLoadRequired = NULL,
	InitialTransformRecordCount = NULL,
	InitialTransformDurationMs = NULL,
	MergeTransformInsertRecordCount = NULL,
	MergeTransformInsertDurationMs = NULL,
	MergeTransformDeleteRecordCount = NULL,
	MergeTransformDeleteDurationMs = NULL,
	IndexReorganizeDurationMs = NULL,
	SqlErrorMessage = NULL,
	StateModifiedTimestamp = GETDATE()", edwDbName, BiConstants.BiAdminSchemaName));

			var sourceTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => TablesToEnableCdc.Contains(t.SourceTable) || TablesToEnableCdc.Contains(t.SourceSchema + "." + t.SourceTable))
				.Select(t => t.SourceSchema + "." + t.SourceTable);

			biConnection.ExecuteNonQuery($"UPDATE [{BiConstants.BiAdminSchemaName}].StagingTableState SET CurrentState = 'New', InitialLoadRequired = 1 WHERE SourceTableName IN ('{string.Join("', '", sourceTables)}')");
		}

		IEnumerable<BiAutomationConfigDataSet.CdcTableConfigRow> CdcConfigTables => cdcTables ?? (cdcTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient));
		IEnumerable<BiAutomationConfigDataSet.CdcTableConfigRow> cdcTables;

		IEnumerable<BiAutomationConfigDataSet.EdwTableConfigRow> EdwBaseTableList => edwBaseTableList ?? (edwBaseTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => StagingTableList.Contains(t.StagingTable)));
		IEnumerable<BiAutomationConfigDataSet.EdwTableConfigRow> edwBaseTableList;

		BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable EdwAggregateTableList => edwAggregateTableList ?? (edwAggregateTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig);
		BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable edwAggregateTableList;

		BiAutomationConfigDataSet.EdwCustomTableConfigDataTable EdwCustomTableList => edwCustomTableList ?? (edwCustomTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig);
		BiAutomationConfigDataSet.EdwCustomTableConfigDataTable edwCustomTableList;

		#endregion

		#region Run ETL

		protected void RunInitialLoad()
		{
			ScanCdc();
			edwTsqlScriptRunner.Run();

			TriggerChange();

			ScanCdc();
			edwTsqlScriptRunner.Run();

			AssertInitialLoadCompleted();
		}

		void TriggerChange()
		{
			const string sqlText = @"
				UPDATE dbo.GlbStaff SET GS_IsValid = 1, GS_SystemLastEditUser = '~UK', GS_SystemLastEditTimeUtc = GetUtcDate();
				UPDATE dbo.GlbStaff SET GS_IsValid = 0, GS_SystemLastEditUser = '~UK', GS_SystemLastEditTimeUtc = GetUtcDate();";
			mainDbConnection.ExecuteNonQuery(sqlText);
		}

		protected void RunIncrementalLoad()
		{
			ScanCdc();
			edwTsqlScriptRunner.Run();
		}

		#endregion

		#region Assertions

		protected string GetEdwEtlServiceTaskLogs()
		{
			return string.Join("\r\n", logger.LogEntries);
		}

		protected void AssertInitialLoadCompleted()
		{
			var sqlText = $"IF EXISTS (SELECT NULL FROM [{BiConstants.BiAdminSchemaName}].MasterState WHERE ParamName = '{BiConstants.InitialLoadRequested}' AND ParamValue = '0') SELECT 1 ELSE SELECT 0";
			AssertEquals($"Logs:\r\n{GetEdwEtlServiceTaskLogs()}\r\n\r\nInitial load completed?", true, Convert.ToBoolean(biConnection.ExecuteScalar(sqlText)));
		}

		protected void AssertTableHasRow(string tableName, string whereClause)
		{
			Assert($"[{tableName}] missing row: {whereClause}", RowExist(tableName, whereClause));
		}

		protected void AssertTableDoesNotHaveRow(string tableName, string whereClause)
		{
			Assert($"[{tableName}] row should not exist: {whereClause}", !RowExist(tableName, whereClause));
		}

		bool RowExist(string tableName, string whereClause)
		{
			var sqlText = string.Format($"IF EXISTS (SELECT NULL FROM {tableName} WHERE {whereClause}) SELECT 1 ELSE SELECT 0");
			return Convert.ToBoolean(biConnection.ExecuteScalar(sqlText));
		}

		protected void AssertTableRowCount(string tableName, string whereClause, int expectedCount)
		{
			var sqlText = string.Format($"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}");
			AssertEquals($"Expected count for [{tableName}] where: {whereClause}", expectedCount, Convert.ToInt32(biConnection.ExecuteScalar(sqlText)));
		}

		protected void AssertIncrementalLoadDoesNotFail()
		{
			var logs = GetEdwEtlServiceTaskLogs();
			AssertNotContains("Logs should not have logged a failed Incremental Master Load", "Staging Incremental Master Load failed", logs);
		}

		#endregion
	}
}
