using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.DataLoad
{
	public class EdwTsqlScriptRunner : TsqlScriptRunner
	{
		public EdwTsqlScriptRunner(DbConnection biConnection, ILogger logger)
			: base(biConnection, logger)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EDW ETL Script Name")]
		public override string ScriptName
		{
			get
			{
				return "EDW ETL Scripts";
			}
		}

		public override string BiDatabaseName
		{
			get
			{
				return Db.EdwDatabaseName;
			}
		}

		protected string MaintenanceNudgeTimeParamName => BiConstants.BimNudgeTimeParamName;

		protected string MaintenanceTaskCode => "BIM";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public override bool ShouldRunEtl()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				var shouldRunEtl = false;

				var bimNudgeTime = BiMasterState.GetParameterDate(biConnection, MaintenanceNudgeTimeParamName);
				if (bimNudgeTime != null)
				{
					if (DateTime.UtcNow - bimNudgeTime > TimeSpan.FromMinutes(15))
					{
						logger.Log(LogType.Debug, $"{MaintenanceTaskCode} Nudge Time has expired. Running ETL.");
						BiMasterState.DeleteParameter(biConnection, MaintenanceNudgeTimeParamName);
						shouldRunEtl = true;
					}
					else
					{
						logger.Log(LogType.Debug, $"{MaintenanceTaskCode} is nudged. Skipping ETL execution until {MaintenanceTaskCode} is finished or nudge time has expired.");
					}
				}
				else
				{
					var lastIndexRebuildDate = BiMasterState.GetParameterDate(biConnection, BiConstants.LastIndexRebuildUtcDt);
					if (IsMaintenanceTaskActiveAndWithinSchedule() &&
						((lastIndexRebuildDate == null) ||
						 (lastIndexRebuildDate != null && DateTime.UtcNow - lastIndexRebuildDate >= TimeSpan.FromHours(12))))
					{
						BiMasterState.SetParameter(biConnection, MaintenanceNudgeTimeParamName, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
						logger.Log(LogType.Debug, $"Nudging {MaintenanceTaskCode} Service Task");
						ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(MaintenanceTaskCode);
					}
					else
					{
						shouldRunEtl = true;
					}
				}

				return shouldRunEtl;
			}
		}

		bool IsMaintenanceTaskActiveAndWithinSchedule()
		{
			var result = false;

			var dailyStartTimeUtc = GetTaskDailyStartTimeUtc();
			if (dailyStartTimeUtc != null)
			{
				var end = dailyStartTimeUtc.Value.AddHours(2);

				var startTime = new TimeSpan(dailyStartTimeUtc.Value.Hour, dailyStartTimeUtc.Value.Minute, dailyStartTimeUtc.Value.Second);
				var endTime = new TimeSpan(end.Hour, end.Minute, end.Second);

				if (TimeBetween(Env.Time.CurrentUtcDateTime, startTime, endTime))
				{
					result = true;
				}
			}

			return result;
		}

		DateTime? GetTaskDailyStartTimeUtc()
		{
			DateTime? dailyStartTimeUtc = null;

			var maintenanceScheduleTask = Factory.LoadTop1<StmScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, MaintenanceTaskCode));
			if (maintenanceScheduleTask != null)
			{
				dailyStartTimeUtc = maintenanceScheduleTask.CalcDailyStartTimeUtc.ToDateTime();
			}

			return dailyStartTimeUtc;
		}

		BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory factory;

		bool TimeBetween(DateTime datetime, TimeSpan startTime, TimeSpan endTime)
		{
			TimeSpan now = datetime.TimeOfDay;
			if (startTime < endTime)
			{
				return startTime <= now && now <= endTime;
			}
			else
			{
				return !(endTime < now && now < startTime);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected override IEnumerable<string> GetErrorList()
		{
			var result = new List<string>();
			var sql = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].usp_GetEdwTableErrors", BiDatabaseName, BiConstants.BiAdminSchemaName); // SQL query

			var errorList = new List<EtlError>();
			biConnection.ExecuteReader(sql, row =>
			{
				var schemaName = row.GetString(0);
				var tableName = row.GetString(1);
				var errorMsg = row.GetString(2);
				ZDateTime? utcTime = null;
				var errorDateTimeObj = row[3];
				if (errorDateTimeObj != null && errorDateTimeObj != DBNull.Value)
				{
					utcTime = new ZDateTime(row.GetDateTime(3), DateTimeKind.Utc);
				}

				errorList.Add(new EtlError($"{schemaName}].[{tableName}", errorMsg, utcTime));
			});

			foreach (var error in errorList)
			{
				result.Add(error.ToString());
			}
			return result;
		}

		public override bool IsInitialLoad()
		{
			return GetInitialLoadStage() != EdwInitialMasterLoadResult.IncrementalMasterLoad || InitialLoadRequired();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Edw stored procedure")]
		public bool InitialLoadRequired()
		{
			using (var cmd = biConnection.Command(string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].usp_IsInitialLoadRequired", BiDatabaseName, BiConstants.BiAdminSchemaName)))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		protected override void RunUnsafe()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				TriggerInitialLoadIfRequired();
				ClearTableStateRecords();
				if (IsInitialLoad())
				{
					PerformInitialLoadSequence();
				}
				else
				{
					PerformIncrementalLoadSequence();
				}
				ClearTransformedRows();
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query")]
		void ClearTableStateRecords()
		{
			biConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE [{0}].[{1}].[StagingTableState]
SET
	IncrementalLoadRecordCount = NULL,
	IncrementalLoadDurationMs = NULL", BiDatabaseName, BiConstants.BiAdminSchemaName));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query")]
		void ClearTransformedRows()
		{
			biConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "EXEC [{0}].[{1}].usp_TruncateTable  '[{1}].[TransformedRow]'", BiDatabaseName, BiConstants.BiAdminSchemaName));
		}

		protected void TriggerInitialLoadIfRequired()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF
	(
		EXISTS (SELECT NULL FROM [{0}].[{1}].StagingTableState WHERE InitialLoadRequired = 1)
		OR EXISTS (SELECT NULL FROM [{0}].[{1}].TransformTableState WHERE InitialLoadRequired = 1)
		OR EXISTS (SELECT NULL FROM [{0}].[{1}].ModelTableState WHERE InitialLoadRequired = 1)
		OR EXISTS (SELECT NULL FROM [{0}].[{1}].CustomTableState WHERE InitialLoadRequired = 1)
	) AND EXISTS (SELECT NULL FROM [{0}].[{1}].MasterState WHERE ParamName = 'INITIAL_LOAD_REQUESTED' AND ParamValue = '0')
		TRUNCATE TABLE [{0}].[{1}].MasterState", // SQL query
				BiDatabaseName, BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		void PerformInitialLoadSequence(EdwInitialMasterLoadResult? previousStage = null)
		{
			var currentStage = GetInitialLoadStage();

			if (currentStage != previousStage)
			{
				switch (currentStage)
				{
					case EdwInitialMasterLoadResult.InitialLoad:
						StagingInitialMasterLoad();
						break;

					case EdwInitialMasterLoadResult.IncrementalLoad:
						StagingIncrementalMasterLoadForInitialLoad();
						break;

					case EdwInitialMasterLoadResult.Merge:
						StagingMergeIncrementalChanges();
						break;

					case EdwInitialMasterLoadResult.Transform:
						TransformMasterLoad(isInitialLoad: true);
						break;

					case EdwInitialMasterLoadResult.IncrementalMasterLoad:
						InitialLoadSummaryLog();
						IncrementalLoadSummaryLog();
						return;

					default:
						throw new EtlExecutionException("Unknown load stage " + currentStage);
				}

				PerformInitialLoadSequence(currentStage);
			}
		}

		void PerformIncrementalLoadSequence()
		{
			if (TablesWithStatusExist(EdwTableStatus.Idle) || TablesWithStatusExist(EdwTableStatus.New))
			{
				StagingIncrementalMasterLoad();
			}

			if (TablesWithStatusExist(EdwTableStatus.Loaded))
			{
				TransformMasterLoad(isInitialLoad: false);

				if (TablesWithStatusExist(EdwTableStatus.Idle))
				{
					IncrementalLoadSummaryLog();
				}
			}
		}

		#region Logging

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public int GetIncrementalLoadRecordCount()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"[{0}].[{1}].usp_GetIncrementalLoadRecordCount", BiDatabaseName, BiConstants.BiAdminSchemaName); // SQL query
				using (var cmd = biConnection.Command(sqlText))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddOutputParameter("@Total", SqlDbType.Int, 0, 0, 0, 0);
					cmd.ExecuteNonQuery();
					return (int)cmd.GetParameterValue("@Total");
				}
			}
		}

		public DateTime GetInitialLoadEndDateTime()
		{
			string result = BiMasterState.GetParameter(biConnection, BiConstants.InitialLoadEndDt, BiDatabaseName);
			return (!string.IsNullOrEmpty(result))
				? Convert.ToDateTime(result, CultureInfo.InvariantCulture)
				: DateTime.MinValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ETL summary log")]
		void InitialLoadSummaryLog()
		{
			var totalCount = GetTotalInitialProcessedRows();

			if (totalCount > 0)
			{
				var builder = new StringBuilder();
				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Processed {0} row(s) for initial load", totalCount));

				var processedTableList = GetTopInitialProcessedTables();
				if (processedTableList.Any())
				{
					builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Top {0} table(s)", processedTableList.Count));
					foreach (var processedTable in processedTableList)
					{
						builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} TransformId: {1} Insert({2})", processedTable.ModelTableName, processedTable.TransformId, processedTable.InsertCount));
					}
				}

				logger.Log(LogType.Information, builder.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Edw stored procedure")]
		int GetTotalInitialProcessedRows()
		{
			using (var cmd = biConnection.Command(string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].usp_GetTotalInitialProcessedRows", BiDatabaseName, BiConstants.BiAdminSchemaName)))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		List<InitialProcessedTableDetails> GetTopInitialProcessedTables()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].[{1}].usp_GetTopInitialProcessedTables", BiDatabaseName, BiConstants.BiAdminSchemaName);
			var processedTableList = new List<InitialProcessedTableDetails>();
			biConnection.ExecuteReader(sqlText, row =>
			{
				var modelSchemaName = row["ModelSchemaName"].ToString();
				var modelTableName = row["ModelTableName"].ToString();
				var transformId = Convert.ToInt32(row["TransformId"].ToString(), CultureInfo.InvariantCulture);
				var count = Convert.ToInt32(row["InitialTransformRecordCount"].ToString(), CultureInfo.InvariantCulture);

				processedTableList.Add(new InitialProcessedTableDetails("[" + modelSchemaName + "].[" + modelTableName + "]", transformId, count));
			});

			return processedTableList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ETL summary log")]
		void IncrementalLoadSummaryLog()
		{
			var incrementalLoadCount = GetTotalNumberIncProcessedTables();
			var initialLoadCount = GetTotalInitialProcessedRows();

			if (incrementalLoadCount > 0)
			{
				var builder = new StringBuilder();
				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total number of processed tables with incremental changes: {0}", incrementalLoadCount));

				var processedTableList = GetIncrementalTopProcessedTables();
				if (processedTableList.Any())
				{
					builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Top {0} table(s)", processedTableList.Count()));
					foreach (var processedTable in processedTableList)
					{
						builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} TransformId: {1} (deletes: {2}, inserts: {3}, net: {4})", processedTable.ModelTableName, processedTable.TransformId, processedTable.DeleteCount, processedTable.InsertCount, processedTable.InsertCount - processedTable.DeleteCount));
					}
				}

				logger.Log(LogType.Information, builder.ToString());
			}
			else if (initialLoadCount == 0)
			{
				logger.Log(LogType.Debug, "No changes processed.");
			}
		}

		int GetTotalNumberIncProcessedTables()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].[{1}].usp_GetTotalNumberIncProcessedTables", BiDatabaseName, BiConstants.BiAdminSchemaName);
			return Convert.ToInt32(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<IncrementalProcessedTableDetails> GetIncrementalTopProcessedTables()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].[{1}].usp_GetIncrementalTopProcessedTables", BiDatabaseName, BiConstants.BiAdminSchemaName);
			var processedTableList = new List<IncrementalProcessedTableDetails>();
			biConnection.ExecuteReader(sqlText, row =>
			{
				var modelSchemaName = row["ModelSchemaName"].ToString();
				var modelTableName = row["ModelTableName"].ToString();
				var transformId = Convert.ToInt32(row["TransformId"].ToString(), CultureInfo.InvariantCulture);
				var insertCount = Convert.ToInt32(row["MergeTransformInsertRecordCount"].ToString(), CultureInfo.InvariantCulture);
				var deleteCount = Convert.ToInt32(row["MergeTransformDeleteRecordCount"].ToString(), CultureInfo.InvariantCulture);

				processedTableList.Add(new IncrementalProcessedTableDetails("[" + modelSchemaName + "].[" + modelTableName + "]", transformId, insertCount, deleteCount));
			});
			return processedTableList;
		}

		struct InitialProcessedTableDetails
		{
			public InitialProcessedTableDetails(string modelTableName, int transformId, int insertCount)
			{
				ModelTableName = modelTableName;
				TransformId = transformId;
				InsertCount = insertCount;
			}

			public string ModelTableName;
			public int TransformId;
			public int InsertCount;
		}

		struct IncrementalProcessedTableDetails
		{
			public IncrementalProcessedTableDetails(string modelTableName, int transformId, int insertCount, int deleteCount)
			{
				ModelTableName = modelTableName;
				TransformId = transformId;
				InsertCount = insertCount;
				DeleteCount = deleteCount;
			}

			public string ModelTableName;
			public int TransformId;
			public int InsertCount;
			public int DeleteCount;
		}

		#endregion

		#region Check Status

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		bool IsMasterStateEmpty()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				@"IF EXISTS (SELECT NULL FROM [{0}].[{1}].MasterState) SELECT 0 ELSE SELECT 1", // SQL Query
				BiDatabaseName, BiConstants.BiAdminSchemaName);
			return Convert.ToBoolean(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool TablesWithStatusExist(EdwTableStatus status)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"IF EXISTS (SELECT NULL FROM [{0}].[{1}].StagingTableState WHERE CurrentState = @CurrentState) SELECT 1 ELSE SELECT 0", BiDatabaseName, BiConstants.BiAdminSchemaName); // SQL query
			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@CurrentState", SqlDbType.VarChar, 100, status.ToString());
				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public EdwInitialMasterLoadResult GetInitialLoadStage()
		{
			string initialLoadRequested = BiMasterState.GetParameter(biConnection, BiConstants.InitialLoadRequested, BiDatabaseName);
			return (EdwInitialMasterLoadResult)Convert.ToInt32(string.IsNullOrEmpty(initialLoadRequested) ? "1" : initialLoadRequested, CultureInfo.InvariantCulture);
		}

		#endregion

		#region Detailed Steps

		void StagingInitialMasterLoad()
		{
			logger.Log(LogType.Debug, "Staging Initial Master Load");

			var initialLoadResult = ExecuteEtlStepWithRetry(
				executeStep: ExecuteStagingInitialMasterLoad,
				shouldRetry: (esilr) => (esilr != EdwStagingInitialLoadResult.Success)
			);

			switch (initialLoadResult)
			{
				case EdwStagingInitialLoadResult.ControlProcessFailure:
				case EdwStagingInitialLoadResult.LoadProcessFailure:
					throw new EtlExecutionException(string.Format(CultureInfo.InvariantCulture, "Staging Initial Load failed.\r\n{0}",
							string.Join("\r\n", GetErrorList())));

				// No specific handling required
				case EdwStagingInitialLoadResult.Success:
					break;

				default:
					throw new EtlExecutionException("Unknown result " + initialLoadResult);
			}
		}

		void StagingIncrementalMasterLoadForInitialLoad()
		{
			var incrementalLoadResult = RunStagingIncrementalMasterLoad();

			switch (incrementalLoadResult)
			{
				case EdwStagingIncrementalLoadResult.NoNewTransactions:
					logger.Log(LogType.Warning, "CDC service task is not running. Make sure it is active.");
					break;

				case EdwStagingIncrementalLoadResult.CdcScanRequired:
					logger.Log(LogType.Debug, "Waiting for CDC scan in the next run");
					break;

				// Already handled at a lower level
				case EdwStagingIncrementalLoadResult.LostChangesDetected:
				case EdwStagingIncrementalLoadResult.RetryRequired:
					break;

				// No specific handling required
				case EdwStagingIncrementalLoadResult.InitialLoadRequired:
				case EdwStagingIncrementalLoadResult.InitialTransformRequired:
				case EdwStagingIncrementalLoadResult.Success:
				case EdwStagingIncrementalLoadResult.SuccessInitialLoad:
				case EdwStagingIncrementalLoadResult.TransformRequired:
					break;

				default:
					throw new EtlExecutionException("Unknown result " + incrementalLoadResult);
			}
		}

		void StagingIncrementalMasterLoad()
		{
			var incrementalLoadResult = RunStagingIncrementalMasterLoad();

			switch (incrementalLoadResult)
			{
				case EdwStagingIncrementalLoadResult.NoNewTransactions:
					logger.Log(LogType.Debug, "No new transaction to process");
					break;

				// Already handled at a lower level
				case EdwStagingIncrementalLoadResult.LostChangesDetected:
				case EdwStagingIncrementalLoadResult.RetryRequired:
					break;

				// No specific handling required
				case EdwStagingIncrementalLoadResult.CdcScanRequired:
				case EdwStagingIncrementalLoadResult.InitialLoadRequired:
				case EdwStagingIncrementalLoadResult.InitialTransformRequired:
				case EdwStagingIncrementalLoadResult.Success:
				case EdwStagingIncrementalLoadResult.SuccessInitialLoad:
				case EdwStagingIncrementalLoadResult.TransformRequired:
					break;

				default:
					throw new EtlExecutionException("Unknown result " + incrementalLoadResult);
			}
		}

		EdwStagingIncrementalLoadResult RunStagingIncrementalMasterLoad()
		{
			logger.Log(LogType.Debug, "Staging Incremental Master Load");

			var incrementalLoadResult = ExecuteEtlStepWithRetry(
				executeStep: ExecuteStagingIncrementalLoad,
				shouldRetry: (esilr) => (esilr == EdwStagingIncrementalLoadResult.RetryRequired)
			);

			switch (incrementalLoadResult)
			{
				case EdwStagingIncrementalLoadResult.LostChangesDetected:
					LogTablesWithLostChanges();
					break;

				case EdwStagingIncrementalLoadResult.RetryRequired:
					logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Staging Incremental Master Load failed.\r\n{0}",
						string.Join("\r\n", GetErrorList())));
					break;
			}

			return incrementalLoadResult;
		}

		EdwStagingIncrementalLoadResult ExecuteStagingIncrementalLoadWithWaitForCdcScan(int retryCount = 0)
		{
			var result = ExecuteStagingIncrementalLoad();

			if (
#if DEBUG
				// Skips sleep and retry if running unit tests
				(!Globals.IsTest) &&
#endif
				(result == EdwStagingIncrementalLoadResult.CdcScanRequired && retryCount < 3)
			)
			{
				logger.Log(LogType.Debug, "Waiting for next CDC scan");
				Thread.Sleep(TimeSpan.FromSeconds(10));
				return ExecuteStagingIncrementalLoadWithWaitForCdcScan(retryCount + 1);
			}

			return result;
		}

		void LogTablesWithLostChanges()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT SourceTableName FROM [{0}].[{1}].StagingTableState WHERE InitialLoadRequired = 1", BiDatabaseName, BiConstants.BiAdminSchemaName); // SQL query
			var tableList = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);

			if (tableList.Any())
			{
				logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Lost change data detected during ETL execution. Triggering initial load for these tables in the next run.\r\n[{0}]", string.Join("]\r\n[", tableList)));
			}
		}

		void StagingMergeIncrementalChanges()
		{
			logger.Log(LogType.Debug, "Merging Incremental Changes To Initial Load");

			var mergeIncrementalChangesResult = ExecuteEtlStepWithRetry(
				executeStep: ExecuteStagingMerge,
				shouldRetry: (esmr) => (esmr == EdwStagingMergeResult.DeleteError || esmr == EdwStagingMergeResult.Others)
			);

			switch (mergeIncrementalChangesResult)
			{
				case EdwStagingMergeResult.DeleteError:
				case EdwStagingMergeResult.Others:
					throw new EtlExecutionException(string.Format(CultureInfo.InvariantCulture, "Merging Incremental Changes To Initial Load failed.\r\n{0}",
						string.Join("\r\n", GetErrorList())));

				// No specific handling required
				case EdwStagingMergeResult.InitialAndIncrementalLoadRequired:
				case EdwStagingMergeResult.Success:
					break;

				default:
					throw new EtlExecutionException("Unknown result " + mergeIncrementalChangesResult);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Check for error message contents")]
		protected void TransformMasterLoad(bool isInitialLoad)
		{
			logger.Log(LogType.Debug, "Transform Master Load");

			var transformMasterLoadResult = ExecuteEtlStepWithRetry(
				executeStep: () => ExecuteTransformMasterLoad(truncateModelTables: isInitialLoad),
				shouldRetry: (etlr) => (etlr == EdwTransformLoadResult.ControlProcessFailure || etlr == EdwTransformLoadResult.TransformFailure)
			);

			switch (transformMasterLoadResult)
			{
				case EdwTransformLoadResult.ControlProcessFailure:
				case EdwTransformLoadResult.TransformFailure:
					var errorList = GetErrorList();
					if (!EnvProxy.IsHostedWithCargowise && errorList.Any(e => e.Contains("Cannot insert duplicate key in object dbo.#Keys")))
					{
						throw new HostedServiceException(string.Join("\r\n", errorList)) { LogException = true };
					}
					else
					{
						throw new EtlExecutionException(string.Format(CultureInfo.InvariantCulture, "Transform Master Load failed.\r\n{0}",
							string.Join("\r\n", errorList)));
					}

				case EdwTransformLoadResult.CorruptedIndex:
					logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Corrupted index found in [{0}]. Rebuilding index in the next run.", GetTableNameWithCorruptedIndex()));
					break;

				// No specific handling required
				case EdwTransformLoadResult.IncrementalLoadRequired:
				case EdwTransformLoadResult.InitialLoadRequired:
				case EdwTransformLoadResult.InitialTransformRequired:
				case EdwTransformLoadResult.Success:
				case EdwTransformLoadResult.SuccessInitialLoad:
					break;

				default:
					throw new EtlExecutionException("Unknown result " + transformMasterLoadResult);
			}
		}

		string GetTableNameWithCorruptedIndex()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"
DECLARE @ModelTableName varchar(700) = ''
SELECT @ModelTableName = [ModelTableName] FROM [{0}].[{1}].[TransformTableState] WHERE SqlErrorMessage = 'Index corruption is detected, requires Index Rebuild.'
IF (@ModelTableName = '')
	SELECT @ModelTableName = [ModelTableName] FROM [{0}].[{1}].[ModelTableState] WHERE SqlErrorMessage = 'Index corruption is detected, requires Index Rebuild.'
SELECT @ModelTableName",
				BiDatabaseName, BiConstants.BiAdminSchemaName);

			var modelTableName = biConnection.ExecuteScalar(sqlText).ToString();
			if (!string.IsNullOrEmpty(modelTableName))
			{
				return modelTableName;
			}
			else
			{
				throw new EtlExecutionException("No table with corrupted index found.");
			}
		}

		#endregion

		#region Stored Procedure Execution

		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		EdwStagingInitialLoadResult ExecuteStagingInitialMasterLoad()
		{
			string initialLoadRequested = BiMasterState.GetParameter(biConnection, BiConstants.InitialLoadRequested, BiDatabaseName);
			bool resetTableState = string.IsNullOrEmpty(initialLoadRequested);

			var query = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].Staging.usp_InitialMasterLoad @reset_table_state = @resetTable, @server_name = @linkedServerName, @batch_size = @batchSize, @error_code = @Output OUTPUT", BiDatabaseName);
			using (var cmd = biConnection.Command(query, commandTimeout))
			{
				cmd.AddParameter("@resetTable", SqlDbType.Bit, resetTableState);
				cmd.AddParameter("@linkedServerName", SqlDbType.VarChar, 1000, LinkedServerNameWithinBrackets);
				cmd.AddParameter("@batchSize", SqlDbType.BigInt, InitialLoadBatchSize);
				cmd.AddOutputParameter("@Output", SqlDbType.Int, 0, 0, 0, null);
				cmd.ExecuteScalar();
				return (EdwStagingInitialLoadResult)cmd.GetParameterValue("@Output");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed to identify SqlClient types")]
		EdwStagingIncrementalLoadResult ExecuteStagingIncrementalLoad()
		{
			try
			{
				if (((IDbConnectionInternals)biConnection).ADOConnection != null)
				{
					printMessages.Clear();
					var dbConnection = ((IDbConnectionInternals)biConnection).ADOConnection;
					if (dbConnection is System.Data.SqlClient.SqlConnection sqlConnectionSys)
					{
						sqlConnectionSys.InfoMessage += (_, args) => printMessages.Add(args.Message);
					}
#if NET
					else if (dbConnection is Microsoft.Data.SqlClient.SqlConnection sqlConnectionMS)
					{
						sqlConnectionMS.InfoMessage += (_, args) => printMessages.Add(args.Message);
					}
#endif
				}

				var query = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].Staging.usp_IncrementalMasterLoad @server_name = @linkedServerName, @error_code = @Output OUTPUT, @print_messages = @printMessages", BiDatabaseName);
				using (var cmd = biConnection.Command(query, commandTimeout))
				{
					cmd.AddParameter("@linkedServerName", SqlDbType.VarChar, 1000, LinkedServerNameWithinBrackets);
					cmd.AddParameter("@printMessages", SqlDbType.Bit, true);
					cmd.AddOutputParameter("@Output", SqlDbType.Int, 0, 0, 0, null);

					cmd.ExecuteScalar();

					return (EdwStagingIncrementalLoadResult)cmd.GetParameterValue("@Output");
				}
			}
			catch (SqlException e)
			{
#if DEBUG
				throw new EtlExecutionException($"Staging incremental load execution failed.\r\nServer: {biConnection.ServerName} \r\nDatabase: {biConnection.CurrentDatabase} \r\n Last print messages:\r\n{string.Join("\r\n", printMessages)}\r\nException message: {e.Message}", e);
#else
				throw e;
#endif
			}
		}

		readonly List<string> printMessages = new List<string>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected EdwStagingMergeResult ExecuteStagingMerge()
		{
			var query = string.Format(CultureInfo.InvariantCulture, @"EXEC [{0}].Staging.usp_MergeIncrementalChangesInitLoad @error_code = @Output OUTPUT", BiDatabaseName);
			using (var cmd = biConnection.Command(query, commandTimeout))
			{
				cmd.AddOutputParameter("@Output", SqlDbType.Int, 0, 0, 0, null);

				cmd.ExecuteNonQuery();

				return (EdwStagingMergeResult)cmd.GetParameterValue("@Output");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual EdwTransformLoadResult ExecuteTransformMasterLoad(bool truncateModelTables)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
@"EXEC [{0}].Transform.usp_MasterTransform @truncate_model_tables = @truncateModelTables,
	@number_of_future_periods_for_partitioning = @numberOfFuturePeriodsForPartitioning,
	@min_number_of_rows_for_partitioning = @minNumberOfRowsForPartitioning,
	@error_code = @Output OUTPUT", BiDatabaseName);
			using (var cmd = biConnection.Command(query, commandTimeout))
			{
				cmd.AddParameter("@truncateModelTables", SqlDbType.Bit, truncateModelTables);
				cmd.AddParameter("@numberOfFuturePeriodsForPartitioning", SqlDbType.Int, FuturePeriodsForPartitioning);
				cmd.AddParameter("@minNumberOfRowsForPartitioning", SqlDbType.BigInt, MinimumRowSizeForPartitioning);
				cmd.AddOutputParameter("@Output", SqlDbType.Int, 0, 0, 0, null);

				cmd.ExecuteScalar();

				return (EdwTransformLoadResult)cmd.GetParameterValue("@Output");
			}
		}

		static readonly int commandTimeout = (int)TimeSpan.FromHours(12).TotalSeconds;

		protected virtual int InitialLoadBatchSize => 10000000;

		protected virtual long MinimumRowSizeForPartitioning => 1000000;

		protected virtual int FuturePeriodsForPartitioning => 6;

		#endregion

		#endregion

		#endregion
	}
}
