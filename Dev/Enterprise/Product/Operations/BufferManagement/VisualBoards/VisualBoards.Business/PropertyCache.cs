using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.VisualBoards.Business
{
	public class PropertyCache
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		public T GetCachedValue<T>(ZGuid keyPK, string propertyName, Func<T> valueGetter)
		{
			var key = GetKey(keyPK, propertyName);

			object val = null;
			if (propertyCache.TryGetValue(key, out val))
			{
				return (T)val;
			}
			else
			{
				return (T)(propertyCache[key] = valueGetter());
			}
		}

		public T GetCachedValue<T>(ZGuid keyPK, string propertyName)
		{
			var key = GetKey(keyPK, propertyName);

			object result;
			return propertyCache.TryGetValue(key, out result) ? (T)result : default(T);
		}

		public T OverwriteCachedValue<T>(ZGuid keyPK, string propertyName, T value)
		{
			var key = GetKey(keyPK, propertyName);
			propertyCache[key] = value;
			return value;
		}

		public void Clear()
		{
			var cache = propertyCache;

			if (cache.Count > 0)
			{
				cache.Clear();
			}
		}

		public bool Remove(ZGuid keyPK, string propertyName)
		{
			var key = GetKey(keyPK, propertyName);

			object notUsed;
			return propertyCache.TryRemove(key, out notUsed);
		}

		static Tuple<string, ZGuid> GetKey(ZGuid keyPK, string propertyName)
		{
			return Tuple.Create(propertyName, keyPK);
		}

		volatile ConcurrentDictionary<Tuple<string, ZGuid>, object> propertyCache = new ConcurrentDictionary<Tuple<string, ZGuid>, object>();

#if DEBUG
		public Dictionary<string, object> GetDumpOfCacheForTest()
		{
			return propertyCache.ToDictionary(t => t.Key.Item2.ToString() + t.Key.Item1, t => t.Value);
		}

#endif

		public void ReplaceWith(PropertyCache newCache)
		{
			propertyCache = newCache.propertyCache;
		}

		public void MergeWith(PropertyCache newCache)
		{
			var oldCache = propertyCache;

			foreach (var item in newCache.propertyCache)
			{
				oldCache[item.Key] = item.Value;
			}
		}
	}
}
