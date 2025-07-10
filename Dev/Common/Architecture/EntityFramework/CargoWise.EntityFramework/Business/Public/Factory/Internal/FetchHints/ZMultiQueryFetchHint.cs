using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZMultiQueryFetchHint : IFetchHint
	{
		public ZMultiQueryFetchHint(ITableSchema tableSchema, ZQuery mainQuery, ZQuery secondQuery)
		{
			this.mainQuery = mainQuery;
			this.secondQuery = secondQuery;
			this.tableSchema = tableSchema;

			fullQuery = new ZQuery(mainQuery);
			fullQuery.AddToFilter(secondQuery);
		}

		readonly ZQuery mainQuery;
		readonly ZQuery secondQuery;
		readonly ITableSchema tableSchema;
		readonly ZQuery fullQuery;

		public ZQuery GetQuery()
		{
			return fullQuery;
		}

		public IQueryHashKey GetHashKeyObject()
		{
			return fullQuery.GetHashKey();
		}

		public string TableName
		{
			get { return tableSchema.TableName; }
		}

		public bool IsDataHintLoaded
		{
			get { return isDataHintLoaded; }
			set { isDataHintLoaded = value; }
		}
		bool isDataHintLoaded;

		string IFetchHint.BuilderKey
		{
			get { return TableName + mainQuery.LiteralTextADO + ":ZMultiQueryFetchHint"; }
		}

		bool IFetchHint.IsNeeded(QueryHistoryProvider provider)
		{
			return !provider.IsQueryCached(TableName, fullQuery);
		}

		void IFetchHint.GenerateQuery(QueryBuilder builder)
		{
			if (builder.IsEmpty)
			{
				builder.Init(mainQuery, new ZQuery());
			}

			builder.AddValue(secondQuery);
		}

		IEnumerable<SchemaColumn> IFetchHint.LoadWithBlobs
		{
			get
			{
				return mainQuery.LoadWithBlobs
					.Union(secondQuery.LoadWithBlobs)
					.Union(mainQuery.BlobFilters)
					.Union(secondQuery.BlobFilters);
			}
		}
	}
}
