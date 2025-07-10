using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.GraphEngine;

namespace Enterprise.Scheduler.GraphEngine
{
	public class DefaultKeyProvider<TKey, TEntity> : IKeyProvider<TKey, TEntity>
		where TKey : IEquatable<TKey>
		where TEntity : IQueueState, IEquatable<TEntity>
	{
		public IEnumerable<IGrouping<TEntity, TKey>> GetKeys(IEnumerable<TEntity> entities)
		{
#pragma warning disable CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
			return entities.Select(e => new Group<TEntity, TKey>(e, e.Keys.Cast<TKey>()));
#pragma warning restore CA2021 // Do not call Enumerable.Cast<T> or Enumerable.OfType<T> with incompatible types
		}
	}
}
