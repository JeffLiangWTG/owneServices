using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ImmediateZMultiQueryFetchHint : IImmediateHint
	{
		public ImmediateZMultiQueryFetchHint(Type businessObjectType, ZQuery mainQuery, ZQuery secondQuery)
		{
			this.mainQuery = mainQuery;
			this.secondQuery = secondQuery;
			this.businessObjectType = businessObjectType;
			this.tableName = BusinessObjectFactory.GetTableNameFromType(businessObjectType);
			fullQuery = new ZQuery(mainQuery);
			fullQuery.AddToFilter(secondQuery);
		}

		readonly ZQuery mainQuery;
		readonly ZQuery secondQuery;
		readonly ZQuery fullQuery;
		readonly Type businessObjectType;
		readonly string tableName;

		public ZQuery GetQuery()
		{
			return fullQuery;
		}

		public string TableName
		{
			get { return tableName; }
		}

		public IQueryHashKey GetHashKeyObject()
		{
			return fullQuery.GetHashKey();
		}

		public bool IsDataHintLoaded
		{
			get { return isDataHintLoaded; }
			set { isDataHintLoaded = value; }
		}
		bool isDataHintLoaded;

		string IFetchHint.BuilderKey
		{
			get { return tableName + mainQuery.LiteralTextADO + ":ImmediateZMultiQuery"; }
		}

		bool IFetchHint.IsNeeded(QueryHistoryProvider provider)
		{
			return !provider.IsQueryCached(tableName, fullQuery);
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

		#region IImmediateHint Members

		Type IImmediateHint.BusinessObjectType
		{
			get { return businessObjectType; }
		}

		#endregion

	}
}
