using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CargoWise.Common
{
	public static class IComparerExtensions
	{
		/// <summary>
		/// Sort multiple IEnumerable<typeparamref name="T"/> into groups based on the IComparer.
		/// Collection groups are kept separate and are indexed by the parameterised collection.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="comparer"></param>
		/// <param name="collections"></param>
		/// <returns></returns>
		public static MultiGroupCollection<T> GroupMerge<T>(this IEqualityComparer<T> comparer, params IEnumerable<T>[] collections)
		{
			Argument.NotNull(collections, nameof(collections));
			Argument.NotNull(comparer, nameof(comparer));

			var dict = new MultiGroupCollection<T>(comparer);
			dict.GroupMerge(collections);

			return dict;
		}

		public static IEnumerable<Group<T>> GroupOrphans<T>(this MultiGroupCollection<T> groupCollection, IEqualityComparer<T> comparer)
		{
			var groups = (IEnumerable<Group<T>>)groupCollection.Values;
			var alreadyGrouped = groups.Split(g => g.Groups().All(gr => gr.Any()));
			var extendedGroups = comparer.GroupMerge(alreadyGrouped.NonMatchingSet.SelectMany(g => g[0]), alreadyGrouped.NonMatchingSet.SelectMany(g => g[1])).Values;
			return alreadyGrouped.MatchingSet.Concat(extendedGroups).ToList();
		}
	}

	[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class MultiGroupCollection<T> : Dictionary<T, Group<T>>
	{
		public MultiGroupCollection(IEqualityComparer<T> comparer)
			: base(comparer)
		{
		}

		public void GroupMerge(params IEnumerable<T>[] collections)
		{
			var start = Depth;
			var end = start + collections.Length;
			Depth = end;

			foreach (var group in Values)
			{
				group.IncreaseDepth(Depth);
			}

			for (var i = start; i < end; i++)
			{
				var collection = collections[i - start];
				MergeInto(i, collection);
			}
		}

		public void MergeInto(int depth, IEnumerable<T> collection)
		{
			if (depth >= Depth)
			{
				throw new ArgumentException(FormattableString.Invariant($"{depth} is not less than Depth {Depth}"), nameof(depth));
			}
			var dict = this;
			foreach (var item in collection)
			{
				if (item != null)
				{
					Group<T> group;
					if (!dict.TryGetValue(item, out group))
					{
						dict[item] = group = new Group<T>(Depth);
					}

					group.Add(depth, item);
				}
			}
		}

		public int Depth { get; private set; }
	}

	public class Group<T>
	{
		internal Group(int depth)
		{
			groups = new List<List<T>>();
			IncreaseDepth(depth);
		}

		readonly List<List<T>> groups;

		/// <summary>
		/// Find the set of elements at the given depth.
		/// </summary>
		/// <param name="depth"></param>
		/// <returns></returns>
		public IReadOnlyList<T> GetGroup(int depth)
		{
			if (depth < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(depth));
			}

			if (depth >= Count)
			{
				throw new ArgumentException("Invalid argument.", nameof(depth));
			}

			return (groups[depth] ?? new List<T>()).AsReadOnly();
		}

		public IEnumerable<T> Enumerate() => groups.WhereNotNull().SelectMany(g => g);

		internal IEnumerable<IEnumerable<T>> Groups()
		{
			foreach (var group in groups)
			{
				yield return group ?? Enumerable.Empty<T>();
			}
		}

		public IReadOnlyList<T> this[int i]
		{
			get
			{
				if (i < 0)
				{
					throw new ArgumentException("Invalid argument.", nameof(i));
				}

				if (i >= Count)
				{
					throw new ArgumentException("Invalid argument.", nameof(i));
				}

				return GetGroup(i);
			}
		}

		public int Count => groups.Count;

		internal void Add(int index, T item)
		{
			if (index < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(index));
			}

			if (index > groups.Count)
			{
				throw new ArgumentException("Invalid argument.", nameof(index));
			}

			var list = groups[index];
			if (list == null)
			{
				groups[index] = list = new List<T>();
			}

			list.Add(item);
		}

		#region CodeContracts

		internal void IncreaseDepth(int depth)
		{
			while (Count < depth)
			{
				groups.Add(null);
			}
		}

		#endregion
	}
}
