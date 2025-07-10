using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	/// <summary>
	/// An IEnumerable that caches the results so that the first time it enumerates the inner enumerable,
	/// then subsequent times it enumerates the cache. The elements of the inner enumerable must not change
	/// while using this enumerable otherwise you may either get an exception or other unintended
	/// behaviour.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class CachedEnumerableWrapper<T> : IEnumerable<T>
	{
		IEnumerator<T> inner;
		readonly List<T> cache = new List<T>();

		public bool IsFullyEnumerated => inner == null;
		public int CacheCount => cache.Count;

		public CachedEnumerableWrapper(IEnumerable<T> inner)
		{
			Argument.NotNull(inner, nameof(inner));

			this.inner = inner.GetEnumerator();
		}

		public T this[int index]
		{
			get
			{
				if (!IsAvailable(index))
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				return cache[index];
			}
		}

		public bool IsAvailable(int index)
		{
			while (inner != null && index >= cache.Count)
			{
				if (inner.MoveNext())
				{
					cache.Add(inner.Current);
				}
				else
				{
					inner.Dispose();
					inner = null;
				}
			}

			return index >= 0 && index < cache.Count;
		}

		public void Flush()
		{
			while (!IsFullyEnumerated)
			{
				IsAvailable(cache.Count * 2);
			}
		}

		#region IEnumerable Members

		public IEnumerator<T> GetEnumerator() => IsFullyEnumerated ? cache.GetEnumerator() : new Enumerator(this);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		sealed class Enumerator : IEnumerator<T>
		{
			int currentIndex = -1;
			readonly CachedEnumerableWrapper<T> owner;

			public Enumerator(CachedEnumerableWrapper<T> owner)
			{
				this.owner = Argument.NotNull(owner, nameof(owner));
			}

			public T Current => owner[currentIndex];
			object IEnumerator.Current => Current;

			public void Dispose() { }
			public void Reset() => currentIndex = -1;
			public bool MoveNext() => owner.IsAvailable(++currentIndex);
		}

		#endregion

	}
}
