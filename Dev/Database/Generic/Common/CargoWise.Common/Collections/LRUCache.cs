using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Common.MemoryManagement;

namespace CargoWise.Common.Collections
{
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LRU")]
	public interface ILRUCache
	{
		void Clear();
	}

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LRU")]
	public static class LRUCache
	{
		internal static void NotifyAllocated(ILRUCache instance)
		{
			lock (allInstancesMutex)
			{
				allInstances.Add(new WeakReference(instance));
			}
		}

		/// <summary>
		/// TESTING ONLY - Get all instances of type LRUCache in the current AppDomain.
		/// </summary>
		public static IEnumerable<ILRUCache> AllInstances
		{
			get
			{
				lock (allInstancesMutex)
				{
					for (int i = allInstances.Count - 1; i >= 0; i--)
					{
						WeakReference instanceRef = allInstances[i];
						ILRUCache instance = instanceRef.Target as ILRUCache;
						if (instance != null)
						{
							yield return instance;
						}
						else
						{
							allInstances.RemoveAt(i);
						}
					}
				}
			}
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Lock)]
		static readonly List<WeakReference> allInstances = new List<WeakReference>();

		static readonly object allInstancesMutex = new object();
	}

	/// <summary>
	/// A least recently used (LRU) cache of objects.
	/// - A small number of objects are strongly referenced and readily available.
	/// - Least recently used objects are available in a hash table and are weak referenced.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "LRU")]
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public class LRUCache<TKey, TValue> : ILRUCache
	{
		public FlushResult ReclaimMemory(FlushAction action)
		{
			lock (mutex)
			{
				if (action == FlushAction.Full)
				{
					Clear();
					return FlushResult.Exhausted;
				}
				else if (weakCache.Count > 0)
				{
					// Remove 'first' iem from cache
					foreach (var kvp in weakCache)
					{
						weakCache.Remove(kvp.Key);
						break;
					}
				}
				else if (strongCache.Count > 0)
				{
					RemoveAt(strongCache.Count - 1);
				}

				return weakCache.Count == 0 && strongCache.Count == 0 ? FlushResult.Exhausted : FlushResult.Partial;
			}
		}

		public int StrongCacheCount
		{
			get
			{
				lock (mutex)
				{
					return strongCache.Count;
				}
			}
		}

		public int WeakCacheCount
		{
			get
			{
				lock (mutex)
				{
					return weakCache.Count;
				}
			}
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public LRUCache()
			: this(EqualityComparer<TKey>.Default, 100)
		{
		}

		/// <summary>
		/// Constructor to allow for initial strong reference capacity
		/// </summary>
		/// <param name="strongReferenceCount"></param>
		public LRUCache(int strongReferenceCount)
			: this(EqualityComparer<TKey>.Default, strongReferenceCount)
		{
			if ((strongReferenceCount + 1) < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(strongReferenceCount));
			}
		}

		/// <summary>
		/// Constructor to allow for initial strong reference capacity and customized equal comparer
		/// </summary>
		/// <param name="equalityComparer"></param>
		/// <param name="strongReferenceCount"></param>
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "strongReferenceCount+1")]
		public LRUCache(IEqualityComparer<TKey> equalityComparer, int strongReferenceCount)
		{
			if ((strongReferenceCount + 1) < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(strongReferenceCount));
			}

			if (strongReferenceCount == int.MaxValue)
			{
				throw new ArgumentException("strongReferenceCount cannot be int.MaxValue", nameof(strongReferenceCount));
			}

			Argument.NotNull(equalityComparer, nameof(equalityComparer));
			this.equalityComparer = equalityComparer;
			this.strongReferenceCount = strongReferenceCount;
			mutex = new object();
			strongCache = new List<KeyValuePairRef<TKey, TValue>>(strongReferenceCount + 1);
			weakCache = new Dictionary<TKey, WeakReference>(equalityComparer);
			LRUCache.NotifyAllocated(this);
			var description = Regex.Replace(this.GetType().FullName, "(, (Version=[0-9.]+|culture=[a-z0-9]+|PublicKeyToken=[0-9a-f]+))+", string.Empty, RegexOptions.IgnoreCase);
			MemoryManager.Register(description, this, FlushCallback.OnAnyThread, ReclaimMemory);
		}

		static FlushResult ReclaimMemory(LRUCache<TKey, TValue> cache, FlushAction action)
		{
			Argument.NotNull(cache, nameof(cache)); // Suggested By ReviewBot 
			return cache.ReclaimMemory(action);
		}

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TValue this[TKey key]
		{
			get
			{
				Argument.NotNull(key, nameof(key));

				TValue result = default(TValue);
				TryGetValue(key, out result);
				return result;
			}
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get.</param>
		/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <exception cref="ArgumentNullException">key is null.</exception>
		/// <returns>true if the cache contains an element with the specified key; otherwise, false.</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			Argument.NotNull(key, nameof(key));

			bool result = false;
			result = TryGetValueFromStrongCache(key, out value);
			if (!result)
			{
				lock (mutex)
				{
					result = TryGetValueFromWeakCache(key, out value);
					if (!result)
					{
						value = default(TValue);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Add a key value pair to the cache
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(TKey key, TValue value)
		{
			Argument.NotNull(key, nameof(key));
			Argument.NotNull(value, nameof(value));
			lock (mutex)
			{
				try
				{
					Remove(key);
					strongCache.Insert(0, new KeyValuePairRef<TKey, TValue>(key, value));
					if (strongCache.Count > strongReferenceCount && strongReferenceCount >= 0)
					{
						RemoveAt(strongReferenceCount);
						if (nextRemoveGCdEntriesIndex++ > 100)
						{
							nextRemoveGCdEntriesIndex = 0;
							ScrubWeakReferences();
						}
					}
				}
				catch (IndexOutOfRangeException ex)
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Possible concurrency problem. strongCache has {0} elements. weakCache has {1} elements.", strongCache.Count, weakCache.Count), ex);
				}
			}
		}

		void RemoveAt(int index)
		{
			KeyValuePairRef<TKey, TValue> lastItem = strongCache[index];
			if (lastItem != null)
			{
				weakCache[lastItem.Key] = new WeakReference(lastItem.Value);
				strongCache.RemoveAt(index);
			}
		}

		/// <summary>
		/// Remove a key from the cache.
		/// </summary>
		public void Remove(TKey key)
		{
			Argument.NotNull(key, nameof(key));

			lock (mutex)
			{
				weakCache.Remove(key);
				for (int i = 0; i < strongCache.Count; i++)
				{
					if (strongCache[i] != null && equalityComparer.Equals(key, strongCache[i].Key))
					{
						strongCache.RemoveAt(i);
					}
				}
			}
		}

		/// <summary>
		/// Clear caches
		/// </summary>
		public void Clear()
		{
			lock (mutex)
			{
				strongCache.Clear();
				weakCache.Clear();
			}
		}

		#region TryGetValueFromStrongCache / TryGetValueFromWeakCache

		bool TryGetValueFromStrongCache(TKey key, out TValue value)
		{
			for (int i = 0; i < strongCache.Count; i++)
			{
				try
				{
					KeyValuePairRef<TKey, TValue> item = strongCache[i];
					if (item != null && equalityComparer.Equals(key, item.Key))
					{
						if (i != 0)
						{
							TrySwap(strongCache, 0, i);
						}
						value = item.Value;
						return true;
					}
				}
				catch (ArgumentOutOfRangeException)
				{
				}
			}
			value = default(TValue);
			return false;
		}

		bool TryGetValueFromWeakCache(TKey key, out TValue value)
		{
			Argument.NotNull(key, nameof(key));

			bool result = false;
			value = default(TValue);

			WeakReference valueRef;
			weakCache.TryGetValue(key, out valueRef);
			object valueObj = valueRef == null ? null : valueRef.Target;
			if (valueObj != null)
			{
				value = (TValue)valueObj;
				weakCache.Remove(key);
				Add(key, value);
				result = true;
			}
			return result;
		}

		#endregion

		#region class KeyValuePairRef

		class KeyValuePairRef<TKey2, TValue2>
		{
			public KeyValuePairRef(TKey2 key, TValue2 value)
			{
				Argument.NotNull(key, nameof(key));
				Argument.NotNull(value, nameof(value));

				this.key = key;
				this.Value = value;
			}

			public TKey2 Key
			{
				get
				{
					return key;
				}
			}

			readonly TKey2 key;
			public readonly TValue2 Value;
		}

		#endregion

		#region Implementation

		readonly IEqualityComparer<TKey> equalityComparer;
		readonly List<KeyValuePairRef<TKey, TValue>> strongCache;
		readonly Dictionary<TKey, WeakReference> weakCache;
		readonly object mutex;
		readonly int strongReferenceCount;
		int nextRemoveGCdEntriesIndex;

		void ScrubWeakReferences()
		{
			List<TKey> keysToRemove = new List<TKey>();
			try
			{
				foreach (KeyValuePair<TKey, WeakReference> item in weakCache)
				{
					WeakReference valueRef = item.Value;
					if (valueRef != null && !valueRef.IsAlive)
					{
						keysToRemove.Add(item.Key);
					}
				}
			}
			catch (InvalidOperationException)
			{
			}
			foreach (TKey key in keysToRemove)
			{
				if (key != null)
				{
					weakCache.Remove(key);
				}
			}
		}

		static void TrySwap<T>(List<T> list, int i, int j)
		{
			Argument.NotNull(list, nameof(list)); // Suggested By ReviewBot 
			T tmp = list[i];
			list[i] = list[j];
			list[j] = tmp;
		}

		#endregion
	}
}
