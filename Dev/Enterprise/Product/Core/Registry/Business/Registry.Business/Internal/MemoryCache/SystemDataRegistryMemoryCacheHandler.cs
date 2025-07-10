using System;
using System.Runtime.Caching;
using CargoWise.Common;

namespace Enterprise.Registry.Business
{
	public static class SystemDataRegistryMemoryCacheHandler
	{
		readonly static LazyOverridable<MemoryCache> MemCache = new LazyOverridable<MemoryCache>(() => new MemoryCache("SystemDataRegistryMemoryCache"));

		public static object GetCacheValue(string cacheKey)
		{
			return MemCache.Value.Get(cacheKey);
		}

		public static bool AddCachedValue(string cacheKey, object value, DateTimeOffset absoluteExpiration)
		{
			return MemCache.Value.Add(cacheKey, value, absoluteExpiration);
		}

		public static object ResetCache(string cacheKey)
		{
			return MemCache.Value.Remove(cacheKey);
		}
	}
}
