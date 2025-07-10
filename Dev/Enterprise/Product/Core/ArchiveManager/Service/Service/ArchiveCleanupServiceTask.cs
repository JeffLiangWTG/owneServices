using System;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ArchiveManager.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ArchiveManagerConstants))]
[assembly: HostedService(ArchiveManagerConstants.ACLServiceTaskCode,
	"Archive Manager Cleanup",
	"SYS",
	typeof(Enterprise.ArchiveManager.Service.ArchiveCleanupServiceTask),
	MinimumPeriod = "1month",
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "1month",
	DefaultScheduleDayOfMonth = 1,
	ActiveByDefault = true
	)]

namespace Enterprise.ArchiveManager.Service
{
	public class ArchiveCleanupServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description Message")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(Integration.LogType.Information, "Started Archive Manager Cleanup");

			var totalRowsDeleted = 0;

			while (!youMustReactToThisToken.IsCancellationRequested)
			{
				try
				{
					var rowsDeleted = CleanupBatchOfRecords();
					totalRowsDeleted += rowsDeleted;

					if (rowsDeleted == 0)
					{
						if (totalRowsDeleted == 0)
						{
							ServiceLogger.Log(Integration.LogType.Information, "There are no rows to delete from ArchiveMainItemQueue.");
						}
						else
						{
							ServiceLogger.Log(Integration.LogType.Information, "There are no more rows to delete.");
						}

						ServiceLogger.Log(Integration.LogType.Information, "Completed successfully.");
						break;
					}
					else
					{
						ServiceLogger.Log(Integration.LogType.Information, $"Deleted {rowsDeleted} rows from ArchiveMainItemQueue.");
					}
				}
				catch (SqlException ex) when (ContainsSQlExceptionToRetry(ex))
				{
					var message = "A non-fatal sql exception occurred during the running of archive queue cleanup, likely due to contention with archive manager (ARC)." +
						$"To reduce database contention, the cleanup service has been stopped and will re-run later. The exception was:{System.Environment.NewLine}{ex.Message}";

					ErrorReporter.ReportDeveloperExceptionOnce("ArchiveCleanupServiceTaskSQLException", message, ex);
					ServiceLogger.Log(Integration.LogType.Warning, message);

					break;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Operation on Queue Table")]
		protected virtual int CleanupBatchOfRecords()
		{
			const int batchSize = 1000;
			var loadingOrLoadedRecordsLifetime = TimeSpan.FromHours(12);
			var untouchedRecordsLifetime = TimeSpan.FromDays(7);

			var sql = @"DELETE TOP(@BatchSize) FROM dbo.ArchiveMainItemQueue WITH (ROWLOCK) WHERE
(
	AIM_SystemLastEditTimeUtc < DATEADD(HOUR, @UntouchedRecordsLifetimeHours, GETUTCDATE())
	AND (AIM_IsLoading = 0 AND AIM_IsLoaded = 0 AND AIM_IsSkipped = 0)
)
OR
(
	AIM_SystemLastEditTimeUtc < DATEADD(HOUR, @LoadingOrLoadedRecordsLifetimeHours, GETUTCDATE())
	AND NOT (AIM_IsLoading = 0 AND AIM_IsLoaded = 0 AND AIM_IsSkipped = 0)
)";

			using var cmd = Db.Connection.Command(sql);
			using var priority = Db.Connection.TemporarySetDeadlockPriority(DeadlockPriority.Low);

			_ = cmd.AddParameter("@BatchSize", SqlDbType.Int, batchSize);
			_ = cmd.AddParameter("@LoadingOrLoadedRecordsLifetimeHours", SqlDbType.Int, -loadingOrLoadedRecordsLifetime.TotalHours);
			_ = cmd.AddParameter("@UntouchedRecordsLifetimeHours", SqlDbType.Int, -untouchedRecordsLifetime.TotalHours);

			return cmd.ExecuteNonQuery();
		}

		bool ContainsSQlExceptionToRetry(SqlException ex)
		{
			return ex.IsInnermostDeadlock() || ex.IsInnermostLockTimeoutExpired() || ex.IsInnermostTimeoutExpired();
		}
	}
}

