using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ServiceTasks
{
	public class JCDDefaultActionStrategy : JCDActionStrategy
	{
		public JCDDefaultActionStrategy(DbConnection connection, ILogger logger) : base(connection, logger)
		{
		}

		protected override bool CanPerform()
		{
			return AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed
					|| AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.ProcessingOldTransactionLines;
		}

		protected override void ProcessData()
		{
			isNudgeRequired = false;

			if (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.ProcessingOldTransactionLines)
			{
				//Check whether all old AL records have been processed, if not then process it
				PopulateQueueFromOldALRecords();
			}
			else if (AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed)
			{
				PopulateReportDataTable();
			}
		}

		protected override string GetCannotPerformMessage()
		{
			string msg = (NoResString)"All JCD related DB Objects haven't been created yet.";
			msg += FormattableString.Invariant($"\r\n Job Costing Data Queue Service Task Controller Registry Value: {AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value}");
			msg += FormattableString.Invariant($"\r\n Missing DB Object Names: {DBObjectChecker.GetMissingDBObjectNames()}");
			return msg;
		}

		#region Queue Population related Functions

		void PopulateQueueFromOldALRecords()
		{
			logger.Log(LogType.Debug, "Populating Job Costing Report Data Queue table for existing transactions.");

			var batchSize = AccountingConfigurationRegistry.Instance.TransactionLineToJobCostingRecordTransformationBatchSize.Value;
			long start = AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.Value;
			long end = start + batchSize - 1;
			long rowLeftToProcess = 1;

			while (rowLeftToProcess > 0)
			{
				BackLogWaiterChecker();

				using (var manager = connection.BeginTransactionWithManager())
				{
					try
					{
						rowLeftToProcess = PopulateJobCostingQueue(connection, start, end);
						manager.CommitTransaction();

						if (rowLeftToProcess > 0)
						{
							logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Transformed record # {0} to {1}. There are {2} more record(s) to be transformed.", start, end, rowLeftToProcess));

							AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)(end + 1));
							start = AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.Value;
							end = start + (rowLeftToProcess < batchSize ? rowLeftToProcess : batchSize) - 1;
						}
						else
						{
							logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Transformed record # {0} to {1}. There are no more records to be transformed.", start, end));

							var maxRowNumber = connection.ExecuteScalar<long>("SELECT ISNULL(MAX(UL_RowNumber), 0) FROM RptDtUnprocessedAccTransactionLines");
							AccountingConfigurationRegistry.Instance.JCDQueueHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)maxRowNumber + 1);
							AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed);

							//Check whether any temporary Tables/Functions/Procedures exists, if so then drop those.
							CheckAndDropTemporaryDBObjects();

							isNudgeRequired = true;
						}
					}
					catch (Exception ex)
					{
						var result = CanRetryTheAction(ex, (NoResString)"Failed to load data into the JobCostingDataQueue table.");
						if (result.retry)
						{
							NudgeAfterSqlException();
						}
						else if (result.throwTheOriginalException)
						{
							throw;
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected virtual long PopulateJobCostingQueue(DbConnection connection, long startRowNumber, long endRowNumber)
		{
			long numberOfUnprocessedRows = 0;
			using (var cmd = connection.Command("RptDtTransformAccTransactionLineToJobCostingQueueRecord"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@startRowNumber", SqlDbType.BigInt, startRowNumber);
				cmd.AddParameter("@endRowNumber", SqlDbType.BigInt, endRowNumber);
				cmd.AddOutputParameter("@rowLeft", SqlDbType.BigInt, 0, 0, 0, 0);
				cmd.ExecuteNonQuery();

				var val = cmd.GetParameterValue("@rowLeft");
				numberOfUnprocessedRows = Convert.ToInt64(val == DBNull.Value ? 0 : val, CultureInfo.InvariantCulture);
			}
			return numberOfUnprocessedRows;
		}

		void CheckAndDropTemporaryDBObjects()
		{
			if (DBObjectChecker.DoesAnyTempJCDDBObjectExist())
			{
				DropTemporaryDBObjects();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message. Not displayed in UI")]
		void DropTemporaryDBObjects()
		{
			var valueHolder = GetValueHolderForRegitryItemsThatRelatedToTempJCDDBObjects();

			var isDropped = false;
			using (var manager = connection.BeginTransactionWithManager())
			{
				try
				{
					new JCDDependentObjectSynchronizer(connection, logger, JCDTempFunctionList.GetVersionManager()).DropObjects(0);

					JCDTableAndPartitionCreator.DeleteTempTableAndIndex(connection, (logText) => logger.Log(LogType.Debug, logText));

					manager.CommitTransaction();

					ResetRegitryItemsThatRelatedToTempJCDDBObjects();

					isDropped = true;
				}
				catch (Exception ex)
				{
					var result = CanRetryTheAction(ex, (NoResString)"Failed to drop temp tables and triggers.");
					if (result.retry)
					{
						NudgeAfterSqlException();
					}
					else if (result.throwTheOriginalException)
					{
						throw;
					}
				}
			}

			if (!isDropped)
			{
				valueHolder.Rollback();
			}
		}

		#endregion

		#region Report Data Populaiton related Functions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message. Not Displayed in an UI, Log Message. Not displayed in UI")]
		protected virtual void PopulateReportDataTable()
		{
			logger.Log(LogType.Debug, "Populating JobCostingData Table");

			using (var manager = connection.BeginTransactionWithManager())
			{
				try
				{
					BackLogWaiterChecker();

					int numberOfLoadedRows = PopulateJobCostingDataFromQueue();

					manager.CommitTransaction();

					LogResultAndNudgeIfRequired(numberOfLoadedRows);
				}
				catch (Exception ex)
				{
					var result = CanRetryTheAction(ex, (NoResString)"Failed to load data into RptDtJobCostingData table from queue.");
					if (result.retry)
					{
						NudgeAfterSqlException();
					}
					else if (result.throwTheOriginalException)
					{
						throw;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		int PopulateJobCostingDataFromQueue()
		{
			int numberOfLoadedRows = 0;
			using (var cmd = connection.Command("RptDtPopulateJobCostingDataFromQueue"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@BatchSize", System.Data.SqlDbType.Int, AccountingConfigurationRegistry.Instance.JobCostingQueueProcessBatchSize.Value);
				cmd.AddParameter("@StartingPeriod", System.Data.SqlDbType.Int, AccountingConfigurationRegistry.Instance.JCDReportDataCollectionStartingPeriod.Value);
				numberOfLoadedRows = cmd.ExecuteProcedureWithReturnValue();
			}
			return numberOfLoadedRows;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message. Not Displayed in an U, Executing Query., Log Message. Not Displayed in an UI")]
		void LogResultAndNudgeIfRequired(int numberOfLoadedRows)
		{
			if (numberOfLoadedRows > 0)
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Completed processing {0} records. Trying to Nudge this service task as there are more records to process.", numberOfLoadedRows));
				isNudgeRequired = true;
			}
			else
			{
				var rowExists = connection.Exists("FROM dbo.JobCostingDataQueue");

				if (rowExists)
				{
					logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "There are records in the Queue. But none of them are eligible to be processed as accounting Periods are missing for \n {0}.", GetMissingAccPeriodYears()));
				}
				else
				{
					logger.Log(LogType.Information, "Completed processing all records.");
				}
			}
		}

		string GetMissingAccPeriodYears()
		{
			var result = string.Empty;

			var sql = @"SELECT JCQ_GCPK, GC_Code, MissingYears
FROM
(
	SELECT TOP 10000 YEAR(JCQ_PostDate) as MissingYears, AM_Period, JCS.JCQ_GCPK
	FROM	dbo.JobCostingDataQueue JCS 
			LEFT JOIN dbo.AccPeriodManagement  AM_PostDate ON AM_PostDate.AM_GC_Company = JCS.JCQ_GCPK AND JCS.JCQ_PostDate between AM_PostDate.AM_StartDate AND AM_PostDate.AM_EndDate		

	UNION

	SELECT TOP 10000 YEAR(JCQ_ReverseDate) as MissingYears, AM_Period, JCS.JCQ_GCPK
	FROM	dbo.JobCostingDataQueue JCS		
			LEFT JOIN dbo.AccPeriodManagement  AM_ReverseDate ON AM_ReverseDate.AM_GC_Company = JCS.JCQ_GCPK AND JCS.JCQ_ReverseDate between AM_ReverseDate.AM_StartDate AND AM_ReverseDate.AM_EndDate
)a
INNER JOIN dbo.GlbCompany GC ON GC.GC_PK = a.JCQ_GCPK
WHERE MissingYears IS NOT NULL AND AM_Period IS NULL
GROUP BY JCQ_GCPK, GC_Code, MissingYears
ORDER BY GC_Code, MissingYears DESC";

			var dt = DataUtils.GetDataTableFromQuery(connection, sql);

			if (dt != null && dt.Rows.Count > 0)
			{
				result = string.Join("\n", dt.Select().Select(r => string.Format(CultureInfo.InvariantCulture, "{0} --> {1}", r["GC_Code"], r["MissingYears"])));
			}

			return result;
		}

		#endregion

		#region Nudging

		protected override bool IsNudgingRequired
		{
			get { return base.IsNudgingRequired || isNudgeRequired; }
		}
		bool isNudgeRequired;

		#endregion
	}
}
