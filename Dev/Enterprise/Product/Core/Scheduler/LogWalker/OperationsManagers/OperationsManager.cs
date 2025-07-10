using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.LogWalker
{
	abstract class OperationsManager
	{
#if DEBUG
		public virtual void QueueAndProcessLogs(ILogger notifier)
		{
			QueueAndProcessLogs(notifier, CancellationToken.None);
		}
#endif

		public void QueueAndProcessLogs(ILogger notifier, CancellationToken token)
		{
			// Note that we will need to revisit this and make a thread-safe thread pool variant if
			// Subscribers are moved to be multi-threaded
			Argument.NotNull(notifier, nameof(notifier));

			ITaskContext backgroundThreadContext = null;
			var logWalkerConfig = LogWalkerConfig.Create();
			var stmJobQueueLogger = new StmJobQueueLogger(
				   LazyCreateBackgroundWorker,
				   logWalkerConfig,
				   new LogWalkerCategoryLogger(notifier));

			IDisposable backgroundWorkerDisposer = new DisposableAction(() =>
			{
				stmJobQueueLogger.Dispose();
				backgroundThreadContext?.Dispose();
				backgroundThreadContext = null;
			});

			var subscriberParameters = new SubscriberParameters()
			{
				Logger = notifier,
				BackgroundWorkerFactory = LazyCreateBackgroundWorker,
				StmJobQueueLogger = stmJobQueueLogger,
				StmJobQueueLoggerConfig = logWalkerConfig
			};

			using (backgroundWorkerDisposer)
			{
				var subscribers = SubscriberProvider.GetAllSubscribersWithValidationAndAppendingToNotificationLog(notifier);
				var subscriberDisposeExceptions = new List<Exception>();

				try
				{
					var ops = GetOrderedOperations();
					RunOperationCycle(subscriberParameters, subscribers, ops, token);
				}
				finally
				{
					foreach (var subscriber in subscribers)
					{
						try
						{
							subscriber.Dispose();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							subscriberDisposeExceptions.Add(ex);
						}
					}
				}

				if (subscriberDisposeExceptions.Count > 0)
				{
					throw new AggregateException(subscriberDisposeExceptions);
				}
			}

			ITaskRunner LazyCreateBackgroundWorker()
			{
				backgroundThreadContext = backgroundThreadContext ?? TaskContext.CreateThreadTaskContext();
				return backgroundThreadContext.GetTaskRunnerForContext();
			}
		}

		void RunOperationCycle(SubscriberParameters susbscriberParameters, LogSubscriber[] subscribers, IEnumerable<ILogWalkerOperation> operations, CancellationToken token)
		{
			var maxAppTransactionCount = 0;
#if DEBUG
			if (Globals.IsTest)
			{ maxAppTransactionCount = 1; }
#endif
			if (Db.Connection.AppTransactionCount > maxAppTransactionCount)
			{
				return;
			}

			foreach (var operation in operations)
			{
				token.ThrowIfCancellationRequested();
				operation.Execute(susbscriberParameters, subscribers, token);
			}

			susbscriberParameters.Logger.Log(LogType.Debug, "LogWalker cycle completed.");
		}

		protected abstract IEnumerable<ILogWalkerOperation> GetOrderedOperations();
		protected virtual SubscriberProvider SubscriberProvider => new SubscriberProvider();
	}
}
