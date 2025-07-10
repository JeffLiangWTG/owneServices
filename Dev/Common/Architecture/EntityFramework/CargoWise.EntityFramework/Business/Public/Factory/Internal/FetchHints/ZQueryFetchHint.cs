using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZQueryFetchHint : IFetchHint
	{
		public ZQueryFetchHint(ITableSchema tableSchema, ZQuery query)
		{
			if (!query.SupportsFetchHints)
			{
				throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "Query of type: {0} does not support fetch hints", query.GetType().Name));
			}
			this.query = query;
			this.tableSchema = tableSchema;
		}

		readonly ZQuery query;
		readonly ITableSchema tableSchema;

		public bool IsPrimaryKeyHint
		{
			get { return false; }
		}

		public ZQuery GetQuery()
		{
			return query;
		}

		public IQueryHashKey GetHashKeyObject()
		{
			return query.GetHashKey();
		}

		public string TableName => tableSchema.TableName;

		bool IFetchHint.IsDataHintLoaded
		{
			get;
			set;
		}

		string IFetchHint.BuilderKey
		{
			get
			{
				if (((ISeparateFetchQuery)query).CannotBeJoinedInFetchHint || query.MaximumRows > 0)
				{
					return FormattableString.Invariant($"{TableName}:ZQuery:{query.LiteralTextSql}");
				}
				else
				{
					var filteredColumns = FindColumns(query)
						.Select(c => c.Name)
						.Distinct()
						.OrderBy(c => c, StringComparer.OrdinalIgnoreCase);
					return FormattableString.Invariant($"{TableName}:ZQuery:{string.Join(",", filteredColumns)}");
				}
			}
		}

		static IEnumerable<SchemaColumn> FindColumns(IFilterPart filterPart)
		{
			foreach (var part in filterPart.SelectRecursive(q => q.FilterParts))
			{
				switch (part)
				{
					case ZSqlParameter param:
						yield return param.SchemaColumn;
						break;
					case ZSQLInFilter inFilter:
						yield return inFilter.Column;
						break;
					case ZSQLColumnComparer zSQLColumnComparer:
						yield return zSQLColumnComparer.Column1;
						yield return zSQLColumnComparer.Column2;
						break;
					default:
						break;
				}
			}
		}

		bool IFetchHint.IsNeeded(QueryHistoryProvider provider)
		{
			return !provider.IsQueryCached(TableName, query);
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
	}
}
