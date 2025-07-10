using System;

namespace CargoWise.EntityFramework
{
	public struct TableIndexHint : IEquatable<TableIndexHint>
	{
		public string IndexName { get; }

		public TableIndexHint(string indexName)
		{
			this.IndexName = indexName;
		}

		public bool Equals(TableIndexHint other)
		{
			return this.IndexName.Equals(other.IndexName);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TableIndexHint))
			{
				return false;
			}
			else
			{
				return this.Equals((TableIndexHint)obj);
			}
		}

		public static bool operator ==(TableIndexHint lhs, TableIndexHint rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(TableIndexHint lhs, TableIndexHint rhs)
		{
			return !lhs.Equals(rhs);
		}

		public override int GetHashCode()
		{
			return IndexName.GetHashCode();
		}
	}
}
