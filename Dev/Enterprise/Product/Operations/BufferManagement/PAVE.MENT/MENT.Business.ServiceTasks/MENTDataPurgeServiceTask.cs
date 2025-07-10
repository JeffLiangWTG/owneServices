using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.PAVE.MENT.Business.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MENTDataPurgeServiceTask.Code,
	MENTDataPurgeServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(MENTDataPurgeServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.PAVE.MENT.Business.ServiceTasks
{
	public class MENTDataPurgeServiceTask : BMSServiceTaskBase
	{
		public const string Code = BMConstants.MENTDataPurgeServiceTaskCode; // Service Task Code
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "MENT Data Purge";

		#region Implementation

		protected override void RunTaskCore(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var factory = new BusinessObjectFactory
			{
				NameForDebugging = "MENTDataPurgeServiceTaskFactory"
			};

			var purgeQuery = GetPurgeQuery();
			var activeMENTQueries = factory.Load<MENTAgedScoreQuery>(purgeQuery);

			if (activeMENTQueries.Any())
			{
				ProcessInBatches(token, activeMENTQueries);
			}
			else
			{
				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "{0}|{1} completed run with no purge-ready records, of {2} records found.", Code, Description, activeMENTQueries.Length));
			}
		}

		#endregion

		#region Processing Methods

		void ProcessInBatches(CancellationToken token, MENTAgedScoreQuery[] activeMENTQueries)
		{
			try
			{
				ProcessInBatches_PurgeAllButLatest(token, activeMENTQueries.Where(query => query.MAQ_PurgeAllButLatestQuantity > 0).ToArray());
				ProcessInBatches_PurgeByPurgeDays(token, activeMENTQueries.Where(query => query.MAQ_PurgeDays > 0).ToArray());
			}
			catch (SqlException ex)
			{
				ServiceLogger.Log(LogType.Error, "An error has occurred purging records", ex);
				throw;
			}
		}

		void ProcessInBatches_PurgeAllButLatest(CancellationToken token, MENTAgedScoreQuery[] queries)
		{
			foreach (var query in queries)
			{
				var processedCount = ProcessPurgeInBatchesAndGetProcessed(
					token,
					GetQueryToPurgeAllButLatest(query.MAQ_Code, query.MAQ_PurgeAllButLatestQuantity));

				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "{0}|{1}: Purged {2} records from {3} by PurgeAllButLatest Quantity.", Code, Description, processedCount, query.MAQ_Code));
			}
		}

		void ProcessInBatches_PurgeByPurgeDays(CancellationToken token, MENTAgedScoreQuery[] queries)
		{
			foreach (var query in queries)
			{
				var purgeDate = ZDateTime.UtcToday.AddDays(-1 * query.MAQ_PurgeDays).ToDateTime();

				var processedCount = ProcessPurgeInBatchesAndGetProcessed(
					token,
					GetQueryToPurgeByPurgeDays(query.MAQ_Code, purgeDate));

				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "{0}|{1}: Purged {2} records from {3} by Purge Days.", Code, Description, processedCount, query.MAQ_Code));
			}
		}

		int ProcessPurgeInBatchesAndGetProcessed(CancellationToken token, string queryToProcess)
		{
			bool shouldProcessNextBatch(int lastProcessedCount) => lastProcessedCount == BatchLimit;
			int processOnePurgeBatch() => (int)Db.Connection.ExecuteScalar(queryToProcess); // We can't get this value with Factory or ZQuery

			token.ThrowIfCancellationRequested();
			return ProcessInGenericBatches(token, processOnePurgeBatch, shouldProcessNextBatch);
		}

		#endregion

		#region Queries

		ZQuery GetPurgeQuery()
		{
			var getActiveQueriesToPurgeQuery = new ZQuery(MENTAgedScoreQuerySchema.MAQ_IsActive, true);
			var nonEmptyPurgeSubquery = new ZQuery(MENTAgedScoreQuerySchema.MAQ_PurgeDays, SQLComparisonOperator.NotEqual, 0);
			nonEmptyPurgeSubquery.AddToFilter(JoinCondition.Or, MENTAgedScoreQuerySchema.MAQ_PurgeAllButLatestQuantity, SQLComparisonOperator.NotEqual, 0);
			getActiveQueriesToPurgeQuery.AddToFilter(nonEmptyPurgeSubquery);

			return getActiveQueriesToPurgeQuery;
		}

		string GetQueryToPurgeAllButLatest(string code, int purgeLatestQuantity)
		{
			return FormattableString.Invariant($@"
DELETE TOP ({BatchLimit}) FROM {MENTAgedScoreMetricSchema.Constants.TableName}
WHERE {MENTAgedScoreMetricSchema.PK.Name} = '{code}'
AND {MENTAgedScoreMetricSchema.MAS_TimeRecordedUtc.Name} NOT IN
(
	SELECT TOP ({purgeLatestQuantity}) {MENTAgedScoreMetricSchema.MAS_TimeRecordedUtc.Name}
	FROM {MENTAgedScoreMetricSchema.Constants.TableName}
	WHERE {MENTAgedScoreMetricSchema.PK.Name} = '{code}'
	GROUP BY {MENTAgedScoreMetricSchema.MAS_TimeRecordedUtc.Name}
	ORDER BY {MENTAgedScoreMetricSchema.MAS_TimeRecordedUtc.Name} DESC
)
SELECT @@ROWCOUNT"); // This is direct SQL
		}

		string GetQueryToPurgeByPurgeDays(string code, ZDateTime purgeDate)
		{
			return FormattableString.Invariant($@"
DELETE TOP ({BatchLimit}) FROM {MENTAgedScoreMetricSchema.Constants.TableName}
WHERE {MENTAgedScoreMetricSchema.PK.Name} = '{code}'
AND {MENTAgedScoreMetricSchema.MAS_TimeRecordedUtc.Name} < '{purgeDate}'
SELECT @@ROWCOUNT"); // This is direct SQL
		}

		#endregion

		protected virtual int BatchLimit { get => 100000; }

		protected override string TaskDescription => Description;

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckBufferManagementWorkflowModeOrBetterEnabledCore();
		}

#if DEBUG

		protected void RunTaskCore()
		{
			RunTaskCore(CancellationToken.None);
		}

#endif
	}
}
