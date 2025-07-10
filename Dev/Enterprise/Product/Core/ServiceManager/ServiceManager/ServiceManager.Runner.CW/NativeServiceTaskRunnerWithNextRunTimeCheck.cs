using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner;

public class NativeServiceTaskRunnerWithNextRunTimeCheck : IServiceTaskRunnerWithNextRunTimeCheck
{
	public NativeServiceTaskRunnerWithNextRunTimeCheck(IServiceTaskRunner serviceTaskRunner, IRunnerLogger runnerLogger)
	{
		this.serviceTaskRunner = serviceTaskRunner ?? throw new ArgumentNullException(nameof(serviceTaskRunner));
		this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
	}

	// Assumption: scheduledRunCommandInfo.NextRunTime and scheduledRunCommandInfo.ExpectedNextRunTime are UTC DateTime.
	NextRunTimeResult CheckNextRunTimeForTaskWithLockedTransaction(IScheduledRunCommandInfo scheduledRunCommandInfo)
	{
		var results = new List<(bool matches, DateTime nextRunTime)>();
		using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromMilliseconds(10)))
		{
			Db.Connection.ExecuteReader($@"
DECLARE @OutputResult TABLE (OutputMatches BIT, OutputNextRunTime DATETIMEOFFSET);
DECLARE @nextRunTimeOffset DATETIMEOFFSET(0) = ToDateTimeOffset(@nextRunTime, '+00:00');
DECLARE @expectedNextRunTimeOffset DATETIMEOFFSET(0) = ToDateTimeOffset(@expectedNextRunTime, '+00:00');
;WITH
	source AS
	(
		SELECT
			Matches     = IIF({StmServiceTaskSchema.Constants.SST_NextRunTime} <= @expectedNextRunTimeOffset, 1, 0),
			NextRunTime = {StmServiceTaskSchema.Constants.SST_NextRunTime},
			SystemLastEditTimeUtc = {StmServiceTaskSchema.Constants.SST_SystemLastEditTimeUtc},
			SystemLastEditUser = {StmServiceTaskSchema.Constants.SST_SystemLastEditUser}
		FROM
			{StmServiceTaskSchema.Constants.TableName} WITH (ROWLOCK, UPDLOCK)
		WHERE
			{StmServiceTaskSchema.Constants.SST_ServiceTaskCode} = @scheduleType
	)
UPDATE
	source
SET
	NextRunTime = IIF(Matches > 0, @nextRunTimeOffset, NextRunTime),
	SystemLastEditTimeUtc = IIF(Matches > 0, GetUtcDate(), SystemLastEditTimeUtc),
	SystemLastEditUser = IIF(Matches > 0, '~BP', SystemLastEditUser)
OUTPUT
	deleted.Matches, deleted.NextRunTime
INTO @OutputResult (OutputMatches, OutputNextRunTime);

SELECT
	OutputMatches,
	OutputNextRunTime
FROM
	@OutputResult;
",
				command =>
				{
					command.AddParameter("@nextRunTime", SqlDbType.DateTime, TruncateMilliSeconds(scheduledRunCommandInfo.NextRunTime));
					command.AddParameter("@scheduleType", SqlDbType.VarChar, 3, scheduledRunCommandInfo.Code);
					command.AddParameter("@expectedNextRunTime", SqlDbType.DateTime, TruncateMilliSeconds(scheduledRunCommandInfo.ExpectedNextRunTime));
				},
				reader =>
				{
					var matches = Convert.ToBoolean(reader["OutputMatches"]);
					var nextRunTime = (DateTimeOffset)reader["OutputNextRunTime"];
					results.Add((matches, nextRunTime.UtcDateTime));
				});
		}

		return results.Count > 0
			? new NextRunTimeResult(!results[0].matches, results[0].nextRunTime)
			: NextRunTimeResult.Empty;

		static DateTime TruncateMilliSeconds(DateTime originalDateTime) => new (originalDateTime.Year, originalDateTime.Month, originalDateTime.Day, originalDateTime.Hour, originalDateTime.Minute, originalDateTime.Second, originalDateTime.Kind);
	}

	NextRunTimeResult UpdateNextTimeWithRetry(IScheduledRunCommandInfo scheduledRunCommandInfo)
	{
		try
		{
			var retryPolicy = new RetryPolicy(new ErrorDetectionStrategy(runnerLogger, scheduledRunCommandInfo), ErrorDetectionStrategy.RetryCount - 1, TimeSpan.FromSeconds(1));
			return retryPolicy.ExecuteAction(() =>
				CheckNextRunTimeForTaskWithLockedTransaction(scheduledRunCommandInfo));
		}
		catch (Exception exception) when (ErrorDetectionStrategy.IsTransientException(exception))
		{
			throw new CouldNotReadNextRunTimeException(exception);
		}
	}

	public ServiceTaskRunResult RunServiceTask(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)
	{
		if (runCommandInfo is IScheduledRunCommandInfo scheduledRunCommandInfo)
		{
			var result = UpdateNextTimeWithRetry(scheduledRunCommandInfo);

			if (!result.NoTaskFound && result.WasSkipped)
			{
				runnerLogger.Log(LogLevel.Information, $"Task run is skipped, due to current next run time [{result.NextRunTime.ToString(LogStringFormats.LoggerTimeMaskWithoutMilliseconds)}] does not match to an expected one", scheduledRunCommandInfo);
				return ServiceTaskRunResult.Success;
			}
		}

		return serviceTaskRunner.RunServiceTask(runCommandInfo, cancellationTokenSource);
	}

	readonly IRunnerLogger runnerLogger;
	readonly IServiceTaskRunner serviceTaskRunner;

	class ErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		public ErrorDetectionStrategy(IRunnerLogger runnerLogger, IScheduledRunCommandInfo scheduledRunCommandInfo)
		{
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
			this.scheduledRunCommandInfo = scheduledRunCommandInfo ?? throw new ArgumentNullException(nameof(scheduledRunCommandInfo));
		}

		public static bool IsTransientException(Exception ex)
		{
			if (ex is SqlException sqlException)
			{
				var exceptionType = new DbErrorMatch(sqlException).ExceptionType;
				return exceptionType == DbErrorType.LockTimeoutExpired;
			}

			return false;
		}

		public bool IsTransient(Exception ex)
		{
			var isTransient = IsTransientException(ex);

			if (isTransient)
			{
				runnerLogger.Log(LogLevel.Debug, $"Could not read next run time. Attempt {++retry} out of {RetryCount}.", scheduledRunCommandInfo);
			}

			return isTransient;
		}

		public const int RetryCount = 10;
		readonly IRunnerLogger runnerLogger;
		readonly IScheduledRunCommandInfo scheduledRunCommandInfo;
		int retry;
	}
}
