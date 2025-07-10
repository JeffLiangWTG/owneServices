using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;

namespace Enterprise.Scheduler.GraphEngine
{
	public class GrEngineServiceSetup<TQueueState> : IPreKeyBacklogCheckerSupporter
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		public GrEngineServiceSetup(IQueueStateFactory<TQueueState> factory, int enqueueBatchSize,
			int dequeueBatchSize, int preKeyMaxBacklogSize, int capacity, GrEngineEnums.PreKeyOption preKeyOption,
			GrEngineEnums.ChainOption chainOption, ISqlApplicationLockProvider lockProvider = null)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			Argument.GreaterThanZero(enqueueBatchSize, nameof(enqueueBatchSize));
			Argument.GreaterThanZero(dequeueBatchSize, nameof(dequeueBatchSize));

			EnqueueBatchSize = enqueueBatchSize;
			DequeueBatchSize = dequeueBatchSize;
			PreKeyMaxBacklogSize = preKeyMaxBacklogSize;
			Capacity = capacity;
			PreKeyMode = preKeyOption;
			ChainMode = chainOption;
			LockProvider = lockProvider ?? new DefaultSqlApplicationLockProvider();
		}

		public IQueueStateFactory<TQueueState> Factory { get; }
		public int EnqueueBatchSize { get; }
		public int DequeueBatchSize { get; }
		public int PreKeyMaxBacklogSize { get; }
		public int Capacity { get; }
		public GrEngineEnums.PreKeyOption PreKeyMode { get; }
		public GrEngineEnums.ChainOption ChainMode { get; }
		public ISqlApplicationLockProvider LockProvider { get; }

		const int BATCH_LOAD_ESTIMATE = 200;
		internal int DequeueListSizeEstimate => Math.Min(BATCH_LOAD_ESTIMATE, DequeueBatchSize);

		public GrEngineLogOptions LogOptions => Factory.LogOptions;

		public void Save(IEnumerable<TQueueState> queues, INotifications notifications = null)
		{
			// This order matters. (IsInDatabase is mutated)
			var factory = Factory;
			var updateList = new List<TQueueState>();
			var insertList = new List<TQueueState>();
			foreach (var queue in queues)
			{
				if (queue.IsInDatabase)
				{
					if (queue.HasChanges)
					{
						updateList.Add(queue);
					}
				}
				else
				{
					insertList.Add(queue);
				}
			}
			notifications?.AddInfo(FormattableString.Invariant($"Save method updateList ({updateList.Count})"));
			notifications?.AddInfo(FormattableString.Invariant($"Save method insertList ({insertList.Count})"));

			factory.Update(updateList);
			factory.InsertAsQueued(insertList);
		}

		int IPreKeyBacklogCheckerSupporter.CountBacklog(string status, GrEngineEnums.Status includeOrExclude) => Factory.CountBacklog(status, includeOrExclude);
	}

	public static class GrEngineEnums
	{
		public enum Status
		{
			Include,
			Exclude
		}

		/// <summary>
		/// This option determines whether keys are generated seperately from loading entities.
		/// When this option is mandatory GreEngine Services must have bespoke implementation of key generation.
		/// </summary>
		public enum PreKeyOption
		{
			None,
			Mandatory
		}

		/// <summary>
		/// A chain is a sequence of entities with shared keys.
		/// This option allows chains to be loaded so that batches can be larger.
		/// </summary>
		public enum ChainOption
		{
			No,
			Yes
		}
	}
}
