using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase
{
	public abstract class CopyAddInfoToRealColumn : DataTransformation
	{
		public string Identifier => GetIdentifier();
		protected virtual string GetIdentifier() => GetType().Name;
		public string SourceTableSchemaName => SourceTableSchema.SqlSchemaName;
		public abstract SchemaStringColumn SourceAddInfoColumn { get; }
		public ITableSchema SourceTableSchema => SourceAddInfoColumn.TableSchema;
		public string SourceTableName => SourceAddInfoColumn.TableName;
		public string SourceTablePrefix => SourceAddInfoColumn.ColumnPrefix;
		public SchemaPKColumn SourceTablePK => SourceTableSchema.PK;

		public string OffLineProcessingTableName => $"Client_{SourceTableName}_{Identifier}";

		public SchemaIntColumn GetClusterKeyColumn(SchemaColumn tableColumn)
		{
			var column = tableColumn.TableSchema.GetSchemaColumn($"{tableColumn.ColumnPrefix}_ClusterKey") as SchemaIntColumn;
			if (column != null && !DbObjectCreator.ColumnExists(Db.Connection, new DbObjectCreator.TableDescriptor(tableColumn.TableSchema.SqlSchemaName, tableColumn.TableName), column.Name))
			{
				column = null;
			}
			return column;
		}

		public string GetTriggerName() => FormattableString.Invariant($"{UpgUtils.UpgraderPrefix}TG_{OffLineProcessingTableName}");

		public string GetClusterKeyTriggerName(string sourceTableClusterKeyColumnName) => FormattableString.Invariant($"{UpgUtils.UpgraderPrefix}TG_{OffLineProcessingTableName}_{sourceTableClusterKeyColumnName}");

		public string CreateOffLineProcessingTable(DbConnection connection, string databaseName, bool sourceHasClusterKeyColumn)
		{
			var reprocessingTableFullName = $"{databaseName.QuoteName()}.{Db.SqlDbOwnerSchema.QuoteName()}.{OffLineProcessingTableName.QuoteName()}";
			var clusterKeyData = string.Empty;
			if (sourceHasClusterKeyColumn)
			{
				clusterKeyData = $", ClusterKey INT NOT NULL DEFAULT 0, INDEX [NR_RC_{Identifier}_ClusterKey] CLUSTERED (ClusterKey ASC)";
			}
			var sql = FormattableString.Invariant($@"-- Create Offline Processing Table if not exist
IF (OBJECT_ID(N'{reprocessingTableFullName}', N'U') IS NULL)
BEGIN
	CREATE TABLE {reprocessingTableFullName} (PK UNIQUEIDENTIFIER NOT NULL PRIMARY KEY{clusterKeyData});
END
");

			connection.ExecuteNonQuery(sql);
			return reprocessingTableFullName;
		}

		public virtual string AdditionalSourceTableJoin => string.Empty;
		public virtual string SourceTableHint => string.Empty;

		protected sealed override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, OffLineProcessingTableName))
			{
				var sourceTableClusterKeyColumn = GetClusterKeyColumn(SourceAddInfoColumn);
				if (TargetTableColumnMapping.Count > 0)
				{
					CopyData(sourceTableClusterKeyColumn);
				}
				DbObjectCreator.DropTriggerIfExists(Db.Connection, GetTriggerName());
				if (sourceTableClusterKeyColumn != null)
				{
					DbObjectCreator.DropTriggerIfExists(Db.Connection, GetClusterKeyTriggerName(sourceTableClusterKeyColumn.Name));
				}
				DbObjectCreator.DropTableIfExists(Db.Connection, OffLineProcessingTableName);
				ExtProperty.Table.Delete(Db.Connection, SourceTableSchemaName, SourceTableName, Identifier);
			}
			UpdateAlreadyProcessedData(string.Join(System.Environment.NewLine, GetAddInfoColumnMapping().Values.Select(x => x.AddInfoPropertyName).OrderBy(x => x)));
		}

		static string GetTablePrefix(string columnName)
		{
			var indexOfUnderscore = columnName.IndexOf('_');
			return columnName.Substring(0, indexOfUnderscore);
		}

		bool ColumnExists(string columnName)
		{
			return SourceTableSchema.All.Any(column => column.Name == columnName);
		}

		protected virtual void CopyData(SchemaIntColumn sourceTableClusterKeyColumn)
		{
			var addInfoCrossApplyBuilder = new List<string>();
			var updateValueBuilder = new List<string>();
			var cteSelectValueBuilder = new List<string>();
			var whereClauseBuilder = new List<string>();
			var targetTableColumnMapping = TargetTableColumnMapping;
			foreach (var columnMapping in targetTableColumnMapping)
			{
				var columnName = columnMapping.Key;
				addInfoCrossApplyBuilder.Add(columnMapping.Value.JoinStatement);
				updateValueBuilder.Add($"{columnName} = New{columnName}");
				cteSelectValueBuilder.Add($"{columnName}, {columnMapping.Value.SelectStatement} AS New{columnName}");
				whereClauseBuilder.Add($"OR ISNULL(NULLIF({columnName}, New{columnName}), NULLIF(New{columnName}, {columnName})) IS NOT NULL");
			}

			var tablePrefix = GetTablePrefix(targetTableColumnMapping.First().Key);
			var systemLastEditTimeUtcColumnName = (tablePrefix + "_SystemLastEditTimeUtc");
			if (ColumnExists(systemLastEditTimeUtcColumnName))
			{
				var quoteName = systemLastEditTimeUtcColumnName.QuoteName();
				updateValueBuilder.Add($"{systemLastEditTimeUtcColumnName} = GETUTCDATE()");
				cteSelectValueBuilder.Add(systemLastEditTimeUtcColumnName);
			}
			var systemLastEditUserColumnName = (tablePrefix + "_SystemLastEditUser");
			if (ColumnExists(systemLastEditUserColumnName))
			{
				var quoteName = systemLastEditUserColumnName.QuoteName();
				updateValueBuilder.Add($"{systemLastEditUserColumnName} = '~BP'");
				cteSelectValueBuilder.Add(systemLastEditUserColumnName);
			}

			var clusterKeyJoin = string.Empty;
			if (sourceTableClusterKeyColumn != null)
			{
				clusterKeyJoin = $" AND SourceTable.{sourceTableClusterKeyColumn.Name.QuoteName()} = ReprocessingTable.ClusterKey";
			}
			var fullSourceTableNameQuoted = $"{SourceTableSchemaName.QuoteName()}.{SourceTableName.QuoteName()}";
			var sqlMergeText = $@"
WITH SourceDataCTE AS
(
	SELECT {string.Join(", ", cteSelectValueBuilder)}
	FROM {fullSourceTableNameQuoted} SourceTable {AdditionalSourceTableJoin}
	INNER JOIN {Db.SqlDbOwnerSchema.QuoteName()}.{OffLineProcessingTableName.QuoteName()} ReprocessingTable ON SourceTable.{SourceTablePK.Name.QuoteName()} = ReprocessingTable.PK{clusterKeyJoin}
	{string.Join("\r\n\t", addInfoCrossApplyBuilder)}
)

UPDATE SourceDataCTE
SET {string.Join(", ", updateValueBuilder)}
WHERE
	1=2
	{string.Join("\r\n\t", whereClauseBuilder)}
";
			Db.Connection.ExecuteNonQuery(sqlMergeText);
		}

		protected string GetAddInfoConversion(SchemaColumn column, string addInfoInLineName)
		{
			string result;
			if (column is SchemaStringColumn)
			{
				result = $"TRY_CAST({addInfoInLineName}.Value AS {column.SqlDbTypeDeclaration})";
				if (!column.IsNullable)
				{
					result = $"COALESCE({result}, '{column.SqlDbDefault.ToString()}')";
				}
			}
			else if (column is SchemaNumericColumn)
			{
				result = $"TRY_CAST({addInfoInLineName}.Value AS {column.SqlDbTypeDeclaration})";
				if (!column.IsNullable)
				{
					result = $"COALESCE({result}, {column.SqlDbDefault.ToString()})";
				}
			}
			else if (column is SchemaGuidColumn guidColumn || column is SchemaDateTimeColumn || column is SchemaPKColumn)
			{
				result = $"TRY_CAST({addInfoInLineName}.Value AS {column.SqlDbTypeDeclaration})";
			}
			else if (column is SchemaBoolColumn boolColumn)
			{
				var trueValue = "'Y'";
				var falseValue = "'N'";
				if (boolColumn.IsBitField)
				{
					trueValue = "1";
					falseValue = "0";
				}
				result = $"IIF({addInfoInLineName}.Value = 'Y', {trueValue}, {falseValue})";
			}
			else if (column is SchemaDateTimeOffsetColumn)
			{
				result = $"TRY_CAST({addInfoInLineName}.Value AS {column.TypeInfo})";
			}
			else
			{
				throw new NotSupportedException($"{column.GetType().Name} is not currently supported in conversion");
			}
			return result;
		}

		public IDictionary<string, (SchemaColumn AddInfoColumnMapping, string AddInfoPropertyName, string SelectStatement, string JoinStatement, string WhereClauseStatement)> TargetTableColumnMapping => GatherTargetTableColumns();

		public HashSet<string> GetAlreadyProcessedAddInfos()
		{
			var alreadyProcessedAddInfos = new HashSet<string>();
			using (var command = Db.Connection.Command($"SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = '{AlreadyProcessedDataName}' AND SD_Owner IS NULL AND SD_DepartmentGuid IS NULL"))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var dbValue = reader[StmDataSchema.Constants.SD_BinaryValue];

					if (dbValue == DBNull.Value)
					{
						continue;
					}

					System.Text.Encoding.ASCII.GetString((byte[])dbValue).SplitByLine().Where(x => !string.IsNullOrEmpty(x)).ForEach(x => alreadyProcessedAddInfos.Add(x));
					break;
				}
			}
			return alreadyProcessedAddInfos;
		}

		string AlreadyProcessedDataName => $"AlreadyProcessed{Identifier}";
		public void UpdateAlreadyProcessedData(string data)
		{
			using (var cmd = Db.Connection.Command($@"MERGE dbo.StmData AS T
USING (VALUES (@Name)) AS S ([_Name])
ON T.SD_Name = S._Name AND SD_Owner IS NULL AND SD_DepartmentGuid IS NULL
WHEN MATCHED THEN
	UPDATE SET SD_BinaryValue = @Data
WHEN NOT MATCHED BY TARGET THEN
	INSERT(SD_PK, SD_Name, SD_BinaryValue)
	VALUES(NEWID(), @Name, @Data);"))
			{
				cmd.AddParameter("@Name", SqlDbType.VarChar, AlreadyProcessedDataName);
				cmd.AddParameter("@Data", SqlDbType.VarBinary, System.Text.Encoding.ASCII.GetBytes(data));
				cmd.ExecuteNonQuery();
			}
		}

		IDictionary<string, (SchemaColumn AddInfoColumnMapping, string AddInfoPropertyName, string SelectStatement, string JoinStatement, string WhereClauseStatement)> GatherTargetTableColumns()
		{
			var columnMappings = new Dictionary<string, (SchemaColumn addInfoColumnMapping, string addInfoPropertyName, string SelectStatement, string JoinStatement, string WhereClauseStatement)>();
			var sourceAddInfoColumnName = SourceAddInfoColumn.Name;
			var sourceAddInfoColumnNameQuoted = sourceAddInfoColumnName.QuoteName();
			var alreadyProcessed = GetAlreadyProcessedAddInfos();
			foreach (var columnMapping in GetAddInfoColumnMapping())
			{
				(var addInfoPropertyName, string selectStatement, string addInfoValueJointStatement) = columnMapping.Value;
				if (!alreadyProcessed.Contains(addInfoPropertyName))
				{
					var targetColumn = columnMapping.Key;
					var addInfoInLineName = "AddInfo" + addInfoPropertyName;
					var addInfoConversionType = GetAddInfoConversion(targetColumn, addInfoInLineName);
					if (string.IsNullOrEmpty(selectStatement))
					{
						selectStatement = addInfoConversionType;
					}
					var joinStatement = $"CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline({sourceAddInfoColumnNameQuoted}, '{addInfoPropertyName}') AS {addInfoInLineName}";
					if (!string.IsNullOrEmpty(addInfoValueJointStatement))
					{
						joinStatement += " " + string.Format(addInfoValueJointStatement, addInfoConversionType);
					}
					columnMappings.Add(targetColumn.Name, (targetColumn, addInfoPropertyName, selectStatement, joinStatement, $"{sourceAddInfoColumnNameQuoted} LIKE '%{addInfoPropertyName}=%'"));
				}
			}
			return columnMappings;
		}

		protected abstract IDictionary<SchemaColumn, (string AddInfoPropertyName, string SelectStatementOverride, string AddInfoValueJointStatement)> GetAddInfoColumnMapping();

		protected virtual string DefaultUserValue => "~BP";
	}
}
