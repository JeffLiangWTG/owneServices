using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Retrieves column changes between "DB Being Upgraded" and "Template DB" using metadata tables.
	/// Tags {0} and {1} in the scripts are replaced by the actual "DB Being Upgraded" and "Template DB" names.
	/// </summary>
	public class ColumnChangeRetriever : MetadataScriptRunner
	{
		public const string WtgPreservedColumnPrefix = "CW!!";

		public ColumnChangeRetriever(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
			: base(upgConnection, dbBeingUpgraded, templateDb)
		{
		}

		public DataTable GetColumnsToDrop()
		{
			return DataUtils.GetDataTableFromQuery(upgConnection, $@"
SELECT
	TabSchema              = CurSch.name,
	TabName                = CurTab.name,
	ColName                = CurCol.name,
	IsComputedColumnChange = CurCol.is_computed
FROM
	[{dbBeingUpgraded}].sys.schemas      AS CurSch
	JOIN [{dbBeingUpgraded}].sys.tables  AS CurTab ON CurTab.schema_id = CurSch.schema_id
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id

	JOIN [{templateDb}].sys.schemas      AS NewSch ON 1=1
		AND NewSch.name = CurSch.name
	JOIN [{templateDb}].sys.tables       AS NewTab ON NewTab.schema_id = NewSch.schema_id
		AND NewTab.name = CurTab.name
	LEFT JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
		AND NewCol.name = CurCol.name
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Exclude CW preserved columns
	AND CurCol.name NOT LIKE '{WtgPreservedColumnPrefix}%'
	-- Only columns not in the new schema
	AND NewCol.name is NULL

"
				);
		}

		public DataTable GetColumnsToAlter()
		{
			return DataUtils.GetDataTableFromQuery(upgConnection, $@"
SELECT
	TabSchema              = NewSch.name,
	TabName                = NewTab.name,
	ColName                = NewCol.name,
	ColType                = NewTyp.name,
	ColLength              = IIF(NewCol.max_length > 0 AND NewTyp.name in (N'nchar', N'nvarchar'), NewCol.max_length / 2, NewCol.max_length),
	ColNullOrNotNull       = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse              = NewCol.is_sparse,
	ColPrecision           = NewCol.precision,
	ColScale               = NewCol.scale,
	ColDefault             = NewDefault.definition,
	ColXmlSchema           = NewXmlSchema.name,
	OldType                = CurTyp.name,
	OldPrecision           = CurCol.precision,
	OldLength              = IIF(CurCol.max_length > 0 AND CurTyp.name in (N'nchar', N'nvarchar'), CurCol.max_length / 2, CurCol.max_length),
	IdentityChanged        = CONVERT(bit, IIF(ISNULL(NewIdCol.seed_value, 0) = ISNULL(CurIdCol.seed_value, 0) AND ISNULL(NewIdCol.increment_value, 0) = ISNULL(CurIdCol.increment_value, 0), 0, 1)),
	IsComputedColumnChange = (NewCol.is_computed | CurCol.is_computed)
FROM
	[{templateDb}].sys.schemas      AS NewSch
	JOIN [{templateDb}].sys.tables  AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types   AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	LEFT JOIN [{templateDb}].sys.identity_columns       AS NewIdCol ON NewIdCol.object_id = NewCol.object_id AND NewIdCol.column_id = NewCol.column_id
	LEFT JOIN [{templateDb}].sys.xml_schema_collections AS NewXmlSchema ON NewXmlSchema.xml_collection_id = NewCol.xml_collection_id
	LEFT JOIN [{templateDb}].sys.computed_columns       AS NewCptCol ON NewCptCol.object_id = NewCol.object_id AND NewCptCol.column_id = NewCol.column_id

	JOIN [{dbBeingUpgraded}].sys.schemas AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN [{dbBeingUpgraded}].sys.tables  AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name
	JOIN [{dbBeingUpgraded}].sys.types   AS CurTyp ON CurTyp.user_type_id = CurCol.user_type_id

	LEFT JOIN [{dbBeingUpgraded}].sys.identity_columns       AS CurIdCol ON CurIdCol.object_id = CurCol.object_id AND CurIdCol.column_id = CurCol.column_id
	LEFT JOIN [{dbBeingUpgraded}].sys.xml_schema_collections AS CurXmlSchema ON CurXmlSchema.xml_collection_id = CurCol.xml_collection_id
	LEFT JOIN [{dbBeingUpgraded}].sys.computed_columns       AS CurCptCol ON CurCptCol.object_id = CurCol.object_id AND CurCptCol.column_id = CurCol.column_id
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns modified in the new schema
	AND
	(1=2
		OR NewTyp.name <> CurTyp.name
		OR NewCol.is_nullable <> CurCol.is_nullable
		OR NewCol.is_sparse <> CurCol.is_sparse
		OR NewCol.max_length <> CurCol.max_length
		OR NewCol.scale <> CurCol.scale
		OR NewCol.precision <> CurCol.precision
		OR ISNULL(NewXmlSchema.name, N'') <> ISNULL(CurXmlSchema.name, N'')
		OR ISNULL(NewIdCol.seed_value, 0) <> ISNULL(CurIdCol.seed_value, 0)
		OR ISNULL(NewIdCol.increment_value, 0) <> ISNULL(CurIdCol.increment_value, 0)
		OR ISNULL(NewCptCol.definition, N'') <> ISNULL(CurCptCol.definition, N'')
		OR ISNULL(NewCptCol.is_persisted, -1) <> ISNULL(CurCptCol.is_persisted, -1)
	)

"
				);
		}

		public DataTable GetColumnsToFixCase()
		{
			return DataUtils.GetDataTableFromQuery(upgConnection, $@"
SELECT
	TabSchema              = NewSch.name,
	TabName                = NewTab.name,
	NewColName             = NewCol.name,
	OldColName             = CurCol.name,
	IsComputedColumnChange = CurCol.is_computed
FROM
	[{templateDb}].sys.schemas      AS NewSch
	JOIN [{templateDb}].sys.tables  AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id

	JOIN [{dbBeingUpgraded}].sys.schemas AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN [{dbBeingUpgraded}].sys.tables  AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns with a different name casing
	AND CurCol.name <> NewCol.name COLLATE SQL_Latin1_General_CP1_CS_AS

"
				);
		}

		public DataTable GetColumnsToAdd()
		{
			var reservedSchemas = string.Join("','", Db.SqlReservedSchemas);

			return DataUtils.GetDataTableFromQuery(upgConnection, $@"
SELECT
	TabSchema                 = NewSch.name,
	TabName                   = NewTab.name,
	ColName                   = NewCol.name,
	ColType                   = NewTyp.name,
	ColLength                 = IIF(NewCol.max_length > 0 AND NewTyp.name in (N'nchar', N'nvarchar'), NewCol.max_length / 2, NewCol.max_length),
	ColNullOrNotNull          = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse                 = NewCol.is_sparse,
	ColPrecision              = NewCol.precision,
	ColScale                  = NewCol.scale,
	ColDefault                = NewDefault.definition,
	ColXmlSchema              = NewXmlSchema.name,
	IdentitySeed              = NewIdCol.seed_value,
	IdentityIncrement         = NewIdCol.increment_value,
	IsComputedColumnChange    = NewCol.is_computed,
	ComputedColumnDefinition  = NewCptCol.definition,
	IsPersistedComputedColumn = NewCptCol.is_persisted
FROM
	[{templateDb}].sys.schemas      AS NewSch
	JOIN [{templateDb}].sys.tables  AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types   AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	LEFT JOIN [{templateDb}].sys.identity_columns       AS NewIdCol     ON NewIdCol.object_id = NewCol.object_id AND NewIdCol.column_id = NewCol.column_id
	LEFT JOIN [{templateDb}].sys.computed_columns       AS NewCptCol    ON NewCptCol.object_id = NewCol.object_id AND NewCptCol.column_id = NewCol.column_id
	LEFT JOIN [{templateDb}].sys.xml_schema_collections AS NewXmlSchema ON NewXmlSchema.xml_collection_id = NewCol.xml_collection_id
		AND NewCol.xml_collection_id <> 0

	JOIN [{dbBeingUpgraded}].sys.schemas      AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN [{dbBeingUpgraded}].sys.tables       AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	LEFT JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name
WHERE 1=1
	-- Exclude tables in system schemas
	AND CurSch.name NOT in ('{reservedSchemas}')
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0
	-- Only columns added in the new schema
	AND CurCol.name is NULL

"
				);
		}

		public DataTable GetColumnsToConvertCharToBit()
		{
			return DataUtils.GetDataTableFromQuery(upgConnection, $@"-- Get list of the columns to populate
SELECT
	TabSchema        = NewSch.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewTyp.name,
	ColLength        = NewCol.max_length,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDef.definition
FROM
	[{templateDb}].sys.schemas      AS NewSch
	JOIN [{templateDb}].sys.tables  AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types   AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDef ON NewDef.object_id = NewCol.default_object_id

	JOIN [{dbBeingUpgraded}].sys.schemas AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN [{dbBeingUpgraded}].sys.tables  AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name
	JOIN [{dbBeingUpgraded}].sys.types   AS CurTyp ON CurTyp.user_type_id = CurCol.user_type_id
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns to be converted from char to bit OR columns for that populating was started
	AND CurTyp.name in (N'char', N'varchar')
	AND CurCol.max_length = 1
	AND NewTyp.name = N'bit'

OPTION (RECOMPILE)

"
				);
		}

		public DataTable GetColumnsToConvertCharToChar()
		{
			return DataUtils.GetDataTableFromQuery(upgConnection, FormattableString.Invariant($@"-- Get list of the columns to populate
SELECT
	TabSchema        = NewSch.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewTyp.name,
	ColLength        = IIF(NewCol.max_length > 0 AND NewTyp.name in (N'nchar', N'nvarchar'), NewCol.max_length / 2, NewCol.max_length),
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDef.definition
FROM
	[{templateDb}].sys.schemas      AS NewSch
	JOIN [{templateDb}].sys.tables  AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types   AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDef ON NewDef.object_id = NewCol.default_object_id

	JOIN [{dbBeingUpgraded}].sys.schemas AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN [{dbBeingUpgraded}].sys.tables  AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name
	JOIN [{dbBeingUpgraded}].sys.types   AS CurTyp ON CurTyp.user_type_id = CurCol.user_type_id

	CROSS APPLY
	(
		SELECT
			Value = CONVERT(bit, IIF(EXISTS(
				SELECT NULL
				FROM
					[{templateDb}].sys.indexes            AS ind
					JOIN [{templateDb}].sys.index_columns AS icol ON icol.object_id = ind.object_id AND icol.index_id = ind.index_id
				WHERE 1=1
					AND ind.object_id = NewCol.object_id
					AND
					(1=2
						OR icol.column_id = NewCol.column_id
						OR ind.has_filter = 1 AND CHARINDEX(QUOTENAME(NewCol.name), ind.filter_definition) > 0
					)
				), 1, 0))
	) AS Indexed

WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns to be converted from [n][var]char to [n][var]char
	AND CurTyp.name in (N'char', N'varchar', N'nchar', N'nvarchar')
	AND NewTyp.name in (N'char', N'varchar', N'nchar', N'nvarchar')

	-- Ignore computed columns
	AND CurCol.is_computed = 0
	AND NewCol.is_computed = 0

	-- Ignore decreasing size
	AND
		IIF(CurCol.max_length = -1
			, 32767
			, IIF(CurTyp.name in (N'nchar', N'nvarchar')
				, CurCol.max_length / 2
				, CurCol.max_length
				)
			)
		<= 
		IIF(NewCol.max_length = -1
			, 32767
			, IIF(NewTyp.name in (N'nchar', N'nvarchar')
				, NewCol.max_length / 2
				, NewCol.max_length
				)
			)

	-- Ignore metadata-only operations
	AND
	(1=2
		OR CurTyp.name <> NewTyp.name
		OR
		(
			CurTyp.name = NewTyp.name
			AND CurCol.max_length <> NewCol.max_length
			AND
			(1=2
				OR CurTyp.name in (N'char', N'nchar')
				OR Indexed.Value = 1
			)
		)
	)

	-- Ignore Offline-only compatible types
	-- NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation
	AND NOT
	(
		NewTyp.name in (N'varchar', N'nvarchar')
		AND NewCol.max_length = -1
		AND NewCol.is_nullable = 0
	)

OPTION (RECOMPILE)

"
				));
		}

		public DataTable GetColumnsToCompleteDateTimesToDateTimeOffsetTransform()
		{
			var prefix = ColumnSynchroniser.ConvertDateTimeToDateTimeOffsetColumnPrefix;
			return DataUtils.GetDataTableFromQuery(upgConnection, Invariant($@"-- {nameof(GetColumnsToCompleteDateTimesToDateTimeOffsetTransform)}
SELECT
	TabSchema  = CurSch.name,
	TabName    = CurTab.name,
	ColName    = CurCol.name,
	OldDefName = TempDef.name
FROM
	[{dbBeingUpgraded}].sys.schemas      AS CurSch
	JOIN [{dbBeingUpgraded}].sys.objects  AS CurTab ON CurTab.schema_id = CurSch.schema_id
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
	JOIN [{dbBeingUpgraded}].sys.types   AS CurTyp ON CurTyp.user_type_id = CurCol.user_type_id

	JOIN [{templateDb}].sys.schemas AS NewSch ON 1=1
		AND NewSch.name = CurSch.name
	JOIN [{templateDb}].sys.objects  AS NewTab ON NewTab.schema_id = NewSch.schema_id
		AND NewTab.name = CurTab.name
	JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
		AND NewCol.name = CurCol.name
	JOIN [{templateDb}].sys.types   AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id

	JOIN [{dbBeingUpgraded}].sys.columns AS TempCol ON TempCol.object_id = CurTab.object_id
		AND TempCol.Name = N{prefix.QuoteName('\'')} + CurCol.name
	JOIN [{dbBeingUpgraded}].sys.types   AS TempTyp ON TempTyp.user_type_id = TempCol.user_type_id
	LEFT JOIN [{dbBeingUpgraded}].sys.default_constraints AS TempDef ON TempDef.object_id = TempCol.default_object_id
WHERE 1=1
	AND CurTab.type = 'U'
	AND NewTab.type = 'U'
	AND TempCol.name like N'{DataUtils.ReplaceSqlLikeWildcard(prefix.QuoteEscapedName('\''))}%'

	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Only columns to be converted from smalldatetime, datetime or datetime2 to datetimeoffset
	AND CurTyp.name in (N'smalldatetime', N'datetime', N'datetime2')
	AND NewTyp.name = N'datetimeoffset'
	AND TempTyp.name = N'datetimeoffset'

OPTION (RECOMPILE)

")
				);
		}

		public DataTable GetColumnsToConvertDateTimesToDate()
		{
			var prefix = ColumnSynchroniser.ConvertDateTimesToDateColumnPrefix;
			return DataUtils.GetDataTableFromQuery(upgConnection, Invariant($@"-- {nameof(GetColumnsToConvertDateTimesToDate)}
SELECT
	TabSchema  = CurSch.name,
	TabName    = CurTab.name,
	ColName    = CurCol.name,
	OldDefName = TempDef.name
FROM
	[{dbBeingUpgraded}].sys.schemas      AS CurSch
	JOIN [{dbBeingUpgraded}].sys.objects  AS CurTab ON CurTab.schema_id = CurSch.schema_id
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
	JOIN [{dbBeingUpgraded}].sys.types   AS CurTyp ON CurTyp.user_type_id = CurCol.user_type_id

	JOIN [{templateDb}].sys.schemas AS NewSch ON 1=1
		AND NewSch.name = CurSch.name
	JOIN [{templateDb}].sys.objects  AS NewTab ON NewTab.schema_id = NewSch.schema_id
		AND NewTab.name = CurTab.name
	JOIN [{templateDb}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
		AND NewCol.name = CurCol.name
	JOIN [{templateDb}].sys.types   AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id

	JOIN [{dbBeingUpgraded}].sys.columns AS TempCol ON TempCol.object_id = CurTab.object_id
		AND TempCol.Name = N{prefix.QuoteName('\'')} + CurCol.name
	JOIN [{dbBeingUpgraded}].sys.types   AS TempTyp ON TempTyp.user_type_id = TempCol.user_type_id
	LEFT JOIN [{dbBeingUpgraded}].sys.default_constraints AS TempDef ON TempDef.object_id = TempCol.default_object_id
WHERE 1=1
	AND CurTab.type = 'U'
	AND NewTab.type = 'U'
	AND TempCol.name like N'{DataUtils.ReplaceSqlLikeWildcard(prefix.QuoteEscapedName('\''))}%'

	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Ignore computed columns
	AND CurCol.is_computed = 0
	AND NewCol.is_computed = 0

	-- Only columns to be converted from a datetime type to date
	AND CurTyp.name IN (N'smalldatetime', N'datetime', N'datetime2')
	AND NewTyp.name = N'date'
	AND TempTyp.name = N'date'

OPTION (RECOMPILE)")
				);
		}

		public DataTable GetColumnsToConvertDecimalToDecimal() => DataUtils.GetDataTableFromQuery(upgConnection, $@"-- {nameof(GetColumnsToConvertDecimalToDecimal)}
SELECT
	TabSchema        = NewSchema.name,
	TabName          = NewTab.name,
	ColId            = NewCol.column_id,
	ColName          = NewCol.name,
	ColType          = NewType.name,
	ColLength        = NewCol.max_length,
	ColPrecision     = NewCol.precision,
	ColScale         = NewCol.scale,
	ColNullOrNotNull = IIF(NewCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse        = NewCol.is_sparse,
	ColDefault       = NewDefault.definition
FROM
	[{templateDb}].sys.schemas                       AS NewSchema
	JOIN [{templateDb}].sys.tables                   AS NewTab     ON NewTab.schema_id = NewSchema.schema_id
	JOIN [{templateDb}].sys.columns                  AS NewCol     ON NewCol.object_id = NewTab.object_id
	JOIN [{templateDb}].sys.types                    AS NewType    ON NewType.user_type_id = NewCol.user_type_id
	LEFT JOIN [{templateDb}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id

	JOIN [{dbBeingUpgraded}].sys.schemas      AS CurSchema ON 1=1
		AND CurSchema.name = NewSchema.name
	JOIN [{dbBeingUpgraded}].sys.tables       AS CurTab    ON CurTab.schema_id = CurSchema.schema_id
		AND CurTab.name = NewTab.name
	JOIN [{dbBeingUpgraded}].sys.columns      AS CurCol    ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name
	JOIN [{dbBeingUpgraded}].sys.types        AS CurType   ON CurType.user_type_id = CurCol.user_type_id
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Ignore computed columns
	AND CurCol.is_computed = 0
	AND NewCol.is_computed = 0

	-- Only columns to be converted from decimal to decimal
	AND CurType.name = 'decimal'
	AND NewType.name = 'decimal'

	-- Exclude columns that are staying the same, losing precision, losing scale, or losing leading digits
	AND (NewCol.precision <> CurCol.precision OR NewCol.scale <> CurCol.scale)
	AND NewCol.precision >= CurCol.precision
	AND NewCol.scale >= CurCol.scale
	AND NewCol.precision - NewCol.scale >= CurCol.precision - CurCol.scale

OPTION (RECOMPILE);
");

		public DataTable GetColumnsToReplaceWithTargetColumnsTransform()
		{
			var prefix = ColumnSynchroniser.ColumnRenamePrefix;
			return DataUtils.GetDataTableFromQuery(upgConnection, Invariant($@"-- {nameof(GetColumnsToReplaceWithTargetColumnsTransform)}
SELECT 
	TabSchema  = OBJECT_SCHEMA_NAME(CurTab.object_id, DB_ID(N{dbBeingUpgraded.QuoteName('\'')})),
	TabName    = CurTab.name,
	ColName    = CurCol.name,
	OldDefName = TargetDef.name
FROM
	[{dbBeingUpgraded}].sys.objects      AS CurTab
	JOIN [{dbBeingUpgraded}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
	JOIN [{dbBeingUpgraded}].sys.columns AS TargetCol ON TargetCol.object_id = CurCol.object_id 
		AND TargetCol.name = N{prefix.QuoteName('\'')} + CurCol.name 
	LEFT JOIN [{dbBeingUpgraded}].sys.default_constraints AS TargetDef ON TargetDef.object_id = TargetCol.default_object_id
WHERE 1=1
	-- Ignore Microsoft-shipped tables
	AND CurTab.type = 'U'
	AND CurTab.is_ms_shipped = 0;
")
				);
		}
	}
}
