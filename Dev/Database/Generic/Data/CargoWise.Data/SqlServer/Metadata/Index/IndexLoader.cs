using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Database.Shared;
using static CargoWise.Data.MetaData;

namespace CargoWise.Data
{
	public static class IndexLoader
	{
		public static IndexInfo LoadTop1(DbConnection connection, string schemaName, string tableName, string indexName)
		{
			Argument.NotNull(connection, nameof(connection));
			if (!(!string.IsNullOrWhiteSpace(schemaName) || !string.IsNullOrWhiteSpace(tableName) || !string.IsNullOrWhiteSpace(indexName)))
			{
				throw new ArgumentException("Invalid argument.", nameof(schemaName));
			}

			var result = LoadInfo(connection, true, schemaName, tableName, indexName);

			return (result.Count > 0) ? result[0] : null;
		}

		public static List<IndexInfo> Load(DbConnection connection, string schemaName, string tableName, string indexName)
		{
			Argument.NotNull(connection, nameof(connection));
			if (!(!string.IsNullOrWhiteSpace(schemaName) || !string.IsNullOrWhiteSpace(tableName) || !string.IsNullOrWhiteSpace(indexName)))
			{
				throw new ArgumentException("Invalid argument.", nameof(schemaName));
			}

			return LoadInfo(connection, false, schemaName, tableName, indexName);
		}

		public static bool Exists(DbConnection connection, string schemaName, string tableName, string indexName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(indexName, nameof(indexName));

			var schemaPredicate = (string.IsNullOrWhiteSpace(schemaName)) ? "" : "AND sch.name = @sch_name"; // Direct SQL query
			var objectPredicate = (string.IsNullOrWhiteSpace(tableName)) ? "" : "AND obj.name = @obj_name"; // Direct SQL query

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- MetaData.IndexInfo.Exists
SELECT
	IndexExists = CONVERT(bit, CASE WHEN EXISTS (
			SELECT
				NULL
			FROM
				sys.schemas      AS sch WITH (NOLOCK)
				JOIN sys.objects AS obj WITH (NOLOCK) ON obj.schema_id = sch.schema_id
				JOIN sys.indexes AS ind WITH (NOLOCK) ON ind.object_id = obj.object_id 
			WHERE
				ind.name = @ind_name
				{0}
				{1}
		) THEN 1 ELSE 0 END)
"
				, schemaPredicate // 0
				, objectPredicate // 1
				);

