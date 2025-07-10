using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common
{
	/// <summary>
	/// Extension methods for the IEnumerable interface.
	/// </summary>
	public static class IEnumerableExtensions
	{
		static int? CountWithoutIterating<TSource>(IEnumerable<TSource> source)
		{
			var collection1 = source as ICollection<TSource>;
			if (collection1 != null)
			{
				return collection1.Count;
			}

			var collection2 = source as ICollection;
			if (collection2 != null)
			{
				return collection2.Count;
			}

			return null;
		}

		/// <summary>
		/// This is Except<typeparamref name="T"/> from System.Linq.
		/// It uses an IComparer<typeparamref name="T"/> rather than IEqualityComparer<typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">Anything</typeparam>
		/// <param name="sourceCollection"></param>
		/// <param name="targetCollection"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public static IEnumerable<T> Except<T>(this IEnumerable<T> sourceCollection, IEnumerable<T> targetCollection, IComparer<T> comparer)
		{
			Argument.NotNull(sourceCollection, nameof(sourceCollection));
			Argument.NotNull(targetCollection, nameof(targetCollection));
			Argument.NotNull(comparer, nameof(comparer));

			var set = new SortedSet<T>(targetCollection, comparer);
			return sourceCollection.Where(t => !set.Contains(t));
		}

		/// <summary>
		/// Determines whether a sequence is <see langword="null"/> or empty.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence to be tested.</param>
		/// <returns>true if the <paramref name="source"/> is <see langword="null"/> or an empty sequence; otherwise, false.</returns>
		public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> source)
		{
			return source == null || !source.Any();
		}

		/// <summary>
		/// Determines whether a sequence contains exactly <paramref name="count"/> elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence that contains elements to be counted.</param>
		/// <param name="count">The number to be compared to the <paramref name="source"/> elements count.</param>
		/// <returns>True if the <paramref name="source"/> contains exactly <paramref name="count"/> elements; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="source"/> is <see langword="null"/>
		/// </exception>
		public static bool IsCountEqualTo<TSource>(this IEnumerable<TSource> source, int count)
		{
			Argument.NotNull(source, nameof(source));

			return (CountWithoutIterating(source) ?? source.Take(count + 1).Count()) == count;
		}

		/// <summary>
		/// Determines whether a sequence contains exactly <paramref name="count"/> elements that satisfy a condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence that contains elements to be tested and counted.</param>
		/// <param name="count">The number to be compared to the <paramref name="source"/> elements count.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>True if the <paramref name="source"/> contains exactly <paramref name="count"/> elements that satisfy a condition; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>
		/// </exception>
		public static bool IsCountEqualTo<TSource>(this IEnumerable<TSource> source, int count, Func<TSource, bool> predicate)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(predicate, nameof(predicate));

			var matching = source.Where(predicate).Take(count + 1).Count();
			return matching == count;
		}

		/// <summary>
		/// Determines whether a sequence contains less than <paramref name="count"/> elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence that contains elements to be counted.</param>
		/// <param name="count">The number to be compared to the <paramref name="source"/> elements count.</param>
		/// <returns>True if the <paramref name="source"/> contains less than <paramref name="count"/> elements; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="source"/> is <see langword="null"/>
		/// </exception>
		public static bool IsCountLessThan<TSource>(this IEnumerable<TSource> source, int count)
		{
			if (!(((source as ICollection<TSource>) != null || (source as ICollection) != null) || source != null))
			{
				throw new ArgumentNullException(nameof(source));
			}

			return (CountWithoutIterating(source) ?? source.Take(count).Count()) < count;
		}

		/// <summary>
		/// Determines whether a sequence contains less than <paramref name="count"/> elements that satisfy a condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence that contains elements to be tested and counted.</param>
		/// <param name="count">The number to be compared to the <paramref name="source"/> elements count.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>True if the <paramref name="source"/> contains less than <paramref name="count"/> elements that satisfy a condition; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>
		/// </exception>
		public static bool IsCountLessThan<TSource>(this IEnumerable<TSource> source, int count, Func<TSource, bool> predicate)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(predicate, nameof(predicate));

			return source.Where(predicate).Take(count).Count() < count;
		}

		/// <summary>
		/// Determines whether a sequence contains more than <paramref name="count"/> elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence that contains elements to be counted.</param>
		/// <param name="count">The number to be compared to the <paramref name="source"/> elements count.</param>
		/// <returns>True if the <paramref name="source"/> contains more than <paramref name="count"/> elements; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="source"/> is <see langword="null"/>
		/// </exception>
		public static bool IsCountMoreThan<TSource>(this IEnumerable<TSource> source, int count)
		{
			if (!(((source as ICollection<TSource>) != null || (source as ICollection) != null) || source != null))
			{
				throw new ArgumentNullException(nameof(source));
			}

			return (CountWithoutIterating(source) ?? source.Take(count + 1).Count()) > count;
		}

		/// <summary>
		/// Determines whether a sequence contains more than <paramref name="count"/> elements that satisfy a condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence that contains elements to be tested and counted.</param>
		/// <param name="count">The number to be compared to the <paramref name="source"/> elements count.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>True if the <paramref name="source"/> contains more than <paramref name="count"/> elements that satisfy a condition; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>
		/// </exception>
		public static bool IsCountMoreThan<TSource>(this IEnumerable<TSource> source, int count, Func<TSource, bool> predicate)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(predicate, nameof(predicate));

			return source.Where(predicate).Skip(count).Any();
		}

		/// <summary>
		/// Performs the specified action on each element of the <paramref name="source"/>.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence to process.</param>
		/// <param name="action">The delegate to perform on each element of the <paramref name="source"/>.</param>
		public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
		{
			if (source != null && action != null)
			{
				foreach (TSource item in source)
				{
					action(item);
				}
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the <paramref name="source"/>.by incorporating the element's index.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of the <paramref name="source"/>.</typeparam>
		/// <param name="source">A sequence to process.</param>
		/// <param name="action">The delegate to perform on each element of the <paramref name="source"/> by incorporating the element's index.</param>
		public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
		{
			if (source != null && action != null)
			{
				foreach (var item in source.Select((value, idx) => (value, idx)))
				{
					action(item.value, item.idx);
				}
			}
		}

		/// <summary>
		/// This function flattens a tree of items with the same type by recursively applying the selector on all
		/// elements of a tree until no more elements are returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="node"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<T> SelectRecursive<T>(this T node, Func<T, IEnumerable<T>> selector)
		{
			Argument.NotNull(selector, nameof(selector)); // Suggested By ReviewBot
			var nodeCollection = selector(node);
			return nodeCollection.Union(nodeCollection.SelectMany(t => t.SelectRecursive(selector)));
		}

		/// <summary>
		/// This function flattens a hierarchy of items with the same type by recursively applying the selector until null is returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="node"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<T> SelectUntilNull<T>(this T node, Func<T, T> selector)
			where T : class
		{
			T result = node;

			while ((result = selector(result)) != null)
			{
				yield return result;
			}
		}

		public static IEnumerable<T> SelectUntilNullSafely<T>(this T node, Func<T, T> selector, IEqualityComparer<T> comparer = null)
			where T : class
		{
			T result = node;
			var set = new HashSet<T>(Enumerable.Empty<T>(), comparer ?? EqualityComparer<T>.Default);

			while ((result = selector(result)) != null)
			{
				if (set.Add(result))
				{
					yield return result;
				}
				else
				{
					yield break;
				}
			}
		}

		public static IEnumerable<T> SelectDistinctRecursive<T, TCollection>(this IEnumerable<T> entities, Func<T, TCollection> selector, IEqualityComparer<T> comparer = null)
			where TCollection : IEnumerable<T>
		{
			Argument.NotNull(selector, nameof(selector));
			var set = new HashSet<T>(entities, comparer ?? EqualityComparer<T>.Default);
			var queue = new Queue<T>(set);

			while (queue.Count > 0)
			{
				var item = queue.Dequeue();

				foreach (var child in selector(item))
				{
					if (set.Add(child))
					{
						queue.Enqueue(child);
					}
				}
			}

			return set;
		}

		public static IEnumerable<T> DistinctDepthFirstHeirarchyTraversal<T, TCollection>(this IEnumerable<T> entities, Func<T, TCollection> selector, IEqualityComparer<T> comparer = null)
			where TCollection : IEnumerable<T>
		{
			Argument.NotNull(selector, nameof(selector));
			var set = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);
			var stack = new Stack<T>(entities.Reverse());

			while (stack.Count > 0)
			{
				var item = stack.Pop();
				if (item != null && set.Add(item))
				{
					yield return item;

					foreach (var child in selector(item).Reverse())
					{
						stack.Push(child);
					}
				}
			}
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var seenKeys = new HashSet<TKey>();
			foreach (var element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> sequence)
		{
			Argument.NotNull(sequence, nameof(sequence)); // Suggested By ReviewBot
			return sequence.Where(e => e != null);
		}

		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
			where T : struct
		{
			Argument.NotNull(sequence, nameof(sequence)); // Suggested By ReviewBot
			return sequence.Where(e => e != null).Select(e => e.Value);
		}

		public static IEnumerable<T> Append<T>(this IEnumerable<T> sequence, params T[] elements)
		{
			Argument.NotNull(sequence, nameof(sequence)); // Suggested By ReviewBot
			Argument.NotNull(elements, nameof(elements)); // Suggested By ReviewBot
			return sequence.Concat(elements);
		}

		public static IEnumerable<T> Except<T>(this IEnumerable<T> sequence, T single)
		{
			Argument.NotNull(sequence, nameof(sequence)); // Suggested By ReviewBot
			return sequence.Except(new[] { single });
		}

		public static IEnumerable<LinkedListNode<T>> Nodes<T>(this LinkedList<T> list)
		{
			for (var node = list.First; node != null && node != list.Last.Next; node = node.Next)
			{
				yield return node;
			}
		}

		public static IEnumerable<T> Yield<T>(this T item)
		{
			if (item != null)
			{
				yield return item;
			}
			else
			{
				yield break;
			}
		}

		public static bool ContainsSameElementsInAnyOrder<T>(this IEnumerable<T> items, IEnumerable<T> other)
		{
			Argument.NotNull(items, nameof(items));
			Argument.NotNull(other, nameof(other));

			var otherCollection = new LinkedList<T>(other);
			foreach (var item in items)
			{
				if (!otherCollection.Remove(item))
				{
					return false;
				}
			}

			return !otherCollection.Any();
		}

		public static bool ContainsSameElementsInAnyOrder<T>(this IEnumerable<T> items, IEnumerable<T> other, Func<T, T, bool> areEqual)
		{
			Argument.NotNull(items, nameof(items));
			Argument.NotNull(other, nameof(other));
			Argument.NotNull(areEqual, nameof(areEqual));
			var otherCollection = new LinkedList<T>(other);
			foreach (var item in items)
			{
				bool wasRemoved = false;
				for (var node = otherCollection.First; node != null; node = node.Next)
				{
					if (areEqual(item, node.Value))
					{
						otherCollection.Remove(node);
						wasRemoved = true;

						break;
					}
				}

				if (!wasRemoved)
				{
					return false;
				}
			}

			return !otherCollection.Any();
		}

		/// <summary>
		/// This is like TakeWhile(); Take everything until the previous element matches the predicate.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> items, Func<T, bool> predicate)
		{
			Argument.NotNull(items, nameof(items));
			Argument.NotNull(predicate, nameof(predicate));

			foreach (var item in items)
			{
				yield return item;
				if (predicate(item))
				{
					break;
				}
			}
		}

		#region IndexOf

		public static int IndexOf<T>(this IEnumerable<T> items, Predicate<T> predicate)
		{
			int i = 0;

			foreach (var item in items)
			{
				if (predicate(item))
				{
					return i;
				}
				i++;
			}

			return -1;
		}

		#endregion

		#region Chunk
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int batchSize)
		{
			using (var enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					yield return NextChunk(enumerator, batchSize - 1);
				}

				IEnumerable<T> NextChunk(IEnumerator<T> list, int chunkSize)
				{
					yield return list.Current;
					for (int i = 0; i < chunkSize && list.MoveNext(); i++)
					{
						yield return list.Current;
					}
				}
			}
		}
		#endregion

		#region Max/Min By

		public static IList<TSource> CollectMaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(selector, nameof(selector));
			return CollectMaxOrMinByCore(source, selector, comparer, max: true);
		}

		public static IList<TSource> CollectMinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(selector, nameof(selector));
			return CollectMaxOrMinByCore(source, selector, comparer, max: false);
		}

		static IList<TSource> CollectMaxOrMinByCore<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, bool max)
		{
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot
			Argument.NotNull(selector, nameof(selector));
			var sourceList = new List<TSource>();
			var currentKey = default(TKey);
			var action = new Action<TSource, TKey>((s, k) =>
			{
				if (!EqualityComparer<TKey>.Default.Equals(currentKey, k))
				{
					sourceList.Clear();
					currentKey = k;
				}

				sourceList.Add(s);
			});

			MaxOrMinByCore(source, selector, action, comparer, throwIfSequenceEmpty: false, max: max, findLastMax: true);

			return sourceList;
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
		{
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot
			Argument.NotNull(selector, nameof(selector));
			var item = default(TSource);
			MaxOrMinByCore(source, selector, (s, _) => item = s, comparer, throwIfSequenceEmpty: true, max: false);
			return item;
		}

		public static TSource MinBySafe<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
		{
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot
			Argument.NotNull(selector, nameof(selector));
			var item = default(TSource);
			MaxOrMinByCore(source, selector, (s, _) => item = s, comparer, throwIfSequenceEmpty: false, max: false);
			return item;
		}

		public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
		{
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot
			Argument.NotNull(selector, nameof(selector));
			var item = default(TSource);
			MaxOrMinByCore(source, selector, (s, _) => item = s, comparer, throwIfSequenceEmpty: true, max: true);
			return item;
		}

		public static TSource MaxBySafe<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
		{
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot
			Argument.NotNull(selector, nameof(selector));
			var item = default(TSource);
			MaxOrMinByCore(source, selector, (s, _) => item = s, comparer, throwIfSequenceEmpty: false, max: true);
			return item;
		}

		static TKey MaxOrMinByCore<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> selector, Action<TSource, TKey> handleNewMax, IComparer<TKey> comparer = null, bool throwIfSequenceEmpty = true, bool max = true, bool findLastMax = false)
		{
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(selector, nameof(selector));
			Argument.NotNull(handleNewMax, nameof(handleNewMax));

			using (var sourceIterator = source.GetEnumerator())
			{
				if (!sourceIterator.MoveNext())
				{
					if (throwIfSequenceEmpty)
					{
						throw new InvalidOperationException("Sequence was empty");
					}
					else
					{
						return default(TKey);
					}
				}

				TSource item = sourceIterator.Current;
				TKey key = selector(item);
				handleNewMax(item, key);

				if (comparer == null)
				{
					comparer = Comparer<TKey>.Default;
				}

				while (sourceIterator.MoveNext())
				{
					TSource candidate = sourceIterator.Current;
					TKey candidateProjected = selector(candidate);

					var comparisonResult = comparer.Compare(candidateProjected, key);

					if ((max && comparisonResult > 0) || (!max && comparisonResult < 0) || (findLastMax && comparisonResult == 0))
					{
						item = candidate;
						key = candidateProjected;
						handleNewMax(item, key);
					}
				}

				return key;
			}
		}

		public static TResult MaxOrDefault<TItem, TResult>(this IEnumerable<TItem> sequence, Func<TItem, TResult> selector)
		{
			Argument.NotNull(sequence, nameof(sequence)); // Suggested By ReviewBot
			Argument.NotNull(selector, nameof(selector));
			return MaxOrMinByCore(sequence, selector, (_, x_) => { }, throwIfSequenceEmpty: false, max: true);
		}

		public static TResult MinOrDefault<TItem, TResult>(this IEnumerable<TItem> sequence, Func<TItem, TResult> selector)
		{
			Argument.NotNull(sequence, nameof(sequence)); // Suggested By ReviewBot
			Argument.NotNull(selector, nameof(selector));
			return MaxOrMinByCore(sequence, selector, (_, x_) => { }, throwIfSequenceEmpty: false, max: false);
		}

		#endregion

		#region ConsecutivePairs

		/// <summary>
		/// Given an unordered set of items to be ordered by a sequence number, return all pairs that are consecutive.
		/// e.g. given the input [1, 2, 3, 4, 5], this would return [[1, 2], [2, 3], [3, 4], [4, 5]]
		/// This function stills just fine when items have the same sequence.
		/// e.g. given the input [1, 2, 2, 3, 3], this would return [[1, 2], [1, 2], [2, 3], [2, 3], [2, 3], [2, 3]].
		///
		/// hint: This function is pretty good for finding consecutive ProcessTasks.
		/// </summary>
		public static IEnumerable<(T, T)> ConsecutivePairs<T>(this IEnumerable<T> sequence, Func<T, int> getSequenceNumber)
		{
			Argument.NotNull(sequence, nameof(sequence));
			Argument.NotNull(getSequenceNumber, nameof(getSequenceNumber));

			var orderedItems = sequence.OrderBy(getSequenceNumber).ToList();

			var i = 0;
			while (i < orderedItems.Count)
			{
				var j = i;
				while (j < orderedItems.Count && getSequenceNumber(orderedItems[i]) == getSequenceNumber(orderedItems[j]))
				{
					j++;
				}

				var k = j;
				while (k < orderedItems.Count && getSequenceNumber(orderedItems[j]) == getSequenceNumber(orderedItems[k]))
				{
					k++;
				}

				// i to j is the span of items with the first identical sequence number
				// j to k is the span of items with the second identical sequence number

				for (var m = i; m < j; m++)
				{
					for (var n = j; n < k; n++)
					{
						yield return (orderedItems[m], orderedItems[n]);
					}
				}

				i = j;
			}
		}

		#endregion

		#region Dictionary Builders

		public static Dictionary<TKey, List<TItem>> ToKeyListDictionary<TKey, TItem>(this IEnumerable<TItem> enumeration, Func<TItem, TKey> keyCreator)
		{
			Argument.NotNull(enumeration, nameof(enumeration));
			Argument.NotNull(keyCreator, nameof(keyCreator));

			var result = new Dictionary<TKey, List<TItem>>();

			foreach (var item in enumeration)
			{
				var key = keyCreator(item);
				List<TItem> list;

				if (!result.TryGetValue(key, out list))
				{
					result.Add(key, list = new List<TItem>());
				}
				list.Add(item);
			}

			return result;
		}

		#endregion

		#region In

		public static bool In<TItem>(this TItem source, Func<TItem, TItem, bool> comparer, IEnumerable<TItem> items)
		{
			Argument.NotNull(items, nameof(items));
			return items.Any(item => comparer(source, item));
		}

		public static bool In<TItem, T>(this TItem source, Func<TItem, T> selector, IEnumerable<TItem> items)
		{
			Argument.NotNull(items, nameof(items));
			Argument.NotNull(selector, nameof(selector));
			return items.Select(selector).Contains(selector(source));
		}

		public static bool In<T>(this T source, IEnumerable<T> items)
		{
			Argument.NotNull(items, nameof(items));
			return items.Contains(source);
		}

		public static bool In<TItem>(this TItem source, Func<TItem, TItem, bool> comparer, params TItem[] items)
		{
			Argument.NotNull(items, nameof(items));
			return source.In(comparer, (IEnumerable<TItem>)items);
		}

		public static bool In<TItem, T>(this TItem source, Func<TItem, T> selector, params TItem[] items)
		{
			Argument.NotNull(items, nameof(items));
			Argument.NotNull(selector, nameof(selector));
			return source.In(selector, (IEnumerable<TItem>)items);
		}

		public static bool In<T>(this T source, params T[] items)
		{
			Argument.NotNull(items, nameof(items));
			return source.In((IEnumerable<T>)items);
		}

		#endregion

		#region Next/Prev

		public static T ElementAfter<T>(this IEnumerable<T> sequence, T element, IEqualityComparer<T> comparer = null)
		{
			var notNullComparer = comparer ?? EqualityComparer<T>.Default;
			var found = false;

			foreach (var item in sequence)
			{
				if (found)
				{
					return item;
				}

				if (notNullComparer.Equals(element, item))
				{
					found = true;
				}
			}

			return default;
		}

		public static T ElementInFrontOf<T>(this IEnumerable<T> sequence, T element, IEqualityComparer<T> comparer = null)
		{
			var notNullComparer = comparer ?? EqualityComparer<T>.Default;
			T prev = default;

			foreach (var item in sequence)
			{
				if (notNullComparer.Equals(item, element))
				{
					return prev;
				}

				prev = item;
			}

			return default;
		}

		#endregion

		#region Split

		public static SplitSet<T> Split<T>(this IEnumerable<T> collection, Func<T, bool> matchOperator)
		{
			Argument.NotNull(collection, nameof(collection));
			Argument.NotNull(matchOperator, nameof(matchOperator));

			var truthy = new List<T>();
			var falsy = new List<T>();

			foreach (var element in collection)
			{
				if (matchOperator(element))
				{
					truthy.Add(element);
				}
				else
				{
					falsy.Add(element);
				}
			}

			return new SplitSet<T>(truthy, falsy);
		}

		#endregion

		#region Modified foreach

		public static void ForEachWithBetween<TItem>(this IEnumerable<TItem> items, Action<TItem> fn, Action between)
		{
			var itr = items.GetEnumerator();
			if (itr.MoveNext())
			{
				var current = itr.Current;
				var next = current;
				while (itr.MoveNext())
				{
					current = next;
					next = itr.Current;
					fn(current);
					between();
				}

				fn(next);
			}
		}

		public static IEnumerable<TOut> SelectWithBetween<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> fn, Func<TOut> between)
		{
			var itr = items.GetEnumerator();
			if (itr.MoveNext())
			{
				yield return fn(itr.Current);

				while (itr.MoveNext())
				{
					yield return between();
					yield return fn(itr.Current);
				}
			}
		}

		#endregion

		#region SearchWithFallback

		public static IEnumerable<TElement> SearchWithFallback<TElement, TKey>(this IEnumerable<TElement> items, Func<TElement, TKey> getKey, params TKey[] keys)
		{
			return SearchWithFallback(items, getKey, keys, EqualityComparer<TKey>.Default);
		}

		public static IEnumerable<TElement> SearchWithFallback<TElement, TKey>(this IEnumerable<TElement> items, Func<TElement, TKey> getKey, TKey[] keys, IEqualityComparer<TKey> equalityComparer)
		{
			var lookup = items.ToLookup(getKey, equalityComparer);
			foreach (var key in keys)
			{
				if (lookup.Contains(key))
				{
					return lookup[key];
				}
			}
			return Enumerable.Empty<TElement>();
		}

		#endregion
	}

	public class SplitSet<T>
	{
		internal SplitSet(IEnumerable<T> matchingSet, IEnumerable<T> nonMatchingSet)
		{
			MatchingSet = matchingSet;
			NonMatchingSet = nonMatchingSet;
		}

		public IEnumerable<T> MatchingSet { get; }
		public IEnumerable<T> NonMatchingSet { get; }
	}
}
