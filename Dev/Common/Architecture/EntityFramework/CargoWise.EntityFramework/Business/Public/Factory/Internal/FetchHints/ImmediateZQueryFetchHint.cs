using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ImmediateZQueryFetchHint : IImmediateHint
	{
		public ImmediateZQueryFetchHint(Type businessObjectType, ZQuery query)
		{
			this.query = query;
			this.businessObjectType = businessObjectType;
			this.tableName = BusinessObjectFactory.GetTableNameFromType(businessObjectType);
		}

		readonly ZQuery query;
		readonly Type businessObjectType;
		readonly string tableName;

		public ZQuery GetQuery()
		{
			return query;
		}

		public string TableName
		{
			get { return tableName; }
		}

		public IQueryHashKey GetHashKeyObject()
		{
			return query.GetHashKey();
		}

		public bool IsDataHintLoaded
		{
			get;
			set;
		}

		string IFetchHint.BuilderKey
		{
			get { return tableName + ":ImmediateZQuery"; }
		}

		bool IFetchHint.IsNeeded(QueryHistoryProvider provider)
		{
			return !provider.IsQueryCached(tableName, query);
		}

		void IFetchHint.GenerateQuery(QueryBuilder builder)
		{
			if (builder.IsEmpty)
			{
				builder.Init(new ZQuery(), new ZQuery());
			}

			builder.AddValue(query);
		}

		IEnumerable<SchemaColumn> IFetchHint.LoadWithBlobs
		{
			get { return query.LoadWithBlobs.Union(query.BlobFilters); }
		}

		#region IImmediateHint Members

		Type IImmediateHint.BusinessObjectType
		{
			get { return businessObjectType; }
		}

		#endregion

	}
}
