using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	class UnionZDBOnlyQuery : ZDBOnlyQuery
	{
		public UnionZDBOnlyQuery(Type typeOfBusinessObjectToQuery) : base(typeOfBusinessObjectToQuery)
		{
		}

		public override void AddAsCompleteSQLStatement(SqlBuilder sqlBuilder, string tableName, bool combineFilterAndParams, SchemaColumn[] selectList = null)
		{
			if (Queries.Count == 0)
			{
				base.AddAsCompleteSQLStatement(sqlBuilder, tableName, combineFilterAndParams, selectList);
				return;
			}

			var lastQuery = Queries.Last();

			foreach (var query in Queries)
			{
				query.IsNoLock = IsNoLock;
				query.AddFilterAndZSQLParameterCollection(FilterString, new ZSqlParameterCollection(Params), ignoreParameterSuffix: true);
				query.AddAsCompleteSQLStatement(sqlBuilder, tableName, combineFilterAndParams, selectList);

				if (lastQuery != query)
				{
					sqlBuilder.Append("\r\nUNION \r\n");
				}
			}

			if (!string.IsNullOrWhiteSpace(AdditionalInfo))
			{
				sqlBuilder.Append("\n" + AdditionalInfo);
			}
		}

		List<ZDBOnlyQuery> Queries { get; } = new List<ZDBOnlyQuery>();

		public void AddSubQuery(ZDBOnlyQuery query) => Queries.Add(query);

		public void AddFilter(ZString filter, ZSqlParameterCollection parameters) => AddFilterAndZSQLParameterCollection(filter, parameters, ignoreParameterSuffix: true);

		public string AdditionalInfo { get; set; }
	}
}
