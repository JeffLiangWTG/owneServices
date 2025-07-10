using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.GraphEngine;

namespace Enterprise.Scheduler.GraphEngine
{
	public class ServiceTaskGrEngine<TKey, TEntity> : GrEngine<TKey, TEntity>
		where TKey : IEquatable<TKey>
		where TEntity : IQueueState, IEquatable<TEntity>
	{
		public ServiceTaskGrEngine()
		{
			tracker = new NotifierGrengineTracker<TKey, TEntity>();
			this.keyProvider = new DefaultKeyProvider<TKey, TEntity>();
		}

		readonly NotifierGrengineTracker<TKey, TEntity> tracker;
		readonly IKeyProvider<TKey, TEntity> keyProvider;
		List<TEntity> newEntities = new List<TEntity>();

		#region Implementation

		internal void AddNew(IEnumerable<TEntity> entities) => newEntities.AddRange(entities);
		internal IDisposable WithNotifications(INotifications notifications) => tracker.WithNotifications(notifications);

		protected override IKeyProvider<TKey, TEntity> GetKeyProvider() => keyProvider;
		protected override IComparer<TEntity> GetOrderComparer() => new QueueStateComparer();
		protected override IProcessedProvider<TEntity> GetProcessedProvider() => new ProcessedProvider();
		protected override IGrEngineTracker<TKey, TEntity> GetTracker() => tracker;

		protected sealed override IEnumerable<TEntity> LoadEntitiesCore()
		{
			var result = newEntities;
			newEntities = new List<TEntity>();
			return result;
		}

		class ProcessedProvider : IProcessedProvider<TEntity>
		{
			public IEnumerable<TEntity> GetProcessed(IEnumerable<TEntity> entities)
			{
				return entities.Where(e => e.Status == QueueStatusCodes.Codes.Processed);
			}
		}

		class QueueStateComparer : IComparer<TEntity>
		{
			public int Compare(TEntity x, TEntity y)
			{
				var r = StatusToOrderPriority(x.Status).CompareTo(StatusToOrderPriority(y.Status));
				if (r == 0)
				{
					return string.Compare(x.OrderInfo, y.OrderInfo, StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					return r;
				}
			}

			int StatusToOrderPriority(string status)
			{
				switch (status)
				{
					case QueueStatusCodes.Codes.Processed:
						return 0;
					case QueueStatusCodes.Codes.Queued:
						return 1;
					case QueueStatusCodes.Codes.Blocked:
						return 2;
					case "":
					case QueueStatusCodes.Codes.PreKey:
						return 3;
					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot sort such an StmQueueState as this [{0}]", status));
				}
			}
		}

		#endregion
	}
}
