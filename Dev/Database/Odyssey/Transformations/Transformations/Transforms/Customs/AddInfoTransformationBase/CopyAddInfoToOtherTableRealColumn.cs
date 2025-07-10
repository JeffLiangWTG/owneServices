using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase
{
	public abstract class CopyAddInfoToOtherTableRealColumn : CopyAddInfoToRealColumn
	{
		protected override void CopyData(SchemaIntColumn sourceTableClusterKeyColumn)
		{
			var sourceTableFullName = $"{SourceTableSchemaName.QuoteName()}.{SourceTableName.QuoteName()}";

			var targetTableSchema = TargetTableSchema;
			var targetTableName = targetTableSchema.TableName;
			var targetTablePK = targetTableSchema.PK;
			var targetTableForeignKeyName = TargetForeignKeyColumn.Name.QuoteName();
			var targetTableFullName = $"{targetTableSchema.SqlSchemaName.QuoteName()}.{targetTableName.QuoteName()}";

			var reprocessingTableFullName = $"{Db.SqlDbOwnerSchema.QuoteName()}.{OffLineProcessingTableName.QuoteName()}";

			var addInfoCrossApplyBuilder = new List<string>();
			var sourceTablePKName = SourceTablePK.Name;
			var targetColumnBuilder = new List<string>();
			var sourceValueBuilder = new List<string>();
			targetColumnBuilder.Add(targetTableForeignKeyName);
			sourceValueBuilder.Add($"SourceTable.{sourceTablePKName}");
			var updateValueBuilder = new List<string>();
			var cteSelectValueBuilder = new List<string>();
			TargetTableMainColumnMapping.ForEach(x =>
			{
				var columnNameQuoted = x.Key.QuoteName();
				targetColumnBuilder.Add(columnNameQuoted);
				sourceValueBuilder.Add(x.Value.selectStatement);
				var keyInLower = x.Key.ToLower();
				if (keyInLower.EndsWith("_systemlastedituser") ||
					keyInLower.EndsWith("_systemlastedittimeutc"))
				{
					updateValueBuilder.Add($"{columnNameQuoted} = {columnNameQuoted}");
					cteSelectValueBuilder.Add($"{columnNameQuoted}");
				}
			});
			var whereClauseBuilder = new List<string>();
			foreach (var columnMapping in TargetTableColumnMapping)
			{
				var columnName = columnMapping.Key;
				var columnNameQuoted = columnName.QuoteName();
				addInfoCrossApplyBuilder.Add(columnMapping.Value.JoinStatement);
				targetColumnBuilder.Add(columnNameQuoted);
				sourceValueBuilder.Add(columnMapping.Value.SelectStatement);
				updateValueBuilder.Add($"{columnNameQuoted} = [New{columnName}]");
				cteSelectValueBuilder.Add($"{columnNameQuoted}, {columnMapping.Value.SelectStatement} AS [New{columnName}]");
				whereClauseBuilder.Add($"OR ISNULL(NULLIF({columnNameQuoted}, [New{columnName}]), NULLIF([New{columnName}], {columnNameQuoted})) IS NOT NULL");
			}
			var targetClusterKeyColumn = GetClusterKeyColumn(TargetForeignKeyColumn);
			var clusterKeyJoin = string.Empty;
			var targetClusterKeyJoin = string.Empty;
			if (sourceTableClusterKeyColumn != null)
			{
				clusterKeyJoin = $" AND SourceTable.{sourceTableClusterKeyColumn.Name.QuoteName()} = ReprocessingTable.ClusterKey";
				if (targetClusterKeyColumn != null)
				{
					targetClusterKeyJoin = $" AND tgt.{targetClusterKeyColumn.Name} = SourceTable.{sourceTableClusterKeyColumn.Name}";
				}
			}

			var joinClause = $"tgt.{targetTableForeignKeyName} = SourceTable.{sourceTablePKName}{targetClusterKeyJoin}";
			var sourceScript = $@"FROM {sourceTableFullName} SourceTable {AdditionalSourceTableJoin}
INNER JOIN {reprocessingTableFullName} ReprocessingTable ON SourceTable.{sourceTablePKName} = ReprocessingTable.PK{clusterKeyJoin}
{string.Join("\r\n", addInfoCrossApplyBuilder)}";

			var sqlMergeText = $@"
WITH SourceDataCTE AS
(
	SELECT {string.Join(", ", cteSelectValueBuilder)}
	{sourceScript}
	INNER JOIN {targetTableFullName} tgt ON {joinClause}
)

UPDATE SourceDataCTE
SET {string.Join(", ", updateValueBuilder)}
WHERE
	1=2
	{string.Join("\r\n\t", whereClauseBuilder)};

INSERT {targetTableFullName} ({targetTablePK.Name}, {string.Join(", ", targetColumnBuilder)})
SELECT NEWID(), {string.Join(", ", sourceValueBuilder)}
{sourceScript}
WHERE NOT EXISTS (SELECT NULL FROM {targetTableFullName} AS tgt WHERE {joinClause});";
			Db.Connection.ExecuteNonQuery(sqlMergeText);
		}

		public ITableSchema TargetTableSchema => TargetForeignKeyColumn.TableSchema;
		public abstract SchemaGuidColumn TargetForeignKeyColumn { get; }

		public IDictionary<string, (SchemaColumn targetColumn, string selectStatement)> TargetTableMainColumnMapping
		{
			get
			{
				var mainColumnMapping = new Dictionary<string, (SchemaColumn targetColumn, string selectStatement)>();
				var sourceTableSchema = SourceTableSchema;
				var targetTableSchema = TargetTableSchema;
				if (targetTableSchema != sourceTableSchema)
				{
					var sourceTablePrefix = sourceTableSchema.PK.ColumnPrefix;
					var sourceTableClusterKeyColumn = GetClusterKeyColumn(SourceAddInfoColumn);
					var sourceTableSystemCreateUserColumn      = sourceTableSchema.GetSchemaColumn($"{sourceTablePrefix}_SystemCreateUser")      as SchemaStringColumn;
					var sourceTableSystemCreateTimeUtcColumn   = sourceTableSchema.GetSchemaColumn($"{sourceTablePrefix}_SystemCreateTimeUtc")   as SchemaDateTimeColumn;
					var sourceTableSystemLastEditUserColumn    = sourceTableSchema.GetSchemaColumn($"{sourceTablePrefix}_SystemLastEditUser")    as SchemaStringColumn;
					var sourceTableSystemLastEditTimeUtcColumn = sourceTableSchema.GetSchemaColumn($"{sourceTablePrefix}_SystemLastEditTimeUtc") as SchemaDateTimeColumn;

					var targetTablePrefix = targetTableSchema.PK.ColumnPrefix;
					var targetTableClusterKeyColumn            = targetTableSchema.GetSchemaColumn($"{targetTablePrefix}_ClusterKey")            as SchemaIntColumn;
					var targetTableSystemCreateUserColumn      = targetTableSchema.GetSchemaColumn($"{targetTablePrefix}_SystemCreateUser")      as SchemaStringColumn;
					var targetTableSystemCreateTimeUtcColumn   = targetTableSchema.GetSchemaColumn($"{targetTablePrefix}_SystemCreateTimeUtc")   as SchemaDateTimeColumn;
					var targetTableSystemLastEditUserColumn    = targetTableSchema.GetSchemaColumn($"{targetTablePrefix}_SystemLastEditUser")    as SchemaStringColumn;
					var targetTableSystemLastEditTimeUtcColumn = targetTableSchema.GetSchemaColumn($"{targetTablePrefix}_SystemLastEditTimeUtc") as SchemaDateTimeColumn;

					var defaultUserValue = DefaultUserValue;
					AddIfExistsInBothSourceAndTarget(sourceTableSystemCreateUserColumn, targetTableSystemCreateUserColumn, mainColumnMapping, (x) => x == null ? $"'{defaultUserValue}'" : $"IIF({x.Name} = '', '{defaultUserValue}', {x.Name})");
					AddIfExistsInBothSourceAndTarget(sourceTableSystemCreateTimeUtcColumn, targetTableSystemCreateTimeUtcColumn, mainColumnMapping, (x) => x == null ? "GETUTCDATE()" : $"COALESCE({x.Name}, GETUTCDATE())");
					AddIfExistsInBothSourceAndTarget(sourceTableSystemLastEditUserColumn, targetTableSystemLastEditUserColumn, mainColumnMapping, (x) => x == null ? $"'{defaultUserValue}'" : $"IIF({x.Name} = '', '{defaultUserValue}', {x.Name})");
					AddIfExistsInBothSourceAndTarget(sourceTableSystemLastEditTimeUtcColumn, targetTableSystemLastEditTimeUtcColumn, mainColumnMapping, (x) => x == null ? "GETUTCDATE()" : $"COALESCE({x.Name}, GETUTCDATE())");
					if (targetTableClusterKeyColumn != null && sourceTableClusterKeyColumn != null)
					{
						mainColumnMapping.Add(targetTableClusterKeyColumn.Name, (targetTableClusterKeyColumn, sourceTableClusterKeyColumn.Name));
					}
				}
				return mainColumnMapping;
			}
		}

		void AddIfExistsInBothSourceAndTarget<T>(T sourceTableColumn, T targetTableColumn, Dictionary<string, (SchemaColumn targetColumn, string selectStatement)> dictionary, Func<T, string> getSourceSelect)
			where T : SchemaColumn
		{
			if (targetTableColumn != null)
			{
				var targetColumnName = targetTableColumn.Name;
				dictionary.Add(targetColumnName, (targetTableColumn, getSourceSelect(sourceTableColumn)));
			}
		}
	}
}
