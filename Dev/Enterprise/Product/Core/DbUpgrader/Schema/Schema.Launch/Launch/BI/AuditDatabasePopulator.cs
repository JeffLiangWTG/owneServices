using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Schema
{
	class AuditDatabasePopulator
	{
		public AuditDatabasePopulator(DbConnection biConnection)
		{
			this.biConnection = biConnection;
		}
		readonly DbConnection biConnection;

		public void Run()
		{
			TruncateTableConfiguration();
			PopulateTableConfiguration();
			PopulateTableState();
			PopulateSchemaHistory();
		}

		void TruncateTableConfiguration()
		{
			biConnection.ExecuteNonQuery("TRUNCATE TABLE biadmin.TableConfiguration");
		}

		void PopulateTableConfiguration()
		{
			var tableValueList = GetTablesFromCdcTableConfig();
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO biadmin.TableConfiguration(SourceSchemaName, SourceTableName, AuditFilter, TableColumnList, TableColumnConvertList, PkName)
VALUES
({0})", string.Join("),\r\n(", tableValueList));

			biConnection.ExecuteNonQuery(sqlText);
		}

		List<string> GetTablesFromCdcTableConfig()
		{
			var tableValueList = new List<string>();
			foreach (var table in ConfigData.CdcTableConfig.Where(t => t.TableInAudit))
			{
				var columnList = "'@SourceSchemaName', '@SourceTableName', '@AuditFilter', '__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,@TableColumnList', '__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,@TableColumnConvertList', '@PkName'";

				var cdcColumnConfig = table.GetOrderedCdcColumnConfigRows().Where(c => c.ColumnInAudit);

				columnList = columnList.Replace("@SourceSchemaName", table.SourceSchema);
				columnList = columnList.Replace("@SourceTableName", table.SourceTable);
				columnList = columnList.Replace("@AuditFilter", table.AuditFilter?.Replace("'", "''"));
				columnList = columnList.Replace("@TableColumnList", string.Join(",", cdcColumnConfig.Select(c => c.SourceColumn)));
				columnList = columnList.Replace("@TableColumnConvertList", string.Join(",", cdcColumnConfig.Select(c => (c.DataType == "xml") ? ($"convert(nvarchar(max),{c.SourceColumn}) AS {c.SourceColumn}") : ($"{c.SourceColumn}"))));
				var pkName = cdcColumnConfig.FirstOrDefault(c => c.IsPrimaryKey)?.SourceColumn;
				columnList = columnList.Replace("'@PkName'", string.IsNullOrEmpty(pkName) ? "NULL" : $"'{pkName}'");

				tableValueList.Add(columnList);
			}
			return tableValueList;
		}

		void PopulateTableState()
		{
			var sqlText =
@"UPDATE biadmin.TableState
SET CurrentState = @state

INSERT INTO biadmin.TableState(SourceSchemaName, SourceTableName, CurrentState)
SELECT SourceSchemaName, SourceTableName, @state
FROM biadmin.TableConfiguration tc WHERE NOT EXISTS
(SELECT NULL FROM biadmin.TableState ts  WHERE ts.SourceSchemaName = tc.SourceSchemaName AND ts.SourceTableName = tc.SourceTableName)

DELETE FROM biadmin.TableState
WHERE SourceSchemaName + '.' + SourceTableName IS NULL OR SourceSchemaName + '.' + SourceTableName NOT IN
(SELECT SourceSchemaName + '.' + SourceTableName FROM biadmin.TableConfiguration)";

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@state", SqlDbType.VarChar, "New");

				cmd.ExecuteNonQuery();
			}
		}

		void PopulateSchemaHistory()
		{
			PopulateSchemaVersionHistory();
			PopulateSchemaMappingHistory();
		}

		void PopulateSchemaVersionHistory()
		{
			biConnection.ExecuteNonQuery(@$"
DECLARE @MaxLsn BINARY(10) = NULL
SELECT @MaxLsn = ISNULL(
	CONVERT(BINARY(10), ParamValue, 1),
	NULL)
FROM biadmin.MasterState
WHERE ParamName = '{BiConstants.LastMaxLsnProcessed}' AND ParamValue <> ''

UPDATE [biadmin].SchemaVersionHistory
SET MaxLsn = @MaxLsn
WHERE MaxLsn IS NULL AND SchemaVersion <> '{SchemaVersion.Application}'

IF NOT EXISTS (SELECT null FROM [biadmin].SchemaVersionHistory WHERE SchemaVersion = '{SchemaVersion.Application}')
	INSERT INTO [biadmin].SchemaVersionHistory (SchemaVersion) SELECT '{SchemaVersion.Application}'");
		}

		void PopulateSchemaMappingHistory()
		{
			if (ShouldPopulateSchemaHistory())
			{
				var schemaVersionId = Convert.ToInt32(biConnection.ExecuteScalar("SELECT MAX(SchemaVersionId) FROM biadmin.SchemaVersionHistory"), CultureInfo.InvariantCulture);

				var getSchemaDetailsQuery = @"
			select CONCAT(@SchemaVersionId, ', ''', @SchemaName, ''', ''', @TableName, ''', ''', c.name, ''', ',
				cc.column_ordinal, ', ''', cc.column_type, ''',', c.max_length, ',', c.precision, ',', c.scale)
			from sys.columns c
			inner join sys.tables t
				on c.object_id = t.object_id
			inner join cdc.change_tables ct
				on ct.source_object_id = t.object_id
			inner join cdc.captured_columns cc
				on c.name = cc.column_name and cc.object_id = ct.object_id
			where
				t.name = @TableName and t.schema_id = SCHEMA_ID(@SchemaName)";

				var insertToSchemaMappingHistoryQuery = @"
			DELETE FROM [biadmin].SchemaMappingHistory WHERE SchemaVersionId = @SchemaVersionId AND SchemaName = @SchemaName AND TableName = @TableName

			INSERT INTO [biadmin].SchemaMappingHistory (SchemaVersionId, SchemaName, TableName, ColumnName, ColumnOrdinal, DataType, MaxLength, Precision, Scale)
			VALUES ({0})";

				List<CdcConfigurationInfo> cdcTableList;
				if (biConnection.Exists("FROM biadmin.SchemaMappingHistory"))
				{
					using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.DatabaseName))
					{
						var enabledCdcTableList = DbRegistry.EnabledCdcTables.LoadValue(Db.Connection);
						DbRegistry.EnabledCdcTables.SaveValue("", Db.Connection);

						cdcTableList = new List<CdcConfigurationInfo>();
						if (!string.IsNullOrEmpty(enabledCdcTableList))
						{
							foreach (var table in enabledCdcTableList.Split(','))
							{
								var schemaName = table.Split('.')[0];
								var tableName = table.Split('.')[1];
								cdcTableList.Add(new CdcConfigurationInfo(schemaName, tableName));
							}
						}
					}
				}
				else
				{
					cdcTableList = ConfigData.CdcTableConfig.Where(t => t.TableInAudit).Select(t => new CdcConfigurationInfo(t.SourceSchema, t.SourceTable)).ToList();
				}

				foreach (var table in cdcTableList)
				{
					IEnumerable<string> columnSchemaList;
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
					using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.DatabaseName))
					using (var cmd = Db.Connection.Command(getSchemaDetailsQuery))
					{
						cmd.AddParameter("@SchemaVersionId", SqlDbType.Int, schemaVersionId);
						cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, table.SchemaName);
						cmd.AddParameter("@TableName", SqlDbType.NVarChar, table.TableName);

						columnSchemaList = DataUtils.GetListOfValuesFromCommand(cmd);
					}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods

					if (columnSchemaList.Any())
					{
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
						using (var cmd = biConnection.Command(string.Format(CultureInfo.InvariantCulture, insertToSchemaMappingHistoryQuery, string.Join("), (", columnSchemaList))))
						{
							cmd.AddParameter("@SchemaVersionId", SqlDbType.Int, schemaVersionId);
							cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, table.SchemaName);
							cmd.AddParameter("@TableName", SqlDbType.NVarChar, table.TableName);

							cmd.ExecuteNonQuery();
						}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
					}
				}
			}
		}

		bool ShouldPopulateSchemaHistory()
		{
			using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.DatabaseName))
			{
				return CdcDatabase.IsEnabled(Db.Connection, Db.DatabaseName);
			}
		}

		BiConfigurationData ConfigData
		{
			get
			{
				return configData ?? (configData = BiAutomationConfigLoader.Instance.ConfigData);
			}
		}
		BiConfigurationData configData;
	}
}
