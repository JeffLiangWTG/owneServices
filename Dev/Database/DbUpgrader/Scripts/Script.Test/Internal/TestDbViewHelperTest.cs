using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing
{
	public static class TestDbViewHelper
	{
		public static void AssertViewIndexes(DbConnection connection, string viewName, params DbIndex[] expectedIndexes)
		{
			var indexes = GetViewIndexes(connection, viewName);
			Assertion.AssertContainsExactElementsInAnyOrder(expectedIndexes, indexes);
		}

		static IEnumerable<DbIndex> GetViewIndexes(DbConnection connection, string viewName)
		{
			var result = new List<DbIndex>();
			var sql = @"
select v.name, i.name index_name, 
	STRING_AGG(c.name, ',') WITHIN GROUP (ORDER BY key_ordinal) columns
from sys.indexes i
join sys.views v on v.object_id = i.object_id
join sys.index_columns ic on ic.object_id = i.object_id AND ic.index_id = i.index_id and is_included_column = 0
join sys.columns c on c.object_id = i.object_id and c.column_id = ic.column_id
where v.name = @viewName
GROUP BY v.name, i.name";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@viewName", SqlDbType.NVarChar, 128, viewName);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new DbIndex(
							(string)reader["index_name"],
							(string)reader["columns"]
						));
					}
				}
			}

			return result;
		}

		public static void AssertViewColumnsMatchUnderlyingTable(DbConnection connection, string viewName, string tableName, ICollection<DbColumn> expectedDifferences, bool isTableASynonym = true)
		{
			var viewColumns = GetViewColumns(connection, viewName, tableName, false);
			var tableColumns = GetTableColumns(connection, tableName, isTableASynonym);

			var columnsMissingFromView = tableColumns.Except(viewColumns);
			var columnsMissingFromTable = viewColumns.Except(tableColumns);

			var actualDifferences = columnsMissingFromView.Union(columnsMissingFromTable);
			Assertion.AssertContainsExactElementsInAnyOrder(expectedDifferences, actualDifferences);
		}

		public static void AssertViewColumns(DbConnection connection, string viewName, string tableName, params DbColumn[] expectedColumns)
		{
			var viewColumns = GetViewColumns(connection, viewName, tableName, isSynonyms: false);

			Assertion.AssertContainsExactElementsInAnyOrder(expectedColumns, viewColumns);
		}

		public static IEnumerable<DbColumn> GetTableColumns(DbConnection connection, string name, bool isSynonyms)
			=> GetTableOrViewColumns(connection, name, null, isSynonyms);

		public static IEnumerable<DbColumn> GetViewColumns(DbConnection connection, string name, string underlyingTableName, bool isSynonyms)
			=> GetTableOrViewColumns(connection, name, underlyingTableName, isSynonyms);

		static IEnumerable<DbColumn> GetTableOrViewColumns(DbConnection connection, string name, string underlyingTableName, bool isSynonyms)
		{
			var result = new List<DbColumn>();
			var refTablename = string.Empty;
			var refDB = string.Empty;

			if (isSynonyms)
			{
				var refSQL = @"
select PARSENAME(base_object_name, 3) as refDB, PARSENAME(base_object_name, 1) as refTablename from sys.synonyms
where name = @tableName";

				using (var cmd = connection.Command(refSQL))
				{
					cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, name);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							refDB = reader["refDB"].ToString();
							refTablename = reader["refTablename"].ToString();
						}
					}
				}

				if (string.IsNullOrEmpty(refDB) || string.IsNullOrEmpty(refTablename))
				{
					return result;
				}
				else
				{
					refDB = "[" + refDB + "].";
				}
			}

			var sql = $@"
select
	column_name,
	data_type,
	(case when character_maximum_length is null then -1 else character_maximum_length end) as character_maximum_length,
	(case when numeric_precision is null then -1 else numeric_precision end) as numeric_precision,
	(case when numeric_scale is null then -1 else numeric_scale end) as numeric_scale
