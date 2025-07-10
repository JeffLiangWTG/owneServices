using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	sealed class SharedDataTableIndexCache : IndexCache<HeapIndexCacheKey, SharedDataTableIndex>
	{
		internal SharedDataTableIndexCache(DataTable table)
		{
			this.table = table;
		}
		readonly DataTable table;
		public static SharedDataTableIndex GetSharedDataTableIndex(DataTable table, ZQuery filter, SchemaColumn indexColumn, DataViewRowState rowStateFilter)
		{
			return GetInstance(table).GetDataViewCore(new HeapIndexCacheKey(filter, indexColumn, rowStateFilter));
		}

		static SharedDataTableIndexCache GetInstance(DataTable table) => IndexCacheDataTableMixin.GetInstance(table).SharedDataTableIndex;

		readonly Dictionary<HeapIndexCacheKey, WeakReference> heapIndexCache = new Dictionary<HeapIndexCacheKey, WeakReference>();

		public static void RemoveSharedDataTableIndex(DataTable table, SharedDataTableIndex index)
		{
			if (index != null && table != null)
			{
				var instance = GetInstance(table);
				if (instance != null)
				{
					var keysToRemove = instance.heapIndexCache.Where(item => item.Value.Target == index).Select(item => item.Key).ToArray();
					foreach (var keyToRemove in keysToRemove)
					{
						instance.heapIndexCache.Remove(keyToRemove);
					}
				}
			}
		}

		protected override SharedDataTableIndex GetNewIndex(HeapIndexCacheKey key)
		{
			return new SharedDataTableIndex(table, key.Filter, key.IndexColumn);
		}
	}

	internal class HeapIndexCacheKey
	{
		public HeapIndexCacheKey(ZQuery filter, SchemaColumn indexColumn, DataViewRowState rowState)
		{
			this.filter = filter;
			this.query = filter.LiteralTextADO;
			this.indexColumn = indexColumn;
			this.rowState = rowState;
		}

		public ZQuery Filter => filter;
		public SchemaColumn IndexColumn => indexColumn;
		public DataViewRowState RowState => rowState;
		readonly ZQuery filter;
		readonly string query;
		readonly SchemaColumn indexColumn;
		readonly DataViewRowState rowState;

		public override int GetHashCode()
		{
			return
				query.GetHashCode() ^
				indexColumn.Name.GetHashCode() ^
				rowState.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is HeapIndexCacheKey cacheKey &&
				cacheKey.query == query &&
				cacheKey.indexColumn.Name == indexColumn.Name &&
				cacheKey.rowState == rowState;
		}
	}
}
