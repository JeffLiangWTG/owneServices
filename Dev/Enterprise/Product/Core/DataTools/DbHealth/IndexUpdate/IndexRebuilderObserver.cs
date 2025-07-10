using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DbHealth.IndexUpdate
{
	public class IndexRebuilderObserver
	{
		static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(2);

		public IndexRebuilderObserver(bool isOnline, TimeSpan initialWait, ObserverAction startAction, ObserverAction endAction)
		{
			this.isOnline = isOnline;
			this.initialWait = initialWait;
			this.startAction = startAction;
			this.endAction = endAction;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public ObserverAction Run(DbConnection runnerConnection, string sql, Action<string> feedbackMethod, int? runnerCommandTimeout = null)
		{
			var resultAction = ObserverAction.None;
			runnerCommandTimeout = runnerCommandTimeout ?? DbCommand.Timeout.Infinite;
			using (var hostSource = new CancellationTokenSource())
			using (var runnerSource = new CancellationTokenSource())
			{
				int runnerSPID = runnerConnection.SPID;
				runnerConnection.ThreadSentry.RelinquishThreadOwnership();

				var hostToken = hostSource.Token;
				var runnerToken = runnerSource.Token;

				var hostTask = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var hostConnection = Db.NewAdminConnection(Db.SqlMasterDb))
					{
						using (var cmd = hostConnection.Command(SQL_Observer, DbCommand.Timeout.Infinite))
						{
							cmd.AddParameter("@blocked_spid", SqlDbType.SmallInt, runnerSPID);
							cmd.AddParameter("@is_online", SqlDbType.Bit, this.isOnline);
							cmd.AddParameter("@start_action", SqlDbType.VarChar, 20, this.startAction.ToString());
							cmd.AddParameter("@end_action", SqlDbType.VarChar, 20, this.endAction.ToString());

							try
							{
								var pollingInterval = IndexRebuilderObserver.DefaultWait + this.initialWait;
								while (!runnerToken.WaitHandle.WaitOne(pollingInterval) && !runnerToken.IsCancellationRequested)
								{
									var blockingProcess = Convert.ToString(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);

									resultAction = GetResultAction(hostConnection, blockingProcess, ref pollingInterval, feedbackMethod);
									if (resultAction.In(ObserverAction.Requeue, ObserverAction.Cancel))
									{
										break;
									}
								}
							}
							catch (ObjectDisposedException)
							{
								/*The cancellation can cause ObjectDisposedException if the current task gets timeout. So just eat it up here.*/
							}
							finally
							{
								hostSource.Cancel();
							}
						}
					}
				});

				var runnerTask = Task.Run(async () =>
				{
					runnerConnection.ThreadSentry.TakeThreadOwnership();

					var cmd = runnerConnection.Command(sql, runnerCommandTimeout);
					runnerConnection.ThreadSentry.RelinquishThreadOwnership();

					try
					{
						_ = await cmd.ExecuteNonQueryAsync(hostToken);
					}
					catch (ObjectDisposedException)
					{
						/*The cancellation can cause ObjectDisposedException if the current task gets timeout. So just eat it up here.*/
					}
					finally
					{
						runnerSource.Cancel();
					}
				});

				try
				{
					Task.WaitAll(hostTask, runnerTask);
				}
				catch (AggregateException aggEx) when (aggEx.InnerException != null && aggEx.InnerException is SqlException sqlEx)
				{
					var errorMatcher = new DbErrorMatch(sqlEx);
					if (errorMatcher.ExceptionType == DbErrorType.SevereError)
					{
						// skip the following error (ErrorCode == -2146232060):
						// A severe error occurred on the current command.  The results, if any, should be discarded.
						// Operation cancelled by user.
					}
					else
					{
						throw;
					}
				}
				finally
				{
					runnerConnection.ThreadSentry.TakeThreadOwnership();
					hostSource.Cancel();
					runnerSource.Cancel();
				}

				return resultAction;
			}
		}

		ObserverAction GetResultAction(AdminConnection hostConnection, string blockingProcess, ref TimeSpan pollingInterval, Action<string> feedbackMethod)
		{
			var result = ObserverAction.None;

			if (!string.IsNullOrWhiteSpace(blockingProcess))
			{
				if (blockingProcess.StartsWith("KILL ", StringComparison.OrdinalIgnoreCase))
				{
					// "KILL 59,LCK_M_SCH_S"

					feedbackMethod?.Invoke("Killing blockers for index");

					var spid = blockingProcess.Split(',')[0].Split(' ')[1];
					var infos = new List<DbConnectionKiller.ConnectionInfo>() { new DbConnectionKiller.ConnectionInfo(sessionId: spid) };
					DbConnectionKiller.PendingRollback(hostConnection, infos, feedbackMethod: null);

					pollingInterval = TimeSpan.Zero;
				}
				else if (blockingProcess.StartsWith("blocked ", StringComparison.OrdinalIgnoreCase))
				{
					// "blocked by system process,LCK_M_SCH_S"

					pollingInterval = IndexRebuilderObserver.DefaultWait;
				}
				else
				{
					// "59,LCK_M_SCH_S"

					var wait_type = blockingProcess.Split(',')[1];

					if (this.isOnline)
					{
						if (wait_type.In("LCK_M_SCH_S", "LCK_M_SCH_S_LOW_PRIORITY", "LCK_M_IS", "LCK_M_IS_LOW_PRIORITY", "LCK_M_S", "LCK_M_S_LOW_PRIORITY"))
						{
							if (this.startAction.In(ObserverAction.Requeue, ObserverAction.Cancel))
							{
								result = this.startAction;
							}
						}
						else if (wait_type.In("LCK_M_SCH_M", "LCK_M_SCH_M_LOW_PRIORITY"))
						{
							if (this.endAction.In(ObserverAction.Requeue, ObserverAction.Cancel))
							{
								result = this.endAction;
							}
						}
					}
					else
					{
						if (wait_type.In("LCK_M_SCH_S", "LCK_M_SCH_S_LOW_PRIORITY", "LCK_M_SCH_M", "LCK_M_SCH_M_LOW_PRIORITY"))
						{
							if (this.startAction.In(ObserverAction.Requeue, ObserverAction.Cancel))
							{
								result = this.startAction;
							}
						}
					}
				}
			}
			else
			{
				// no blockers

				pollingInterval = IndexRebuilderObserver.DefaultWait + this.initialWait;
			}

			return result;
		}

		#region Implementation

		readonly bool isOnline;
		readonly TimeSpan initialWait;
		readonly ObserverAction startAction;
		readonly ObserverAction endAction;

		string SQL_Observer
		{
			get
			{
				return @"-- ISU/GRC Observer
DECLARE
	@spid              varchar(100)
	, @wait_type       nvarchar(60)
	, @is_user_process bit

SELECT TOP(1)
	@spid              = CONVERT(varchar(100), s.session_id)
	, @wait_type       = w.wait_type
	, @is_user_process = s.is_user_process
FROM
	sys.dm_os_waiting_tasks   AS w
	JOIN sys.dm_exec_sessions AS s ON s.session_id = w.blocking_session_id
WHERE 1=1
	AND w.session_id = @blocked_spid
	AND w.blocking_session_id <> w.session_id
	AND w.wait_type in ('LCK_M_SCH_S', 'LCK_M_SCH_S_LOW_PRIORITY', 'LCK_M_IS', 'LCK_M_IS_LOW_PRIORITY', 'LCK_M_S', 'LCK_M_S_LOW_PRIORITY', 'LCK_M_SCH_M', 'LCK_M_SCH_M_LOW_PRIORITY')

if (@spid is NOT NULL)
begin
	if (@is_user_process = 0)
	begin
		SET @spid = 'blocked by system process';
	end
	else if
		(
			@is_online = 1
			AND
			(
				@start_action = 'KillBlockers' AND @wait_type in ('LCK_M_SCH_S', 'LCK_M_SCH_S_LOW_PRIORITY', 'LCK_M_IS', 'LCK_M_IS_LOW_PRIORITY', 'LCK_M_S', 'LCK_M_S_LOW_PRIORITY')
				OR @end_action = 'KillBlockers' AND @wait_type in ('LCK_M_SCH_M', 'LCK_M_SCH_M_LOW_PRIORITY')
			)

			OR @is_online = 0
			AND
			(
				@start_action = 'KillBlockers' AND @wait_type in ('LCK_M_SCH_S', 'LCK_M_SCH_S_LOW_PRIORITY', 'LCK_M_SCH_M', 'LCK_M_SCH_M_LOW_PRIORITY')
				--OR @end_action = 'KillBlockers' AND @wait_type in ('LCK_M_SCH_M', 'LCK_M_SCH_M_LOW_PRIORITY')
			)
		)
	begin
		SET @spid = CONCAT('KILL ', @spid);
		EXEC (@spid);
	end
end

SELECT
	info = CONCAT(@spid + ',', @wait_type)

";
			}
		}

		#endregion // Implementation
	}
}
