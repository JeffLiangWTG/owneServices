using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public static class ZQueryHelper
	{
		public static ZQuery Generate(Table table, IEnumerable<Criteria> criterias, int? maxRows = null, bool fetchOnlyFromLocalCache = false)
		{
			var resultQuery = new ZQuery();

			if (maxRows.HasValue)
			{
				if (maxRows.Value < 1)
				{
					throw new ArgumentException("Max rows must be a positive integer.", nameof(maxRows));
				}
				resultQuery.MaximumRows = maxRows.Value;
			}
			resultQuery.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;

			var tableSchema = EnterpriseSchema.GetTableSchema(table.Name);
			foreach (var criteria in criterias.Where(c => c.TableName == table.Name))
			{
				resultQuery.AddToFilter(new ZQuery(tableSchema.GetSchemaColumn(criteria.ColumnName), criteria.Value), JoinCondition.And);
			}
			return resultQuery;
		}
	}
}
