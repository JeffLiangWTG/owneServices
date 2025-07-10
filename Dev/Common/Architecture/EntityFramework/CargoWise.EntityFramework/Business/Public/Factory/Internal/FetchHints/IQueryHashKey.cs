using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public interface IQueryHashKey
	{
		IEnumerable<object> KeyParts { get; }
	}

	class QueryHashKeyComparer : IEqualityComparer<IQueryHashKey>
	{
		public bool Equals(IQueryHashKey x, IQueryHashKey y)
		{
			return ReferenceEquals(x, y) || x.KeyParts.SequenceEqual(y.KeyParts);
		}
		public int GetHashCode(IQueryHashKey obj)
		{
			return obj.KeyParts.Aggregate(0, (hash, x) => x.GetHashCode() ^ hash);
		}
	}
}