			using (var cmd = connection.Command(sql))
			{
				if (!string.IsNullOrWhiteSpace(schemaName))
				{
					cmd.AddParameter("@sch_name", SqlDbType.NVarChar, 128, schemaName);
				}

				if (!string.IsNullOrWhiteSpace(tableName))
				{
					cmd.AddParameter("@obj_name", SqlDbType.NVarChar, 128, tableName);
				}

				cmd.AddParameter("@ind_name", SqlDbType.NVarChar, 128, indexName);

				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		static List<IndexInfo> LoadInfo(DbConnection connection, bool useTop1, string schemaName, string tableName, string indexName)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new List<IndexInfo>();

			var top_1 = (useTop1) ? "TOP (1)" : ""; // Direct SQL query
			var top_1_order_by = (useTop1) ? "ORDER BY Sch.name, Obj.name, Ind.name" : ""; // Direct SQL query

			var schPredicate = (string.IsNullOrWhiteSpace(schemaName)) ? "" : "AND Sch.name = @sch_name"; // Direct SQL query
			var objPredicate = (string.IsNullOrWhiteSpace(tableName)) ? "" : "AND Obj.name = @obj_name"; // Direct SQL query
			var indPredicate = (string.IsNullOrWhiteSpace(indexName)) ? "" : "AND Ind.name = @ind_name"; // Direct SQL query

			var sql = FormattableString.Invariant($@"-- MetaData.IndexInfo.LoadInfo
;WITH
	data AS (
			SELECT {top_1}
				sch_name       = Sch.name,
				obj_id         = Obj.object_id,
				obj_name       = Obj.name,
				ind_id         = Ind.index_id,
				ind_name       = Ind.name,
				ind_unique     = Ind.is_unique,
				ind_clustered  = CONVERT(bit, CASE WHEN Ind.index_id = 1 THEN 1 ELSE 0 END),
				ind_filter     = ISNULL(Ind.filter_definition, N'')

				, opt_PAD_INDEX              = Ind.is_padded
				, opt_FILLFACTOR             = Ind.fill_factor
				, opt_IGNORE_DUP_KEY         = Ind.ignore_dup_key
				, opt_ALLOW_ROW_LOCKS        = Ind.allow_row_locks
				, opt_ALLOW_PAGE_LOCKS       = Ind.allow_page_locks
				, opt_STATISTICS_NORECOMPUTE = Stat.no_recompute
				, opt_STATISTICS_INCREMENTAL = Stat.is_incremental
				, opt_DATA_COMPRESSION       = Part.data_compression
			FROM
				sys.schemas      AS Sch
				JOIN sys.objects AS Obj ON Obj.schema_id = Sch.schema_id
				JOIN sys.indexes AS Ind ON Ind.object_id = Obj.object_id

				JOIN sys.stats      AS Stat ON Stat.object_id = Ind.object_id AND Stat.stats_id = Ind.index_id
				JOIN sys.partitions AS Part ON Part.object_id = Ind.object_id AND Part.index_id = Ind.index_id
					AND Part.partition_number = 1 -- For non-partitioned tables and indexes, the value of this column is 1
			WHERE 1=1
				AND Obj.is_ms_shipped = 0
				AND Obj.type in ('U', 'V') -- user tables and views
				AND Ind.type in (1, 2) -- 1 = Clustered rowstore (B-tree), 2 = Nonclustered rowstore (B-tree)
				{schPredicate}
				{objPredicate}
				{indPredicate}
			{top_1_order_by}
		)
SELECT
	sch_name       = Ind.sch_name,
	obj_name       = Ind.obj_name,
	ind_name       = Ind.ind_name,
	ind_unique     = Ind.ind_unique,
	ind_clustered  = Ind.ind_clustered,
	ind_filter     = Ind.ind_filter,
	col_name       = Col.name,
	col_included   = IndCol.is_included_column,
	col_descending = IndCol.is_descending_key

	, Ind.opt_PAD_INDEX
	, Ind.opt_FILLFACTOR
	, Ind.opt_IGNORE_DUP_KEY
	, Ind.opt_ALLOW_ROW_LOCKS
	, Ind.opt_ALLOW_PAGE_LOCKS
	, Ind.opt_STATISTICS_NORECOMPUTE
	, Ind.opt_STATISTICS_INCREMENTAL
	, Ind.opt_DATA_COMPRESSION
FROM
	data                   AS Ind
	JOIN sys.index_columns AS IndCol ON IndCol.object_id = Ind.obj_id AND IndCol.index_id = Ind.ind_id
	JOIN sys.columns       AS Col    ON Col.object_id = IndCol.object_id AND Col.column_id = IndCol.column_id
ORDER BY
	Ind.sch_name, Ind.obj_name, Ind.ind_name,
	IndCol.is_included_column,
	IndCol.key_ordinal,
	Col.name
");

			var prev_sch_name = "";
			var prev_tab_name = "";
			var prev_ind_name = "";
			IndexInfo.Builder builder = null;
			using (var cmd = connection.Command(sql))
			{
				if (!string.IsNullOrWhiteSpace(schemaName))
				{
					cmd.AddParameter("@sch_name", SqlDbType.NVarChar, 128, schemaName);
				}

				if (!string.IsNullOrWhiteSpace(tableName))
				{
					cmd.AddParameter("@obj_name", SqlDbType.NVarChar, 128, tableName);
				}

				if (!string.IsNullOrWhiteSpace(indexName))
				{
					cmd.AddParameter("@ind_name", SqlDbType.NVarChar, 128, indexName);
				}

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var sch_name = (string)reader["sch_name"];
						var tab_name = (string)reader["obj_name"];
						var ind_name = (string)reader["ind_name"];

						if (prev_sch_name != sch_name || prev_tab_name != tab_name || prev_ind_name != ind_name)
						{
							if (builder != null)
							{
								result.Add(builder.GetInfo());
							}

							var ind_unique = Convert.ToBoolean(reader["ind_unique"], CultureInfo.InvariantCulture);
							var ind_clustered = Convert.ToBoolean(reader["ind_clustered"], CultureInfo.InvariantCulture);
							var ind_filter = (string)reader["ind_filter"];

							builder = IndexInfo.Builder.New(sch_name, tab_name, ind_name)
								.Unique(ind_unique).Clustered(ind_clustered)
								.Where(ind_filter);

							builder.Option(IndexOptions.PAD_INDEX, Convert.ToBoolean(reader["opt_PAD_INDEX"], CultureInfo.InvariantCulture));
							builder.Option(IndexOptions.FILLFACTOR, Convert.ToInt32(reader["opt_FILLFACTOR"], CultureInfo.InvariantCulture));
							builder.Option(IndexOptions.IGNORE_DUP_KEY, Convert.ToBoolean(reader["opt_IGNORE_DUP_KEY"], CultureInfo.InvariantCulture));
							builder.Option(IndexOptions.ALLOW_ROW_LOCKS, Convert.ToBoolean(reader["opt_ALLOW_ROW_LOCKS"], CultureInfo.InvariantCulture));
							builder.Option(IndexOptions.ALLOW_PAGE_LOCKS, Convert.ToBoolean(reader["opt_ALLOW_PAGE_LOCKS"], CultureInfo.InvariantCulture));
							builder.Option(IndexOptions.STATISTICS_NORECOMPUTE, Convert.ToBoolean(reader["opt_STATISTICS_NORECOMPUTE"], CultureInfo.InvariantCulture));
							builder.Option(IndexOptions.STATISTICS_INCREMENTAL, Convert.ToBoolean(reader["opt_STATISTICS_INCREMENTAL"], CultureInfo.InvariantCulture));
							builder.Option(IndexOptions.DATA_COMPRESSION, GetDataCompression(Convert.ToInt32(reader["opt_DATA_COMPRESSION"], CultureInfo.InvariantCulture)));

							prev_sch_name = sch_name;
							prev_tab_name = tab_name;
							prev_ind_name = ind_name;
						}

						var col_name = (string)reader["col_name"];
						var col_included = Convert.ToBoolean(reader["col_included"], CultureInfo.InvariantCulture);
						var col_descending = Convert.ToBoolean(reader["col_descending"], CultureInfo.InvariantCulture);

						builder?.AddColumn(col_name, col_included, col_descending);
					}

					if (builder != null)
					{
						result.Add(builder.GetInfo());
					}
				}
			}

			return result;
		}
	}
}
