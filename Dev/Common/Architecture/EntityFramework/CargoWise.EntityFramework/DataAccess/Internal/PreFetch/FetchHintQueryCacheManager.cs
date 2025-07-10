using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	class FetchHintQueryCacheManager
	{
		public FetchHintQueryCacheManager(IEnumerable<IFetchHint> fetchHints)
		{
			var fetchHintDictionaries = new Dictionary<HashSet<SchemaColumn>, Dictionary<string, SortedList<string, HintAndQuery>>>(new LoadWithBlobsListEquityComparer());
			var shortestAlphComparer = new ShortestAlphComparer();
			foreach (IFetchHint hint in fetchHints.ToArray())
			{
				Dictionary<string, SortedList<string, HintAndQuery>> dictionary;
				HashSet<SchemaColumn> loadWithBlobs = new HashSet<SchemaColumn>(hint.LoadWithBlobs);
				fetchHintDictionaries.TryGetValue(loadWithBlobs, out dictionary);
				if (dictionary == null)
				{
					dictionary = new Dictionary<string, SortedList<string, HintAndQuery>>();
					fetchHintDictionaries.Add(loadWithBlobs, dictionary);
				}
				SortedList<string, HintAndQuery> list;
				if (!dictionary.TryGetValue(hint.TableName, out list))
				{
					list = new SortedList<string, HintAndQuery>(shortestAlphComparer);
					dictionary.Add(hint.TableName, list);
				}
				ZQuery query = hint.GetQuery();

				foreach (var blob in hint.LoadWithBlobs)
				{
					query.IncludeBlob(blob);
				}
				string sqlText = query.LiteralTextADO;
				if (!list.ContainsKey(sqlText))
				{
					list.Add(sqlText, new HintAndQuery() { Hint = hint, Query = query });
				}
			}
			foreach (var item in fetchHintDictionaries)
			{
				var cacheManager = GetCacheManager(item.Key);
				var cacheManagersToCheckAgainst = new List<CacheManager>();
				foreach (var otherCache in cacheManagers)
				{
					if (item.Key.IsProperSubsetOf(otherCache.Key))
					{
						cacheManagersToCheckAgainst.Add(otherCache.Value);
					}
				}
				PopulateCacheManagerWithShortestSQLScriptFetchHint(GetCacheManager(item.Key), item.Value, cacheManagersToCheckAgainst);
			}
		}

		public bool IsShortestSQLScriptFetchHint(IFetchHint hint)
		{
			var manager = GetCacheManager(hint);
			return manager.ContainsHint(hint);
		}

		void PopulateCacheManagerWithShortestSQLScriptFetchHint(CacheManager cacheManagerToPopulate, Dictionary<string, SortedList<string, HintAndQuery>> dictionaryToProcess, List<CacheManager> cacheManagersToCheckAgainst)
		{
			var queryCache = cacheManagerToPopulate.QueryCache;
			foreach (KeyValuePair<string, SortedList<string, HintAndQuery>> pair in dictionaryToProcess)
			{
				foreach (HintAndQuery hintAndQuery in pair.Value.Values)
				{
					string tableName = hintAndQuery.Hint.TableName;
					ZQuery query = hintAndQuery.Query;

					if (!queryCache.IsCached(tableName, query))
					{
						bool isAlreadyCached = false;
						foreach (var cache in cacheManagersToCheckAgainst)
						{
							if (cache.QueryCache.IsCached(tableName, query))
							{
								isAlreadyCached = true;
								break;
							}
						}
						if (!isAlreadyCached && !query.IsEmpty) // We can never run empty fetch hints so this is not added to the cache.
						{
							cacheManagerToPopulate.AddHint(hintAndQuery.Hint);
							queryCache.Store(tableName, query);
						}
					}
				}
			}
		}

		readonly Dictionary<HashSet<SchemaColumn>, CacheManager> cacheManagers = new Dictionary<HashSet<SchemaColumn>, CacheManager>(new LoadWithBlobsListEquityComparer());

		CacheManager GetCacheManager(IFetchHint hint)
		{
			return GetCacheManager(new HashSet<SchemaColumn>(hint.LoadWithBlobs));
		}

		CacheManager GetCacheManager(HashSet<SchemaColumn> loadWithBlobs)
		{
			CacheManager cacheManager;
			cacheManagers.TryGetValue(loadWithBlobs, out cacheManager);
			if (cacheManager == null)
			{
				cacheManager = new CacheManager();
				cacheManagers.Add(loadWithBlobs, cacheManager);
			}
			return cacheManager;
		}

		// this object is only for performance as there is no reason to call Hint.GetQuery() more than once.
		struct HintAndQuery
		{
			public IFetchHint Hint;
			public ZQuery Query;
		}

		class LoadWithBlobsListEquityComparer : IEqualityComparer<HashSet<SchemaColumn>>
		{
			public bool Equals(HashSet<SchemaColumn> x, HashSet<SchemaColumn> y)
			{
				return x.SetEquals(y);
			}

			public int GetHashCode(HashSet<SchemaColumn> obj)
			{
				int hashCode = 0;
				foreach (SchemaColumn column in obj)
				{
					hashCode ^= column.Name.GetHashCode();
				}
				return hashCode;
			}
		}

		class CacheManager
		{
			public void AddHint(IFetchHint hint)
			{
				Hints.Add(hint, true);
			}

			public bool ContainsHint(IFetchHint hint)
			{
				return Hints.ContainsKey(hint);
			}

			public int HintCount
			{
				get { return hints != null ? hints.Count : 0; }
			}

			Dictionary<IFetchHint, bool> Hints
			{
				get { return hints ?? (hints = new Dictionary<IFetchHint, bool>()); }
			}
			Dictionary<IFetchHint, bool> hints;

			public QueryCacheManager QueryCache
			{
				get { return queryCache ?? (queryCache = new QueryCacheManager()); }
			}
			QueryCacheManager queryCache;
		}

		class ShortestAlphComparer : IComparer<string>
		{
			#region IComparer<string> Members

			int IComparer<string>.Compare(string x, string y)
			{
				int result = x.Length.CompareTo(y.Length);
				if (result == 0)
				{
					result = x.CompareTo(y);
				}
				return result;
			}

			#endregion
		}
	}
}
