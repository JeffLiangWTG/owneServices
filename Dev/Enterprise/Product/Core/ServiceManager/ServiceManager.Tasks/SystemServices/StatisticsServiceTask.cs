using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.SystemServices;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("STC",
	"Statistics aggregator",
	"SYS",
	typeof(StatisticsServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "1week",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "12hours"
	)]

namespace Enterprise.ServiceManager.Tasks.SystemServices
{
	public class StatisticsServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Step 1/7: Summarising usage statistics (chunk: {0:N0} rows)", SystemDataRegistry.Instance.StatisticsTopRowCountForSummarise.Value));
			CreateStatistics(DbCommand.Timeout.Infinite);

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Step 2/7: Removing data older than {0} days", SystemDataRegistry.Instance.StatisticsMinimumRetentionRawDataDays.Value));
			DeleteOldStmUsage(DbCommand.Timeout.Infinite);

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Step 3/7: Removing Action SubName information older than {0} days", SystemDataRegistry.Instance.StatisticsRemoveModuleIdAfterDays.Value));
			AggregateSubName(DbCommand.Timeout.Infinite);

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Step 4/7: Removing Staff information older than {0} days", SystemDataRegistry.Instance.StatisticsRemoveStaffAfterDays.Value));
			AggregateStaff(DbCommand.Timeout.Infinite);

			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Step 5/7: Removing Branch information older than {0} days", SystemDataRegistry.Instance.StatisticsRemoveBranchAfterDays.Value));
			AggregateBranch(DbCommand.Timeout.Infinite);

			ServiceLogger.Log(LogType.Information, "Step 6/7: Aggregating statistics by Period");
			AggregatePeriod(DbCommand.Timeout.Infinite);

			ServiceLogger.Log(LogType.Information, "Step 7/7: Aggregating complete");
		}

		#region Implementation

#if DEBUG
		protected
#endif
		void CreateStatistics(int? commandTimeout)
		{
			var watch = new Stopwatch();
			watch.Start();

			var topRowCount = SystemDataRegistry.Instance.StatisticsTopRowCountForSummarise.Value;

			ServiceLogger.Log(LogType.Information, FormattableString.Invariant($".   backlog: {QueueBacklog:N0} rows"));

			var performedRows = topRowCount;
			while (canContinue && performedRows == topRowCount)
			{
				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				using (var cmd = Db.Connection.Command("StatisticsServiceSummarise", commandTimeout))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@TopRowCount", SqlDbType.BigInt, topRowCount);

					performedRows = (int)cmd.ExecuteScalar();
					transactionManager.CommitTransaction();
				}

#if DEBUG
				DoCount_ForTest();
#endif

				if (watch.Elapsed >= TimeSpan.FromMinutes(1))
				{
					ServiceLogger.Log(LogType.Information, FormattableString.Invariant($".   backlog: {QueueBacklog:N0} rows"));
					watch.Restart();
				}
			}

			watch.Stop();
		}

		long QueueBacklog => Db.Connection.ExecuteScalar<long>("SELECT rows = SUM(rows) FROM sys.partitions WHERE object_id = OBJECT_ID(N'dbo.StmUsageQueue', N'U') AND index_id in (0, 1)");

#if DEBUG
		protected
#endif
		void DeleteOldStmUsage(int? commandTimeout)
		{
			var topRowCount = SystemDataRegistry.Instance.StatisticsTopRowCountForDelete.Value;
			var minimumRetentionDays = SystemDataRegistry.Instance.StatisticsMinimumRetentionRawDataDays.Value;

			// Using a Temp table here to avoid deadlock with PerformanceStatisticsPersister
			var sql = @"
IF OBJECT_ID('tempdb..#StmUsage_ToBeDeleted', 'U') IS NOT NULL
	DROP TABLE #StmUsage_ToBeDeleted;

SELECT TOP(@TopRowCount) XW_PK
INTO #StmUsage_ToBeDeleted
FROM dbo.StmUsage WITH (READPAST, READCOMMITTEDLOCK)
	LEFT JOIN dbo.StmUsageQueue
		ON XW_PK = XI_XW
WHERE 1=1
    AND XW_EndTimeUtc < @DeleteDataBefore
	AND XI_XW IS NULL
ORDER BY
    XW_EndTimeUtc;

DELETE dbo.StmUsage WITH (ROWLOCK)
WHERE XW_PK IN (SELECT XW_PK FROM #StmUsage_ToBeDeleted);

SELECT @@ROWCOUNT;

DROP TABLE #StmUsage_ToBeDeleted;
";

			var maxRetryCount = 3;
			var retryCount = 0;
			var performedRows = topRowCount;
			while (canContinue && performedRows == topRowCount)
			{
				using (var transaction = Db.Connection.BeginTransactionWithManager())
				using (var cmd = Db.Connection.Command(sql, commandTimeout))
				{
					cmd.AddParameter("@TopRowCount", SqlDbType.BigInt, topRowCount);
					cmd.AddParameter("@DeleteDataBefore", SqlDbType.DateTime, ZDateTime.UtcNow.AddDays(-minimumRetentionDays).ToDateTime());

					try
					{
						performedRows = (int)cmd.ExecuteScalar();
						transaction.CommitTransaction();
					}
					catch (SqlException ex)
					{
						var baseSqlException = ex.GetBaseException() as SqlException;
						var errorMatch = new DbErrorMatch(baseSqlException);
						if (errorMatch.ExceptionType == DbErrorType.DeadlockError)
						{
							if (++retryCount > maxRetryCount)
							{
								// max try count exceeded => skip this step and go to the next one
								break;
							}

							var message = @"Step 2/7: Retry. Removing old data was blocked by another process due to high user activity.
If this happens frequently, we recommend to choose more appropriate time to run this service task.";
							ServiceLogger.Log(LogType.Warning, message);

							Thread.Sleep(TimeSpan.FromMinutes(1));
						}
						else
						{
							throw;
						}
					}

#if DEBUG
					DoCount_ForTest();
#endif
				}
			}
		}

		void Aggregate(int? commandTimeout, int minimumRetentionDays, string dbCommandName)
		{
			if (canContinue)
			{
				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				using (var cmd = Db.Connection.Command(dbCommandName, commandTimeout))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@date_start", SqlDbType.SmallDateTime, ZDateTime.UtcNow.AddDays(-minimumRetentionDays).ToSmallDateTime().ToDateTime());

					cmd.ExecuteNonQuery();
					transactionManager.CommitTransaction();
				}
			}
		}

