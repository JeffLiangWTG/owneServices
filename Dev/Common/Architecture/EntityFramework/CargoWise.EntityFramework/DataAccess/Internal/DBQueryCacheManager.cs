using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Async;
using CargoWise.Common.Collections;

namespace CargoWise.EntityFramework
{
	public class DBQueryCacheManager
	{
		public DBQueryCacheManager(IThreadSentry sentry = null)
		{
			internalManager = new DBQueryCacheManager_Unsafe();
			this.sentry = sentry ?? ThreadSentryProvider.GetThreadSentry(false);
		}
		readonly DBQueryCacheManager_Unsafe internalManager;
		internal readonly IThreadSentry sentry;
		readonly DistinctConcurrentQueue<string> clearTables = new DistinctConcurrentQueue<string>(StringComparer.OrdinalIgnoreCase);
		volatile bool clear;
		volatile bool clearViews;

		#region Public API

		#region Threadsafe operations

		public void Clear()
		{
			clear = true;
		}

		public void Clear(string tableName)
		{
			clearTables.TryEnqueue(tableName);
		}

		public void ClearViewsQueries()
		{
			clearViews = true;
		}

		#endregion

		#region Thread Local Operations

		public DataRow[] GetCachedValue(string tableName, ZQuery filter)
		{
			if (filter.IgnoreDbQueryCache)
			{
				return null;
			}
			else
			{
				sentry.EnsureCurrentThreadIsOwner();
				ExecuteQueue();
				return internalManager.GetCachedValue(tableName, filter);
			}
		}

		public void Store(DataRow[] cachedValue, string tableName, ZQuery filter)
		{
			sentry.EnsureCurrentThreadIsOwner();
			ExecuteQueue();
			internalManager.Store(cachedValue, tableName, filter);
		}

		#endregion

		#endregion

		void ExecuteQueue()
		{
			if (clearViews)
			{
				internalManager.ClearViewsQueries();
				clearViews = false;
			}
			if (clear)
			{
				internalManager.Clear();
				clear = false;
			}
			while (clearTables.TryDequeue(out var key))
			{
				internalManager.Clear(key);
			}
		}
	}

	internal class DBQueryCacheManager_Unsafe
	{
		public DBQueryCacheManager_Unsafe()
		{
			cache = new Dictionary<string, DataRow[]>(StringComparer.OrdinalIgnoreCase);
			viewsKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		internal readonly Dictionary<string, DataRow[]> cache;
		readonly HashSet<string> viewsKeys;
		internal const string Separator = "^";

		#region Clear

		public void Clear()
		{
			cache.Clear();
		}

		public void Clear(string tableName)
		{
			foreach (string key in cache.Keys.Where(key => key.IndexOf(tableName, StringComparison.OrdinalIgnoreCase) >= 0).ToList())
			{
				cache.Remove(key);
			}
		}

		public void ClearViewsQueries()
		{
			foreach (string viewKey in viewsKeys)
			{
				cache.Remove(viewKey);
			}
			viewsKeys.Clear();
		}

		#endregion

		public DataRow[] GetCachedValue(string tableName, ZQuery filter)
		{
			DataRow[] result = null;
			if (!filter.ReLoadExistingRows)
			{
				var key = GetKey(tableName, filter);
				cache.TryGetValue(key, out result);

				if (result == null)
				{
					key = GetKey(tableName, filter, true);
					cache.TryGetValue(key, out result);
				}

				if (result != null && result.Any(row => row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached))
				{
					result = result.Where(row => row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached).ToArray();
					cache[key] = result; // replace cached value so perf hit only taken once
				}
			}
			return result;
		}

		internal string GetKey(string tableName, ZQuery filter, bool switchBrackets = false)
		{
			string filterText = filter.LiteralTextADO;

			if (switchBrackets)
			{
				if (filterText.StartsWith("(", StringComparison.OrdinalIgnoreCase) && filterText.EndsWith(")", StringComparison.OrdinalIgnoreCase))
				{
					filterText = filterText.Substring(1, filterText.Length - 2);
				}
				else
				{
					filterText = "(" + filterText + ")";
				}
			}

			return tableName + Separator + filterText + filter.MaximumRows.ToString() + filter.OrderBy;
		}

		public void Store(DataRow[] cachedValue, string tableName, ZQuery filter)
		{
			var key = GetKey(tableName, filter);

			if ((tableName.StartsWith("VIEW", StringComparison.OrdinalIgnoreCase) || tableName.StartsWith("VW_", StringComparison.OrdinalIgnoreCase)))
			{
				viewsKeys.Add(key);
			}

			cache[key] = cachedValue;
		}
	}
}
