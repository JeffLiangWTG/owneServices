using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	internal sealed class FetchHintQueryStorer : IDisposable
	{
		sealed class QueryHolder
		{
			public string TableName;
			public ZQuery Query;
		}

		public FetchHintQueryStorer(QueryCacheManager manager)
		{
			list = new List<QueryHolder>();
			this.manager = manager;
		}

		public void AddQuery(string tableName, ZQuery query)
		{
			list.Add(new QueryHolder() { TableName = tableName, Query = query });
		}

		readonly List<QueryHolder> list;
		readonly QueryCacheManager manager;

		#region IDisposable Members

		public void Dispose()
		{
			list.ForEach(holder =>
			{
				if (holder != null)
				{
					manager.Store(holder.TableName, holder.Query);
				}
			});
			list.Clear();
		}

		#endregion
	}
}
