using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common
{
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public class ConcurrentHashSet<T> : IEnumerable<T>
	{
		public ConcurrentHashSet()
		{
			dictionary = new ConcurrentDictionary<T, bool>();
		}

		public ConcurrentHashSet(IEnumerable<T> collection)
		{
			Argument.NotNull(collection, nameof(collection));
			dictionary = new ConcurrentDictionary<T, bool>(collection.Select(o => new KeyValuePair<T, bool>(o, true)));
		}

		public ConcurrentHashSet(IEqualityComparer<T> comparer)
		{
			Argument.NotNull(comparer, nameof(comparer));
			dictionary = new ConcurrentDictionary<T, bool>(comparer);
		}

		public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			Argument.NotNull(comparer, nameof(comparer));
			Argument.NotNull(collection, nameof(collection));
			dictionary = new ConcurrentDictionary<T, bool>(collection.Select(o => new KeyValuePair<T, bool>(o, true)), comparer);
		}

		public ConcurrentHashSet(int concurrencyLevel, int capacity)
		{
			if (concurrencyLevel < 1)
			{
				throw new System.ArgumentException("Invalid argument.", nameof(concurrencyLevel)); // Hard codded exception message
			}

			if (capacity < 0)
			{
				throw new System.ArgumentException("Invalid argument.", nameof(capacity)); // Hard codded exception message
			}

			dictionary = new ConcurrentDictionary<T, bool>(concurrencyLevel, capacity);
		}

		public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			Argument.NotNull(collection, nameof(collection));
			Argument.NotNull(comparer, nameof(comparer));
			if (concurrencyLevel < 1)
			{
				throw new System.ArgumentException("Invalid argument.", nameof(concurrencyLevel)); // Hard codded exception message
			}

			dictionary = new ConcurrentDictionary<T, bool>(concurrencyLevel, collection.Select(o => new KeyValuePair<T, bool>(o, true)), comparer);
		}

		public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer)
		{
			if (concurrencyLevel < 1)
			{
				throw new System.ArgumentException("Invalid argument.", nameof(concurrencyLevel)); // Hard codded exception message
			}

			Argument.NotNull(comparer, nameof(comparer));
			if (capacity < 0)
			{
				throw new System.ArgumentException("Invalid argument.", nameof(capacity)); // Hard codded exception message
			}

			dictionary = new ConcurrentDictionary<T, bool>(concurrencyLevel, capacity, comparer);
		}

		public int Count => dictionary.Count;

		public bool TryAdd(T item)
		{
			Argument.NotNull(item, nameof(item));
			return dictionary.TryAdd(item, true);
		}

		public bool Contains(T item)
		{
			Argument.NotNull(item, nameof(item));
			return dictionary.ContainsKey(item);
		}

		public bool TryRemove(T item)
		{
			Argument.NotNull(item, nameof(item));
			return dictionary.TryRemove(item, out _);
		}

		public void Clear()
		{
			dictionary.Clear();
		}

		#region IEnumerable<T>

		public IEnumerator<T> GetEnumerator()
		{
			return dictionary.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return dictionary.Keys.GetEnumerator();
		}

		#endregion

		readonly ConcurrentDictionary<T, bool> dictionary;
	}
}