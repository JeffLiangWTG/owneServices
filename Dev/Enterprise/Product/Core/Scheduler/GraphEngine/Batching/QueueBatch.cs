using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Scheduler.GraphEngine
{
	public sealed class QueueBatch<TQueueState> : Disposable, IEnumerable<TQueueState>
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		public QueueBatch(IEnumerable<TQueueState> items, params IDisposable[] locks)
		{
			Argument.NotNull(items, nameof(items));
			Argument.NotNull(locks, nameof(locks));

			this.items = items;
			this.locks = locks;
		}

		readonly IEnumerable<TQueueState> items;
		readonly IEnumerable<IDisposable> locks;

		public T[] Load<T>(BusinessObjectFactory factory)
			where T : BusinessObject
		{
			Argument.NotNull(factory, nameof(factory));

			var groups = items.GroupBy(g => g.TableCode);
			var queries = groups.Select(g => new ZQuery(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(g.Key).PK, g.Select(s => s.ParentID)));
			return queries.SelectMany(query =>
			{
				query.ReLoadExistingRows = true;
				return factory.Load<T>(query);
			}).ToArray();
		}

		#region IEnumerable

		public IEnumerator<TQueueState> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Disposable

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				locks.ForEach(l => l.Dispose());
			}
		}

		#endregion
	}
}
