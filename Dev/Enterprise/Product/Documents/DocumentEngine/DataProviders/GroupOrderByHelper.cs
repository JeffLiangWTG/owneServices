using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DocumentEngine.DataProviders
{
	class GroupOrderByHelper
	{
		public IDataRowSource[] GetOrderedGroupBy(IEnumerable<GroupBySource> groups)
		{
			return groups
				.OrderBy(valuePair => valuePair.Keys, new GroupComparer())
				.Select(group => group.Group)
				.ToArray();
		}

		public IDataRowSource[] GetNonOrderedGroupBy(IEnumerable<GroupBySource> groups)
		{
			return groups
				.Select(group => group.Group)
				.ToArray();
		}

		class GroupComparer : IComparer<IEnumerable<IComparable>>
		{
			public int Compare(IEnumerable<IComparable> x, IEnumerable<IComparable> y)
			{
				var xEnumerator = x.GetEnumerator();
				var yEnumerator = y.GetEnumerator();

				while (xEnumerator.MoveNext())
				{
					if (!yEnumerator.MoveNext())
					{
						return 1;
					}

					if (xEnumerator.Current != null && yEnumerator.Current == null) { return 1; }
					if (xEnumerator.Current == null && yEnumerator.Current != null) { return -1; }

					if (xEnumerator.Current != null && yEnumerator.Current != null && xEnumerator.Current.GetType() != yEnumerator.Current.GetType())
					{
						return (xEnumerator.Current as string)?.Length == 0 ? -1 : 1;
					}

					var result = xEnumerator.Current != null ? xEnumerator.Current.CompareTo(yEnumerator.Current) : 0;

					if (result != 0)
					{
						return result;
					}
				}

				if (yEnumerator.MoveNext())
				{
					return -1;
				}

				return 0;
			}
		}
	}

	class GroupEqualityComparer : IEqualityComparer<IEnumerable<IComparable>>
	{
		public bool Equals(IEnumerable<IComparable> x, IEnumerable<IComparable> y)
		{
			return x.SequenceEqual(y);
		}

		public int GetHashCode(IEnumerable<IComparable> obj)
		{
			return obj.Aggregate(0, (current, group) => current ^ group.GetHashCode());
		}
	}

	#region GroupByResult

	class GroupByResult<T>
	{
		public GroupByResult(IEnumerable<IComparable> keys, T group)
		{
			Keys = keys;
			Group = group;
		}

		public IEnumerable<IComparable> Keys { get; private set; }
		public T Group { get; private set; }
	}

	class GroupByIndexes : GroupByResult<IList<int>>
	{
		public GroupByIndexes(IEnumerable<IComparable> keys, IList<int> group) : base(keys, group) { }
	}

	class GroupBySource : GroupByResult<IDataRowSource>
	{
		public GroupBySource(IEnumerable<IComparable> keys, IDataRowSource group) : base(keys, group) { }
	}

	#endregion
}
