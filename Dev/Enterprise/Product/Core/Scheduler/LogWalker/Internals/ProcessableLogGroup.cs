using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker
{
	public sealed class ProcessableLogGroup
	{
		internal ProcessableLogGroup(BusinessObjectFactory factory, LogSubscriber subscriber, int depth, int retryCount, LogBatch queuedLogs, RecurringLogTracker recursionTracker, IList<Exception> priorExceptions, IDisposable overrideDataRefreshDisable = null)
		{
			Factory = factory;
			Subscriber = subscriber;
			Depth = depth;
			RetryCount = retryCount;
			this.queuedLogs = queuedLogs;
			RecursionTracker = recursionTracker;
			PriorExceptions = priorExceptions ?? Array.Empty<Exception>();
			OverrideDataRefreshDisable = overrideDataRefreshDisable;
		}

		readonly LogBatch queuedLogs;
		public BusinessObjectFactory Factory { get; }
		LogBatchKey BatchKeyProvider => queuedLogs.Key;
		public object BatchKey => BatchKeyProvider.Key;
		public int Count => queuedLogs.Count;

		/// <summary>
		/// Depth is for tracking recurring logs (logs created in the LWK service task by other logs)
		/// This doesn't have anything to do with retries. When a log has to be retried, Depth should not change.
		/// </summary>
		public int Depth { get; }
		public int RetryCount { get; }
		public LogSubscriber Subscriber { get; }
		public IEnumerable<IQueuedLog> Logs => queuedLogs.Select(s => s.Item);
		internal RecurringLogTracker RecursionTracker { get; }
		public IList<Exception> PriorExceptions { get; }
		IDisposable OverrideDataRefreshDisable { get; }

		public void FreeLocks()
		{
			foreach (var log in queuedLogs.Where(l => !l.IsDisposed))
			{
				log.Dispose();
			}
			OverrideDataRefreshDisable?.Dispose();
		}

		public void FreeLocks(IEnumerable<IQueuedLog> completedLogs)
		{
			var set = completedLogs.ToHashSet();
			foreach (var log in queuedLogs.Where(t => set.Contains(t.Item)))
			{
				log.Dispose();
			}
			OverrideDataRefreshDisable?.Dispose();
		}

		/// <summary>
		/// We call this method when a batch of logs is partially complete, and we have to abort processing early due to a save occurring in the middle of the batch.
		/// </summary>
		internal ProcessableLogGroup GetSubset(BusinessObjectFactory factory, ICollection<IQueuedLog> unprocessedLogs, IList<Exception> priorExceptions)
		{
			var set = unprocessedLogs.ToHashSet();
			var newSet = queuedLogs.Where(t => set.Contains(t.Item))
				.Select(t => ImportLogIntoAnotherFactory(factory, t))
				.ToList();
			return new ProcessableLogGroup(factory, Subscriber, Depth, RetryCount, new LogBatch(BatchKeyProvider, newSet), RecursionTracker, priorExceptions, OverrideDataRefreshDisable);
		}

		/// <summary>
		/// We call this method when a batch of logs has an error and we need to process the logs one at a time.
		/// </summary>
		internal IEnumerable<ProcessableLogGroup> SplitGroup(Exception ex)
		{
			var result = new List<ProcessableLogGroup>();
			foreach (var log in queuedLogs)
			{
				var factory = Subscriber.GetFactoryForProcessing(enableDataRefresh: true);
				var reloadedLog = ImportLogIntoAnotherFactory(factory, log);
				var exceptions = ex != null ? PriorExceptions.Append(ex).ToList() : PriorExceptions;

				if (reloadedLog?.Item == null)
				{
					ErrorReporter.ReportOnce("Reloaded Log is null.", new AggregateException(exceptions));
				}

				var group = new ProcessableLogGroup(factory, Subscriber, Depth, RetryCount + 1,
					new LogBatch(BatchKeyProvider, new[] { reloadedLog }), RecursionTracker, exceptions, OverrideDataRefreshDisable ?? DataRefreshManager.BeginRefreshOverrideForServiceTask())
				;
				result.Add(group);
			}
			return result;
		}

		static AppLockedItem<IQueuedLog> ImportLogIntoAnotherFactory(BusinessObjectFactory factory, AppLockedItem<IQueuedLog> tuple)
		{
			var itemReloaded = tuple.Item is StmJobQueue jobQueue ? factory.Load<StmJobQueue>(jobQueue.PK) : tuple.Item;
			return new AppLockedItem<IQueuedLog>(itemReloaded, tuple.Lock);
		}
	}
}
