using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common.Collections
{
	/// <summary>
	/// Extension methods for the IList interface.
	/// </summary>
	public static class IListExtensions
	{
		/// <summary>
		/// Enumerates an IList collection. The enumeration is re-run if the count changes at the
		/// end of the enumeration. This is to handle the case where more objects are created during
		/// an enumeration that need to also be enumerated. This should only be used if re-enumeration
		/// over the same objects is acceptable.
		/// </summary>
		/// <param name="list">The IList that is to be enumerated over.</param>
		/// <param name="retryCount">The maximum number of times to retry the enumeration.</param>
		/// <returns>
		/// An IEnumerable that is to returns elements. The enumeration is re-run if the count changes
		/// at the end of an iteration.
		/// </returns>
		public static IEnumerable EnumerateWithCountChangeRetry(this IList list, int retryCount)
		{
			for (int i = 0; i < retryCount; i++)
			{
				int countBefore = list.Count;

				ArrayList copiedList = new ArrayList(list);
				foreach (object item in copiedList)
				{
					yield return item;
				}
				if (copiedList.Count == list.Count)
				{
					break;
				}
			}
		}

		public static IList<T> StableSort<T>(this IList<T> ilist, IComparer<T> comparer)
		{
			Argument.NotNull(ilist, nameof(ilist)); // Suggested By ReviewBot 
			if (ilist.Count > 1)
			{
				var sortedList = new List<T>(ilist.Count);
				sortedList.AddRange(ilist.OrderBy(x => x, comparer));
				var array = ilist as T[];
				if (array != null && array.Length >= sortedList.Count)
				{
					sortedList.CopyTo(array);
				}
				else
				{
					ilist.Clear();
					var list = ilist as List<T>;
					if (list != null)
					{
						list.AddRange(sortedList);
					}
					else
					{
						sortedList.ForEach(ilist.Add);
					}
				}
			}

			return ilist;
		}

		/// <summary>
		/// It's like stable-sort but it returns true if the order changed.
		/// </summary>
		public static bool StableSortWithReorderCheck<T>(this List<T> list, IComparer<T> comparer)
		{
			Argument.NotNull(list, nameof(list));
			return MergeSort.Sort(list, comparer ?? Comparer<T>.Default);
		}

		public static IList<T> StableSort<T>(this IList<T> ilist, Comparison<T> comparison)
		{
			Argument.NotNull(ilist, nameof(ilist)); // Suggested By ReviewBot 
			Argument.NotNull(comparison, nameof(comparison));
			return ilist.StableSort((ZComparer<T>)comparison);
		}

		public static IList<T> StableSort<T>(this IList<T> ilist, params Func<T, IComparable>[] selectors)
		{
			Argument.NotNull(selectors, nameof(selectors)); // Suggested By ReviewBot 
			Argument.NotNull(ilist, nameof(ilist));
			for (int i = selectors.Length - 1; i >= 0; i--)
			{
				var selector = selectors[i];
				ilist.StableSort((x, y) => selector(x).CompareTo(selector(y)));
			}

			return ilist;
		}

		/// <summary>
		/// Remove all extension method for IList
		/// </summary>
		public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
		{
			Argument.NotNull(list, nameof(list));
			int count = 0;

			for (var i = list.Count - 1; i >= 0; i--)
			{
				if (match(list[i]))
				{
					++count;
					list.RemoveAt(i);
				}
			}

			return count;
		}

		/// <summary>
		/// Inserts an item maintaining the sort order of the list. Assumes the List has already been sorted.
		/// </summary>
		public static void InsertInSortOrder<T>(this IList<T> list, T item, IComparer<T> comparer = null)
		{
			Argument.NotNull(list, nameof(list));

			// If the item is not in the list, List<T>.BinarySearch returns the bitwise complement of the index of the first item larger than it.
			// Inserting the item at the complement (~) of the negative number preserves the sort order. 
			var index = list.BinarySearch(item, comparer);
			list.Insert(index < 0 ? ~index : index, item);
		}

		/// <summary>
		/// Binary search for item in an IList
		/// </summary>
		/// <returns>
		/// The zero-based index of item in the sorted list, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or,
		/// if there is no larger element, the bitwise complement of list.Count
		/// </returns>
		public static int BinarySearch<T>(this IList<T> list, T item, IComparer<T> comparer = null)
		{
			Argument.NotNull(list, nameof(list));

			comparer = comparer ?? Comparer<T>.Default;

			var lower = 0;
			var upper = list.Count - 1;

			while (lower <= upper)
			{
				var middle = lower + (upper - lower) / 2;
				var comparisonResult = comparer.Compare(item, list[middle]);
				if (comparisonResult == 0)
				{
					return middle;
				}

				if (comparisonResult < 0)
				{
					upper = middle - 1;
				}
				else
				{
					lower = middle + 1;
				}
			}

			return ~lower;
		}

		/// <summary>
		/// O(1) concatenation of read-only collections.
		/// </summary>
		public static IReadOnlyCollection<T> ConcatCollection<T>(this IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2)
		{
			Argument.NotNull(list1, nameof(list1));
			Argument.NotNull(list2, nameof(list2));
			if (list1.Count == 0)
			{
				return list2;
			}
			else if (list2.Count == 0)
			{
				return list1;
			}
			else
			{
				return new ConcatCollection<T>(new[] { list1, list2 });
			}
		}

		/// <summary>
		/// Check for repeating patterns in a set.
		/// Beware. This implementation is O(n^2)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public static bool IsRepeatingPattern<T>(this IList<T> list, IEqualityComparer<T> comparer = null)
		{
			if (list.Count < 2)
			{
				return false;
			}
			else
			{
				comparer = comparer ?? EqualityComparer<T>.Default;

				var patternEndIndex = 0;
				var len = list.Count;
				var maxPatternSize = len / 2;

				while (patternEndIndex < maxPatternSize)
				{
					var matchCheckIndex = patternEndIndex + 1;
					while (comparer.Equals(list[matchCheckIndex], list[matchCheckIndex % (patternEndIndex + 1)]))
					{
						matchCheckIndex++;
						if (matchCheckIndex >= len)
						{
							return true;
						}
					}

					if (matchCheckIndex > 2 * patternEndIndex)
					{
						patternEndIndex = matchCheckIndex;
					}
					else
					{
						patternEndIndex++;
					}
				}

				return false;
			}
		}
	}
}
