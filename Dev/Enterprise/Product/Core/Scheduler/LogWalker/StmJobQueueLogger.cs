using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker
{
	public interface IStmJobQueueLoggerConfig
	{
		TimeSpan LogInterval { get; }
		TimeSpan LogAllowedLag { get; }
		int MaxItems { get; }
	}

	public interface IStmJobQueueLogger
	{
		void GoForthAndLog();
	}

	public sealed class StmJobQueueLogger : IStmJobQueueLogger, IDisposable
	{
		const string sessionLock = "LWK_Logging_Lock_" + nameof(LogWalkerCategories);
		readonly IStmJobQueueLoggerConfig loggerConfig;
		readonly ICategoryLogger<LogWalkerCategories> logger;
		readonly Func<ITaskRunner> lazyTaskRunner;
		ITaskRunner taskRunner;
		IDisposable connectionDisposer;
		IDisposable appLockDisposer;
		readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		Task periodicLoggingTask = Task.CompletedTask;
		Task continueLoggingTask = Task.CompletedTask;
		bool keepLogging;
		ScopeGuard scopeGuard;

		public StmJobQueueLogger(Func<ITaskRunner> lazyTaskRunner, IStmJobQueueLoggerConfig loggerConfig, ICategoryLogger<LogWalkerCategories> logger)
		{
			this.lazyTaskRunner = lazyTaskRunner;
			this.loggerConfig = loggerConfig;
			this.logger = logger;
		}

		public void GoForthAndLog()
		{
			if (!logger.ShouldLog(LogWalkerCategories.StmJobQueueReport))
			{
				return;
			}
			else if (periodicLoggingTask.IsCompleted)
			{
				// We call wait to propagate any exceptions that have occured.
				periodicLoggingTask.Wait();
				periodicLoggingTask = TaskRunner.EnqueueTask(() => PeriodicallyLog(cancellationTokenSource.Token));
			}
			else if (continueLoggingTask.IsCompleted)
			{
				// We call wait to propagate any exceptions that have occured.
				continueLoggingTask.Wait();
				continueLoggingTask = TaskRunner.EnqueueTask(() => ContinueLogging(cancellationTokenSource.Token));
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive")]
		public void Dispose()
		{
			taskRunner?.EnqueueTask(() =>
			{
				appLockDisposer?.Dispose();
				connectionDisposer?.Dispose();
				appLockDisposer = null;
				connectionDisposer = null;
				keepLogging = false;
			}).Wait();

			taskRunner = null;
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
		}

		ITaskRunner TaskRunner
		{
			get
			{
				return taskRunner = taskRunner ?? lazyTaskRunner.Invoke();
			}
		}

		async Task PeriodicallyLog(CancellationToken cancellationToken)
		{
			using (connectionDisposer = Db.DisposableActionForDbConnection())
			{
				var connection = Db.Connection;
				if (!connection.TryGetLock(sessionLock, TimeSpan.Zero, out var appLock))
				{
					// Another LWK is already logging
					// Wait before trying to obtain the lock again
					await WaitTillNextTryLogTime(cancellationToken);
					return;
				}

				using (appLock)
				{
					keepLogging = true;

					while (keepLogging)
					{
						using (scopeGuard = new ScopeGuard())
						{
							keepLogging = false;
							DoTheLogging();

							// Note that we want to hold the appLock for the interval and greedily
							// perform the logging if still being notified
							await WaitTillNextLogTime(cancellationToken);
						}
					}
				}
			}
		}

		async Task ContinueLogging(CancellationToken cancellationToken)
		{
			if (scopeGuard.InGuard)
			{
				keepLogging = true;
				await WaitTillNextTryLogTime(cancellationToken);
			}
			else
			{
				// There is a race between this task being queued and the logging task finishing
				// We do nothing in the case of having lost.
			}
		}
#if DEBUG
		internal
#endif
		void DoTheLogging()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var query = new ZDBOnlyQuery(typeof(StmJobQueue));
			query.AddToFilter(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusQueued);
			query.OrderBy = StmJobQueueSchema.SJ_ProcessOnOrAfterUtc.Name;
			query.MaximumRows = loggerConfig.MaxItems;

			var items = factory.Load<IQueuedLog>(query).GroupBy(item => item.SJ_ALogReference).SelectMany(item => item);
			var sb = new StringBuilder();

			items.ForEachWithBetween(
				(item) =>
				{
					sb.Append("(");
					sb.Append(item.PrintBasic());
					sb.Append(")");
				},
				() => sb.Append(", "));

			if (sb.Length > 0)
			{
				logger.Log(LogWalkerCategories.StmJobQueueReport, LogType.Information, sb.ToString());
			}
		}

		async Task WaitTillNextLogTime(CancellationToken cancellationToken)
		{
			await Task.Delay(loggerConfig.LogInterval, cancellationToken);
		}

		async Task WaitTillNextTryLogTime(CancellationToken cancellationToken)
		{
			await Task.Delay(new TimeSpan(loggerConfig.LogInterval.Ticks / 2), cancellationToken);
		}

		struct ScopeGuard : IDisposable
		{
			bool disposed;

			public bool InGuard => !disposed;

			public void Dispose()
			{
				disposed = true;
			}
		}
	}
}
