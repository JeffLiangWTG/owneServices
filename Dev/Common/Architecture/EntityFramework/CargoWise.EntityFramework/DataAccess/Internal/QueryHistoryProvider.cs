using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class QueryHistoryProvider
	{
		internal QueryHistoryProvider(RowFactory rowFactory)
		{
			this.rowFactory = rowFactory;
		}

		public bool IsQueryCached(string tableName, ZQuery filter)
		{
			return rowFactory.IsCached(tableName, filter);
		}

		public bool HasLoadedRow(string tableName, ZGuid pK)
		{
			return rowFactory.HasLoadedRowCompletely(tableName, pK);
		}

		readonly RowFactory rowFactory;
	}
}
