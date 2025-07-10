using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;

namespace CargoWise.Bi.Product.Manager.Business
{
	#region SuppressResourceStringsCheckRegion

	public class EdwInformation : BiDatabaseInformation
	{
		public EdwInformation(string serverName, string dbName)
			: base(serverName, dbName)
		{
		}

		public new string ServerName
		{
			get
			{
				return base.ServerName;
			}
		}

		public new string DatabaseName
		{
			get
			{
				return base.DatabaseName;
			}
		}

		#region EDW Data Information

		public string LastInitialLoadServerDateTime { get; private set; }
		public string LastTransactionDate { get; private set; }
		public string LastTransformDate { get; private set; }
		public string LastIndexRebuildDate { get; private set; }
		public string LastTranslationTransformDate { get; private set; }

		public string SourceTableSize { get; private set; }
		public string EdwTableSize { get; private set; }

		public EdwTableCollection<EdwSourceTable> EdwSourceTables { get; private set; }
		public EdwTableCollection<EdwStagingTable> EdwStagingTables { get; private set; }
		public EdwTableCollection<EdwBaseTable> EdwBaseTables { get; private set; }
		public EdwTableCollection<EdwAggregateTable> EdwAggregateTables { get; private set; }
		public EdwTableCollection<EdwCustomTable> EdwCustomTables { get; private set; }

		public void RefreshInfo()
		{
			using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(ServerName, DatabaseName))
			{
				RetrieveEdwStagingTablesInfo(biConnection);
				RetrieveEdwBaseTablesInfo(biConnection);
				RetrieveEdwAggregateTablesInfo(biConnection);
				RetrieveEdwCustomTablesInfo(biConnection);
				RetrieveEdwSourceTablesInfo();
				RetrieveMasterStateInfo(biConnection);
				RetrieveTableSizeInfo(biConnection);
			}
		}

