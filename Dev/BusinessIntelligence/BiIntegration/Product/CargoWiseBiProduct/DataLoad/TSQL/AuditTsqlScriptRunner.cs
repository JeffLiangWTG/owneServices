using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;

namespace CargoWise.Bi.Product.DataLoad
{
	class AuditTsqlScriptRunner : TsqlScriptRunner
	{
		public AuditTsqlScriptRunner(DbConnection biConnection, ILogger logger)
			: base(biConnection, logger)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Audit ETL Script Name")]
		public override string ScriptName
		{
			get
			{
				return "Audit ETL Scripts";
			}
		}

		public override string BiDatabaseName
		{
			get
			{
				return Db.AuditDatabaseName;
			}
		}

		public override bool IsInitialLoad()
		{
			return false;
		}

		public override bool ShouldRunEtl()
		{
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		List<EtlError> GetErrorMessageFromTableState()
		{
			var errorList = new List<EtlError>();
			var sqlText = $"SELECT SourceSchemaName, SourceTableName, SqlErrorMessage, SqlErrorDatetimeUTC FROM [{BiDatabaseName}].[{BiConstants.BiAdminSchemaName}].TableState WHERE SqlErrorMessage <> ''";

			using (var cmd = biConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader.GetString(0) + '.' + reader.GetString(1);
					var errorMsg = reader.GetString(2);
					ZDateTime? utcTime = null;
					var errorDateTimeObj = reader[3];
					if (errorDateTimeObj != null && errorDateTimeObj != DBNull.Value)
					{
						utcTime = new ZDateTime(reader.GetDateTime(3), DateTimeKind.Utc);
					}

					errorList.Add(new EtlError(tableName, errorMsg, utcTime));
				}
			}

			return errorList;
		}

		void GetLogsFromMasterState(List<EtlError> errorList)
		{
			var errorMessage = BiMasterState.GetParameter(biConnection, BiConstants.LastEtlErrorMessage);
			errorList.Add(new EtlError("MasterState", errorMessage, ZDateTime.UtcNow));
		}

		protected override IEnumerable<string> GetErrorList()
		{
			var errorList = GetErrorMessageFromTableState();
			if (!ErrorMessagesArePopulated(errorList))
			{
				GetLogsFromMasterState(errorList);
			}

			var result = new List<string>();
			foreach (var error in errorList)
			{
				result.Add(error.ToString());
			}

			return result;
		}

		public static bool ErrorMessagesArePopulated(List<EtlError> errorList)
		{
			// necessary because of defect WI00702509 where TableState did not contain any SQL error messages
			return errorList.Any(e => !string.IsNullOrEmpty(e.ErrorMessage));
		}

		protected override void RunUnsafe()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				PerformMasterAuditLoad();
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Check for error message contents")]
#if DEBUG
		public
#endif

		void PerformMasterAuditLoad()
		{
			try
			{
				PerformMasterAuditLoadUnsafe();
			}
			catch (EtlExecutionException ex) when (ex.Message.Contains("Lock request time out period exceeded."))
			{
				var blockingQueryExceptionMessage = GetBlockingQueryExceptionMessage(ex.Message);
				logger.Log(LogType.Error, blockingQueryExceptionMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging message")]
		string GetBlockingQueryExceptionMessage(string message)
		{
			var blockingSessionQuery = "";
			int blockingSessionId;
			string lockedTableName = GetTwoPartTableNameFromFirstSquareBrackets(message);

			using (var conn = Db.NewAdminConnection(BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection), Db.AuditDatabaseName))
			{
				var getBlockingSessionIdQuery = $@"
	SELECT TOP 1 request_session_id FROM sys.dm_tran_locks
	WHERE
	resource_database_id = DB_ID('{Db.AuditDatabaseName}')
	AND RESOURCE_TYPE = N'OBJECT'
	AND RESOURCE_ASSOCIATED_ENTITY_ID = object_id(N'{Db.AuditDatabaseName}.{lockedTableName}')
";
				var errorMessage = "Audit ETL failed due to a lock request timeout";
				blockingSessionId = Convert.ToInt16(conn.ExecuteScalar(getBlockingSessionIdQuery));
				if (blockingSessionId != 0)
				{
					var getQueryFromSessionIdQuery = $"DBCC INPUTBUFFER({blockingSessionId})";
					using (var cmd = conn.Command(getQueryFromSessionIdQuery))
					{
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								blockingSessionQuery = Convert.ToString(reader["EventInfo"]);
							}
						}
					}
					errorMessage += $" caused by the following query:\r\nSPID = {blockingSessionId}\r\nQuery = {blockingSessionQuery}";
				}
				return errorMessage;
			}
		}

		string GetTwoPartTableNameFromFirstSquareBrackets(string message)
		{
			var pattern = "\\[(.*?)\\]";
			var matches = Regex.Matches(message, pattern);
			return matches[0].Groups[1].Value;
		}

