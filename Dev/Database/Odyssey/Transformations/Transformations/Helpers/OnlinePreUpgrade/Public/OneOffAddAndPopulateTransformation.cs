using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	public abstract class OneOffAddAndPopulateTransformation : TablePreSynchroniser
	{
		protected OneOffAddAndPopulateTransformation(IUpgradeManager manager, string dbBeingUpgraded, string templateDb) : base(manager, dbBeingUpgraded, templateDb)
		{
		}

		protected abstract string UserDescription { get; }
		protected abstract string StatusName { get; }
		protected abstract string WatermarkName { get; }
		protected virtual Type TwinOfflineTransformation => null;

		public void Run()
		{
			if (!ShouldRun())
			{
				return;
			}

			RunTransformation();
		}

		public virtual bool ShouldRun()
		{
			if (TwinOfflineTransformation == null)
			{
				return false;
			}

			var transformationVersion = Mapper
				.GetAllMappings()
				.SingleOrDefault(mapping => mapping.TransformationType == TwinOfflineTransformation)
				?.MappedVersion;
			return (transformationVersion?.CompareTo(Manager.TransformationVersionBeforeUpgrade) ?? 0) > 0;
		}

		protected abstract void RunTransformation();

		protected void DoPopulate(params (string SqlSchemaName, string TableName, string TableOrderBy, IEnumerable<PopulatedColumn> PopulatingColumns)[] populatingColumns)
		{
			if (populatingColumns.Length == 0)
			{
				return;
			}

			var columnsToPopulate = GetColumnsToPopulateDataTable(populatingColumns.Select(tuple => (tuple.SqlSchemaName, tuple.TableName, tuple.PopulatingColumns.Select(column => column.ColumnName))));
			var populatedTables = GetColumnsToPopulate(columnsToPopulate, (grouping, table) => CreatePopulatedColumns(grouping, table, populatingColumns));

			DoPopulate(
				populatedTables,
				populatedTables.Count,
				populatedTables.Sum(x => x.TargetTableSize),
				UserDescription,
				"(+)",
				StatusName,
				WatermarkName);
		}

		protected DataTable GetColumnsToPopulateDataTable(IEnumerable<(string sqlSchemaName, string tableName, IEnumerable<string> columnNames)> newColumnsSet, string temporaryColumnPrefix = "")
			=> GetColumnsToPopulateDataTable(TargetDb, TemplateDB, StatusName, WatermarkName, newColumnsSet, temporaryColumnPrefix);

		public static DataTable GetColumnsToPopulateDataTable(string targetDb, string templateDb, string statusName, string watermarkName, IEnumerable<(string sqlSchemaName, string tableName, IEnumerable<string> columnNames)> newColumnsSet, string temporaryColumnPrefix = "")
		{
			string CreateTableQuery((string sqlSchemaName, string tableName, IEnumerable<string> newColumnNames) columnSet)
			{
				return $@"
(1=1
	AND NewSchema.name = '{columnSet.sqlSchemaName}'
	AND NewTab.name = '{columnSet.tableName}'
	AND NewCol.name IN ({string.Join(", ", columnSet.newColumnNames.Select(s => $"'{s}'"))})
)";
			}

			var varyingJoinClause = temporaryColumnPrefix.IsNullOrEmpty() ?
				FormattableString.Invariant($@"
					LEFT JOIN [{targetDb}].sys.columns AS CurCol    ON CurCol.object_id = CurTab.object_id AND CurCol.name = NewCol.name") :
				FormattableString.Invariant($@"
					JOIN [{targetDb}].sys.columns      AS CurCol    ON CurCol.object_id = CurTab.object_id AND CurCol.name = NewCol.name
					JOIN [{targetDb}].sys.types        AS CurType   ON CurType.user_type_id = CurCol.user_type_id
					JOIN [{targetDb}].sys.partitions   AS CurPart   ON CurPart.object_id = CurTab.object_id AND CurPart.index_id in (0, 1)
					LEFT JOIN [{targetDb}].sys.columns AS CurNewCol ON CurNewCol.object_id = CurCol.object_id AND CurNewCol.name = '{temporaryColumnPrefix}' + CurCol.name");

			var varyingColumnCheck = temporaryColumnPrefix.IsNullOrEmpty() ? FormattableString.Invariant($@"OR CurCol.name is NULL") : FormattableString.Invariant($@"OR CurNewCol.name is NULL");

			var sql = FormattableString.Invariant($@"
-- Get list of the columns to populate
SELECT
	TabSchema        = NewSchema.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewType.name,
	ColLength        = CASE NewType.name WHEN 'nvarchar' THEN NewCol.max_length / 2 ELSE NewCol.max_length END,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = CASE NewCol.is_nullable WHEN 1 THEN 'NULL' ELSE 'NOT NULL' END,
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDefault.definition,
	ColExtProp       = ISNULL(CurColStatus.SEP_Value, 'None'),
	TabExtProp       = ISNULL(CurTabWatermark.SEP_Value, '0')
FROM
	[{templateDb}].sys.schemas                       AS NewSchema
	JOIN [{templateDb}].sys.tables                   AS NewTab     ON NewTab.schema_id = NewSchema.schema_id
	JOIN [{templateDb}].sys.columns                  AS NewCol     ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types                    AS NewType    ON NewType.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	JOIN [{targetDb}].sys.schemas      AS CurSchema ON CurSchema.name = NewSchema.name
	JOIN [{targetDb}].sys.tables       AS CurTab    ON CurTab.schema_id = CurSchema.schema_id AND CurTab.name = NewTab.name
	{varyingJoinClause}

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurColStatus ON 1=1
		AND CurColStatus.SEP_Class = 'Column'
		AND CurColStatus.SEP_DatabaseNameSuffix = ''
		AND CurColStatus.SEP_SchemaName = CurSchema.name
		AND CurColStatus.SEP_MajorObjectName = CurTab.name
		AND CurColStatus.SEP_MinorObjectName = CurCol.name
		AND CurColStatus.SEP_Name = '{statusName}'

	LEFT JOIN [{targetDb}].dbo.StmExtendedProperty AS CurTabWatermark ON 1=1
		AND CurTabWatermark.SEP_Class = 'Table'
		AND CurTabWatermark.SEP_DatabaseNameSuffix = ''
		AND CurTabWatermark.SEP_SchemaName = CurSchema.name
		AND CurTabWatermark.SEP_MajorObjectName = CurTab.name
		AND CurTabWatermark.SEP_MinorObjectName = ''
		AND CurTabWatermark.SEP_Name = '{watermarkName}'
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	AND
	(1=2
		{varyingColumnCheck} -- new columns were not created
		OR CurColStatus.SEP_Value is NOT NULL -- new columns were created but not completely populated
	)

	AND
	(
		{string.Join(" OR ", newColumnsSet.Select(CreateTableQuery))}
	)

OPTION (RECOMPILE);
");

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		void CreatePopulatedColumns(IGrouping<string, ColumnChangeMetadata> table, PopulatedTable populatedTable, (string SqlSchemaName, string TableName, string TableOrderBy, IEnumerable<PopulatedColumn> PopulatingColumns)[] populatingTables)
		{
			var populatingTable = populatingTables
				.Where(tuple => string.Equals(populatedTable.TargetTableSchema, tuple.SqlSchemaName, StringComparison.OrdinalIgnoreCase))
				.Single(tuple => string.Equals(table.Key, tuple.TableName, StringComparison.OrdinalIgnoreCase));
			var populatingColumns = populatingTable
				.PopulatingColumns
				.ToList();

			foreach (var populatingColumn in populatingColumns)
			{
				var col = table.SingleOrDefault(column => string.Equals(populatingColumn.ColumnName, column.ColumnName, StringComparison.OrdinalIgnoreCase));

				if (col == null)
				{
					continue;
				}

				populatingColumn.ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture);
				populatingColumn.ColumnName = col.ColumnName;
				populatingColumn.ColumnType = col.GetFullTypeDeclaration();
				populatingColumn.ColumnNullable = col.IsNullable ? "NULL" : "NOT NULL";
				populatingColumn.IsSparse = col.IsSparse;

				if (!populatingColumn.IsSparse.Value)
				{
					populatingColumn.ColumnDefaultConstraint = string.IsNullOrEmpty(col.GetDefaultClause())
						? null
						: $"CONSTRAINT {col.CalculatedDefaultConstraintName.QuoteName()} DEFAULT {col.GetDefaultClause()}";
				}

				var targetWhereClause = populatingColumn.PopulateTargetWhereClause;
				if (string.IsNullOrEmpty(targetWhereClause))
				{
					populatingColumn.PopulateTargetWhereClause = populatingColumn.PopulatePreAddWhereClause;
				}

				if (!populatedTable.TargetTableStatus.HasValue)
				{
					populatedTable.TargetTableStatus = (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), true);
				}
			}

			populatedTable.ColumnList.AddRange(populatingColumns);
			populatedTable.OrderByExpression = populatingTable.TableOrderBy;

			PopulateTableProperties(populatedTable);
		}
	}
}
