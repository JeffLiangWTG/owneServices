using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Configuration;
using CargoWise.Data;
using CargoWise.Resource.Shared;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DbUpgrader.Shared;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace Enterprise.DbUpgrader.Schema.Template
{
	public class AuditDbTemplate : LatestSchemaDbTemplate
	{
		public AuditDbTemplate(IUpgradeManager manager, string templateDbName, string serverName)
			: base(manager, templateDbName, serverName)
		{
		}

		protected override void CreateAllDbObjects(DbConnection conn)
		{
			_ = conn.ExecuteNonQuery(GetAuditDbSchemaScript());
			var requiredTables = LoadCdcConfigurationFromShared();
			AddRequiredTablesAndColumns(conn, requiredTables);
		}

		protected virtual IEnumerable<CdcRequiredTable> LoadCdcConfigurationFromShared()
		{
			return CdcRequiredTableHelper.Load(Assembly.Load("Enterprise.DbUpgrader.Resource"));
		}

		protected virtual string GetAuditDbSchemaScript()
		{
			var manager = new ScriptManager();
			return manager.AuditDbSchemaScript;
		}

		protected virtual BiConfigurationData GetBiConfigurationData()
		{
			return new BiConfigurationData();
		}

		void AddRequiredTablesAndColumns(DbConnection conn, IEnumerable<CdcRequiredTable> requiredTables)
		{
			if (requiredTables.Any())
			{
				var tableConfig = GetBiConfigurationData()
					.CdcTableConfig
					.ToDictionary(x => TableKey(x.SourceSchema, x.SourceTable), StringComparer.InvariantCultureIgnoreCase);
				AddRequiredTables(conn, tableConfig, requiredTables);
				AddRequiredColumns(conn, tableConfig, requiredTables);
			}
		}

		static string TableKey(string schemaName, string tableName) => $"{schemaName}.{tableName}";

		void AddRequiredTables(DbConnection conn, IReadOnlyDictionary<string, CdcTableConfigRow> tableConfig, IEnumerable<CdcRequiredTable> requiredTables)
		{
			var missingRequiredTables = DataUtils.GetDataTableFromQuery(conn, GenerateScriptForFindingMissingRequiredTables(requiredTables));
			foreach (var row in missingRequiredTables.Select())
			{
				var schemaName = row["schemaName"].ToString();
				var tableName = row["tableName"].ToString();
				if (tableConfig.TryGetValue(TableKey(schemaName, tableName), out var cdcTable))
				{
					CreateAuditTableAndIndex(conn, cdcTable);
				}
			}
		}

		void AddRequiredColumns(DbConnection conn, IReadOnlyDictionary<string, CdcTableConfigRow> tableConfig, IEnumerable<CdcRequiredTable> requiredTables)
		{
			var missingRequiredColumns = DataUtils.GetDataTableFromQuery(conn, GenerateScriptForFindingMissingRequiredColumns(requiredTables));

			foreach (var requiredTable in requiredTables)
			{
				var missingColumns = missingRequiredColumns.Select()
					.Where(x =>
						x["schemaName"].ToString().Equals(requiredTable.SchemaName, StringComparison.InvariantCultureIgnoreCase) &&
						x["tableName"].ToString().Equals(requiredTable.TableName, StringComparison.InvariantCultureIgnoreCase))
					.Select(x => x["columnName"].ToString());
				if (missingColumns.Any() && tableConfig.TryGetValue(TableKey(requiredTable.SchemaName, requiredTable.TableName), out var cdcTable))
				{
					var addColumnsDefinition = BiScriptHelper.GenerateAuditTableAddColumnsDefinition(cdcTable, missingColumns);
					_ = conn.ExecuteNonQuery(addColumnsDefinition);
				}
			}
		}

		void CreateAuditTableAndIndex(DbConnection conn, CdcTableConfigRow cdcTable)
		{
			var pkName = GetPrimaryKey(cdcTable, cdcTable.SourceSchema, cdcTable.SourceTable);
			// GenerateScriptForFindingMissingRequiredTables returned all required tables in Issue 01821948
			// Can not reproduce it locally, maybe to do with SQL Replication or Mirroring
			// Here we use IF OBJECT_ID to ensure don't create tables twice
			var script = "IF OBJECT_ID('[" + cdcTable.SourceSchema + "].[" + cdcTable.SourceTable + "]') IS NULL\r\nBEGIN\r\n" +
				BiScriptHelper.GenerateAuditTableDefinition(cdcTable) +
				BiScriptHelper.GenerateAuditStartLsnIndexDefinition(cdcTable.SourceSchema, cdcTable.SourceTable, pkName) + "END\r\n";
			_ = conn.ExecuteNonQuery(script);
		}

		string GenerateScriptForFindingMissingRequiredTables(IEnumerable<CdcRequiredTable> requiredTables)
		{
			return string.Format(CultureInfo.InvariantCulture,
$@"SELECT ReqTab.* FROM
(
	VALUES {string.Join(", ", requiredTables.Select(x => $"('{x.SchemaName}', '{x.TableName}')"))}
) AS ReqTab (schemaName, tableName)
INNER JOIN [{dbName}].sys.schemas CurSch on CurSch.name = ReqTab.schemaName
LEFT JOIN [{dbName}].sys.tables CurTab on CurTab.schema_id = CurSch.schema_id AND CurTab.name = ReqTab.tableName
WHERE CurTab.name IS NULL");
		}

		string GetPrimaryKey(CdcTableConfigRow cdcTableConfig, string schemaName, string tableName)
		{
			var cdcColumnConfigRows = cdcTableConfig.GetOrderedCdcColumnConfigRows();
			foreach (CdcColumnConfigRow cdcColumnConfig in cdcColumnConfigRows)
			{
				if (cdcColumnConfig.IsPrimaryKey)
				{
					return cdcColumnConfig.SourceColumn;
				}
			}

			throw new BiConfigurationException(string.Format(CultureInfo.InvariantCulture, "Primary key not found in {0}.{1}", schemaName, tableName));
		}

		string GenerateScriptForFindingMissingRequiredColumns(IEnumerable<CdcRequiredTable> requiredTables)
		{
			return string.Format(CultureInfo.InvariantCulture,
$@"SELECT ReqCol.* FROM
(
	VALUES {string.Join(", ", requiredTables.SelectMany(table => table.ColumnNames.Select(column => $"('{table.SchemaName}', '{table.TableName}', '{column}')")))}
) AS ReqCol (schemaName, tableName, columnName)
INNER JOIN [{dbName}].sys.schemas CurSch on CurSch.name = ReqCol.schemaName
INNER JOIN [{dbName}].sys.tables CurTab on CurTab.schema_id = CurSch.schema_id AND CurTab.name = ReqCol.tableName
LEFT JOIN [{dbName}].sys.columns CurCol on CurCol.object_id = CurTab.object_id AND CurCol.name = ReqCol.columnName
WHERE CurCol.name IS NULL");
		}
	}
}