		void RetrieveTableSizeInfo(DbConnection biConnection)
		{
			CalculateSourceTableSizes();
			RetrieveEdwTableSizes(biConnection);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveEdwTableSizes(DbConnection biConnection)
		{
			using var cmd = biConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_RetrieveEdwTableSizes");
			cmd.CommandType = CommandType.StoredProcedure;
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var edwTotalUsedSize = Convert.ToDecimal(reader["Used_MB"], CultureInfo.InvariantCulture);
					var edwTotalSize = Convert.ToDecimal(reader["Total_MB"], CultureInfo.InvariantCulture);
					var usedPercentage = edwTotalUsedSize / edwTotalSize;
					var unit = "MB";
					if (edwTotalSize > 10000)
					{
						edwTotalSize /= 1024;
						edwTotalUsedSize /= 1024;
						unit = "GB";
					}

					EdwTableSize = string.Format(CultureInfo.InvariantCulture, "{0:n} {2} ({1:n} {2} used, {3:P0})", edwTotalSize, edwTotalUsedSize, unit, usedPercentage); // file size format
				}
			}
		}

		void CalculateSourceTableSizes()
		{
			decimal sourceTableTotalSize = 0;
			decimal sourceTableTotalUsedSize = 0;
			foreach (EdwSourceTable sourceTable in EdwSourceTables)
			{
				sourceTableTotalSize += sourceTable.TotalSize;
				sourceTableTotalUsedSize += sourceTable.SizeUsed;
			}
			var usedPercentage = sourceTableTotalUsedSize / sourceTableTotalSize;
			var unit = "MB";
			if (sourceTableTotalSize > 10000)
			{
				sourceTableTotalSize /= 1024;
				sourceTableTotalUsedSize /= 1024;
				unit = "GB";
			}

			SourceTableSize = string.Format(CultureInfo.InvariantCulture, "{0:n} {2} ({1:n} {2} used, {3:P0})", sourceTableTotalSize, sourceTableTotalUsedSize, unit, usedPercentage); // file size format
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveEdwSourceTablesInfo()
		{
			List<string> sourceTableList = new List<string>();
			foreach (EdwStagingTable stagingTable in EdwStagingTables)
			{
				sourceTableList.Add(stagingTable.SourceSchema + "." + stagingTable.Name);
			}
			var sourceTableSql = string.Join("','", sourceTableList);
			var sqlText = $@"
				SELECT 
					s.name as SchemaName,
					t.name as TableName,
					MAX(p.rows) AS RowCounts,
					CAST(ROUND((SUM(a.used_pages) / 128.00), 2) AS NUMERIC(36, 2)) AS Used_MB,
					CAST(ROUND((SUM(a.total_pages) - SUM(a.used_pages)) / 128.00, 2) AS NUMERIC(36, 2)) AS Unused_MB,
					CAST(ROUND((SUM(a.total_pages) / 128.00), 2) AS NUMERIC(36, 2)) AS Total_MB
				FROM sys.tables t with (nolock)
					INNER JOIN sys.indexes i with (nolock) ON t.OBJECT_ID = i.object_id
					INNER JOIN sys.partitions p with (nolock) ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
					INNER JOIN sys.allocation_units a with (nolock) ON p.partition_id = a.container_id
					INNER JOIN sys.schemas s with (nolock) ON t.schema_id = s.schema_id
				WHERE s.name + '.' + t.name IN ('{sourceTableSql}')
				GROUP BY t.Name, s.name";

			EdwSourceTables = new EdwTableCollection<EdwSourceTable>();
			using (var reader = Db.Connection.Command(sqlText).ExecuteReader()) // Edw source table system data
			{
				while (reader.Read())
				{
					var schemaName = reader["SchemaName"].ToString();
					var tableName = reader["TableName"].ToString();
					var sourceTable = new EdwSourceTable(schemaName, tableName);

					sourceTable.RowCount = Convert.ToInt64(reader["RowCounts"], CultureInfo.InvariantCulture);
					sourceTable.SizeUsed = Convert.ToDecimal(reader["Used_MB"], CultureInfo.InvariantCulture);
					sourceTable.SizeUnused = Convert.ToDecimal(reader["Unused_MB"], CultureInfo.InvariantCulture);
					sourceTable.TotalSize = Convert.ToDecimal(reader["Total_MB"], CultureInfo.InvariantCulture);

					EdwSourceTables.Add(sourceTable);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveMasterStateInfo(DbConnection biConnection)
		{
			using var cmd = biConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_RetrieveMasterStateInfo");
			cmd.CommandType = CommandType.StoredProcedure;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					if (reader[1] != DBNull.Value)
					{
						var paramName = reader[0].ToString();
						var paramValue = reader[1].ToString();

						if (paramName.Equals(BiConstants.InitialLoadEndDt, StringComparison.OrdinalIgnoreCase))
						{
							LastInitialLoadServerDateTime = new ZDateTime(paramValue).ToBestReadableDateTimeString();
						}
						else if (paramName.Equals(BiConstants.LastLsnTransformedUtc, StringComparison.OrdinalIgnoreCase))
						{
							var utcTime = new ZDateTime(paramValue, DateTimeKind.Utc);
							LastTransactionDate = new ZDateTime(Env.Time.GetLocalTimeFromUtc(utcTime.ToDateTime())).ToBestReadableDateTimeString();
						}
						else if (paramName.Equals(BiConstants.LastDateTransformRunUtc, StringComparison.OrdinalIgnoreCase))
						{
							var utcTime = new ZDateTime(paramValue, DateTimeKind.Utc);
							LastTransformDate = new ZDateTime(Env.Time.GetLocalTimeFromUtc(utcTime.ToDateTime())).ToBestReadableDateTimeString();
						}
						else if (paramName.Equals(BiConstants.LastIndexRebuildUtcDt, StringComparison.OrdinalIgnoreCase))
						{
							var utcTime = new ZDateTime(paramValue, DateTimeKind.Utc);
							LastIndexRebuildDate = new ZDateTime(Env.Time.GetLocalTimeFromUtc(utcTime.ToDateTime())).ToBestReadableDateTimeString();
						}
						else if (paramName.Equals(BiConstants.LastTranslationTransformDateUtc, StringComparison.OrdinalIgnoreCase))
						{
							var utcTime = new ZDateTime(paramValue, DateTimeKind.Utc);
							LastTranslationTransformDate = new ZDateTime(Env.Time.GetLocalTimeFromUtc(utcTime.ToDateTime())).ToBestReadableDateTimeString();
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveEdwStagingTablesInfo(DbConnection biConnection)
		{
			EdwStagingTables = new EdwTableCollection<EdwStagingTable>();

			using var cmd = biConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_RetrieveEdwStagingTablesInfo");
			cmd.CommandType = CommandType.StoredProcedure;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schemaName = reader["StagingSchemaName"].ToString();
					var tableName = reader["StagingTableName"].ToString();
					var stagingTable = new EdwStagingTable(schemaName, tableName);

					stagingTable.SourceSchema = reader["SourceSchemaName"].ToString();
					stagingTable.CurrentState = reader["CurrentState"].ToString();
					stagingTable.InitialLoadRequired = reader["InitialLoadRequired"].ToString();
					stagingTable.InitialLoadRecordCount = new ZLong(reader["InitialLoadRecordCount"]);
					stagingTable.InitialLoadDuration = new ZLong(reader["InitialLoadDurationMs"]);
					stagingTable.IncrementalLoadRecordCount = new ZLong(reader["IncrementalLoadRecordCount"]);
					stagingTable.IncrementalLoadDuration = new ZLong(reader["IncrementalLoadDurationMs"]);
					stagingTable.SqlErrorMessage = reader["SqlErrorMessage"].ToString();
					stagingTable.EnableEtl = Convert.ToBoolean(reader["EnableEtl"], CultureInfo.InvariantCulture);

					var sqlErrorDatetimeUTC = reader["SqlErrorDatetimeUTC"];
					if (sqlErrorDatetimeUTC != DBNull.Value)
					{
						stagingTable.SqlErrorDatetimeUTC = new ZDateTime(sqlErrorDatetimeUTC);
					}
					else
					{
						stagingTable.SqlErrorDatetimeUTC = ZDateTime.Empty;
					}

					EdwStagingTables.Add(stagingTable);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveEdwBaseTablesInfo(DbConnection biConnection)
		{
			EdwBaseTables = new EdwTableCollection<EdwBaseTable>();

			using var cmd = biConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_RetrieveEdwBaseTablesInfo");
			cmd.CommandType = CommandType.StoredProcedure;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schema = reader["ModelSchemaName"].ToString();
					var name = reader["ModelTableName"].ToString();
					var baseTable = new EdwBaseTable(schema, name);

					baseTable.RowCount = new ZLong(reader["RowCounts"]);
					baseTable.TotalSize = new ZDecimal(reader["Total_MB"]);
					baseTable.SizeUnused = new ZDecimal(reader["Unused_MB"]);
					baseTable.SizeUsed = new ZDecimal(reader["Used_MB"]);
					baseTable.TransformId = new ZInt(reader["TransformId"]);
					baseTable.DependencyOrder = new ZInt(reader["DependencyOrder"]);
					baseTable.CurrentState = reader["CurrentState"].ToString();
					baseTable.InitialLoadRequired = reader["InitialLoadRequired"].ToString();
					baseTable.InitialTransformRecordCount = new ZLong(reader["InitialTransformRecordCount"]);
					baseTable.InitialTransformDuration = new ZLong(reader["InitialTransformDurationMs"]);
					baseTable.MergeTransformInsertRecordCount = new ZLong(reader["MergeTransformInsertRecordCount"]);
					baseTable.MergeTransformInsertDuration = new ZLong(reader["MergeTransformInsertDurationMs"]);
					baseTable.MergeTransformDeleteRecordCount = new ZLong(reader["MergeTransformDeleteRecordCount"]);
					baseTable.MergeTransformDeleteDuration = new ZLong(reader["MergeTransformDeleteDurationMs"]);
					baseTable.SqlErrorMessage = reader["SqlErrorMessage"].ToString();
					var sqlErrorDatetimeUTC = reader["SqlErrorDatetimeUTC"];
					if (sqlErrorDatetimeUTC != DBNull.Value)
					{
						baseTable.SqlErrorDatetimeUTC = new ZDateTime(sqlErrorDatetimeUTC);
					}
					else
					{
						baseTable.SqlErrorDatetimeUTC = ZDateTime.Empty;
					}
					var isIndexReorganized = reader["IsIndexReorganized"];
					if (isIndexReorganized != DBNull.Value)
					{
						baseTable.IsIndexReorganized = Convert.ToBoolean(isIndexReorganized, CultureInfo.InvariantCulture);
					}
					else
					{
						baseTable.IsIndexReorganized = false;
					}
					baseTable.IndexReorganizationErrorMessage = reader["IndexReorganizationSqlErrorMessage"].ToString();

					EdwBaseTables.Add(baseTable);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveEdwAggregateTablesInfo(DbConnection biConnection)
		{
			EdwAggregateTables = new EdwTableCollection<EdwAggregateTable>();

			using var cmd = biConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_RetrieveEdwAggregateTablesInfo");
			cmd.CommandType = CommandType.StoredProcedure;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schema = reader["ModelSchemaName"].ToString();
					var name = reader["ModelTableName"].ToString();
					var aggregateTable = new EdwAggregateTable(schema, name);

					aggregateTable.RowCount = new ZLong(reader["RowCounts"]);
					aggregateTable.TotalSize = new ZDecimal(reader["Total_MB"]);
					aggregateTable.SizeUnused = new ZDecimal(reader["Unused_MB"]);
					aggregateTable.SizeUsed = new ZDecimal(reader["Used_MB"]);
					aggregateTable.CurrentState = reader["CurrentState"].ToString();
					aggregateTable.InitialLoadRequired = reader["InitialLoadRequired"].ToString();
					aggregateTable.InitialLoadRecordCount = new ZLong(reader["InitialLoadRecordCount"]);
					aggregateTable.InitialLoadDuration = new ZLong(reader["InitialLoadDurationMs"]);
					aggregateTable.IncrementalInsertRecordCount = new ZLong(reader["IncrementalInsertRecordCount"]);
					aggregateTable.IncrementalInsertDuration = new ZLong(reader["IncrementalInsertDurationMs"]);
					aggregateTable.IncrementalDeleteRecordCount = new ZLong(reader["IncrementalDeleteRecordCount"]);
					aggregateTable.IncrementalDeleteDuration = new ZLong(reader["IncrementalDeleteDurationMs"]);
					aggregateTable.SqlErrorMessage = reader["SqlErrorMessage"].ToString();
					var sqlErrorDatetimeUTC = reader["SqlErrorDatetimeUTC"];
					if (sqlErrorDatetimeUTC != DBNull.Value)
					{
						aggregateTable.SqlErrorDatetimeUTC = new ZDateTime(sqlErrorDatetimeUTC);
					}
					else
					{
						aggregateTable.SqlErrorDatetimeUTC = ZDateTime.Empty;
					}
					var isIndexReorganized = reader["IsIndexReorganized"];
					if (isIndexReorganized != DBNull.Value)
					{
						aggregateTable.IsIndexReorganized = Convert.ToBoolean(isIndexReorganized, CultureInfo.InvariantCulture);
					}
					else
					{
						aggregateTable.IsIndexReorganized = false;
					}
					aggregateTable.IndexReorganizationErrorMessage = reader["IndexReorganizationSqlErrorMessage"].ToString();

					EdwAggregateTables.Add(aggregateTable);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RetrieveEdwCustomTablesInfo(DbConnection biConnection)
		{
			EdwCustomTables = new EdwTableCollection<EdwCustomTable>();

			using var cmd = biConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_RetrieveEdwCustomTablesInfo");
			cmd.CommandType = CommandType.StoredProcedure;
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schema = reader["ModelSchemaName"].ToString();
					var name = reader["ModelTableName"].ToString();
					var customTable = new EdwCustomTable(schema, name);

					customTable.RowCount = new ZLong(reader["RowCounts"]);
					customTable.TotalSize = new ZDecimal(reader["Total_MB"]);
					customTable.SizeUnused = new ZDecimal(reader["Unused_MB"]);
					customTable.SizeUsed = new ZDecimal(reader["Used_MB"]);
					customTable.CurrentState = reader["CurrentState"].ToString();
					customTable.InitialLoadRequired = reader["InitialLoadRequired"].ToString();
					customTable.InitialTransformRecordCount = new ZLong(reader["InitialTransformRecordCount"]);
					customTable.InitialTransformDuration = new ZLong(reader["InitialTransformDurationMs"]);
					customTable.MergeTransformInsertRecordCount = new ZLong(reader["MergeTransformInsertRecordCount"]);
					customTable.MergeTransformInsertDuration = new ZLong(reader["MergeTransformInsertDurationMs"]);
					customTable.MergeTransformDeleteRecordCount = new ZLong(reader["MergeTransformDeleteRecordCount"]);
					customTable.MergeTransformDeleteDuration = new ZLong(reader["MergeTransformDeleteDurationMs"]);
					customTable.SqlErrorMessage = reader["SqlErrorMessage"].ToString();
					var sqlErrorDatetimeUTC = reader["SqlErrorDatetimeUTC"];
					if (sqlErrorDatetimeUTC != DBNull.Value)
					{
						customTable.SqlErrorDatetimeUTC = new ZDateTime(sqlErrorDatetimeUTC);
					}
					else
					{
						customTable.SqlErrorDatetimeUTC = ZDateTime.Empty;
					}
					var isIndexReorganized = reader["IsIndexReorganized"];
					if (isIndexReorganized != DBNull.Value)
					{
						customTable.IsIndexReorganized = Convert.ToBoolean(isIndexReorganized, CultureInfo.InvariantCulture);
					}
					else
					{
						customTable.IsIndexReorganized = false;
					}
					customTable.IndexReorganizationErrorMessage = reader["IndexReorganizationSqlErrorMessage"].ToString();

					EdwCustomTables.Add(customTable);
				}
			}
		}

		#endregion
	}

	#endregion
}
