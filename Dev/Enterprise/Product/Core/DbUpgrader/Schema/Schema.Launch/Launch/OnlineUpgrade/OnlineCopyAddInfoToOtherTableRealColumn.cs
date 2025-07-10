using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase;
using Enterprise.DbUpgrader.Transformations.Transforms;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	abstract class OnlineCopyAddInfoToOtherTableRealColumn<T> : OnlineCopyAddInfoToRealColumn<T>
		where T : CopyAddInfoToOtherTableRealColumn
	{
		public static string GetInsteadOfDeleteTriggerName(string tableName) => FormattableString.Invariant($"CopyAddInfoFrom_TG_{tableName}_DELETE");

		protected OnlineCopyAddInfoToOtherTableRealColumn(IUpgradeManager manager, string dbBeingUpgraded, string templateDb)
			: base(manager, dbBeingUpgraded, templateDb)
		{
		}

		protected override ITableSchema GetTargetTableSchema(T transformation) => transformation.TargetTableSchema;
		protected override SchemaIntColumn GetTargetClusterKeyColumn(T transformation) => transformation.GetClusterKeyColumn(transformation.SourceAddInfoColumn) != null ? transformation.GetClusterKeyColumn(transformation.TargetForeignKeyColumn) : null;
		protected override IEnumerable<string> GetTargetColumnNames(T transformation) => transformation.TargetTableMainColumnMapping.Select(x => x.Key).Union(transformation.TargetTableColumnMapping.Select(c => c.Value.AddInfoColumnMapping.Name));
		protected override void PopulateNonMappedColumnIfNeeded(T transformation, PopulatedTable populatedTable, ColumnChangeMetadata col, PopulatedColumn populatedColumn)
		{
			if (transformation.TargetTableMainColumnMapping.TryGetValue(col.ColumnName, out var mainStatement))
			{
				populatedColumn.PopulateSource = "LEFT JOIN (SELECT 1 _DUMMY) AS DUMMY ON 1=2";
				populatedColumn.PopulateExpression = mainStatement.selectStatement;
				if (populatedColumn.ColumnNullable.Equals("NOT NULL", StringComparison.OrdinalIgnoreCase)
					&& populatedColumn.ColumnName.Substring(transformation.TargetForeignKeyColumn.ColumnPrefix.Length) is string columnNameSuffix
					&& (
						columnNameSuffix == "_SystemCreateTimeUtc"
						|| columnNameSuffix == "_SystemLastEditTimeUtc")
					)
				{
					populatedColumn.ColumnNullable = "NULL";
				}
				populatedTable.ColumnList.Add(populatedColumn);
			}
		}

		protected override void CreateTargetTableDataIfNeededCore(T transformation)
		{
			if (!hasAnotherInsteadOfDeleteTrigger)
			{
				var sourceTableSchema = transformation.SourceTableSchema;
				var targetTableSchema = transformation.TargetTableSchema;
				if (sourceTableSchema != targetTableSchema && !DbObjectCreator.TriggerExists(Db.Connection, sourceTableSchema.TableName, insteadOfDeleteTriggerName))
				{
					if (!string.IsNullOrEmpty(createTargetTablesScript))
					{
						Db.Connection.ExecuteNonQuery(createTargetTablesScript);
					}
					Db.Connection.ExecuteNonQuery(insteadOfDeleteTriggerCreationScript);
				}
			}
		}

		string GetCreateTableScript(T transformation)
		{
			var targetTableSchema = transformation.TargetTableSchema;
			var targetTableSchemaName = targetTableSchema.SqlSchemaName;
			var targetTableName = targetTableSchema.TableName;
			var targetTableFullName = $"{TargetDb.QuoteName()}.{targetTableSchemaName.QuoteName()}.{targetTableName.QuoteName()}";
			var targetForeignKeyName = transformation.TargetForeignKeyColumn.Name;
			var targetForeignKeyNameQuoted = targetForeignKeyName.QuoteName();
			var targetTablePK = targetTableSchema.PK;
			var targetTablePKName = targetTablePK.Name;
			var primaryKeyIndexName = $"PK_UX__{targetTablePKName}";
			var foreignKeyIndexName = $"FK_RX__{targetForeignKeyName}";
			var targetTablePKNameQuoted = targetTablePKName.QuoteName();
			var targetTableClusterKeyDefinition = string.Empty;
			var targetTableClusterKeyIndexDefinition = string.Empty;
			var targetTableClusterKeyName = GetTargetClusterKeyColumn(transformation)?.Name;
			if (targetTableClusterKeyName != null)
			{
				targetTableClusterKeyDefinition = $@"
	{targetTableClusterKeyName.QuoteName()} INT NOT NULL DEFAULT 0,";
				targetTableClusterKeyIndexDefinition = $@"
	INDEX [NR_UC__{targetTableClusterKeyName}] CLUSTERED ({targetTableClusterKeyName.QuoteName()} ASC),";
			}
			var createTableSql = $@"-- Create table
CREATE TABLE {targetTableFullName}
(
	{targetTablePKNameQuoted} UNIQUEIDENTIFIER NOT NULL,
	{targetForeignKeyNameQuoted} UNIQUEIDENTIFIER NULL,{targetTableClusterKeyDefinition}
	CONSTRAINT {primaryKeyIndexName.QuoteName()} PRIMARY KEY NONCLUSTERED ({targetTablePKNameQuoted} ASC),{targetTableClusterKeyIndexDefinition}
	INDEX {foreignKeyIndexName.QuoteName()} NONCLUSTERED ({targetForeignKeyNameQuoted} ASC)
);";
			return createTableSql;
		}

		(bool hasAnotherInsteadOfDeleteTrigger, string insteadOfDeleteTriggerName, string insteadOfDeleteTriggerCreationScript, string targetTablesCreationScript) GetInsteadOfDeleteTriggerData()
		{
			var transformationVersionBeforeUpgrade = Manager.TransformationVersionBeforeUpgrade;
			var transformationVersionBeforeUpgradeKey = transformationVersionBeforeUpgrade.ToString();
			var deleteTriggerDictionary = GetInsteadOfDeleteTriggerDictionary();
			var dictionary = deleteTriggerDictionary.GetOrAdd(transformationVersionBeforeUpgradeKey, (_) => GetInsteadOfDeleteTriggerDictionary(transformationVersionBeforeUpgrade));
			return dictionary[OfflineTransformation.SourceTableSchema];
		}

		Dictionary<ITableSchema, (bool hasAnotherInsteadOfDeleteTrigger, string insteadOfDeleteTriggerName, string insteadOfDeleteTriggerCreationScript, string targetTablesCreationScript)> GetInsteadOfDeleteTriggerDictionary(VersionLabel transformationVersionBeforeUpgrade)
		{
			var targetDbQuoted = TargetDb.QuoteName();
			(var hasAnotherInsteadOfDeleteTriggerDictionary, var sourceTableDictionary) = GatherMappedData(transformationVersionBeforeUpgrade, targetDbQuoted);
			return hasAnotherInsteadOfDeleteTriggerDictionary.ToDictionary(x => x.Key, y =>
			{
				var hasAnotherInsteadOfDeleteTrigger = y.Value;
				string insteadOfDeleteTriggerName = null;
				string insteadOfDeleteTriggerCreationScript = null;
				string targetTablesCreationScript = null;
				if (!hasAnotherInsteadOfDeleteTrigger)
				{
					var sourceTableSchema = y.Key;
					(insteadOfDeleteTriggerName, insteadOfDeleteTriggerCreationScript, targetTablesCreationScript) = GetInsteadOfDeleteTriggerCreationScript(sourceTableSchema, sourceTableDictionary[sourceTableSchema], targetDbQuoted);
				}
				return (hasAnotherInsteadOfDeleteTrigger, insteadOfDeleteTriggerName, insteadOfDeleteTriggerCreationScript, targetTablesCreationScript);
			});
		}

		(Dictionary<ITableSchema, bool> hasAnotherInsteadOfDeleteTriggerDictionary, Dictionary<ITableSchema, Dictionary<ITableSchema, (string createTargetTableScript, string deleteTargetRecordScript)>> sourceTableDictionary) GatherMappedData(VersionLabel transformationVersionBeforeUpgrade, string targetDbQuoted)
		{
			var hasAnotherInsteadOfDeleteTriggerDictionary = new Dictionary<ITableSchema, bool>();
			var sourceTableDictionary = new Dictionary<ITableSchema, Dictionary<ITableSchema, (string createTargetTableScript, string deleteTargetRecordScript)>>();
			var offlineTransformationType = typeof(CopyAddInfoToOtherTableRealColumn);
			foreach (var transformation in GetAllMappings()
				.Where(mapping =>
				{
					return offlineTransformationType.IsAssignableFrom(mapping.TransformationType)
					&& mapping.MappedVersion.CompareTo(transformationVersionBeforeUpgrade) > 0;
				}).Select(x => CreateTransformationInstance(x)))
			{
				var sourceTableSchema = transformation.SourceTableSchema;
				if (!hasAnotherInsteadOfDeleteTriggerDictionary.TryGetValue(sourceTableSchema, out var hasAnotherInsteadOfDeleteTrigger))
				{
					var sourceTableName = sourceTableSchema.TableName;
					hasAnotherInsteadOfDeleteTrigger = DbObjectCreator.GetInsteadOfDeleteTriggerName(Db.Connection, sourceTableSchema.SqlSchemaName, sourceTableName) is string existingDeleteTriggername
						&& existingDeleteTriggername.Length > 0
						&& existingDeleteTriggername != GetInsteadOfDeleteTriggerName(sourceTableName);
					hasAnotherInsteadOfDeleteTriggerDictionary.Add(sourceTableSchema, hasAnotherInsteadOfDeleteTrigger);
				}
				if (!hasAnotherInsteadOfDeleteTrigger)
				{
					if (!sourceTableDictionary.TryGetValue(sourceTableSchema, out var dictionary))
					{
						dictionary = new Dictionary<ITableSchema, (string createTargetTableScript, string deleteTargetRecordScript)>();
						sourceTableDictionary.Add(sourceTableSchema, dictionary);
					}
					var targetTableSchema = transformation.TargetTableSchema;
					if (!dictionary.ContainsKey(targetTableSchema))
					{
						var targetTableFullName = $"{targetDbQuoted}.{targetTableSchema.SqlSchemaName.QuoteName()}.{targetTableSchema.TableName.QuoteName()}";
						dictionary.Add(targetTableSchema, ($@"IF (OBJECT_ID(N'{targetTableFullName}', N'U') IS NULL)
BEGIN
{GetCreateTableScript(transformation)}
END;
", $"	DELETE {targetTableFullName} FROM {targetTableFullName} S INNER JOIN DELETED D ON S.{transformation.TargetForeignKeyColumn.Name.QuoteName()} = D.{sourceTableSchema.PK.Name.QuoteName()};"));
					}
				}
			}
			return (hasAnotherInsteadOfDeleteTriggerDictionary, sourceTableDictionary);
		}

		(string insteadOfDeleteTriggerName, string insteadOfDeleteTriggerCreationScript, string targetTablesCreationScript) GetInsteadOfDeleteTriggerCreationScript(ITableSchema sourceTableSchema, Dictionary<ITableSchema, (string createTargetTableScript, string deleteTargetRecordScript)> targetTableSchemaData, string targetDbQuoted)
		{
			var targetTablesCreationScript = new List<string>(targetTableSchemaData.Count);
			var deleteTargetRecordsScript = new List<string>(targetTableSchemaData.Count);
			targetTableSchemaData.ForEach(x =>
			{
				targetTablesCreationScript.Add(x.Value.createTargetTableScript);
				deleteTargetRecordsScript.Add(x.Value.deleteTargetRecordScript);
			});
			var sourceTableFullName = $"{targetDbQuoted}.{sourceTableSchema.SqlSchemaName.QuoteName()}.{sourceTableSchema.TableName.QuoteName()}";
			var triggerName = GetInsteadOfDeleteTriggerName(sourceTableSchema.TableName);
			var sourcePrimaryKeyNameQuoted = sourceTableSchema.PK.Name.QuoteName();
			var deleteTriggerSql = $@"
-------------------------------------------------------
-- ONLINE UPGRADE TRIGGER
-- Name: {triggerName}
-------------------------------------------------------
CREATE TRIGGER {triggerName.QuoteName()} ON {sourceTableFullName} INSTEAD OF DELETE AS
BEGIN
	IF (@@rowcount = 0) RETURN;
	SET NOCOUNT ON;
{string.Join("\r\n", deleteTargetRecordsScript)}
	DELETE {sourceTableFullName} FROM {sourceTableFullName} S INNER JOIN DELETED D ON S.{sourcePrimaryKeyNameQuoted} = D.{sourcePrimaryKeyNameQuoted};
END;";
			return (triggerName, deleteTriggerSql, string.Join("\r\n", targetTablesCreationScript));
		}

		protected virtual ConcurrentDictionary<string, Dictionary<ITableSchema, (bool hasAnotherInsteadOfDeleteTrigger, string insteadOfDeleteTriggerName, string insteadOfDeleteTriggerCreationScript, string targetTablesCreationScript)>> GetInsteadOfDeleteTriggerDictionary() => insteadOfDeleteTriggerDictionary ?? (insteadOfDeleteTriggerDictionary = new ConcurrentDictionary<string, Dictionary<ITableSchema, (bool hasAnotherInsteadOfDeleteTrigger, string insteadOfDeleteTriggerName, string insteadOfDeleteTriggerCreationScript, string targetTablesCreationScript)>>());
		[ThreadStatic]
		static ConcurrentDictionary<string, Dictionary<ITableSchema, (bool hasAnotherInsteadOfDeleteTrigger, string insteadOfDeleteTriggerName, string insteadOfDeleteTriggerCreationScript, string targetTablesCreationScript)>> insteadOfDeleteTriggerDictionary;

		protected override void EnsureCanProcessOnline()
		{
			var data = GetInsteadOfDeleteTriggerData();
			hasAnotherInsteadOfDeleteTrigger = data.hasAnotherInsteadOfDeleteTrigger;
			if (!hasAnotherInsteadOfDeleteTrigger)
			{
				insteadOfDeleteTriggerName = data.insteadOfDeleteTriggerName;
				insteadOfDeleteTriggerCreationScript = data.insteadOfDeleteTriggerCreationScript;
				createTargetTablesScript = data.targetTablesCreationScript;
				base.EnsureCanProcessOnline();
			}
		}
		bool hasAnotherInsteadOfDeleteTrigger;
		string insteadOfDeleteTriggerName;
		string insteadOfDeleteTriggerCreationScript;
		string createTargetTablesScript;

		T CreateTransformationInstance(Mapping mapping)
		{
			var result = (T)Activator.CreateInstance(mapping.TransformationType);
			// if you end up here because of a null reference exception there is a missing test. This code was not referenced and no concrete test hit this code path.
			result.Initialise(null, Manager);
			return result;
		}

		protected override bool MustProcessOffline()
		{
			return hasAnotherInsteadOfDeleteTrigger || base.MustProcessOffline();
		}

		protected override bool HasTargetTable(string tableName, string tableSchema) => DbObjectCreator.TableExists(Db.Connection, TargetDb, tableName, tableSchema: tableSchema);

		protected override string CreatePopulateTargetColumns(PopulatedTable table, string id_FromParameterName, string id_ToParameterName)
		{
			var transformation = OfflineTransformation;
			var targetTableFullName = table.TargetTableFullName;
			var sourceTablePKNameQuoted = transformation.SourceTablePK.Name.QuoteName();
			var sourceTableFullName = $"{TargetDb.QuoteName()}.{transformation.SourceTableSchemaName.QuoteName()}.{transformation.SourceTableName.QuoteName()}";
			var targetTableMainColumnMapping = transformation.TargetTableMainColumnMapping.ToDictionary(x => x.Key, y => y.Value.selectStatement);
			var targetTableColumnMapping = transformation.TargetTableColumnMapping;
			var mainColumnNames = new List<(string columnNameQuoted, string newColumnNameQuoted)>();
			var addInfoColumnNames = new List<(string columnNameQuoted, string newColumnNameQuoted)>();
			var targetForeignKeyColumnNameQuoted = transformation.TargetForeignKeyColumn.Name.QuoteName();
			var targetTableClusterKeyColumnName = transformation.GetClusterKeyColumn(transformation.TargetForeignKeyColumn)?.Name;
			var targetTableClusterKeyColumnNameQuoted = targetTableClusterKeyColumnName.QuoteName();
			var hasTargetTableClusterKeyColumn = !string.IsNullOrEmpty(targetTableClusterKeyColumnName);
			var sourceTableClusterKeyNameQuoted = transformation.GetClusterKeyColumn(transformation.SourceAddInfoColumn)?.Name?.QuoteName();
			var targetClusterkeyJoin = string.Empty;
			var sourceClusterkeyJoin = string.Empty;
			var preAddTableClusterKeyJoin = string.Empty;
			var isClusterKeyInColumnList = false;
			var whereClauseBuilder = new List<string>();
			foreach (var column in table.ColumnList)
			{
				var columnName = column.ColumnName;
				var columnNameQuoted = columnName.QuoteName();
				var newColumnName = "New" + columnName;
				var newColumnNameQuoted = newColumnName.QuoteName();
				if (targetTableColumnMapping.ContainsKey(columnName))
				{
					addInfoColumnNames.Add((columnNameQuoted, newColumnNameQuoted));
					whereClauseBuilder.Add($"OR ISNULL(NULLIF(tgt.{columnNameQuoted}, tmp.{newColumnNameQuoted}), NULLIF(tmp.{newColumnNameQuoted}, tgt.{columnNameQuoted})) IS NOT NULL");
				}
				else if (targetTableMainColumnMapping.ContainsKey(columnName))
				{
					targetTableMainColumnMapping.Remove(columnName);
					if (hasTargetTableClusterKeyColumn && !isClusterKeyInColumnList)
					{
						isClusterKeyInColumnList = columnName == targetTableClusterKeyColumnName;
					}
					mainColumnNames.Add((columnNameQuoted, newColumnNameQuoted));
				}
			}
			if (isClusterKeyInColumnList)
			{
				var newTargetTableClusterKeyColumnName = "New" + targetTableClusterKeyColumnName;
				var newTargetTableClusterKeyColumnNameQuoted = newTargetTableClusterKeyColumnName.QuoteName();
				preAddTableClusterKeyJoin = $" AND tmp.{newTargetTableClusterKeyColumnNameQuoted} = tgt.{targetTableClusterKeyColumnNameQuoted}";
				sourceClusterkeyJoin = $" AND tmp.{targetTableClusterKeyColumnNameQuoted} = src.{sourceTableClusterKeyNameQuoted}";
				targetClusterkeyJoin = $" AND tgt.{targetTableClusterKeyColumnNameQuoted} = src.{sourceTableClusterKeyNameQuoted}";
			}

			var selectSourceStatementBuilder = new List<string>();
			var sourceColumnNamesStatementBuilder = new List<string>();
			var updateStatementBuilder = new List<string>();
			sourceColumnNamesStatementBuilder.Add(targetForeignKeyColumnNameQuoted);
			targetTableMainColumnMapping.ForEach(m =>
			{
				selectSourceStatementBuilder.Add(m.Value);
				sourceColumnNamesStatementBuilder.Add(m.Key.QuoteName());
			});
			var selectStatementForUpdateBuilder = new List<string>();
			addInfoColumnNames.ForEach(m =>
			{
				(var columnNameQuoted, var newColumnNameQuoted) = m;
				selectSourceStatementBuilder.Add($"tmp.{columnNameQuoted}");
				sourceColumnNamesStatementBuilder.Add(columnNameQuoted);
				updateStatementBuilder.Add($"{columnNameQuoted} = tmp.{newColumnNameQuoted}");
				selectStatementForUpdateBuilder.Add($"tmp.{columnNameQuoted} AS {newColumnNameQuoted}");
			});
			mainColumnNames.ForEach(m =>
			{
				(var columnNameQuoted, var newColumnNameQuoted) = m;
				selectSourceStatementBuilder.Add($"tmp.{columnNameQuoted}");
				sourceColumnNamesStatementBuilder.Add(columnNameQuoted);
				selectStatementForUpdateBuilder.Add($"tmp.{columnNameQuoted} AS {newColumnNameQuoted}");
				if (targetTableClusterKeyColumnNameQuoted != columnNameQuoted)
				{
					updateStatementBuilder.Add($"{columnNameQuoted} = tmp.{newColumnNameQuoted}");
				}
			});
			var sourceColumnNamesStatement = string.Join(", ", sourceColumnNamesStatementBuilder);

			var sql = $@"-- Populating target table
SET NOCOUNT ON;
DECLARE @totalRowCount INT

UPDATE {targetTableFullName}
SET
	{string.Join(",\r\n\t", updateStatementBuilder)}
FROM {targetTableFullName} tgt
INNER JOIN {sourceTableFullName} AS src ON src.{sourceTablePKNameQuoted} = tgt.{targetForeignKeyColumnNameQuoted}{targetClusterkeyJoin}
INNER JOIN
(
	SELECT PK,
		{string.Join(",\r\n\t\t", selectStatementForUpdateBuilder)}
	FROm {table.PreAddTableFullName} AS tmp
	WHERE tmp.Id BETWEEN {id_FromParameterName} AND {id_ToParameterName}
) AS tmp ON tmp.PK = tgt.{targetForeignKeyColumnNameQuoted}{preAddTableClusterKeyJoin}
WHERE
	1=2
	{string.Join("\r\n\t", whereClauseBuilder)}
OPTION (MAXDOP 1);

SET @totalRowCount = @@ROWCOUNT;

INSERT {targetTableFullName} ({transformation.TargetTableSchema.PK.Name.QuoteName()}, {sourceColumnNamesStatement})
SELECT NEWID(), tmp.PK, {string.Join(", ", selectSourceStatementBuilder)}
FROM {sourceTableFullName} AS src
INNER JOIN {table.PreAddTableFullName} AS tmp ON tmp.PK = src.{sourceTablePKNameQuoted}{sourceClusterkeyJoin}
WHERE NOT EXISTS (SELECT NULL FROM {targetTableFullName} AS tgt WHERE tmp.PK = tgt.{targetForeignKeyColumnNameQuoted}{targetClusterkeyJoin})
AND tmp.Id BETWEEN {id_FromParameterName} AND {id_ToParameterName}
OPTION (MAXDOP 1);

SELECT @totalRowCount + @@ROWCOUNT;
";

			return sql;
		}
	}
}
