using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Transformation
{
	public class UpdateScheduledReportTemplateForJobHistoryReport
	{
		public UpdateScheduledReportTemplateForJobHistoryReport()
			: this(new BusinessObjectFactoryProvider(Db.Connection))
		{
		}

		internal UpdateScheduledReportTemplateForJobHistoryReport(BusinessObjectFactoryProvider businessObjectFactoryProvider)
		{
			BusinessObjectFactoryProvider = Argument.NotNull(businessObjectFactoryProvider, nameof(businessObjectFactoryProvider));
		}

		BusinessObjectFactoryProvider BusinessObjectFactoryProvider { get; }

		const string TempTableName = "dbo.ClientTransformTable_WI00769394";
		const int BatchSize = 100;

		public string Description => (NoResString)"Remove Reference to DateRange Filter Field from scheduled reports for Job History Report";

		public void Run(Action<string> logAction, CancellationToken token)
		{
			if (DataUtils.ObjectExists(Db.Connection, TempTableName))
			{
				var continueRunningBatches = true;
				while (continueRunningBatches)
				{
					continueRunningBatches = RunPerBatch(logAction, token);
					token.ThrowIfCancellationRequested();
				}

				RunRemainderIndividually(logAction, token);

				CompleteAndCleanup(logAction);
			}
		}

		bool RunPerBatch(Action<string> logAction, CancellationToken token)
		{
			BusinessObjectFactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
			var factory = BusinessObjectFactoryProvider.Current;
			var reportsToUpdatePks = new DynamicBusinessObjectCollection(factory);
			reportsToUpdatePks.Load($"SELECT TOP ({BatchSize}) JHR_S5_PK FROM {TempTableName} WHERE JHR_FailedBatch = 0");

			var continueProcessing = reportsToUpdatePks.Count > 0;
			if (continueProcessing)
			{
				var modifiedReports = new List<ZGuid>();

				var reportKeys = reportsToUpdatePks.Select(r => (ZGuid)r["JHR_S5_PK"]).ToArray();

				var query = new ZDBOnlyQuery(typeof(ReportScheduleTask));
				query.AddToFilter(StmScheduleTaskSchema.PK, reportKeys);
				var scheduledReportTasks = factory.Load<ReportScheduleTask>(query);

				foreach (var scheduledReportTask in scheduledReportTasks)
				{
					if (token.IsCancellationRequested)
					{
						logAction((NoResString)"Cancellation token was requested. Transformation has been interrupted.");
						token.ThrowIfCancellationRequested();
					}

					if (UpdateReport(scheduledReportTask))
					{
						modifiedReports.Add(scheduledReportTask.PK);
					}
				}

				if (modifiedReports.Count == 0 || SaveFactorySafely(factory, reportKeys, "JHR_FailedBatch"))
				{
					DeletePKsFromTempTable(reportKeys);
				}

				logAction($"Processed {scheduledReportTasks.Length} records.");
			}
			else
			{
				logAction((NoResString)"Completed processing all Report Schedule Task batch records.");
			}

			return continueProcessing;
		}

		void RunRemainderIndividually(Action<string> logAction, CancellationToken token)
		{
			var entriesToProcess = Db.Connection.ExecuteScalar<int>($"SELECT Count(*) FROM {TempTableName}");

			if (entriesToProcess > 0)
			{
				logAction($"{entriesToProcess} records could not be processed in Batch. Will attempt to process individually.");

				var continueRunning = true;
				while (continueRunning)
				{
					if (token.IsCancellationRequested)
					{
						logAction((NoResString)"Cancellation token was requested. Transformation has been interrupted.");
						token.ThrowIfCancellationRequested();
					}

					BusinessObjectFactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
					var factory = BusinessObjectFactoryProvider.Current;

					var queryResult = Db.Connection.ExecuteScalar($"SELECT TOP 1 JHR_S5_PK FROM {TempTableName} WHERE JHR_FailedIndividually = 0");
					continueRunning = queryResult != null;
					if (continueRunning)
					{
						var reportToUpdatePK = (Guid)queryResult;
						continueRunning = reportToUpdatePK != Guid.Empty;

						if (continueRunning)
						{
							var scheduledReportTask = factory.Load<ReportScheduleTask>(reportToUpdatePK);

							var reportUpdated = UpdateReport(scheduledReportTask);
							if (!reportUpdated || SaveFactorySafely(factory, new[] { new ZGuid(reportToUpdatePK) }, "JHR_FailedIndividually"))
							{
								DeletePKsFromTempTable(new[] { new ZGuid(reportToUpdatePK) });
							}

							logAction($"Individual Record Processed. {--entriesToProcess} record(s) remaining.");
						}
					}
				}
			}
		}

		void DeletePKsFromTempTable(ZGuid[] reportKeysToDelete)
		{
			var deleteSQL = $"DELETE {TempTableName} WHERE JHR_S5_PK IN (SELECT Value FROM @ReportPKsToDelete)";
			Db.Connection.ExecuteNonQuery(deleteSQL, (cmd) =>
			{
				cmd.AddTableValuedParameter("@ReportPKsToDelete", StmScheduleTaskSchema.PK, reportKeysToDelete);
			});
		}

		void CompleteAndCleanup(Action<string> logAction)
		{
			BusinessObjectFactoryProvider.CreateNewAndReclaimMemoryWithoutSave();
			var failedReports = new DynamicBusinessObjectCollection(BusinessObjectFactoryProvider.Current);
			failedReports.Load($"SELECT JHR_S5_PK FROM {TempTableName} WHERE JHR_FailedBatch = 1 AND JHR_FailedIndividually = 1");

			if (failedReports.Count > 0)
			{
				logAction($"{failedReports.Count} Report Schedule Task failed to be processed.");

				var failedPKs = string.Join("\r\n", failedReports.Select(r => (ZGuid)r["JHR_S5_PK"]));
				var errorMessage = $@"Error processing scheduled report. The following ReportScheduleTask PK's could not be processed:
{failedPKs}";
				ErrorReporter.ReportOnce("ac88591a-fb75-4740-a2dd-9f22b124b0de", errorMessage);
			}
			else
			{
				logAction($"Successfully processed all Report Schedule Tasks.");
			}

			Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, (NoResString)"DROP TABLE {0}", TempTableName));
		}

		bool SaveFactorySafely(BusinessObjectFactory factory, IEnumerable<ZGuid> reportPKs, string columnType)
		{
			var isSuccessfulSave = false;
			try
			{
				factory.Save();

				isSuccessfulSave = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Db.Connection.ExecuteNonQuery(
					string.Format(CultureInfo.InvariantCulture, (NoResString)"UPDATE {0} SET {1}= 1 WHERE JHR_S5_PK IN ('{2}')",
						TempTableName,
						columnType,
						string.Join("','", reportPKs)));
			}

			return isSuccessfulSave;
		}

		bool UpdateReport(ReportScheduleTask scheduledReportTask)
		{
			var isModified = false;

			if (scheduledReportTask != null)
			{
				try
				{
					var deserialisedReportInfo = ScheduledReportHelper.DeserializeStreamToReportSerializationInfo(scheduledReportTask.S5_ScheduleState);
					var report = deserialisedReportInfo?.Report;
					if (report != null)
					{
						var dateRangeFilter = report.FilterCollection.OfType<DateRangeField>().FirstOrDefault(f => !string.IsNullOrEmpty(f.FieldName));
						if (dateRangeFilter != null)
						{
							isModified = true;

							dateRangeFilter.FieldName = string.Empty;

							//To avoid empty ReportID exception when serializing
							report.ColumnHeadingManager.OverrideHeadingsSerialization(report.ColumnHeadingManager.headingsxml);
							var jsonResult = JsonConverterHelper.Serialize(deserialisedReportInfo);
							scheduledReportTask.S5_ScheduleState = Encoding.UTF8.GetBytes(jsonResult);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					isModified = false;
					ErrorReporter.ReportOnce("1e099933-f09b-4c83-9e37-5c35e8f0e302", ex.Message, ex);
				}
			}

			return isModified;
		}
	}
}
