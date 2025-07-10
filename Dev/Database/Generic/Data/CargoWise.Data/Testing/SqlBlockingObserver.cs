using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.MemoryManagement;
using CargoWise.Database.Abstractions;
using CargoWise.Integration;
using Microsoft.Extensions.DependencyInjection;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.Testing
{
	public class SqlBlockingObserver
	{
		#region Instance

		static SqlBlockingObserver()
		{
			MemoryManager.Register("SqlBlockerObserver", FlushCallback.OnAnyThread, delegate(FlushAction action) // diagnostic tool name
			{
				Instance.Clear();
				return FlushResult.Exhausted;
			});
		}
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly SqlBlockingObserver instance = new ();

		static SqlBlockingObserver Instance
		{
			get
			{
				return instance;
			}
		}

		internal SqlBlockingObserver()
		{
			lockingInfoList = new RingBuffer<string>(50);
		}

		void Clear()
		{
			lock (lockingInfoList)
			{
				lockingInfoList.Clear();
			}
		}

		void AddLockingInfo(string lockingInfo)
		{
			lock (lockingInfoList)
			{
				if (!IsEmptyJsonString(lockingInfo))
				{
					lockingInfoList.Add(lockingInfo);
				}
			}
		}

		bool IsEmptyJsonString(string text)
		{
			return string.IsNullOrWhiteSpace(text) || string.Equals(text, "{}");
		}

		string BuildLockingInfo()
		{
			lock (lockingInfoList)
			{
				var stringBuilder = new StringBuilder();
				if (lockingInfoList.Count > 0)
				{
					_ = stringBuilder.AppendLine("[");
					var hasElements = false;
					foreach (var info in lockingInfoList)
					{
						if (hasElements)
						{
							_ = stringBuilder.AppendLine(",").Append(info);
						}
						else
						{
							_ = stringBuilder.Append(info);
							hasElements = true;
						}
					}
					_ = stringBuilder.AppendLine("]");
				}

				return stringBuilder.ToString();
			}
		}

		readonly RingBuffer<string> lockingInfoList;

		#endregion

		public static string GetExtraDebugInformation()
		{
			var stringBuilder = new StringBuilder();

			TryAndCatch("LAST SQL QUERY", () => SqlEventTracker.Instance.LastSqlQuery);
			TryAndCatch("SQL Events", () => SqlEventTracker.Instance.SqlEventDescription);
			TryAndCatch("SQL Failed Events", () => SqlEventTracker.Instance.SqlFailedEventDescription);
			TryAndCatch("LOGGED IN USERS", () => GetLoggedInUsers());

			return stringBuilder.ToString();

			void TryAndCatch(string description, Func<string> getInfo)
			{
				_ = stringBuilder.AppendLine($"-- {description} --");

				try
				{
					_ = stringBuilder.AppendLine(getInfo());
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					_ = stringBuilder.AppendLine($"Failed to {description} with error: {ex.Message}");
				}
			}

			static string GetLoggedInUsers()
			{
				var result = new StringBuilder();
				var query = GlobalServiceProvider.Instance.GetRequiredService<IActiveUserQuery>();

				foreach (var userName in query.GetActiveUsers(true))
				{
					_ = result.AppendLine(userName);
				}

				return result.ToString();
			}
		}

		public static string GetLockingInfo()
		{
			return Instance.BuildLockingInfo();
		}

		public static void Start(TimeSpan interval)
		{
			if (Interlocked.Increment(ref referenceCounter) != 1)
			{
				return;
			}

			Instance.Clear();

			var cancellationSource = new CancellationTokenSource();
			var cancellationToken = cancellationSource.Token;
			var observerTask = Task.Run(() =>
			{
				using var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb);
				while (!cancellationToken.WaitHandle.WaitOne(interval))
				{
					Instance.AddLockingInfo(CollectLockingInfo(adminConnection));
				}
			});

			disposable = new DisposableAction(() =>
			{
				if (Interlocked.Decrement(ref referenceCounter) <= 0)
				{
					try
					{
						cancellationSource.Cancel();
						observerTask.Wait(cancellationToken);
					}
					catch (OperationCanceledException) { }

					_ = Interlocked.Exchange(ref referenceCounter, 0);
					_ = MemoryManager.Flush(FlushAction.Full, 1);

					cancellationSource.Dispose();
					disposable = null;
				}
			});

			static string CollectLockingInfo(AdminConnection adminConnection)
			{
				return adminConnection.ExecuteScalar<string>(SqlStatement);
			}
		}

		public static void Stop()
		{
			disposable?.Dispose();
		}

		[ThreadSafe]
		static int referenceCounter = 0;

		[ThreadSafe]
		static volatile IDisposable disposable = null;

		const string SqlStatement = @"
;WITH BlockersCTE AS
(
	SELECT
		session_id
		, blocking_session_id
		, sql_handle
		, CAST(blocking_session_id AS VARCHAR(MAX)) AS BlockingChain
		, 1 as level
	FROM sys.dm_exec_requests
	WHERE 1=1
		AND blocking_session_id > 0

	UNION ALL

	SELECT
		blk.session_id
		, req.blocking_session_id
		, blk.sql_handle
		, CAST(blk.BlockingChain + ' -> ' + CAST(req.blocking_session_id AS VARCHAR) AS VARCHAR(MAX))
		, level + 1
	FROM BlockersCTE blk
	JOIN sys.dm_exec_requests req
		ON blk.blocking_session_id = req.session_id
), HeadBlockers as
(
	SELECT
		session_id
		, blocking_session_id
		, sql_handle
		, BlockingChain
		, DENSE_RANK() OVER (PARTITION BY session_id ORDER BY level DESC) AS rank
	FROM BlockersCTE
), HeadBlockersCTE as
(
	SELECT
		session_id, blocking_session_id, sql_handle
	FROM
		HeadBlockers
	WHERE
		rank = 1
)

SELECT
	(
		SELECT
			session_id
			, blocking_session_id
		FROM
			HeadBlockersCTE
		FOR JSON PATH
	) AS LockingInfo,
	(
		SELECT
			waitingTasks.session_id
			, waitingTasks.blocking_session_id
			, waitingTasks.wait_type
			, waitingTasks.wait_duration_ms
			, sqlText.text AS sql_text
			, blocker.login_name as blocking_login_name
			, blocker.host_name as blocking_host_name
			, blocker.host_process_id as blocking_host_process_id
			, blocker.login_time as blocking_login_time
			, blocker.last_request_start_time as blocking_last_request_start_time
			, blocker.last_request_end_time as blocking_last_request_end_time
			, blocker.status as blocking_status
		FROM
			sys.dm_os_waiting_tasks AS waitingTasks
				INNER JOIN HeadBlockersCTE AS blk ON waitingTasks.session_id = blk.session_id
			OUTER APPLY
				sys.dm_exec_sql_text(blk.sql_handle) AS sqlText
			OUTER APPLY
				(SELECT
					login_name
					, host_name
					, host_process_id
					, login_time
					, last_request_start_time
					, last_request_end_time
					, status
				FROM sys.dm_exec_sessions WHERE session_id = blk.blocking_session_id
			) AS blocker
		FOR JSON PATH
	) AS WaitingTasks
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
;
";
	}
}
