using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.BusinessIntelligence
{
	public class RecreateIndexesForAuditDb : AuditDataTransformation
	{
		public override string UserDescription => "Create __$command_id column in audit tables and recreate indexes";

		public override void RunAuditTransformation(DbConnection auditConnection, TransformationSection section, CancellationToken token)
		{
			var tcExists = TableConfigurationExists(auditConnection);
			if (tcExists)
			{
				if (section == TransformationSection.OnlinePreUpgrade)
				{
					ShowStartTransformation();

					EnsureTableConfigurationHasPkNameColumn(auditConnection);
					tablesListForRecreateIndex = GetListofTableToRecreateIndex(auditConnection);
					if (tablesListForRecreateIndex.Any())
					{
						AddCommandIdColumnToAuditTablesIfNotExists(auditConnection);
						ResetIndexMaintenanceSchedule(auditConnection, DateTime.MaxValue);
						KillIndexMaintenanceTask(auditConnection);
						RecreateIndexesForAuditTables(auditConnection);
					}

					TransformationCompleted();
				}
				else if (section == TransformationSection.OfflinePostUpgrade)
				{
					ShowStartTransformation();
	#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
					ResetIndexMaintenanceSchedule(auditConnection, DateTime.UtcNow);
	#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
					TransformationCompleted();
				}
			}
		}
		
		bool TableConfigurationExists(DbConnection auditConnection)
		{
			var sqlText = @"IF EXISTS (
					SELECT NULL FROM sys.tables t
					WHERE t.schema_id = SCHEMA_ID('biadmin') 
					AND t.name = 'TableConfiguration'
				) SELECT 1 ELSE SELECT 0";
			return Convert.ToBoolean(auditConnection.ExecuteScalar(sqlText));
		}

		void ResetIndexMaintenanceSchedule(DbConnection auditConnection, DateTime dateTime)
		{
			var sqlText = $@"
IF EXISTS (SELECT NULL FROM [{BiConstants.BiAdminSchemaName}].[MasterState] WHERE ParamName = @ParamName)
BEGIN
	UPDATE [{BiConstants.BiAdminSchemaName}].[MasterState] SET ParamValue = @ParamValue WHERE ParamName = @ParamName
END ELSE
BEGIN
	INSERT INTO [{BiConstants.BiAdminSchemaName}].[MasterState] (ParamName, ParamValue)
	VALUES(@ParamName, @ParamValue)
END";
			using (var cmd = auditConnection.Command(sqlText))
			{
				cmd.AddParameter("@ParamName", System.Data.SqlDbType.NVarChar, 128, BiConstants.LastIndexRebuildUtcDt);
				cmd.AddParameter("@ParamValue", System.Data.SqlDbType.NVarChar, 128, dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
				cmd.ExecuteNonQuery();
			}
		}

		void AddCommandIdColumnToAuditTablesIfNotExists(DbConnection auditConnection)
		{
			ShowInfo("Adding __$command_id to audit tables");
			var sqlText = @"
DECLARE @sqlStmt NVARCHAR(MAX) = ''
SELECT @sqlStmt = @sqlStmt + 'IF NOT EXISTS (select null from sys.columns where name=''__$command_id'' and object_id = object_id(''' + schema_name(schema_id) + '.' + name + ''')) ALTER TABLE [' + schema_name(schema_id) + '].[' + name + '] ADD [__$command_id] int NOT NULL DEFAULT 0;' + CHAR(13) + CHAR(10)
from sys.tables where schema_name(schema_id) <> 'biadmin' order by schema_id, name

EXEC (@sqlStmt)";

			auditConnection.ExecuteNonQuery(sqlText);
		}

		void KillIndexMaintenanceTask(DbConnection auditConnection)
		{
			try
			{
				ShowInfo("Killing index maintenance task");
				var sqlText = @"
declare @DbId int = DB_ID(DB_NAME())
declare @SpName varchar(128) = 'usp_IndexMaintenance'
declare @killCmd varchar(max) = ''

SELECT
  @killCmd = 'KILL ' + CAST(es.session_id as varchar)
FROM sys.dm_exec_connections as qs 
	inner join sys.dm_exec_sessions es on es.session_id = qs.session_id
	CROSS APPLY sys.dm_exec_sql_text(qs.most_recent_sql_handle) st 
WHERE
  es.database_id = DB_ID(DB_NAME())
  and object_name(st.objectid) = @SpName
  and st.dbid = @DbId

IF (@killCmd <> '')
EXEC @killCmd
";
				auditConnection.ExecuteNonQuery(sqlText);
			}
			catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.NotAnActiveProcessId)
			{
			}
		}

		void RecreateIndexesForAuditTables(DbConnection auditConnection)
		{
			ShowInfo("Recreating index for audit tables");
			foreach (var auditTable in tablesListForRecreateIndex)
			{
				ShowInfo($"(+) [{auditTable.SchemaName}].[{auditTable.TableName}]");

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS (select null from sys.indexes where name='IX_{1}_StartLsn' and object_id = object_id('{0}.{1}'))
	DROP INDEX [IX_{1}_StartLsn] ON [{0}].[{1}];

CREATE NONCLUSTERED INDEX [IX_{1}_StartLsn] ON [{0}].[{1}]
(
	[__$start_lsn] ASC,
	[__$command_id] ASC,
	[__$seqval] ASC,
	[__$operation] ASC
) INCLUDE (__$update_mask, {2}) WITH (DATA_COMPRESSION = PAGE)", auditTable.SchemaName, auditTable.TableName, auditTable.PrimaryKeyName);

				auditConnection.ExecuteNonQuery(sqlText);
			}
		}

		IEnumerable<AuditTable> tablesListForRecreateIndex { get; set; }

		public IEnumerable<AuditTable> GetListofTableToRecreateIndex(DbConnection auditConnection)
		{
			var sqlText = @"
SELECT
	SourceSchemaName AS SchemaName,
	SourceTableName AS TableName,
	PkName
INTO #TABLE_LIST
FROM biadmin.TableConfiguration;

(	
	SELECT DISTINCT
		s.name, t.name, config.PkName
	FROM sys.tables t
		INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
		LEFT JOIN sys.indexes i ON i.object_id = t.object_id AND i.name = 'IX_' + t.name + '_StartLsn'
		INNER JOIN 
		#TABLE_LIST config ON config.SchemaName  COLLATE database_default = s.name AND config.TableName  COLLATE database_default = t.name 
	WHERE
		s.name <> 'biadmin' AND i.name is NULL
)
UNION
(
	SELECT DISTINCT
		s.name, t.name, config.PkName
	FROM sys.tables t
		INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
		INNER JOIN sys.indexes i ON i.object_id = t.object_id
		INNER JOIN sys.index_columns ic ON ic.index_id = i.index_id AND ic.object_id = t.object_id
		INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
		INNER JOIN 
		#TABLE_LIST config ON config.SchemaName  COLLATE database_default = s.name AND config.TableName  COLLATE database_default = t.name 
	WHERE
		i.name = 'IX_' + t.name + '_StartLsn'
		AND ic.key_ordinal <> 0
		AND config.PKName is not NULL
	GROUP BY s.name, t.name, config.PkName
	HAVING STRING_AGG(c.name, ',') <> '__$start_lsn,__$command_id,__$seqval,__$operation'
)
UNION
(
	SELECT DISTINCT 
		s.name, t.name, config.PkName
	FROM 
		sys.tables t 
	LEFT JOIN
		sys.indexes i ON t.object_id = i.object_id AND i.type = 2 AND i.name = 'IX_' + t.name + '_StartLsn'
	LEFT JOIN
		sys.schemas s ON s.schema_id = t.schema_id
	LEFT JOIN 
		#TABLE_LIST config ON config.SchemaName  COLLATE database_default = s.name AND config.TableName  COLLATE database_default = t.name 
	LEFT JOIN 
		sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1		
	LEFT JOIN 
		sys.columns c ON c.object_id = i.object_id  AND c.column_id = ic.column_id AND (c.name = '__$update_mask' OR c.name = config.PkName COLLATE database_default)
	WHERE 
		schema_name(t.schema_id) <> 'biadmin' 
		AND c.name is NULL
		AND config.PkName is not NULL
);

DROP TABLE #TABLE_LIST;";
			var result = new List<AuditTable>();
			using (var reader = auditConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var schemaName = reader[0].ToString();
					var tableName = reader[1].ToString();
					var pkName = reader[2].ToString();
					result.Add(new AuditTable
					{
						SchemaName = schemaName, TableName = tableName, PrimaryKeyName = pkName
					});
				}
			}
			return result;
		}

		public void EnsureTableConfigurationHasPkNameColumn(DbConnection connection)
		{
			var columnExists = DbObjectCreator.ColumnExists(connection, Db.AuditDatabaseName, "biadmin", "TableConfiguration", "PkName");
			if (!columnExists)
			{
				var tableData = new StringBuilder();
				foreach (var table in BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInAudit))
				{
					var cdcColumnConfig = table.GetOrderedCdcColumnConfigRows().Where(c => c.ColumnInAudit);
					var pkName = cdcColumnConfig.FirstOrDefault(c => c.IsPrimaryKey)?.SourceColumn;
					if (pkName == null)
					{
						continue;
					}
					var schemaName = table.SourceSchema;
					var tableName = table.SourceTable;
					tableData.Append($"('{tableName}', '{schemaName}', '{pkName}'),\n");
				}
				tableData.Remove(tableData.Length - 2, 2); // To remove the comma and \n
				connection.ExecuteNonQuery(@"
ALTER TABLE biadmin.TableConfiguration
ADD PkName varchar(128)");
				var sqlText = string.Format(CultureInfo.InvariantCulture, @$"
UPDATE biadmin.TableConfiguration
SET PkName = ReferData.tablePK
FROM biadmin.TableConfiguration
JOIN (VALUES 
{tableData}
) as ReferData(tableName, tableSchema, tablePK)
ON tableName = SourceTableName AND tableSchema = SourceSchemaName;");
				connection.ExecuteNonQuery(sqlText);
			}
		}

		public struct AuditTable
		{
			public string SchemaName;
			public string TableName;
			public string PrimaryKeyName;
		}
	}
}
