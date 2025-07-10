using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker
{
	class NewsArchive
	{
		public NewsArchive(IEnumerable<LogSubscriber> logSubscribers)
		{
			this.logSubscribers = logSubscribers.ToArray();
		}
		readonly LogSubscriber[] logSubscribers;

		public void CleanupOldLogs(CancellationToken token, ILogger notifier = null)
		{
			var currentBatch = 1;
			var totalProcessedLogs = 0;
			var totalFailedLogs = 0;
			var totalDeadLogs = 0;

			var currentDateTime = ZDateTime.UtcNow.ToDateTime();
			try
			{
				var processedLogs = 0;
				var failedLogs = 0;
				var deadLogs = 0;
				var iterations = 0;

				using (Db.Connection.TemporarySetDeadlockPriority(DeadlockPriority.Min))
				using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(10)))
				{
					foreach (var subscriber in logSubscribers)
					{
						iterations = 0;
						do
						{
							token.ThrowIfCancellationRequested();
							processedLogs = PurgeOldLogs(currentDateTime.AddHours(-HoursToKeepProcessedLogs), JobQueueStatus.StatusProcessed, subscriber.Name);
							totalProcessedLogs += processedLogs;
							currentBatch++;
						}
						while (processedLogs > 0 && ++iterations < MaximumIterations);
					}

					foreach (var subscriber in logSubscribers)
					{
						iterations = 0;
						do
						{
							token.ThrowIfCancellationRequested();
							failedLogs = PurgeOldLogs(currentDateTime.AddHours(-HoursToKeepFailedLogs), JobQueueStatus.StatusFailed, subscriber.Name);
							totalFailedLogs += failedLogs;
							currentBatch++;
						}
						while (failedLogs > 0 && ++iterations < MaximumIterations);
					}

					if (logSubscribers.Any())
					{
						iterations = 0;
						do
						{
							token.ThrowIfCancellationRequested();
							deadLogs = PurgeDeadLogs(currentDateTime.AddHours(-HoursToKeepDeadLogs));
							totalDeadLogs += deadLogs;
							currentBatch++;
						}
						while (deadLogs > 0 && ++iterations < MaximumIterations);
					}
				}

				notifier?.Log((totalProcessedLogs > 0 || totalFailedLogs > 0 || totalDeadLogs > 0) ? LogType.Information : LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Cleaned:{0} PRS log(s), {1} FAL log(s), {2} dead log(s).", totalProcessedLogs, totalFailedLogs, totalDeadLogs));
			}
			catch (SqlException ex)
			{
				var exceptionType = new DbErrorMatch(ex).ExceptionType;
				if (exceptionType == DbErrorType.LockTimeoutExpired || exceptionType == DbErrorType.DeadlockError)
				{
					if (notifier != null)
					{
						notifier.Warning(ex.Message);
						notifier.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Cleaned:{0} PRS log(s), {1} FAL log(s), {2} dead log(s).", totalProcessedLogs, totalFailedLogs, totalDeadLogs));
						notifier.Log(LogType.Information, $"Lock error occured on batch no {currentBatch} with batch size of {BatchSize}");
					}
				}
				else
				{
					throw;
				}
			}
		}

		int PurgeOldLogs(DateTime purgeDate, string statusToCleanup, string subscriber)
		{
			return Db.Connection.ExecuteScalar<int>(CleanupSqlCommand,
				cmd =>
				{
					cmd.AddParameter("@BatchSize", SqlDbType.Int, BatchSize);
					cmd.AddParameterBasedOnDbColumn("@Subscriber", subscriber, StmJobQueueSchema.SJ_FilterName);
					cmd.AddParameterBasedOnDbColumn("@StatusToCleanup", statusToCleanup, StmJobQueueSchema.SJ_Status);
					cmd.AddParameterBasedOnDbColumn("@DateTimeThreshold", purgeDate, StmJobQueueSchema.SJ_PostedTimeUtc);
				});
		}

		int PurgeDeadLogs(DateTime purgeDate)
		{
			return Db.Connection.ExecuteScalar<int>(CleanupDeadLogsSqlCommand,
				cmd =>
				{
					cmd.AddParameter("@BatchSize", SqlDbType.Int, BatchSize);
					cmd.AddTableValuedParameter("@Subscribers", StmJobQueueSchema.SJ_FilterName, logSubscribers.Select(s => s.Name).ToArray());
					cmd.AddParameterBasedOnDbColumn("@DateTimeThreshold", purgeDate, StmJobQueueSchema.SJ_PostedTimeUtc);
				});
		}

		const string CleanupSqlCommand = @"
DELETE TOP (@BatchSize)
	StmJobQueue WITH (READPAST, READCOMMITTEDLOCK)
WHERE 1=1
	AND SJ_FilterName = @Subscriber
	AND SJ_Status = @StatusToCleanup
	AND SJ_PostedTimeUtc < @DateTimeThreshold

SELECT @@ROWCOUNT
";

		const string CleanupDeadLogsSqlCommand = @"
			DELETE TOP (@BatchSize) StmJobQueue WITH (READPAST, READCOMMITTEDLOCK)
			WHERE SJ_PostedTimeUtc < @DateTimeThreshold
			AND SJ_FilterName NOT IN (SELECT VALUE FROM @Subscribers)
			SELECT @@ROWCOUNT";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int HoursToKeepProcessedLogs => SystemDataRegistry.Instance.LogWalkerClearProcessedLogsAfterHours.Value;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int HoursToKeepFailedLogs => SystemDataRegistry.Instance.LogWalkerClearFailedLogsAfterHours.Value;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int HoursToKeepDeadLogs => 24 * 31 * 3; // Around three months.
		const int MaximumIterations = 1000; //Upperbound to prevent a single subscriber looping infinetely, 1,000 iterations purges up to 1,000,000 logs at the default batch size of 1000 logs
		readonly int BatchSize = SystemDataRegistry.Instance.LogWalkerPurgeBatchSize.Value;
	}
}
