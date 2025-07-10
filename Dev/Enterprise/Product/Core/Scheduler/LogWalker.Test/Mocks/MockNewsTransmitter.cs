using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.LogWalker.Test
{
	[Serializable]
	public class MockNewsTransmitter : NewsTransmitter
	{
		public MockNewsTransmitter(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters)
			: base(subscriber, subscribers, subscriberParameters)
		{
			subscriber.SetDefaultLogger(subscriberParameters.Logger);
		}

		public void ProcessLogs(IList<IQueuedLog> logs, CancellationToken token = new CancellationToken())
		{
			useQueueOverride = true;
			try
			{
				QueueOverride.Enqueue(logs.Select(s => new AppLockedItem<IQueuedLog>(s, null)).ToArray());
				ProcessLogQueueBatch(logs.Count, token);
			}
			finally
			{
				useQueueOverride = false;
			}
		}
		bool useQueueOverride;

		public void ProcessLogsFromDb(int batchSize, CancellationToken token = new CancellationToken())
		{
			ProcessLogQueueBatch(batchSize, token);
		}

		Queue<AppLockedItem<IQueuedLog>[]> QueueOverride { get; } = new Queue<AppLockedItem<IQueuedLog>[]>();
		public Func<int, ProcessableLogGroup, IEnumerable<ProcessableLogGroup>> SaveProcessedLogsOverride { get; set; }

		StmJobQueueBatch GetQueueOverride() => QueueOverride.Any() ? new StmJobQueueBatch(QueueOverride.Dequeue()) : new StmJobQueueBatch(Array.Empty<AppLockedItem<IQueuedLog>>());

		protected override IEnumerable<ProcessableLogGroup> SaveProcessedLogs(int maximumDepth, ProcessableLogGroup currentLogGroup, LogSubscriberResult result, ITransactionManager disposable, DisposableManager disposableManager)
		{
			return SaveProcessedLogsOverride != null ? SaveProcessedLogsOverride(maximumDepth, currentLogGroup) : base.SaveProcessedLogs(maximumDepth, currentLogGroup, result, disposable, disposableManager);
		}

		public StmJobQueueBatch ReadQueueForTest()
		{
			return base.ReadQueue(1000);
		}

		protected override StmJobQueueBatch ReadQueue(int batchSize)
		{
			if (useQueueOverride)
			{
				return GetQueueOverride();
			}
			else
			{
				return base.ReadQueue(batchSize);
			}
		}

		public static IQueuedLog CreateLogQueueItemForTest(BusinessObjectFactory factory, StmALog log, LogSubscriber subscriber) => CreateLogQueueItem(factory, log, subscriber);
	}
}