from {refDB}information_schema.columns
where table_name = @tableName
";
			if (!string.IsNullOrEmpty(underlyingTableName))
			{
				sql += $@"
and (
	column_name not in (
		select column_name from {refDB}information_schema.columns
		where table_name = @underlyingTableName
		and (column_name not like '__[_]PK' and column_name not like '___[_]PK')
		and (column_name not like '__[_]ClusterKey' and column_name not like '___[_]ClusterKey')
	) or (
		ordinal_position > (
			select ordinal_position from {refDB}information_schema.columns
			where table_name = @tableName
			and column_name like '__[_]SystemLastEditUser'
		)
	)
)";
			}

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, isSynonyms ? refTablename : name);

				if (!string.IsNullOrEmpty(underlyingTableName))
				{
					cmd.AddParameter("@underlyingTableName", SqlDbType.NVarChar, 128, underlyingTableName);
				}

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new DbColumn(
							(string)reader["column_name"],
							(string)reader["data_type"],
							(int)reader["character_maximum_length"],
							(int)reader["numeric_precision"],
							(int)reader["numeric_scale"]
						));
					}
				}
			}

			return result;
		}

		public struct DbIndex : IEquatable<DbIndex>
		{
			public readonly string IndexName;
			public readonly string Columns;

			public DbIndex(string indexName, string columns)
			{
				IndexName = indexName;
				Columns = columns;
			}

			public static bool operator ==(DbIndex a, DbIndex b) => Equals(a, b);
			public static bool operator !=(DbIndex a, DbIndex b) => !(a == b);

			public bool Equals(DbIndex other) =>
				IndexName == other.IndexName &&
				Columns == other.Columns;

			public override bool Equals(object obj) => obj != null && GetType().Equals(obj.GetType()) && Equals((DbIndex)obj);

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = -1118741485;
					hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(IndexName);
					hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Columns);
					return hashCode;
				}
			}

			public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Index Name: {0} | Columns: {1}", new object[] { IndexName, Columns });
		}

		public struct DbColumn : IEquatable<DbColumn>
		{
			public readonly string ColumnName;
			public readonly string DataType;
			public readonly int CharacterMaximumLength;
			public readonly int Precision;
			public readonly int Scale;

			public DbColumn(string columnName, SqlDbType sqlDbType, int characterMaximumLength, int precision = -1, int scale = -1)
				: this(columnName, sqlDbType.ToString().ToLowerInvariant(), characterMaximumLength, precision, scale)
			{
			}

			public DbColumn(string columnName, string dataType, int characterMaximumLength, int precision = -1, int scale = -1)
			{
				ColumnName = columnName;
				DataType = dataType;
				CharacterMaximumLength = characterMaximumLength;
				Precision = precision;
				Scale = scale;
			}

			public static bool operator ==(DbColumn a, DbColumn b) => Equals(a, b);
			public static bool operator !=(DbColumn a, DbColumn b) => !(a == b);

			public bool Equals(DbColumn other) =>
				ColumnName == other.ColumnName &&
				DataType == other.DataType &&
				CharacterMaximumLength == other.CharacterMaximumLength &&
				Precision == other.Precision &&
				Scale == other.Scale;

			public override bool Equals(object obj) => !(obj == null || !GetType().Equals(obj.GetType())) && Equals((DbColumn)obj);

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = -1118741485;
					hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ColumnName);
					hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DataType);
					hashCode = hashCode * -1521134295 + CharacterMaximumLength.GetHashCode();
					hashCode = hashCode * -1521134295 + Precision.GetHashCode();
					hashCode = hashCode * -1521134295 + Scale.GetHashCode();
					return hashCode;
				}
			}

			public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Column Name: {0} | Data Type: {1} | Character Maximum Length: {2} | Precision: {3} | Scale: {4}", new object[] { ColumnName, DataType, CharacterMaximumLength, Precision, Scale });
		}
	}
}

