using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Database.Shared;
using static CargoWise.Data.MetaData;

namespace CargoWise.Data
{
	public static class SpatialIndexLoader
	{
		public static SpatialIndexInfo LoadTop1(DbConnection connection, string schemaName = null, string tableName = null, string indexName = null)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = LoadInfo(connection, true, schemaName, tableName, indexName);

			return (result.Count > 0) ? result[0] : null;
		}

		public static List<SpatialIndexInfo> Load(DbConnection connection, string schemaName = null, string tableName = null, string indexName = null)
		{
			Argument.NotNull(connection, nameof(connection));

			return LoadInfo(connection, false, schemaName, tableName, indexName);
		}

		static List<SpatialIndexInfo> LoadInfo(DbConnection connection, bool useTop1, string schemaName = null, string tableName = null, string indexName = null)
		{
			Argument.NotNull(connection, nameof(connection));

			var result = new List<SpatialIndexInfo>();

			var top_1 = (useTop1) ? "TOP (1)" : ""; // Direct SQL query
			var top_1_order_by = (useTop1) ? "ORDER BY sch.name, obj.name, ind.name" : ""; // Direct SQL query

			var schPredicate = (string.IsNullOrWhiteSpace(schemaName)) ? "" : "AND sch.name = @sch_name"; // Direct SQL query
			var objPredicate = (string.IsNullOrWhiteSpace(tableName)) ? "" : "AND obj.name = @obj_name"; // Direct SQL query
			var indPredicate = (string.IsNullOrWhiteSpace(indexName)) ? "" : "AND ind.name = @ind_name"; // Direct SQL query

			var sql = string.Format(CultureInfo.InvariantCulture, @"-- MetaData.SpatialIndexInfo.LoadInfo
;WITH
	data AS (
			SELECT {0}
				sch_name         = sch.name,
				obj_id           = obj.object_id,
				obj_name         = obj.name,
				ind_id           = ind.index_id,
				ind_name         = ind.name,
				ind_spatial_type = ind.spatial_index_type
				, ind.is_padded
				, ind.fill_factor
				, ind.allow_row_locks
				, ind.allow_page_locks
			FROM
				sys.schemas              AS sch
				JOIN sys.objects         AS obj ON obj.schema_id = sch.schema_id
				JOIN sys.spatial_indexes AS ind ON ind.object_id = obj.object_id
			WHERE
				obj.is_ms_shipped = 0
				{2}
				{3}
				{4}
			{1}
		)
SELECT
	ind.sch_name, ind.obj_name, ind.ind_name
	, col_name = col.name
	, ind.ind_spatial_type
	--, tss.tessellation_scheme
	, tss.cells_per_object

	, bounding_box_xmin = ISNULL(tss.bounding_box_xmin, 0)
	, bounding_box_ymin = ISNULL(tss.bounding_box_ymin, 0)
	, bounding_box_xmax = ISNULL(tss.bounding_box_xmax, 0)
	, bounding_box_ymax = ISNULL(tss.bounding_box_ymax, 0)

	, level_1_grid = ISNULL(tss.level_1_grid, 0)
	, level_2_grid = ISNULL(tss.level_2_grid, 0)
	, level_3_grid = ISNULL(tss.level_3_grid, 0)
	, level_4_grid = ISNULL(tss.level_4_grid, 0)
	--, tss.level_1_grid_desc
	--, tss.level_2_grid_desc
	--, tss.level_3_grid_desc
	--, tss.level_4_grid_desc

	, ind_PAD_INDEX        = ind.is_padded
	, ind_FILLFACTOR       = ind.fill_factor
	, ind_ALLOW_ROW_LOCKS  = ind.allow_row_locks
	, ind_ALLOW_PAGE_LOCKS = ind.allow_page_locks
	, ind_DATA_COMPRESSION = par.data_compression
	--, tss.*
FROM
	data                                 AS ind
	JOIN sys.internal_tables             AS sit     ON sit.parent_object_id = ind.obj_id AND sit.parent_minor_id = ind.ind_id
	JOIN sys.partitions                  AS par     ON par.object_id = sit.object_id
	JOIN sys.spatial_index_tessellations AS tss     ON tss.object_id = ind.obj_id AND tss.index_id = ind.ind_id
	JOIN sys.index_columns               AS ind_col ON ind_col.object_id = ind.obj_id AND ind_col.index_id = ind.ind_id
	JOIN sys.columns                     AS col     ON col.object_id = ind_col.object_id AND col.column_id = ind_col.column_id
ORDER BY
	ind.sch_name, ind.obj_name, ind.ind_name
OPTION (USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'))
"
				, top_1          // 0
				, top_1_order_by // 1
				, schPredicate   // 2
				, objPredicate   // 3
				, indPredicate   // 4
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
						var col_name = (string)reader["col_name"];

						var ind_tesselation = GetTessellationScheme(Convert.ToInt32(reader["ind_spatial_type"], CultureInfo.InvariantCulture));
						var cells_per_object = Convert.ToInt32(reader["cells_per_object"], CultureInfo.InvariantCulture);

						var bounding_box_xmin = Convert.ToDouble(reader["bounding_box_xmin"], CultureInfo.InvariantCulture);
						var bounding_box_ymin = Convert.ToDouble(reader["bounding_box_ymin"], CultureInfo.InvariantCulture);
						var bounding_box_xmax = Convert.ToDouble(reader["bounding_box_xmax"], CultureInfo.InvariantCulture);
						var bounding_box_ymax = Convert.ToDouble(reader["bounding_box_ymax"], CultureInfo.InvariantCulture);

						var level_1_grid = GetGridSize(Convert.ToInt32(reader["level_1_grid"], CultureInfo.InvariantCulture));
						var level_2_grid = GetGridSize(Convert.ToInt32(reader["level_2_grid"], CultureInfo.InvariantCulture));
						var level_3_grid = GetGridSize(Convert.ToInt32(reader["level_3_grid"], CultureInfo.InvariantCulture));
						var level_4_grid = GetGridSize(Convert.ToInt32(reader["level_4_grid"], CultureInfo.InvariantCulture));

						var ind_PAD_INDEX = Convert.ToBoolean(reader["ind_PAD_INDEX"], CultureInfo.InvariantCulture);
						var ind_FILLFACTOR = Convert.ToInt32(reader["ind_FILLFACTOR"], CultureInfo.InvariantCulture);
						var ind_ALLOW_ROW_LOCKS = Convert.ToBoolean(reader["ind_ALLOW_ROW_LOCKS"], CultureInfo.InvariantCulture);
						var ind_ALLOW_PAGE_LOCKS = Convert.ToBoolean(reader["ind_ALLOW_PAGE_LOCKS"], CultureInfo.InvariantCulture);
						var ind_DATA_COMPRESSION = GetDataCompression(Convert.ToInt32(reader["ind_DATA_COMPRESSION"], CultureInfo.InvariantCulture));

						if (ind_tesselation == TessellationScheme.NONE)
						{
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Loaded Invalid TessellationScheme: <{0}>", ind_tesselation));
						}

						if (cells_per_object < 1 || cells_per_object > 8192)
						{
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Loaded Invalid cells_per_object: <{0}>", cells_per_object));
						}

						var builder = SpatialIndexInfo.Builder.New(sch_name, tab_name, ind_name, ind_tesselation)
							.Key(col_name)
							.CellsPerObject(cells_per_object)
							.Option(IndexOptions.PAD_INDEX, ind_PAD_INDEX)
							.Option(IndexOptions.FILLFACTOR, ind_FILLFACTOR)
							.Option(IndexOptions.ALLOW_ROW_LOCKS, ind_ALLOW_ROW_LOCKS)
							.Option(IndexOptions.ALLOW_PAGE_LOCKS, ind_ALLOW_PAGE_LOCKS)
							.Option(IndexOptions.DATA_COMPRESSION, ind_DATA_COMPRESSION)
							;

						if (builder.RequiresBox)
						{
							if (bounding_box_xmin >= bounding_box_xmax || bounding_box_ymin >= bounding_box_ymax)
							{
								throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Loaded Invalid Bounding Box: <{0}, {1}, {2}, {3}>", bounding_box_xmin, bounding_box_ymin, bounding_box_xmax, bounding_box_ymax));
							}

							builder.Box(bounding_box_xmin, bounding_box_ymin, bounding_box_xmax, bounding_box_ymax);
						}

						if (builder.RequiresGrid)
						{
							if (level_1_grid == GridSize.NONE || level_2_grid == GridSize.NONE || level_3_grid == GridSize.NONE || level_4_grid == GridSize.NONE)
							{
								throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Loaded Invalid Tessellation Grid: <{0}, {1}, {2}, {3}>", level_1_grid, level_2_grid, level_3_grid, level_4_grid));
							}

							builder.Grid(level_1_grid, level_2_grid, level_3_grid, level_4_grid);
						}

						result.Add(builder.GetInfo());
					}
				}
			}

			return result;
		}

		internal static GridSize GetGridSize(int size)
		{
			switch (size)
			{
				case 16:
					return GridSize.LOW;
				case 64:
					return GridSize.MEDIUM;
				case 256:
					return GridSize.HIGH;
				default:
					return GridSize.NONE;
			}
		}

		internal static TessellationScheme GetTessellationScheme(int scheme)
		{
			switch (scheme)
			{
				case 1:
					return TessellationScheme.GEOMETRY_GRID;
				case 2:
					return TessellationScheme.GEOGRAPHY_GRID;
				case 3:
					return TessellationScheme.GEOMETRY_AUTO_GRID;
				case 4:
					return TessellationScheme.GEOGRAPHY_AUTO_GRID;
				default:
					return TessellationScheme.NONE;
			}
		}
	}
}
