using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Async;
using CargoWise.Common.Collections;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// WARNING WARNING WARNING WARNING
	/// 
	/// This wrapper class exists because QueryCacheManger is accessed cross-thread.
	/// The strategy we are using is to create a thread safe queue so that things are only processed on their own thread.
	///
	/// WARNING WARNING WARNING WARNING
	/// </summary>
	class QueryCacheManager
	{
		public QueryCacheManager(IThreadSentry sentry = null)
		{
			this.sentry = sentry ?? ThreadSentryProvider.GetThreadSentry(false, takeStackTraces: false);
			internalManager = new QueryCacheManager_Unsafe();
			tablesToClear = new DistinctConcurrentQueue<string>(StringComparer.OrdinalIgnoreCase);
			viewsToClear = new DistinctConcurrentQueue<string>(StringComparer.OrdinalIgnoreCase);
		}
		readonly QueryCacheManager_Unsafe internalManager;
		internal readonly IThreadSentry sentry;

		// Using bespoke collections to track state so that factories that are never used do not have memory leaks.
		// Dictionaries clip off any duplicate entries.
		readonly DistinctConcurrentQueue<string> tablesToClear;
		readonly DistinctConcurrentQueue<string> viewsToClear;
		volatile bool clearAllTables;
		volatile bool clearViewsWithoutExplicitTableMapping;

		#region Public API

		#region Thread Safe

		public void Clear()
		{
			clearAllTables = true;
			// We don't bother flushing tablesToClear because that would be a race condition with ExecuteQueue
			// This way we can be lock free.
		}

		public void Clear(string tableName)
		{
			if (!clearAllTables)
			{
				tablesToClear.TryEnqueue(tableName);
			}
		}

		/// <summary>
		/// Confusingly, ClearViewsQueries follows a completely different pattern to Clear() and Clear(string tableName)
		/// ClearViewsQueries will clear views EXCEPT those that exist in QueryCacheManager_Unsafe.TableToViewMapping
		///
		/// The only time views in QueryCacheManager_Unsafe.TableToViewMapping will be cleared if the the tables in the map
		/// are explicitly passed in as arguments.
		/// </summary>
		/// <param name="tableNames"></param>
		public void ClearViewsQueries(IEnumerable<string> tableNames)
		{
			foreach (var name in tableNames)
			{
				if (QueryCacheManager_Unsafe.TableToViewMapping.ContainsKey(name))
				{
					viewsToClear.TryEnqueue(name);
				}
			}

			if (viewsToClear.Count == 0)
			{
				clearViewsWithoutExplicitTableMapping = true;
			}
		}

		#endregion

		#region Unsafe operations that should only be called from owner thread.

		public void Store(string tableName, ZQuery filter)
		{
			sentry.EnsureCurrentThreadIsOwner();
			ExecuteQueue();
			internalManager.Store(tableName, filter);
		}

		public bool IsCached(string tableName, ZQuery filter)
		{
			sentry.EnsureCurrentThreadIsOwner();
			ExecuteQueue();
			return internalManager.IsCached(tableName, filter);
		}

		#endregion

		#region Test
#if DEBUG
		internal Dictionary<string, CacheForCheckingIFWeHaveSeenAQueryBefore> CachedKeyDictionaries => internalManager.CachedKeyDictionaries;

		public string[] CachedKeys => internalManager.CachedKeys;
#endif
		#endregion

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Being lock free is better than stopping the unimportant race condition.")]
		void ExecuteQueue()
		{
			if (clearAllTables)
			{
				internalManager.Clear();
				tablesToClear.Clear();
				clearAllTables = false;
			}
			else if (tablesToClear.Any())
			{
				while (tablesToClear.TryDequeue(out var key))
				{
					internalManager.Clear(key);
				}
			}

			if (viewsToClear.Any())
			{
				var keys = viewsToClear.Clear();
				internalManager.ClearViewsQueries(keys);
				clearViewsWithoutExplicitTableMapping = false;
			}
			else if (clearViewsWithoutExplicitTableMapping)
			{
				internalManager.ClearViewsQueries(Enumerable.Empty<string>());
				clearViewsWithoutExplicitTableMapping = false;
			}
		}
	}

	class QueryCacheManager_Unsafe
	{
		public QueryCacheManager_Unsafe()
		{
			CreateDictionary();
		}

		Dictionary<string, CacheForCheckingIFWeHaveSeenAQueryBefore> cachedKeyDictionaries;
		HashSet<string> viewsKeys;

		#region Clear

		public void Clear()
		{
			CreateDictionary();
		}

		void CreateDictionary()
		{
			cachedKeyDictionaries = new Dictionary<string, CacheForCheckingIFWeHaveSeenAQueryBefore>(StringComparer.OrdinalIgnoreCase);
			viewsKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		public void Clear(string tableName)
		{
			cachedKeyDictionaries.Remove(tableName);
		}

		public void ClearViewsQueries(IEnumerable<string> tableNames)
		{
			var viewsToRemove = new HashSet<string>();

			foreach (var tableName in tableNames)
			{
				if (TableToViewMapping.TryGetValue(tableName, out var viewKey))
				{
					viewsToRemove.Add(viewKey);
				}
			}

			var viewsToSpare = TableToViewMapping.Values.Where(viewName => !viewsToRemove.Contains(viewName)).ToHashSet();
			viewsKeys.RemoveWhere((viewKey) =>
			{
				if (!viewsToSpare.Contains(viewKey))
				{
					cachedKeyDictionaries.Remove(viewKey);
					return true;
				}
				return false;
			});
		}

		internal static ImmutableDictionary<string, string> TableToViewMapping { get; } = new Dictionary<string, string>
		{
			{ "ProcessTasks", "ViewProcessTask" },
			{ "ProcessHeader", "ViewProcessHeader" },
		}.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

		#endregion

		public void Store(string tableName, ZQuery filter)
		{
			if (!cachedKeyDictionaries.TryGetValue(tableName, out var diamondCache))
			{
				diamondCache = new CacheForCheckingIFWeHaveSeenAQueryBefore();
				cachedKeyDictionaries[tableName] = diamondCache;

				if (tableName.StartsWith((NoResString)"view", StringComparison.OrdinalIgnoreCase) || tableName.StartsWith("vw_", StringComparison.OrdinalIgnoreCase))
				{
					viewsKeys.Add(tableName);
				}
			}

			var blobs = filter.LoadWithBlobs.Select(s => s.Name).ToArray();

			if (diamondCache != null && !diamondCache.ShouldMatchAllFutureContains(blobs))
			{
				if (filter.IsEmpty)
				{
					diamondCache.MatchAllFutureContains(blobs);
				}
				else
				{
					var storeWholeKey = true;

					if (!filter.IsTopNQuery)
					{
						var orParts = filter.GetOrParts();
						if (orParts.Length > 0)
						{
							storeWholeKey = false;
							foreach (var orPart in orParts)
							{
								diamondCache.AddQueryToCache(GetKey(orPart), blobs);
							}
						}
					}

					if (storeWholeKey)
					{
						diamondCache.AddQueryToCache(GetKey(filter), blobs);
					}
				}
			}
		}

		public bool IsCached(string tableName, ZQuery filter)
		{
			if (filter == null || !cachedKeyDictionaries.TryGetValue(tableName, out var dictionary) || dictionary == null)
			{
				return false;
			}

			var result = false;

			if (!filter.ReLoadExistingRows)
			{
				var blobKeys = filter.LoadWithBlobs.Select(s => s.Name).ToArray();
				result = dictionary.ShouldMatchAllFutureContains(blobKeys);
				if (result)
				{
					return result;
				}

				var exactKey = GetKey(filter);
				result = dictionary.Contains(exactKey, blobKeys);
				if (result)
				{
					return result;
				}

				var fullSetKey = GetUnorderedFullSetKey(filter);
				result = dictionary.Contains(fullSetKey, blobKeys);
				if (result)
				{
					return result;
				}

				var compositeParts = filter.GetCompositeParts();
				foreach (var andPart in compositeParts)
				{
					var baseKey = GetUnorderedFullSetKey(andPart);
					result = dictionary.Contains(baseKey, blobKeys);

					if (result)
					{
						return result;
					}
				}

				//Check all M in N subsets of AND parts, for example so we find an 'X and Y' fetch hint when we do a query 'X and Y and Z'.
				//(For performance reasons, M <= 2 and N <= 5 for now, but feel free to expand this later, future coders.)
				if (compositeParts.Length <= 5)
				{
					for (var i = 0; i < compositeParts.Length - 1; ++i)
					{
						for (var j = i + 1; j < compositeParts.Length; ++j)
						{
							var query = new ZQuery();
							query.AddToFilter(compositeParts[i]);
							query.AddToFilter(compositeParts[j]);

							var baseKey = GetUnorderedFullSetKey(query);
							result = dictionary.Contains(baseKey, blobKeys);

							if (result)
							{
								return result;
							}
						}
					}
				}

				if (!filter.IsTopNQuery)
				{
					var orParts = filter.GetOrParts();
					if (orParts.Length > 1)
					{
						result = orParts.All(orPart => dictionary.Contains(GetKey(orPart), blobKeys));
					}
				}
				if (result)
				{
					return result;
				}
			}

			return result;
		}

		string GetKey(ZQuery filter)
		{
			if (filter.MaximumRows == null)
			{
				return filter.FilterPartsHashKey;
			}

			var maxRows = filter.MaximumRows != null ? filter.MaximumRows.Value.ToString(CultureInfo.InvariantCulture) : "";
			return filter.FilterPartsHashKey + maxRows + filter.OrderBy;
		}

		string GetUnorderedFullSetKey(ZQuery filter)
		{
			return filter.FilterPartsHashKey;
		}

		#region Test
#if DEBUG

		public string[] CachedKeys
		{
			get
			{
				var keys = new List<string>();

				foreach (var innerDictionary in cachedKeyDictionaries.Values)
				{
					keys.AddRange(innerDictionary.Keys);
				}

				return keys.ToArray();
			}
		}

		internal Dictionary<string, CacheForCheckingIFWeHaveSeenAQueryBefore> CachedKeyDictionaries => cachedKeyDictionaries;

#endif
		#endregion
	}
}
