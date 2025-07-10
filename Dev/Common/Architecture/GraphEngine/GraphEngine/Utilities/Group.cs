using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CargoWise.GraphEngine
{
	public class Group<TKey, TEntity> : ReadOnlyCollection<TEntity>, IGrouping<TKey, TEntity>
	{
		public delegate IEnumerable<TEntity> GetGroupMembers(TKey key);
		public Group(TKey key, GetGroupMembers getter)
			: this(key, getter(key))
		{
		}

		public Group(TKey key, IEnumerable<TEntity> entities)
			: base(entities.ToList())
		{
			Key = key;
		}

		public TKey Key { get; }
	}
}
