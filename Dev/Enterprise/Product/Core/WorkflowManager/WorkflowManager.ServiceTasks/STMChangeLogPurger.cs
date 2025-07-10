using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class StmChangeLogPurger
	{
		public StmChangeLogPurger(DbConnection connection)
		{
			databaseConnection = connection;
		}

		public void Purge(CancellationToken token)
		{
			try
			{
				var dateTimeToPurgeLogsAfter = DateAndTimeOfLastProcessedLog != DateTime.MinValue ? DateAndTimeOfLastProcessedLog.AddHours(-HoursToKeepProcessedLogs) : (DateTime)SqlDateTime.MinValue;
				int processedLogs = 0, totalProcessedLogs = 0;
				var iterations = 0;

				using (databaseConnection.TemporarySetDeadlockPriority(DeadlockPriority.Min))
				using (databaseConnection.TemporarySetLockTimeout(TimeSpan.FromSeconds(10)))
				{
					do
					{
						token.ThrowIfCancellationRequested();
						processedLogs = purgeOldLogs(dateTimeToPurgeLogsAfter);
						totalProcessedLogs += processedLogs;
					}
					while (processedLogs > 0 && ++iterations < maximumNumberOfIterations);
				}
			}
			catch (SqlException ex)
			{
				var exceptionType = new DbErrorMatch(ex).ExceptionType;
				if (!(exceptionType == DbErrorType.LockTimeoutExpired || exceptionType == DbErrorType.DeadlockError))
				{
					throw;
				}
			}
		}

		int purgeOldLogs(DateTime purgeDate)
		{
			string cleanUpOldLogsSQLCommand = $@"
		DELETE TOP (@RowsToDelete) StmChangeLog WITH (READCOMMITTEDLOCK, READPAST)
		WHERE SY_PostedTimeUtc <= @PurgeDateTimeThreshold
		SELECT @@ROWCOUNT
		";

			return databaseConnection.ExecuteScalar<int>(cleanUpOldLogsSQLCommand,
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@PurgeDateTimeThreshold", purgeDate, StmChangeLogSchema.SY_PostedTimeUtc);
					cmd.AddParameter("@RowsToDelete", SqlDbType.Int, rowDeleteCount);
				});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int HoursToKeepProcessedLogs => SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
		readonly DateTime DateAndTimeOfLastProcessedLog = SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.Value;
		readonly DbConnection databaseConnection;
		const int rowDeleteCount = 5000;
		const int maximumNumberOfIterations = 500; //Prevents the purger from looping infinitely
	}
}
