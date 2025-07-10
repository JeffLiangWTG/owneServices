using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.DataTransfer.Native.DB.Sql;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Native.DB
{
	public static class DataRowExtension
	{
		[ThreadSafe]
		static readonly ConcurrentDictionary<string, IEnumerable<ReadOnlyConstraint>> constraintsCache = new ConcurrentDictionary<string, IEnumerable<ReadOnlyConstraint>>();

		public static IDictionary<string, object> ToDictionary(this DataRow dataRow)
		{
			var dict = new Dictionary<string, object>();
			foreach (DataColumn column in dataRow.Table.Columns)
			{
				dict.Add(column.ColumnName, dataRow[column.ColumnName]);
			}
			return dict;
		}

		public static bool HasSameSchema(this DataRow self, DataRow other)
		{
			var selfSchema = self.Table.Columns;
			var otherSchema = other.Table.Columns;

			if (selfSchema.Count != otherSchema.Count)
			{
				return false;
			}

			foreach (DataColumn column in selfSchema)
			{
				if (!otherSchema.Contains(column.ColumnName))
				{
					return false;
				}
			}

			return true;
		}

		public static IDictionary<string, object> PrimaryKey(this DataRow dataRow)
		{
			var primary = dataRow.Table.PrimaryKey;
			return dataRow.ToDictionary()
				.Where(column => primary.Any(key => key.ColumnName == column.Key))
				.ToDictionary(column => column.Key, column => column.Value);
		}

		public static Table TableDef(this DataRow row)
		{
			return Table.Get(row.Table.TableName);
		}

#if DEBUG
		public static void ClearConstraintCache()
		{
			constraintsCache.Clear();
		}

		public static int ConstraintCacheCount()
		{
			return constraintsCache.Count;
		}
#endif

		public static IEnumerable<ReadOnlyConstraint> Constraints(this DataRow row)
		{
			return constraintsCache.GetOrAdd(row.Table.TableName, tableName =>
			{
				var table = row.TableDef();
				if (table.CandidateKeyConstraints == null)
				{
					return Enumerable.Empty<ReadOnlyConstraint>();
				}
				return table.CandidateKeyConstraints
							.Select(constraint => ReadOnlyConstraint.Create(constraint))
							.ToList();
			});
		}

		public static CandidateKey CandidateKey(this DataRow row, ReadOnlyConstraint constraint)
		{
			return new CandidateKey(constraint, row);
		}

		public static IEnumerable<CandidateKey> CandidateKeys(this DataRow row)
		{
			foreach (var constraint in row.Constraints())
			{
				var candidateKey = row.CandidateKey(constraint);
				yield return candidateKey;
			}
		}

		public static bool Match(this DataRow row, Criteria criteria)
		{
			if (!criteria.TableName.Equals(row.Table.TableName))
			{
				return false;
			}

			var value = criteria.Value;
			var columnName = criteria.ColumnName;

			if (row[columnName] is String)
			{
				return ((string)row[columnName]).Contains(value.ToString().Trim());
			}

			return row[columnName].Equals(value);
		}
	}
}
