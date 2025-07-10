using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public static class ZQueryExtensions
	{
		public static ZQuery AddEmptyAsNullToFilter(this ZQuery query, SchemaGuidColumn schemaColumn, ZGuid foreignKey)
		{
			var value = foreignKey.IsEmpty ? DBNull.Value : (object)foreignKey;
			return query.AddToFilter(schemaColumn, value);
		}

		public static IEnumerable<string> DistinctBySqlComparisonOperator(this IEnumerable<object> values, SQLComparisonOperator sqlComparisonOperator)
		{
			return values.Select(v => v.ToString()).OrderBy(v => v.Length).Distinct(new StringComparerBySqlComparisonOperator(sqlComparisonOperator));
		}

		class StringComparerBySqlComparisonOperator : IEqualityComparer<string>
		{
			readonly SQLComparisonOperator ComparisonOperator;

			public StringComparerBySqlComparisonOperator(SQLComparisonOperator @operator)
			{
				ComparisonOperator = @operator;
			}

			public bool Equals(string x, string y)
			{
				if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
				{
					throw new ArgumentException("Input values cannot be null or empty.");
				}

				if (ComparisonOperator == SQLComparisonOperator.StartsWith)
				{
					return y.StartsWith(x);
				}

				if (ComparisonOperator == SQLComparisonOperator.EndsWith)
				{
					return y.EndsWith(x);
				}

				throw new NotSupportedException($"Only {nameof(SQLComparisonOperator.StartsWith)} & {nameof(SQLComparisonOperator.EndsWith)} are supported for this Distinct.");
			}

			public int GetHashCode(string obj)
			{
				return default;
			}
		}
	}

	public static class RelatedColumnFilter
	{
		public static ZDBOnlySubQuery GetSubQueryForRelatedTextColumn(this SQLComparisonOperator op, Type relatedTable, SchemaColumn relatedColumn, SchemaColumn filterColumn, ZString filterValue)
		{
			if (op.IsNegativeSQLOperator() && !filterValue.IsEmpty)
			{
				var filter = new ZDBOnlySubQuery(relatedTable, relatedColumn, true);
				filter.AddToFilter(filterColumn, op.GetNegatingSQLOperatorIfNotInSubquery(), filterValue);
				return filter;
			}
			else if (op is EqualComparisonOperator && filterValue.IsEmpty)
			{
				var filter = new ZDBOnlySubQuery(relatedTable, relatedColumn, true);
				filter.AddToFilter(filterColumn, SQLComparisonOperator.NotEqual, ZString.Empty);
				return filter;
			}
			else
			{
				var filter = new ZDBOnlySubQuery(relatedTable, relatedColumn);
				filter.AddToFilter(filterColumn, op, filterValue);
				return filter;
			}
		}
	}

	public static class GuidExtensions
	{
		public static string ToSqlGuid(this Guid guid)
		{
			return "'" + guid.ToString() + "'";
		}

		public static string ToSqlGuid(this ZGuid guid)
		{
			return "'" + guid.ToString() + "'";
		}

		public static object ToSqlParameter(this ZGuid guid)
		{
			if (guid.IsEmpty)
			{
				return DBNull.Value;
			}

			return guid.ToGuid();
		}
	}
}
