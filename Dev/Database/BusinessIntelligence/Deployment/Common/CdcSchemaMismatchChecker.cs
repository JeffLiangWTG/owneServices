namespace Enterprise.ChangeDataCapture.Common
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using CargoWise.Bi.Common;
	using CargoWise.Data;

	#region SuppressResourceStringsCheckRegion

	public static class CdcSchemaMismatchChecker
	{
		public static string GetCdcSchemaMismatchLogs(DbConnection connection, bool treatExcessTablesAsMismatch = true)
		{
			string result = null;

			if (CdcDatabase.IsEnabled(connection, connection.CurrentDatabase))
			{
				var eligibleCdcConfigTables = CdcConfigurationInfo.GetEligibleCdcTables(connection);
				var cdcMismatches = GetCdcMismatches(connection, eligibleCdcConfigTables);
				result = CreateCdcMismatchLogs(cdcMismatches, treatExcessTablesAsMismatch);
			}

			return result;
		}

		#region Determine mismatches

		static CdcMismatches GetCdcMismatches(DbConnection connection, IEnumerable<CdcConfigurationInfo> eligibleCdcConfigTables)
		{
			var cdcMismatches = new CdcMismatches();

			var databaseCdcTableList = GetCdcTableListFromDb(connection);

			cdcMismatches.MissingTableList = eligibleCdcConfigTables.Where(t =>
				!databaseCdcTableList.Any(dt =>
					string.Equals(dt.SchemaName, t.SchemaName, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(dt.TableName, t.TableName, StringComparison.OrdinalIgnoreCase))).ToList();

			cdcMismatches.MissingCdcTableList = eligibleCdcConfigTables.Where(t =>
				databaseCdcTableList.Any(dt =>
					string.Equals(dt.SchemaName, t.SchemaName, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(dt.TableName, t.TableName, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(dt.CaptureInstance, t.CaptureInstance, StringComparison.OrdinalIgnoreCase))).ToList();

			cdcMismatches.ExcessCdcTableList = databaseCdcTableList.Where(dt =>
				!string.IsNullOrEmpty(dt.CaptureInstance) &&
				!eligibleCdcConfigTables.Any(t =>
					string.Equals(dt.SchemaName, t.SchemaName, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(dt.TableName, t.TableName, StringComparison.OrdinalIgnoreCase))).ToList();

			bool isAuditEnabled = !string.IsNullOrEmpty(BiServers.LoadAuditServerUsingCacheIfPossible(connection));
			bool isEdwEnabled = !string.IsNullOrEmpty(BiServers.LoadDataWarehouseServerUsingCacheIfPossible(connection));

			foreach (var cdcConfigTable in eligibleCdcConfigTables.Where(t => databaseCdcTableList.Contains(t)))
			{
				var cdcTable = new CdcTable(cdcConfigTable.SchemaName, cdcConfigTable.TableName);
				var eligibleCdcColumns = cdcTable.GetEligibleCdcColumns(isAuditEnabled, isEdwEnabled);

				GetCdcMismatchesForTable(connection, cdcMismatches, cdcConfigTable, eligibleCdcColumns);
			}

			return cdcMismatches;
		}

		static void GetCdcMismatchesForTable(DbConnection connection, CdcMismatches cdcMismatches, CdcConfigurationInfo cdcTable, IEnumerable<string> eligibleCdcColumns)
		{
			var cdcColumnList = GetCdcColumnListForTableFromDb(connection, cdcTable);
			var columnList = GetColumnListForTableFromDb(connection, cdcTable);

			cdcMismatches.MissingColumnList.AddRange(
				eligibleCdcColumns
				.Where(c => !Contains(columnList, c))
				.Select(c => new CdcConfigurationInfo(cdcTable.SchemaName, cdcTable.TableName, cdcTable.CaptureInstance, c)));

			cdcMismatches.MissingCdcColumnList.AddRange(
				eligibleCdcColumns
				.Where(c => !Contains(cdcColumnList, c) && Contains(columnList, c))
				.Select(c => new CdcConfigurationInfo(cdcTable.SchemaName, cdcTable.TableName, cdcTable.CaptureInstance, c)));

			cdcMismatches.ExcessCdcColumnList.AddRange(
				cdcColumnList
				.Where(c => !Contains(eligibleCdcColumns, c))
				.Select(c => new CdcConfigurationInfo(cdcTable.SchemaName, cdcTable.TableName, cdcTable.CaptureInstance, c)));
		}

		static bool Contains(IEnumerable<string> container, string content)
		{
			return container.Any(c => string.Equals(c, content, StringComparison.OrdinalIgnoreCase));
		}

		static List<CdcConfigurationInfo> GetCdcTableListFromDb(DbConnection connection)
		{
			var cdcTableList = new List<CdcConfigurationInfo>();

			using (var cmd = connection.Command("usp_GetCdcTableListFromDb"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var schema = reader["schemaName"].ToString();
						var table = reader["tableName"].ToString();

						string captureInstance = null;
						var captureInstanceObj = reader["captureInstance"];
						if (captureInstanceObj != DBNull.Value)
						{
							captureInstance = captureInstanceObj.ToString();
						}

						cdcTableList.Add(new CdcConfigurationInfo(schema, table, captureInstance));
					}
				}
			}

			return cdcTableList;
		}

		static IEnumerable<string> GetCdcColumnListForTableFromDb(DbConnection connection, CdcConfigurationInfo cdcTable)
		{
			using (var cmd = connection.Command("usp_GetCdcColumnListForTableFromDb"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@CaptureInstance", SqlDbType.NVarChar, 128, cdcTable.CaptureInstance);
				var cdcColumnList = DataUtils.GetListOfValuesFromCommand(cmd);
				return cdcColumnList;
			}
		}

		static IEnumerable<string> GetColumnListForTableFromDb(DbConnection connection, CdcConfigurationInfo table)
		{
			using (var cmd = connection.Command("usp_GetColumnListForTableFromDb"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, table.SchemaName);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, table.TableName);
				var columnList = DataUtils.GetListOfValuesFromCommand(cmd);
				return columnList;
			}
		}

		#endregion

		#region Logs

		static string CreateCdcMismatchLogs(CdcMismatches cdcMismatches, bool treatExcessTablesAsMismatch)
		{
			var builder = new StringBuilder();

			CheckMissingTables(cdcMismatches, builder);
			CheckMissingCdcTables(cdcMismatches, builder);
			CheckMissingColumns(cdcMismatches, builder);
			CheckMissingCdcColumns(cdcMismatches, builder);
			CheckExcessCaptureInstances(cdcMismatches, treatExcessTablesAsMismatch, builder);
			CheckExcessCdcColumns(cdcMismatches, builder);

			return (builder.Length == 0) ? null : builder.ToString();
		}

		static void CheckMissingTables(CdcMismatches cdcMismatches, StringBuilder builder)
		{
			if (cdcMismatches.MissingTableList != null && cdcMismatches.MissingTableList.Any())
			{
				builder.AppendLine("The following CDC configured tables were not found in the database:");
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n\r\n", string.Join("\r\n", cdcMismatches.MissingTableList.Select(t => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", t.SchemaName, t.TableName))));
			}
		}

		static void CheckMissingCdcTables(CdcMismatches cdcMismatches, StringBuilder builder)
		{
			if (cdcMismatches.MissingCdcTableList != null && cdcMismatches.MissingCdcTableList.Any())
			{
				builder.AppendLine("The following tables should be enabled for CDC:");
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n\r\n", string.Join("\r\n", cdcMismatches.MissingCdcTableList.Select(t => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", t.SchemaName, t.TableName))));
			}
		}

		static void CheckMissingColumns(CdcMismatches cdcMismatches, StringBuilder builder)
		{
			if (cdcMismatches.MissingColumnList != null && cdcMismatches.MissingColumnList.Any())
			{
				builder.AppendLine("The following CDC configured columns were not found in the database:");
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n\r\n", string.Join("\r\n", cdcMismatches.MissingColumnList.Select(c => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", c.SchemaName, c.TableName, c.ColumnName))));
			}
		}

		static void CheckMissingCdcColumns(CdcMismatches cdcMismatches, StringBuilder builder)
		{
			if (cdcMismatches.MissingCdcColumnList != null && cdcMismatches.MissingCdcColumnList.Any())
			{
				builder.AppendLine("The following columns should be enabled for CDC:");
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n\r\n", string.Join("\r\n", cdcMismatches.MissingCdcColumnList.Select(c => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", c.SchemaName, c.TableName, c.ColumnName))));
			}
		}

		static void CheckExcessCaptureInstances(CdcMismatches cdcMismatches, bool treatExcessTablesAsMismatch, StringBuilder builder)
		{
			if (treatExcessTablesAsMismatch && cdcMismatches.ExcessCdcTableList != null && cdcMismatches.ExcessCdcTableList.Any())
			{
				builder.AppendLine("The following capture instances should be disabled for CDC:");
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n\r\n", string.Join("\r\n", cdcMismatches.ExcessCdcTableList.Select(t => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}] [{2}]", t.SchemaName, t.TableName, t.CaptureInstance))));
			}
		}

		static void CheckExcessCdcColumns(CdcMismatches cdcMismatches, StringBuilder builder)
		{
			if (cdcMismatches.ExcessCdcColumnList != null && cdcMismatches.ExcessCdcColumnList.Any())
			{
				builder.AppendLine("The following columns should be disabled for CDC:");
				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n\r\n", string.Join("\r\n", cdcMismatches.ExcessCdcColumnList.Select(c => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].[{2}]", c.SchemaName, c.TableName, c.ColumnName))));
			}
		}

		#endregion

		#region CDC Mismatch Object

		class CdcMismatches
		{
			public CdcMismatches()
			{
				MissingCdcTableList = new List<CdcConfigurationInfo>();
				ExcessCdcTableList = new List<CdcConfigurationInfo>();
				MissingCdcColumnList = new List<CdcConfigurationInfo>();
				ExcessCdcColumnList = new List<CdcConfigurationInfo>();
				MissingTableList = new List<CdcConfigurationInfo>();
				MissingColumnList = new List<CdcConfigurationInfo>();
			}

			public List<CdcConfigurationInfo> MissingCdcTableList;
			public List<CdcConfigurationInfo> ExcessCdcTableList;
			public List<CdcConfigurationInfo> MissingCdcColumnList;
			public List<CdcConfigurationInfo> ExcessCdcColumnList;
			public List<CdcConfigurationInfo> MissingTableList;
			public List<CdcConfigurationInfo> MissingColumnList;
		}

		#endregion
	}

	#endregion
}