		void PerformMasterAuditLoadUnsafe()
		{
			logger.Log(LogType.Debug, "Master Audit Load");
			logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Last max Lsn processed:0x{0}", BiMasterState.GetParameter(biConnection, BiConstants.LastMaxLsnProcessed, BiDatabaseName)));

			var masterLoadResult = ExecuteEtlStepWithRetry(
				executeStep: ExecuteMasterAuditLoadInBatch,
				shouldRetry: (amlr) => (amlr == AuditMasterLoadResult.SqlError)
			);

			switch (masterLoadResult)
			{
				case AuditMasterLoadResult.Success:
					AuditSummaryLog();
					break;
				case AuditMasterLoadResult.SqlError:
					HandleInternalSqlError();
					break;
				case AuditMasterLoadResult.NoMaxLsn:
					logger.Log(LogType.Debug, "CDC service task is not running. Make sure it is active.");
					break;
				case AuditMasterLoadResult.NoNewTransactions:
					logger.Log(LogType.Debug, "No new transaction to process.");
					break;
				case AuditMasterLoadResult.SuccessWithLostChanges:
					logger.Log(LogType.Warning, "ETL detected missing CDC records. Load started from the first available change row.");
					break;
				case AuditMasterLoadResult.CorruptedIndex:
					RebuildIndexForRequiredTable();
					RerunMasterAuditLoad();
					break;
				case AuditMasterLoadResult.LsnTimeMappingError:
					LogLsnTimeMappingError();
					break;
				case AuditMasterLoadResult.CorruptedIndexCdcHistorySummary:
					RebuildIndexForCdcHistorySummary();
					RerunMasterAuditLoad();
					break;
				default:
					throw new EtlExecutionException("Unknown result " + masterLoadResult);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging message")]
		void HandleInternalSqlError()
		{
			var errorList = GetErrorList();
			if (errorList.Any(e => e.Contains("A timeout occurred while waiting for memory resources to execute the query in resource pool 'NonInteractive'")))
			{
				logger.Warning("A timeout occurred while waiting for memory resources to execute the query in resource pool 'NonInteractive'. Retrying Master Audit Load.");
				Thread.Sleep(TimeSpan.FromSeconds(5));
				RerunMasterAuditLoad();
			}
			else
			{
				throw new EtlExecutionException(string.Format(CultureInfo.InvariantCulture,
					"Master Audit Load failed.\r\n{0}",
					string.Join("\r\n", errorList)));
			}
		}

		protected virtual void RerunMasterAuditLoad()
		{
			PerformMasterAuditLoad();
		}

		#region Logging

		internal void AuditSummaryLog()
		{
			var totalCount = GetProcessedRowsTotal();

			if (totalCount > 0)
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Processed {0} row(s)", totalCount)); // ETL summary log

				var processedTableDict = GetTopInitialProcessedTables();
				if (processedTableDict.Any())
				{
					logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Top {0} table(s)", processedTableDict.Count)); // ETL summary log
					foreach (var processedTable in processedTableDict)
					{
						logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "[{0}] : {1}", processedTable.Key, processedTable.Value)); // ETL summary log
					}
				}
			}
			logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Audit ETL completed at Lsn:{0}", BiMasterState.GetParameter(biConnection, BiConstants.LastMaxLsnProcessed, BiDatabaseName))); // ETL summary log
		}

		public int GetProcessedRowsTotal()
		{
			var sqlText = $"SELECT SUM(ISNULL(LoadRecordCount, 0)) FROM [{BiDatabaseName}].[{BiConstants.BiAdminSchemaName}].TableState";
			return Convert.ToInt32(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		Dictionary<string, int> GetTopInitialProcessedTables()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT TOP 5 SourceSchemaName, SourceTableName, ISNULL(LoadRecordCount, 0) AS LoadRecordCount
FROM [{0}].[{1}].TableState
WHERE ISNULL(LoadRecordCount, 0) > 0
ORDER BY ISNULL(LoadRecordCount, 0) DESC",
				BiDatabaseName, BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				var processedTableDict = new Dictionary<string, int>();

				while (reader.Read())
				{
					var sourceSchemaName = reader["SourceSchemaName"].ToString();
					var sourceTableName = reader["SourceTableName"].ToString();
					var count = Convert.ToInt32(reader["LoadRecordCount"].ToString(), CultureInfo.InvariantCulture);

					processedTableDict[sourceSchemaName + "." + sourceTableName] = count;
				}

				return processedTableDict;
			}
		}

		void LogLsnTimeMappingError()
		{
			var errorMsg = BiMasterState.GetParameter(biConnection, BiConstants.LastEtlErrorMessage, BiDatabaseName);

			logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Error while populating LsnTimeMapping table:\r\n{0}", errorMsg));
		}

		#endregion

		#region Batch Mode

