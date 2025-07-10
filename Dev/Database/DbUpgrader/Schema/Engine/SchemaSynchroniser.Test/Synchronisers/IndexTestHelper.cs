using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	public static class IndexTestHelper
	{
		/// <summary>
		/// Check if an index exists or not
		/// Note: If the index is expected to exist the follow attributes must match as specified
		///       IsUnique    => ind.is_unique = 1
		///       IsClustered => ind.type = 1
		///       FillFactor  => ind.fill_factor
		///       Filter      => ind.filter_definition
		/// </summary>
		public static void AssertIndexExistInDb(DbConnection connection, string dbName, string tableName, string indexName, bool isUnique, bool isClustered, int fillFactor, string filter, string compression)
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*)
				FROM
					[{0}].sys.objects tab
					INNER JOIN [{0}].sys.indexes ind
						ON ind.object_id = tab.object_id
					LEFT JOIN [{0}].sys.partitions par
						ON par.object_id = ind.object_id
						AND par.index_id = ind.index_id
						AND par.partition_number = 1
						AND par.data_compression > 0
				WHERE
					tab.name = '{1}'
					AND ind.name = '{2}'
					AND ind.is_primary_key = 0
					AND ind.is_unique_constraint = 0
					AND ind.type != 0",
				dbName, tableName, indexName);

			sqlText += String.Format(CultureInfo.InvariantCulture, " AND ind.is_unique {0} 1", (isUnique) ? "=" : "!=");
			sqlText += String.Format(CultureInfo.InvariantCulture, " AND ind.type {0} 1", (isClustered) ? "=" : "!=");
			sqlText += String.Format(CultureInfo.InvariantCulture, " AND ind.fill_factor = {0}", fillFactor.ToString());
			sqlText += String.Format(CultureInfo.InvariantCulture, " AND par.data_compression_desc {0}", String.IsNullOrWhiteSpace(compression) ? "is null" : "= '" + compression + "'");

			if (!String.IsNullOrWhiteSpace(filter))
			{
				sqlText += String.Format(CultureInfo.InvariantCulture, " AND ind.filter_definition {0}", filter.ToLower() == "null" ? "is null" : "= '" + filter + "'");
			}

			int qtyRows = Convert.ToInt32(connection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Index: {0}.{1} exists?", tableName, indexName);

			Assertion.AssertEquals(assertMessage, true, qtyRows == 1);
		}

		public static string[] GetIndexKeyColumnsInOrder(DbConnection connection, string dbName, string tableName, string indexName)
		{
			return GetIndexColumnsInOrder(connection, dbName, tableName, indexName, false);
		}

		public static string[] GetIndexIncludeColumnsInOrder(DbConnection connection, string dbName, string tableName, string indexName)
		{
			return GetIndexColumnsInOrder(connection, dbName, tableName, indexName, true);
		}

		static string[] GetIndexColumnsInOrder(DbConnection connection, string dbName, string tableName, string indexName, bool isIncludedColumn)
		{
			string sqlText = String.Format(@"
				SELECT
					col.name
				FROM
					[{0}].sys.tables tab
					INNER JOIN [{0}].sys.indexes ind
						ON tab.object_id = ind.object_id
					INNER JOIN [{0}].sys.index_columns ikey
						ON ikey.object_id = ind.object_id
						AND ikey.index_id = ind.index_id
					INNER JOIN [{0}].sys.columns col
						ON col.object_id = ikey.object_id
						AND col.column_id = ikey.column_id
				WHERE
					tab.name = '{1}'
					AND ind.name = '{2}'
					AND ikey.is_included_column = {3}
				ORDER BY
					ikey.index_column_id",
				dbName, tableName, indexName,
				isIncludedColumn ? "1" : "0");

			DataTable indexColumns = DataUtils.GetDataTableFromQuery(connection, sqlText);
			string[] result = new string[indexColumns.Rows.Count];

			for (int i = 0; i < indexColumns.Rows.Count; i++)
			{
				result[i] = indexColumns.Rows[i][0].ToString();
			}

			return result;
		}

		public static void AssertXmlIndexExists(DbConnection connection, string dbName, string tableName, string indexName, string parentIndexName, string secondaryType, string nameSpace = null, string path = null)
		{
			string sqlText = String.Format(@"
				SELECT
					count(*)
				FROM
					[{0}].sys.xml_indexes ind
					INNER JOIN [{0}].sys.tables tab
						ON tab.object_id = ind.object_id
					LEFT JOIN [{0}].sys.xml_indexes parentind
						ON parentind.object_id = ind.object_id
						AND parentind.index_id = ind.using_xml_index_id
					LEFT JOIN [{0}].sys.selective_xml_index_namespaces xin
						ON xin.object_id = ind.object_id
						AND xin.index_id = ind.index_id
					LEFT JOIN [{0}].sys.selective_xml_index_paths xip
						ON xip.object_id = ind.object_id
						AND xip.index_id = ind.index_id
				WHERE
					tab.name = '{1}'
					AND ind.name = '{2}'
					AND parentind.name {3}
					AND ind.secondary_type_desc {4}
					{5}
					{6}",
				dbName,
				tableName,
				indexName,
				parentIndexName == null ? "IS NULL" : "= '" + parentIndexName + "'",
				secondaryType == null ? "IS NULL" : "= '" + secondaryType + "'",
				nameSpace == null ? "" : "AND xin.prefix = '" + nameSpace + "'",
				path == null ? "" : "AND xip.name = '" + path + "'");

			int qtyRows = Convert.ToInt32(connection.ExecuteScalar(sqlText));
			Assertion.AssertEquals(tableName + "." + indexName, 1, qtyRows);
		}

		public static void AssertIndexNotExists(DbConnection connection, string dbName, string tableName, string indexName)
		{
			string sqlText = String.Format(@"
				SELECT
					count(*)
				FROM
					[{0}].sys.xml_indexes ind
					INNER JOIN [{0}].sys.tables tab
						ON tab.object_id = ind.object_id
				WHERE
					tab.name = '{1}'
					AND ind.name = '{2}'",
				dbName,
				tableName,
				indexName);

			int qtyRows = Convert.ToInt32(connection.ExecuteScalar(sqlText));
			Assertion.AssertEquals(tableName + "." + indexName, 0, qtyRows);
		}

		public static IEnumerable<string> GetIndexDefinitions(DbConnection connection, string dbName, string fullTableName)
		{
			var sql = String.Format(@"
WITH
	Data AS (
			SELECT DISTINCT
				[|] = '|'
				, obj_name       = CONCAT(s.name, '.', o.name)
				, ind_name       = i.name
				, ind_type_desc  = CASE WHEN i.is_unique = 1 THEN 'UNIQUE ' ELSE '       ' END + CASE WHEN i.type = 2 THEN 'NON' ELSE '   ' END + 'CLUSTERED'
				, ind_filter     = i.filter_definition
				, ind_options    =
					CASE
						WHEN i.fill_factor <> 0 THEN
							CASE
								WHEN p.data_compression_desc is null THEN CONCAT('FILLFACTOR = ', i.fill_factor)
								ELSE CONCAT('FILLFACTOR = ', i.fill_factor, ', DATA_COMPRESSION = ', p.data_compression_desc)
							END
						ELSE
							CASE
								WHEN p.data_compression_desc is null THEN ''
								ELSE CONCAT('DATA_COMPRESSION = ', p.data_compression_desc)
							END
					END COLLATE database_default
				, col_ordinal    = CASE WHEN ic.key_ordinal > 0 THEN ic.key_ordinal ELSE 100 END
				, col_name       = c.name + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE '' END
				, col_included   = ic.is_included_column
			FROM
				[{0}].sys.schemas            AS s
				INNER JOIN [{0}].sys.tables        AS o  ON o.schema_id = s.schema_id
				INNER JOIN [{0}].sys.indexes       AS i  ON i.object_id = o.object_id
				INNER JOIN [{0}].sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
				INNER JOIN [{0}].sys.columns       AS c  ON c.object_id = ic.object_id AND c.column_id = ic.column_id
				LEFT JOIN  [{0}].sys.partitions    AS p  ON p.object_id = i.object_id AND p.index_id = i.index_id AND p.partition_number = 1 AND p.data_compression > 0
			WHERE 1=1
				AND o.is_ms_shipped = 0
				AND i.type > 0
				AND o.object_id = OBJECT_ID(@fullTableName, N'U')
		)
	, Grouped AS (
			SELECT
				obj_name, ind_name, ind_type_desc, ind_filter, ind_options
				, key_columns      = dbo.CLRCssvAgg(CASE WHEN col_included = 0 THEN col_name ELSE '' END)
				, included_columns = dbo.CLRCssvAgg(CASE WHEN col_included = 1 THEN col_name ELSE '' END)
			FROM
				Data
			GROUP BY
				obj_name, ind_name, ind_type_desc, ind_filter, ind_options
		)
SELECT
	ind_definition = CONCAT(''
		, 'CREATE ', ind_type_desc, ' INDEX ', ind_name, ' ON ', obj_name
		, ' (', key_columns, ')'
		, CASE WHEN included_columns > '' THEN CONCAT(' INCLUDE (', included_columns, ')') ELSE '' END
		, CASE WHEN ind_filter > '' THEN CONCAT(' WHERE ', ind_filter) ELSE '' END
		, CASE WHEN ind_options > '' THEN CONCAT(' WITH (', ind_options, ')') ELSE '' END
		, ';'
		)
FROM
	Grouped
ORDER BY
	obj_name, ind_name
"
				, dbName // 0
				);

			var indexes = new List<string>();
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@fullTableName", SqlDbType.NVarChar, 128, String.Format("[{0}].{1}", dbName, fullTableName));

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						indexes.Add(reader["ind_definition"].ToString());
					}
				}
			}

			return indexes;
		}
	}
}
