using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CargoWise.GraphEngine
{
	public class Chain<TEntity> : ReadOnlyCollection<TEntity>, IEquatable<Chain<TEntity>>
	{
		public Chain(IList<TEntity> entities)
			: base(entities)
		{
		}

		public override int GetHashCode()
		{
			return this.Aggregate(0, (x, y) => x.GetHashCode() ^ y.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			var other = obj as Chain<TEntity>;

			if (other != null)
			{
				return other.Equals(this);
			}
			else
			{
				return base.Equals(obj);
			}
		}

		public bool Equals(Chain<TEntity> other)
		{
			return this.SequenceEqual(other);
		}
	}
}
