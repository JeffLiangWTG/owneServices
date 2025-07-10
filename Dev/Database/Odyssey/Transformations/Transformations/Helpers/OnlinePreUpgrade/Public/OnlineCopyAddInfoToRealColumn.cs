using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	public abstract class OnlineCopyAddInfoToRealColumn<T> : TablePreSynchroniser
		where T : Transformation.DataModification.AddInfoTransformationBase.CopyAddInfoToRealColumn
	{
		protected OnlineCopyAddInfoToRealColumn(IUpgradeManager manager, string dbBeingUpgraded, string templateDb)
			: base(manager, dbBeingUpgraded, templateDb)
		{
		}

		public void Run()
		{
			try
			{
				if (OfflineTransformation.TargetTableColumnMapping.Count > 0)
				{
					if (!ShouldRun())
					{
						return;
					}

					RunTransformation();
				}
			}
			finally
			{
				hasTargetTable = false;
				hasTargetColumnWithDifferentDefinition = false;
			}
		}

		bool ShouldRun()
		{
			var result = false;
			if (OfflineTransformation.TargetTableColumnMapping.Count > 0)
			{
				var twinOfflineTransformation = OfflineTransformation.GetType();
				var transformationVersion = GetAllMappings()
					.SingleOrDefault(mapping => mapping.TransformationType == twinOfflineTransformation)
					?.MappedVersion;
				result = ((transformationVersion?.CompareTo(Manager.TransformationVersionBeforeUpgrade) ?? 0) > 0) && EnsureColumnExistsInTemplateAndCheckMatchingWithTarget();
			}
			return result;
		}

		protected virtual IEnumerable<Mapping> GetAllMappings() => Mapper.GetAllMappings();
		protected abstract string UserDescription { get; }
		protected string StatusName => statusName ?? (statusName = $"{StatusNameCore}Status");
		string statusName;
		protected virtual string StatusNameCore => OfflineTransformation.GetType().Name;
		protected virtual bool MustProcessOffline() => hasTargetColumnWithDifferentDefinition;

		void RunTransformation()
		{
			var transformation = OfflineTransformation;
			var sourceTableClusterKeyColumn = transformation.GetClusterKeyColumn(transformation.SourceAddInfoColumn);
			SetupTablesAndTriggersIfNeeded(sourceTableClusterKeyColumn);
			if (MustProcessOffline())
			{
				FlaggedAllMatchedApplicableRecordsToBeProcessedOffline(sourceTableClusterKeyColumn);
				return;
			}

			var tableSchema = GetTargetTableSchema(transformation);
			var columnsToPopulate = GetColumnsToPopulateDataTable(tableSchema.SqlSchemaName, tableSchema.TableName, GetTargetColumnNames(transformation));
			var populatedTables = GetColumnsToPopulate(columnsToPopulate, CreatePopulatedColumns);

			DoPopulate(
				populatedTables,
				populatedTables.Count,
				populatedTables.Sum(x => x.TargetTableSize),
				UserDescription,
				"(+)",
				StatusName,
				OfflineTransformation.Identifier);
		}

		protected virtual ITableSchema GetTargetTableSchema(T transformation) => transformation.TargetTableColumnMapping.First().Value.AddInfoColumnMapping.TableSchema;
		protected virtual SchemaIntColumn GetTargetClusterKeyColumn(T transformation) => transformation.GetClusterKeyColumn(transformation.SourceAddInfoColumn);

		protected virtual IEnumerable<string> GetTargetColumnNames(T transformation)
		{
			if (GetTargetClusterKeyColumn(transformation) is SchemaIntColumn sourceTableClusterKeyColumn)
			{
				yield return sourceTableClusterKeyColumn.Name;
			}
			foreach (var pair in transformation.TargetTableColumnMapping)
			{
				yield return pair.Value.AddInfoColumnMapping.Name;
			}
		}

		void CreatePopulatedColumns(IGrouping<string, ColumnChangeMetadata> table, PopulatedTable populatedTable)
		{
			var transformation = OfflineTransformation;
			var targetTableColumnMapping = transformation.TargetTableColumnMapping;
			var addInfoColumnList = new List<PopulatedColumn>();
			foreach (var col in table.OrderBy(c => c.GetColumnId()))
			{
				var populatedColumn = new PopulatedColumn();
				populatedColumn.ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture);
				populatedColumn.ColumnName = col.ColumnName;
				populatedColumn.ColumnType = col.GetFullTypeDeclaration();
				populatedColumn.ColumnNullable = (col.IsNullable) ? "NULL" : "NOT NULL";
				populatedColumn.IsSparse = col.IsSparse;

				if (!populatedColumn.IsSparse.Value)
				{
					var defaultClause = col.GetDefaultClause();
					if (defaultClause != null)
					{
						populatedColumn.ColumnDefaultConstraint = FormattableString.Invariant($" CONSTRAINT {col.CalculatedDefaultConstraintName.QuoteName()} DEFAULT {defaultClause}");
					}
				}

				if (!populatedTable.TargetTableStatus.HasValue)
				{
					populatedTable.TargetTableStatus = (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), true);
				}

				if (targetTableColumnMapping.TryGetValue(col.ColumnName, out var targetTableColumnData))
				{
					populatedColumn.PopulateSource = targetTableColumnData.JoinStatement;
					populatedColumn.PopulateExpression = targetTableColumnData.SelectStatement;
					populatedColumn.PopulatePreAddWhereClause = $"OR {targetTableColumnData.WhereClauseStatement}";
					addInfoColumnList.Add(populatedColumn);
				}
				else
				{
					PopulateNonMappedColumnIfNeeded(transformation, populatedTable, col, populatedColumn);
				}
			}
			populatedTable.ColumnList.AddRange(addInfoColumnList);
		}

		protected virtual void PopulateNonMappedColumnIfNeeded(T transformation, PopulatedTable populatedTable, ColumnChangeMetadata col, PopulatedColumn populatedColumn)
		{
			if (GetTargetClusterKeyColumn(transformation) is SchemaIntColumn sourceTableClusterKeyColumn && col.ColumnName == sourceTableClusterKeyColumn.Name)
			{
				populatedColumn.PopulateSource = "LEFT JOIN (SELECT 1 _DUMMY) AS DUMMY ON 1=2";
				populatedColumn.PopulateExpression = col.ColumnName;
				populatedTable.ColumnList.Add(populatedColumn);
			}
		}

		public T OfflineTransformation => offlineTransformation ??= Activator.CreateInstance<T>();
		T offlineTransformation;

		void SetupTablesAndTriggersIfNeeded(SchemaIntColumn sourceTableClusterKeyColumn)
		{
			var transformation = OfflineTransformation;
			var schemaName = transformation.SourceTableSchemaName;
			var tableName = transformation.SourceTableName;
			var offLineProcessingTableName = transformation.OffLineProcessingTableName;
			if (DbObjectCreator.TableExists(Db.Connection, transformation.OffLineProcessingTableName))
			{
				if (sourceTableClusterKeyColumn != null)
				{
					SetupClusterKeyTriggerAndFixOfflineProcessingTableIfNeeded(schemaName, tableName, transformation.SourceTablePK.Name, sourceTableClusterKeyColumn.Name, offLineProcessingTableName);
				}
			}
			else
			{
				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				{
					CreateTargetTableDataIfNeeded(transformation);
					SetupOffLineProcessingTableAndTrigger(transformation, sourceTableClusterKeyColumn, schemaName, tableName);
					FlagColumnStatusIfNeeded(transformation);
					transactionManager.CommitTransaction();
				}
			}
		}

		void SetupClusterKeyTriggerAndFixOfflineProcessingTableIfNeeded(string sourceSchemaName, string sourceTableName, string sourceTablePrimaryKeyName, string sourceTableClusterKeyColumnName, string offLineProcessingTableName)
		{
			var triggerName = OfflineTransformation.GetClusterKeyTriggerName(sourceTableClusterKeyColumnName);
			if (!DbObjectCreator.TriggerExists(Db.Connection, sourceTableName, triggerName))
			{
				var sourceTableFullName = FormattableString.Invariant($"{TargetDb.QuoteName()}.{sourceSchemaName.QuoteName()}.{sourceTableName.QuoteName()}");
				var offLineProcessingTableFullName = FormattableString.Invariant($"{TargetDb.QuoteName()}.[dbo].{offLineProcessingTableName.QuoteName()}");
				var sourceTablePrimaryKeyNameQuoted = sourceTablePrimaryKeyName.QuoteName();
				var sourceTableClusterKeyColumnNameQuoted = sourceTableClusterKeyColumnName.QuoteName();
				SetupClusterKeyTrigger(sourceTableFullName, sourceTablePrimaryKeyNameQuoted, sourceTableClusterKeyColumnNameQuoted, offLineProcessingTableName, triggerName);
				var updateOffLineProcessingTableNameSql = FormattableString.Invariant($@"
UPDATE {offLineProcessingTableFullName}
SET ClusterKey = source.{sourceTableClusterKeyColumnNameQuoted}
FROM {offLineProcessingTableFullName} rt
INNER JOIN {sourceTableFullName} source ON rt.PK = source.{sourceTablePrimaryKeyNameQuoted} AND rt.ClusterKey <> source.{sourceTableClusterKeyColumnNameQuoted};");
				Db.Connection.ExecuteNonQuery(updateOffLineProcessingTableNameSql);
			}
		}

		void SetupClusterKeyTrigger(string sourceTableFullName, string sourceTablePrimaryKeyNameQuoted, string sourceTableClusterKeyColumnNameQuoted, string offLineProcessingTableFullName, string triggerName)
		{
			var triggerNameQuoted = triggerName.QuoteName();
			var updateTriggerSql = FormattableString.Invariant($@"
-------------------------------------------------------
-- ONLINE UPGRADE TRIGGER
-- Name: {triggerNameQuoted}
-------------------------------------------------------
CREATE TRIGGER {triggerNameQuoted} ON {sourceTableFullName} FOR UPDATE AS
BEGIN
	IF (@@rowcount = 0) RETURN;
	SET NOCOUNT ON;

	IF update({sourceTableClusterKeyColumnNameQuoted})
	BEGIN
		UPDATE {offLineProcessingTableFullName}
		SET ClusterKey = inserted.{sourceTableClusterKeyColumnNameQuoted}
		FROM {offLineProcessingTableFullName} rt
		INNER JOIN inserted ON rt.PK = inserted.{sourceTablePrimaryKeyNameQuoted}

		IF (@@error	!= 0) ROLLBACK TRANSACTION;
	END;
END;");
			Db.Connection.ExecuteNonQuery(updateTriggerSql);
		}

		void FlagColumnStatusIfNeeded(T transformation)
		{
			if (!MustProcessOffline())
			{
				var statusName = StatusName;
				var targetClusterKeyColumn = GetTargetClusterKeyColumn(transformation);

				foreach (var column in transformation.TargetTableColumnMapping.Select(x => x.Value.AddInfoColumnMapping).Union(targetClusterKeyColumn == null ? Enumerable.Empty<SchemaColumn>() : new SchemaColumn[] { targetClusterKeyColumn }))
				{
					var targetTableSchema = column.TableSchema;
					var targetTableSchemaName = targetTableSchema.SqlSchemaName;
					var targetTableName = targetTableSchema.TableName;
					if (ExtProperty.Column.Select(Db.Connection, targetTableSchemaName, targetTableName, column.Name, statusName) == null)
					{
						ExtProperty.Column.Update(Db.Connection, targetTableSchemaName, targetTableName, column.Name, statusName, nameof(TargetTableStatus.None));
					}
				}
			}
		}

		void CreateTargetTableDataIfNeeded(T transformation)
		{
			if (!MustProcessOffline())
			{
				CreateTargetTableDataIfNeededCore(transformation);
			}
		}

		protected virtual void CreateTargetTableDataIfNeededCore(T transformation) { }

		void SetupOffLineProcessingTableAndTrigger(T transformation, SchemaIntColumn sourceTableClusterKeyColumn, string schemaName, string tableName)
		{
			var offLineProcessingTableName = transformation.OffLineProcessingTableName;
			Manager.ShowInfoMessage(FormattableString.Invariant($"Creating OffLine Processing Table {offLineProcessingTableName} and trigger"));
			var offLineProcessingTableFullName = transformation.CreateOffLineProcessingTable(Db.Connection, TargetDb, sourceTableClusterKeyColumn != null);
			var sourceTablePrimaryKeyNameQuoted = transformation.SourceTablePK.Name.QuoteName();
			var sourceTableFullName = FormattableString.Invariant($"{TargetDb.QuoteName()}.{transformation.SourceTableSchemaName.QuoteName()}.{transformation.SourceTableName.QuoteName()}");
			var clusterKeyColumn = string.Empty;
			var sourceClusterKeyData = string.Empty;
			var clusterKeyCheck = string.Empty;
			if (sourceTableClusterKeyColumn != null)
			{
				clusterKeyColumn = ", ClusterKey";
				var sourceTableClusterKeyColumnNameQuoted = sourceTableClusterKeyColumn.Name.QuoteName();
				sourceClusterKeyData = $", inserted.{sourceTableClusterKeyColumnNameQuoted}";
				clusterKeyCheck = $" AND rt.ClusterKey = inserted.{sourceTableClusterKeyColumnNameQuoted}";
				SetupClusterKeyTrigger(sourceTableFullName, sourceTablePrimaryKeyNameQuoted, sourceTableClusterKeyColumnNameQuoted, offLineProcessingTableFullName, transformation.GetClusterKeyTriggerName(sourceTableClusterKeyColumn.Name));
			}
			var triggerName = transformation.GetTriggerName().QuoteName();
			var updateTriggerSql = FormattableString.Invariant($@"
-------------------------------------------------------
-- ONLINE UPGRADE TRIGGER
-- Name: {triggerName}
-------------------------------------------------------
CREATE TRIGGER {triggerName} ON {sourceTableFullName} FOR INSERT, UPDATE AS
BEGIN
	IF (@@rowcount = 0) RETURN;
	SET NOCOUNT ON;

	IF update({transformation.SourceAddInfoColumn.Name})
	BEGIN
		INSERT {offLineProcessingTableFullName} (PK{clusterKeyColumn})
		SELECT inserted.{sourceTablePrimaryKeyNameQuoted}{sourceClusterKeyData}
		FROM inserted {transformation.AdditionalSourceTableJoin}
		WHERE NOT EXISTS (SELECT NULL FROM {offLineProcessingTableFullName} rt WHERE rt.PK = inserted.{sourceTablePrimaryKeyNameQuoted}{clusterKeyCheck})

		IF (@@error	!= 0) ROLLBACK TRANSACTION;
	END;
END;");
			Db.Connection.ExecuteNonQuery(updateTriggerSql);
		}

		void FlaggedAllMatchedApplicableRecordsToBeProcessedOffline(SchemaIntColumn sourceTableClusterKeyColumn)
		{
			var transformation = OfflineTransformation;
			var clusterKeyColumn = string.Empty;
			var sourceClusterKeyData = string.Empty;
			var clusterKeyCheck = string.Empty;
			if (sourceTableClusterKeyColumn != null)
			{
				clusterKeyColumn = ", ClusterKey";
				var sourceTableClusterKeyColumnNameQuoted = sourceTableClusterKeyColumn.Name.QuoteName();
				sourceClusterKeyData = $", {sourceTableClusterKeyColumnNameQuoted}";
				clusterKeyCheck = $" AND rt.ClusterKey = {sourceTableClusterKeyColumnNameQuoted}";
			}
			var sourceTablePrimaryKeyNameQuoted = transformation.SourceTablePK.Name.QuoteName();
			var offLineProcessingTableFullName = transformation.CreateOffLineProcessingTable(Db.Connection, TargetDb, sourceTableClusterKeyColumn != null);
			var sqlText = $@"
INSERT {offLineProcessingTableFullName} (PK{clusterKeyColumn})
SELECT {sourceTablePrimaryKeyNameQuoted}{sourceClusterKeyData}
FROM {TargetDb.QuoteName()}.{transformation.SourceTableSchemaName.QuoteName()}.{transformation.SourceTableName.QuoteName()} {transformation.AdditionalSourceTableJoin}
WHERE NOT EXISTS (SELECT NULL FROM {offLineProcessingTableFullName} rt WHERE rt.PK = {sourceTablePrimaryKeyNameQuoted}{clusterKeyCheck});";
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		protected virtual void EnsureCanProcessOnline()
		{
			EnsureThatAddInfoCanBeMappedToARealColumn();
		}

		void EnsureThatAddInfoCanBeMappedToARealColumn()
		{
			var transformation = OfflineTransformation;
			var tableSchema = GetTargetTableSchema(transformation);
			var sqlSchemaName = tableSchema.SqlSchemaName;
			var tableName = tableSchema.TableName;
			hasTargetTable = HasTargetTable(tableName, tableSchema: sqlSchemaName);
			var columnNames = transformation.TargetTableColumnMapping.Select(x => x.Value.AddInfoColumnMapping.Name).ToArray();
			var templateColumnsDefinition = GetExistingColumnsDefinition(TemplateDB, sqlSchemaName, tableName, columnNames);
			var targetColumnsDefinition = hasTargetTable ? GetExistingColumnsDefinition(TargetDb, sqlSchemaName, tableName, columnNames) : null;
			foreach (var columnName in columnNames)
			{
				if (!templateColumnsDefinition.TryGetValue(columnName, out var templateColumnDefinition))
				{
					throw new InvalidSchemaColumnException(columnName, tableName);
				}
				if (!hasTargetColumnWithDifferentDefinition && hasTargetTable && targetColumnsDefinition.TryGetValue(columnName, out var targetColumnDefinition))
				{
					hasTargetColumnWithDifferentDefinition = templateColumnDefinition.ColType != targetColumnDefinition.ColType
						|| templateColumnDefinition.ColLength != targetColumnDefinition.ColLength
						|| templateColumnDefinition.ColPrecision != targetColumnDefinition.ColPrecision
						|| templateColumnDefinition.ColScale != targetColumnDefinition.ColScale
						|| templateColumnDefinition.ColSparse != targetColumnDefinition.ColSparse;
				}
			}
		}

		protected virtual bool HasTargetTable(string tableName, string tableSchema) => true;
		protected bool hasTargetTable { get; private set; }
		bool hasTargetColumnWithDifferentDefinition;

		protected sealed override void PopulatePreAddColumns(PopulatedTable table)
		{
			var transformation = OfflineTransformation;
			var sourceTableClusterKeyColumn = transformation.GetClusterKeyColumn(transformation.SourceAddInfoColumn);
			var sourceTablePKNameQuoted = transformation.SourceTablePK.Name.QuoteName();
			var columnNames = JoinData(", {0}", ", ", table.ColumnList.Where(c => !string.IsNullOrWhiteSpace(c.PopulateSource)).Select(c => c.ColumnName.QuoteName()));
			var selectColumns = JoinData(",\r\n\t{0}", ",\r\n\t", table.ColumnList.Where(c => !string.IsNullOrWhiteSpace(c.PopulateSource)).Select(c => FormattableString.Invariant($"{c.ColumnName.QuoteName()} = {c.PopulateExpression}")));
			var populateSources = JoinData("\r\n\t{0}", "\r\n\t", table.ColumnList.Select(c => c.PopulateSource).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct());
			var whereClause = JoinData("{0}", "\r\n\t", table.ColumnList.Select(c => c.PopulatePreAddWhereClause).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct());
			var populateMainSource = $"{TargetDb.QuoteName()}.{transformation.SourceTableSchemaName.QuoteName()}.{transformation.SourceTableName.QuoteName()} AS target {transformation.AdditionalSourceTableJoin}";
			var orderByClause = string.Empty;
			if (sourceTableClusterKeyColumn != null)
			{
				orderByClause = $@"ORDER BY {sourceTableClusterKeyColumn.Name.QuoteName()}
";
			}
			var sql = FormattableString.Invariant($@"-- Populating pre-add table
SET NOCOUNT ON;

INSERT {table.PreAddTableFullName} WITH (TABLOCKX) (PK{columnNames})
SELECT
	PK = target.{sourceTablePKNameQuoted}{selectColumns}
FROM
	{populateMainSource}{populateSources}
WHERE 1=2
	{whereClause}
{orderByClause}OPTION (RECOMPILE);
");

			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override string CreatePopulateTargetColumns(PopulatedTable table, string id_FromParameterName, string id_ToParameterName)
		{
			var transformation = OfflineTransformation;
			var targetTableFullName = table.TargetTableFullName;
			var sourceTablePKNameQuoted = transformation.SourceTablePK.Name.QuoteName();
			var targetTableColumnMapping = transformation.TargetTableColumnMapping;
			var addInfoColumnNames = new List<string>();
			var clusterKeyJoin = string.Empty;
			var selectStatementBuilder = new List<string>();
			if (GetTargetClusterKeyColumn(transformation) is SchemaIntColumn targetClusterKeyColumn)
			{
				clusterKeyJoin = $" AND tmp.ClusterKey = target.{targetClusterKeyColumn.Name.QuoteName()}";
				selectStatementBuilder.Add($"tmp.{targetClusterKeyColumn.Name.QuoteName()} AS ClusterKey");
			}
			var updateStatementBuilder = new List<string>();
			var whereClauseBuilder = new List<string>();
			foreach (var column in table.ColumnList)
			{
				var columnName = column.ColumnName;
				if (targetTableColumnMapping.ContainsKey(columnName))
				{
					var columnNameQuoted = columnName.QuoteName();
					var newColumnName = "New" + columnName;
					var newColumnNameQuoted = newColumnName.QuoteName();
					selectStatementBuilder.Add($"tmp.{columnNameQuoted} AS {newColumnNameQuoted}");
					updateStatementBuilder.Add($"{columnNameQuoted} = tmp.{newColumnNameQuoted}");
					whereClauseBuilder.Add($"OR ISNULL(NULLIF(target.{columnNameQuoted}, tmp.{newColumnNameQuoted}), NULLIF(tmp.{newColumnNameQuoted}, target.{columnNameQuoted})) IS NOT NULL");
				}
			}

			updateStatementBuilder.AddRange(GetSystemLastEditColumns(table, updateStatementBuilder));

			var sql = $@"-- Populating target table
SET NOCOUNT ON;

UPDATE {targetTableFullName}
SET
	{string.Join(",\r\n\t", updateStatementBuilder)}
FROM {targetTableFullName} target
INNER JOIN
(
	SELECT PK,
		{string.Join(",\r\n\t\t", selectStatementBuilder)}
	FROM {table.PreAddTableFullName} AS tmp
	WHERE tmp.Id BETWEEN {id_FromParameterName} AND {id_ToParameterName}
) AS tmp ON tmp.PK = target.{sourceTablePKNameQuoted}{clusterKeyJoin}
WHERE
	1=2
	{string.Join("\r\n\t\t", whereClauseBuilder)}

OPTION (MAXDOP 1);

SELECT @@ROWCOUNT;
";

			return sql;
		}

		string JoinData(string formatWhenNotEmpty, string separator, IEnumerable<string> values)
		{
			var result = string.Join(separator, values);
			if (result.Length > 0)
			{
				result = string.Format(CultureInfo.InvariantCulture, formatWhenNotEmpty, result);
			}
			return result;
		}

		protected bool EnsureColumnExistsInTemplateAndCheckMatchingWithTarget()
		{
			var result = false;
			var sourceTableName = OfflineTransformation.SourceTableName;
			var sourceTableSchemaName = OfflineTransformation.SourceTableSchemaName;
			if (DbObjectCreator.ColumnExists(Db.Connection, TargetDb, sourceTableSchemaName, sourceTableName, OfflineTransformation.SourceAddInfoColumn.Name))
			{
				EnsureCanProcessOnline();
				result = true;
			}
			return result;
		}

		DataTable GetColumnsToPopulateDataTable(string sqlSchemaName, string tableName, IEnumerable<string> columnNames)
		{
			var sql = FormattableString.Invariant($@"
-- Get list of the columns to populate
SELECT
	TabSchema        = NewSchema.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewType.name,
	ColLength        = NewCol.max_length,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = CASE NewCol.is_nullable WHEN 1 THEN 'NULL' ELSE 'NOT NULL' END,
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDefault.definition,
	ColExtProp       = ISNULL(CurColStatus.SEP_Value, 'None'),
	TabExtProp       = ISNULL(CurTabWatermark.SEP_Value, '0')
FROM
	[{TemplateDB}].sys.schemas                       AS NewSchema
	JOIN [{TemplateDB}].sys.tables                   AS NewTab     ON NewTab.schema_id = NewSchema.schema_id
	JOIN [{TemplateDB}].sys.columns                  AS NewCol     ON NewCol.object_id = NewTab.object_id
	JOIN [{TemplateDB}].sys.types                    AS NewType    ON NewType.user_type_id = NewCol.user_type_id
	LEFT JOIN [{TemplateDB}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	JOIN [{TargetDb}].sys.schemas      AS CurSchema ON CurSchema.name = NewSchema.name
	JOIN [{TargetDb}].sys.tables       AS CurTab    ON CurTab.schema_id = CurSchema.schema_id AND CurTab.name = NewTab.name
	LEFT JOIN [{TargetDb}].sys.columns AS CurCol    ON CurCol.object_id = CurTab.object_id AND CurCol.name = NewCol.name

	LEFT JOIN [{TargetDb}].dbo.StmExtendedProperty AS CurColStatus ON 1=1
		AND CurColStatus.SEP_Class = 'Column'
		AND CurColStatus.SEP_DatabaseNameSuffix = ''
		AND CurColStatus.SEP_SchemaName = CurSchema.name
		AND CurColStatus.SEP_MajorObjectName = CurTab.name
		AND CurColStatus.SEP_MinorObjectName = CurCol.name
		AND CurColStatus.SEP_Name = '{StatusName}'

	LEFT JOIN [{TargetDb}].dbo.StmExtendedProperty AS CurTabWatermark ON 1=1
		AND CurTabWatermark.SEP_Class = 'Table'
		AND CurTabWatermark.SEP_DatabaseNameSuffix = ''
		AND CurTabWatermark.SEP_SchemaName = CurSchema.name
		AND CurTabWatermark.SEP_MajorObjectName = CurTab.name
		AND CurTabWatermark.SEP_MinorObjectName = ''
		AND CurTabWatermark.SEP_Name = '{OfflineTransformation.Identifier}'
WHERE
	1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	AND
	(
		1=2
		OR CurCol.name is NULL -- new columns were not created
		OR CurColStatus.SEP_Value is NOT NULL -- new columns were created but not completely populated
	)

	AND
	(
		1=1
		AND NewSchema.name = '{sqlSchemaName}'
		AND NewTab.name = '{tableName}'
		AND NewCol.name IN ({string.Join(", ", columnNames.Select(s => $"'{s}'"))})
	)

OPTION (RECOMPILE);
");

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		Dictionary<string, (string ColType, short ColLength, byte ColPrecision, byte ColScale, bool ColSparse)> GetExistingColumnsDefinition(string databaseName, string sqlSchemaName, string tableName, IEnumerable<string> columnNames)
		{
			var sql = FormattableString.Invariant($@"
-- Get list of the existing columns definition
SELECT
	ColName          = CurCol.name,
	ColType          = CurType.name,
	ColLength        = CurCol.max_length,
	ColPrecision     = CurCol.precision,
	ColScale         = CurCol.scale,
	ColSparse        = CurCol.is_sparse
FROM
	[{databaseName}].sys.schemas                       AS CurSchema
	JOIN [{databaseName}].sys.tables                   AS CurTab     ON CurTab.schema_id = CurSchema.schema_id
	JOIN [{databaseName}].sys.columns                  AS CurCol     ON CurCol.object_id = CurTab.object_id
	JOIN [{databaseName}].sys.types                    AS CurType    ON CurType.user_type_id = CurCol.user_type_id

WHERE
	1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0

	AND
	(
		1=1
		AND CurSchema.name = '{sqlSchemaName}'
		AND CurTab.name = '{tableName}'
		AND CurCol.name IN ({string.Join(", ", columnNames.Select(s => $"'{s}'"))})
	)

OPTION (RECOMPILE);
");

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql).Rows.Cast<DataRow>()
				.Select(row => (row[ColNameColumn].ToString(), row)).ToDictionary(x => x.Item1, r => (r.row[ColTypeDataColumn].ToString(), (short)r.row[LengthDataColumn], (byte)r.row[PrecisionDataColumn], (byte)r.row[ScaleDataColumn], (bool)r.row[SparseDataColum]));
		}

		const string ColNameColumn = "ColName";
		const string ColTypeDataColumn = "ColType";
		const string LengthDataColumn = "ColLength";
		const string PrecisionDataColumn = "ColPrecision";
		const string ScaleDataColumn = "ColScale";
		const string SparseDataColum = "ColSparse";
	}
}