		AuditMasterLoadResult ExecuteMasterAuditLoadInBatch()
		{
			AuditMasterLoadResult result = AuditMasterLoadResult.Success;
			bool initialRun = true;
			bool stop = false;
			bool lostChangesDetected = false;
			bool batchMode = false;
			int batchesExecuted = 0;

			while (!stop)
			{
				ExecuteMasterAuditLoad(resetLoadCount: initialRun, output: ref result, batchMode: ref batchMode);

				if (result == AuditMasterLoadResult.TableConfigurationError ||
					result == AuditMasterLoadResult.NoMaxLsn ||
					result == AuditMasterLoadResult.NoNewTransactions ||
					result == AuditMasterLoadResult.SqlError ||
					!batchMode
					)
				{
					stop = true;
				}

				if (result == AuditMasterLoadResult.NoNewTransactions && !initialRun)
				{
					result = AuditMasterLoadResult.Success;
				}

				if (result == AuditMasterLoadResult.SuccessWithLostChanges)
				{
					lostChangesDetected = true;
				}

				initialRun = false;

				if (batchMode && ((result == AuditMasterLoadResult.Success) || (result == AuditMasterLoadResult.SuccessWithLostChanges)))
				{
					batchesExecuted++;
				}
			}

			if (batchesExecuted > 1 && ((result == AuditMasterLoadResult.Success) || (result == AuditMasterLoadResult.SuccessWithLostChanges)))
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, @"Audit ETL was executed in {0} batches.", batchesExecuted));
			}

			if (result == AuditMasterLoadResult.Success && lostChangesDetected)
			{
				result = AuditMasterLoadResult.SuccessWithLostChanges;
			}

			return result;
		}

		#endregion

		#region Stored Procedure Execution

		#region SuppressResourceStringsCheckRegion

		protected virtual int BatchModeSize
		{
			get
			{
				return 1000000;
			}
		}

		protected virtual float MaxIntervalMinutes
		{
			get
			{
				return 5.0f;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void ExecuteMasterAuditLoad(bool resetLoadCount, ref AuditMasterLoadResult output, ref bool batchMode)
		{
			var query = string.Format(CultureInfo.InvariantCulture, @"[{0}].[{1}].usp_MasterAuditLoad", BiDatabaseName, BiConstants.BiAdminSchemaName);
			using (var cmd = biConnection.Command(query, commandTimeout))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.AddParameter("@server_name", SqlDbType.VarChar, 1000, LinkedServerNameWithinBrackets);
				cmd.AddParameter("@max_interval_min", SqlDbType.Float, MaxIntervalMinutes);
				cmd.AddParameter("@min_trans_count_for_batch_mode", SqlDbType.Int, BatchModeSize);
				cmd.AddParameter("@reset_load_count", SqlDbType.Bit, resetLoadCount);

				cmd.AddOutputParameter("@error_code", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@is_batch_mode", SqlDbType.Int, 0, 0, 0, null);

				cmd.ExecuteNonQuery();

				output = (AuditMasterLoadResult)cmd.GetParameterValue("@error_code");
				batchMode = Convert.ToBoolean(cmd.GetParameterValue("@is_batch_mode"), CultureInfo.InvariantCulture);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RebuildIndexForRequiredTable()
		{
			var sourceTableName = GetSourceTableNameWithCorruptedIndex();
			logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Rebuilding index for table [{0}]", sourceTableName.Replace(".", "].[")));

			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"ALTER INDEX [{0}] ON [{1}] REBUILD PARTITION = ALL;", // SQL query
				"cci_" + sourceTableName.Replace(".", "_"),
				sourceTableName.Replace(".", "].["));
			using (var cmd = biConnection.Command(sqlText, 0))
			{
				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RebuildIndexForCdcHistorySummary()
		{
			logger.Log(LogType.Debug, "Rebuilding index for table CdcHistorySummary");

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"ALTER INDEX [cci_biadmin_CdcHistorySummary] ON [{0}].[{1}].[CdcHistorySummary] REBUILD PARTITION = ALL;", BiDatabaseName, BiConstants.BiAdminSchemaName); // SQL query
			using (var cmd = biConnection.Command(sqlText, 0))
			{
				cmd.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		string GetSourceTableNameWithCorruptedIndex()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT @SourceSchemaName = [SourceSchemaName], @SourceTableName = [SourceTableName] FROM [{0}].[{1}].[TableState] WHERE SqlErrorMessage = 'Index corruption is detected, requires Index Rebuild.'",
				BiDatabaseName, BiConstants.BiAdminSchemaName);

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddOutputParameter("@SourceSchemaName", SqlDbType.VarChar, 128, 0, 0, null);
				cmd.AddOutputParameter("@SourceTableName", SqlDbType.VarChar, 128, 0, 0, null);
				cmd.ExecuteNonQuery();

				var sourceSchemaName = cmd.GetParameterValue("@SourceSchemaName").ToString();
				var sourceTableName = cmd.GetParameterValue("@SourceTableName").ToString();
				if (!string.IsNullOrEmpty(sourceSchemaName) && !string.IsNullOrEmpty(sourceTableName))
				{
					return sourceSchemaName + "." + sourceTableName;
				}
				else
				{
					throw new EtlExecutionException("No table with corrupted index found.");
				}
			}
		}
#if DEBUG
		public
#endif
		static readonly int commandTimeout = (int)TimeSpan.FromHours(6).TotalSeconds;

		#endregion

		#endregion

		#endregion
	}
}
