using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.EntityFramework
{
	abstract class IndexCache<TKey, TIndex>
	{
		protected abstract TIndex GetNewIndex(TKey key);

		protected TIndex FindDataViewCore(TKey key)
		{
			CleanUpNullTarget();

			if (cache.TryGetValue(key, out var viewRef))
			{
				var result = (TIndex)viewRef.Target;
				if (result == null)
				{
					cache.Remove(key);
				}
				return result;
			}
			else
			{
				return default;
			}
		}

		protected TIndex GetDataViewCore(TKey key)
		{
			lock (this)
			{
				var result = FindDataViewCore(key);
				if (result == null)
				{
					result = GetNewIndex(key);
					cache.Add(key, new WeakReference(result));
				}
				return result;
			}
		}

		protected void RemoveDataViewCore(TIndex index)
		{
			var keysToRemove = cache.Where(item => index.Equals(item.Value.Target)).Select(item => item.Key).ToArray();
			foreach (var keyToRemove in keysToRemove)
			{
				cache.Remove(keyToRemove);
			}
		}

		void CleanUpNullTarget()
		{
			void CleanupCache<T>(Dictionary<T, WeakReference> dict)
			{
				var nullReferenceList = dict.Where(pair => pair.Value.Target == null).Select(pair => pair.Key).ToList();
				nullReferenceList.ForEach(key => dict.Remove(key));
			}

			if (nextCleanUpRun < DateTime.UtcNow)
			{
				CleanupCache(cache);
				nextCleanUpRun = DateTime.UtcNow.AddMinutes(CleanUpIntervalMinutes);
			}
		}

		protected internal const int CleanUpIntervalMinutes = 5;
		protected internal DateTime nextCleanUpRun = DateTime.UtcNow.AddSeconds(-1); // Use DateTime instead of ZDateTime for performance
		protected internal readonly Dictionary<TKey, WeakReference> cache = new Dictionary<TKey, WeakReference>();
	}

	sealed class IndexCacheDataTableMixin
	{
		public IndexCacheDataTableMixin(DataTable table)
		{
			SharedDataTableIndex = new SharedDataTableIndexCache(table);
			DataViewCache = new DataViewCache(table);
		}

		public SharedDataTableIndexCache SharedDataTableIndex { get; }
		public DataViewCache DataViewCache { get; }
		internal static IndexCacheDataTableMixin GetInstance(DataTable table)
		{
			Argument.NotNull(table, "table");

			var result = table.ExtendedProperties[ExtendedPropertiesKey] as IndexCacheDataTableMixin;
			if (result == null)
			{
				result = new IndexCacheDataTableMixin(table);
				table.ExtendedProperties[ExtendedPropertiesKey] = result;
			}
			return result;
		}

		[SuppressThreadStaticFieldMessage]
		static readonly object ExtendedPropertiesKey = new object();
	}
}
