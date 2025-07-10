using System;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.LogWalker
{
	public class QueueLogs : ILogWalkerOperation
	{
		public void Execute(SubscriberParameters subscriberParameters, LogSubscriber[] allSubscribers, CancellationToken token)
		{
			int newLogQueueItemsCount = CreateNewLogQueueItems(subscriberParameters, allSubscribers, token);
			subscriberParameters.Logger.Log(newLogQueueItemsCount > 0 ? LogType.Information : LogType.Debug, string.Format(CultureInfo.InvariantCulture, "{0} log event(s) queued.", newLogQueueItemsCount));
		}

		protected virtual int CreateNewLogQueueItems(SubscriberParameters subscriberParameters, LogSubscriber[] queueAndProcessSubscribers, CancellationToken token)
		{
			var queuePublisher = new NewsPublisher();

			int totalQueuedLogs = 0;
			using (var lockTimeout = Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(5)))
			using (var priority = Db.Connection.TemporarySetDeadlockPriority(-10))
			{
				try
				{
					var queuedLogs = 0;
					do
					{
						token.ThrowIfCancellationRequested();
						using (var transactionManager = Db.Connection.BeginTransactionWithManager())
						{
							queuedLogs = queuePublisher.CreateLogQueueItemsForWorkflowTriggerEvents(Db.Connection);
							transactionManager.CommitTransaction();
						}

						Thread.Sleep(SystemDataRegistry.Instance.LogWalkerDelayDurationInCreatingLogQueueItems.Value);

						// this is done to get each batch completed separately
						using (var transactionManager = Db.Connection.BeginTransactionWithManager())
						{
							queuedLogs += queuePublisher.CreateLogQueueItemsForEverythingExceptWorkflowTriggerEvents(Db.Connection, queueAndProcessSubscribers);
							transactionManager.CommitTransaction();
						}

						totalQueuedLogs += queuedLogs;
						if (queuedLogs > 0)
						{
							Nudge();
						}
					} while (queuedLogs > 0);
				}
				catch (SqlException ex)
				{
					var exceptionType = new DbErrorMatch(ex).ExceptionType;
					if (exceptionType == DbErrorType.LockTimeoutExpired || exceptionType == DbErrorType.DeadlockError)
					{
						subscriberParameters.Logger.Warning(ex.Message);
					}
					else
					{
						throw;
					}
				}
			}

			return totalQueuedLogs;
		}

		void Nudge()
		{
			const string LogWalkerServiceTaskCode = "LWK";
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(LogWalkerServiceTaskCode);
		}
	}
}