#if DEBUG
		protected
#endif
		void AggregateSubName(int? commandTimeout)
		{
			Aggregate(commandTimeout, SystemDataRegistry.Instance.StatisticsRemoveModuleIdAfterDays.Value, "StatisticsServiceAggregateSubName");
		}

#if DEBUG
		protected
#endif
		void AggregateStaff(int? commandTimeout)
		{
			Aggregate(commandTimeout, SystemDataRegistry.Instance.StatisticsRemoveStaffAfterDays.Value, "StatisticsServiceAggregateStaff");
		}

#if DEBUG
		protected
#endif
		void AggregateBranch(int? commandTimeout)
		{
			Aggregate(commandTimeout, SystemDataRegistry.Instance.StatisticsRemoveBranchAfterDays.Value, "StatisticsServiceAggregateBranch");
		}

#if DEBUG
		protected
#endif
		void AggregatePeriod(int? commandTimeout)
		{
			var now = ZDateTime.UtcNow.ToSmallDateTime();
			AlterPeriodFunction(now, DbCommand.Timeout.Default);

			if (canContinue)
			{
				var date_start = SystemDataRegistry.Instance.StatisticsAggregationParameters.Value
					.Cast<StatisticsFoldupInfo>()
					.Max(rule => rule.GetWaitDate(now));

				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				using (var cmd = Db.Connection.Command("StatisticsServiceAggregatePeriod", commandTimeout))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@date_start", SqlDbType.SmallDateTime, date_start.ToDateTime());
					cmd.AddParameter("@now", SqlDbType.SmallDateTime, now.ToDateTime());

					cmd.ExecuteNonQuery();
					transactionManager.CommitTransaction();
				}
			}
		}

#if DEBUG
		protected
#endif
		void AlterPeriodFunction(ZDateTime now, int? commandTimeout)
		{
			if (canContinue)
			{
				var rules = string.Join(System.Environment.NewLine, SystemDataRegistry.Instance.StatisticsAggregationParameters.Value
					.Cast<StatisticsFoldupInfo>()
					.OrderBy(r => r.GetWaitDate(now))
					.Select(rule =>
						string.Format(@"			WHEN @date < DATEADD({0,-7}, -{1,3}, @now) THEN count_{2}{3}",
							rule.WaitScale, rule.WaitAmount,
							rule.AggregateScale, rule.AggregateAmount > 1 ? string.Format(" / {0}", rule.AggregateAmount) : "")));

				var alter_function_sql = string.Format(@"ALTER FUNCTION StatisticsPeriodInline
(
	@now  smalldatetime,
	@date smalldatetime
)

RETURNS TABLE
AS
RETURN

WITH Counts AS
(
	SELECT
		count_Year    = DATEDIFF(YEAR,    0, @date),
		count_Quarter = DATEDIFF(QUARTER, 0, @date),
		count_Month   = DATEDIFF(MONTH,   0, @date),
		count_Day     = DATEDIFF(DAY,     0, @date),
		count_Hour    = DATEDIFF(HOUR,    0, @date),
		count_Minute  = DATEDIFF(MINUTE,  0, @date)
)
SELECT
	calc_period =
		CASE
{0}
			ELSE count_Minute
		END
FROM
	Counts
", rules);
				using (var connection = Db.NewAdminConnection())
				using (var cmd = connection.Command(alter_function_sql, commandTimeout))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

#if DEBUG
		public void CreateStatistics_ForTest()
		{
			CreateStatistics(DbCommand.Timeout.Default);
		}

		public void DeleteOldStmUsage_ForTest()
		{
			DeleteOldStmUsage(DbCommand.Timeout.Default);
		}

		public void AggregateSubName_ForTest()
		{
			AggregateSubName(DbCommand.Timeout.Default);
		}

		public void AggregateBranch_ForTest()
		{
			AggregateBranch(DbCommand.Timeout.Default);
		}

		public void AggregateStaff_ForTest()
		{
			AggregateStaff(DbCommand.Timeout.Default);
		}

		public void AggregatePeriod_ForTest()
		{
			AggregatePeriod(DbCommand.Timeout.Default);
		}

		public void AlterPeriodFunction_ForTest(ZDateTime now)
		{
			AlterPeriodFunction(now, DbCommand.Timeout.Default);
		}

		protected virtual void DoCount_ForTest()
		{
			CallCount_ForTest++;
		}

		internal int CallCount_ForTest;
#endif

		#region IInteruptibleServiceTask Members

		public void Stop()
		{
			canContinue = false;
		}

		bool canContinue = true;

		#endregion // IInteruptibleServiceTask Members

		#endregion // Implementation
	}
}
