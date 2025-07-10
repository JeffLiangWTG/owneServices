using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Scheduler.GraphEngine;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public abstract class ShouldDequeueStrategy<TQueueState>
		where TQueueState : class, IQueueState
	{
		public abstract bool ShouldDequeue(IEnumerable<TQueueState> newEntities);
	}

	public class AlwaysDequeueStrategy<TQueueState> : ShouldDequeueStrategy<TQueueState>
		where TQueueState : class, IQueueState
	{
		public override bool ShouldDequeue(IEnumerable<TQueueState> newEntities) => true;
	}

	public class DequeueWhenFullOrNoNewItems<TEntity, TQueueState> : ShouldDequeueStrategy<TQueueState>
		where TEntity : BusinessObject
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		public DequeueWhenFullOrNoNewItems(GrEngineEnqueuer<TEntity, TQueueState> enqueuer, int capacity)
		{
			this.enqueuer = enqueuer;
			this.capacity = capacity;
		}
		readonly GrEngineEnqueuer<TEntity, TQueueState> enqueuer;
		readonly int capacity;

		public override bool ShouldDequeue(IEnumerable<TQueueState> newEntities)
		{
			return enqueuer.ItemsLoaded >= capacity
				|| (newEntities != null && !newEntities.Any());
		}
	}
}
