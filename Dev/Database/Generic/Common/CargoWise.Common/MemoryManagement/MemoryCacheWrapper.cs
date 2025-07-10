using System;
using System.Linq;
using System.Runtime.Caching;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common.MemoryManagement
{
	[ThreadSafe]
	public abstract class MemoryCacheWrapper
	{
		readonly MemoryCache cache;
		readonly string keyPrefix;

		protected MemoryCacheWrapper()
		{
			cache = MemoryCache.Default;
			keyPrefix = GetType().Name;
		}

		public T GetCachedValue<T>(string key)
		{
			var cacheItem = cache.Get(GetRealKey(key));

			return cacheItem != null ? (T)cacheItem : default;
		}

		public T GetCachedOrCalculateValue<T>(string key, Func<T> valueCreator)
		{
			Argument.NotNull(valueCreator, nameof(valueCreator));

			var realKey = GetRealKey(key);
#if DEBUG
			if (CacheDelayForTest != TimeSpan.Zero)
			{
				System.Threading.Thread.Sleep(CacheDelayForTest);
			}
#endif
			var value = cache.Get(realKey);

			if (value != null)
			{
				return (T)value;
			}

			var result = valueCreator();
			AddToCache(realKey, key, result);

			return result;
		}

#if DEBUG
		public TimeSpan CacheDelayForTest = TimeSpan.Zero;
#endif

		public void AddToCache<T>(string key, T value)
		{
			AddToCache(GetRealKey(key), key, value);
		}

		public void SetInCache<T>(string key, T value)
		{
			SetInCache(GetRealKey(key), key, value);
		}

		public void Clear()
		{
			foreach (var item in cache.Where(kv => kv.Key != null && kv.Key.StartsWith(keyPrefix, StringComparison.Ordinal)).ToArray())
			{
				cache.Remove(item.Key);
			}
		}

		protected virtual TimeSpan GetAbsoluteExpiry(string key)
		{
			return TimeSpan.MaxValue;
		}

		protected virtual TimeSpan GetSlidingExpiration(string key)
		{
			return ObjectCache.NoSlidingExpiration;
		}

		#region Implementation

		protected string GetRealKey(string key)
		{
			return keyPrefix + "." + key;
		}

		void AddToCache(string realKey, string proposedKey, object value)
		{
			cache.Add(realKey, value, GetCacheItemPolicy(proposedKey));
		}

		void SetInCache(string realKey, string proposedKey, object value)
		{
			cache.Set(realKey, value, GetCacheItemPolicy(proposedKey));
		}

		CacheItemPolicy GetCacheItemPolicy(string proposedKey)
		{
			var absoluteExpiration = GetAbsoluteExpiry(proposedKey);
			var expirationTime = absoluteExpiration == TimeSpan.MaxValue ? ObjectCache.InfiniteAbsoluteExpiration : DateTimeOffset.UtcNow.Add(absoluteExpiration);
			var slidingExpiration = GetSlidingExpiration(proposedKey);

			return new CacheItemPolicy { AbsoluteExpiration = expirationTime, SlidingExpiration = slidingExpiration };
		}

		#endregion
	}
}
