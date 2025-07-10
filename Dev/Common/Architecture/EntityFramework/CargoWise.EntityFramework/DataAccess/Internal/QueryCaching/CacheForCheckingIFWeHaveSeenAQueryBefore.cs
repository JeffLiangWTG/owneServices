using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public class CacheForCheckingIFWeHaveSeenAQueryBefore
	{
		const string NO_BLOB_COLUMN = "NOBL0B"; // It is very unlikely we will have a column named NOBL0B
		static StringComparer Comparer => StringComparer.OrdinalIgnoreCase;
		public IEnumerable<string> Keys => filtersByCachePart.SelectMany(s => s.Value).Distinct();
		readonly Dictionary<string, HashSet<string>> filtersByCachePart = new Dictionary<string, HashSet<string>>(Comparer);

		IEnumerable<string> GetGroupCacheParts(string[] blobColumns)
		{
			yield return NO_BLOB_COLUMN;
			foreach (var blobColumn in blobColumns)
			{
				yield return blobColumn;
			}
		}

		public void MatchAllFutureContains(params string[] blobColums)
		{
			ClearCache(blobColums);
			AddQueryToCache(string.Empty, blobColums);
		}

		void ClearCache(string[] blobColums)
		{
			foreach (var col in GetGroupCacheParts(blobColums))
			{
				if (filtersByCachePart.TryGetValue(col, out HashSet<string> value))
				{
					value.Clear();
				}
			}
		}

		public bool ShouldMatchAllFutureContains(params string[] blobColumns) => Contains(string.Empty, blobColumns);

		public void AddQueryToCache(string filter, params string[] blobColumns)
		{
			foreach (var cachePart in GetGroupCacheParts(blobColumns))
			{
				AddGroupedByCacheGroup(cachePart, filter);
			}
		}

		void AddGroupedByCacheGroup(string cacheGroup, string filter)
		{
			HashSet<string> matchingParts;
			if (!filtersByCachePart.TryGetValue(cacheGroup, out matchingParts))
			{
				filtersByCachePart[cacheGroup] = matchingParts = new HashSet<string>(Comparer);
			}
			matchingParts.Add(filter);
		}

		public bool Contains(string filter, params string[] blobColumns) => GetGroupCacheParts(blobColumns).All(blobColumn => ContainsInCachedGroup(blobColumn, filter));

		bool ContainsInCachedGroup(string blobColumn, string filter) => filtersByCachePart.TryGetValue(blobColumn, out HashSet<string> val) && (val.Contains(filter) || val.Contains(string.Empty));
	}
}
