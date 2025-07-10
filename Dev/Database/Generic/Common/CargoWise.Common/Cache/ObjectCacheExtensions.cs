using System;
using System.Runtime.Caching;

namespace CargoWise.Common.Cache
{
	public static class ObjectCacheExtensions
	{
		public static T GetOrAdd<T>(this ObjectCache cache,
									string key,
									Func<T> valueFactory,
									CacheItemPolicy policy)
		{
			var newValue = new Lazy<T>(valueFactory);
			var oldValue = cache.AddOrGetExisting(key, newValue, policy) as Lazy<T>;

			try
			{
				return (oldValue ?? newValue).Value;
			}
			catch
			{
				cache.Remove(key);
				throw;
			}
		}
	}
}
