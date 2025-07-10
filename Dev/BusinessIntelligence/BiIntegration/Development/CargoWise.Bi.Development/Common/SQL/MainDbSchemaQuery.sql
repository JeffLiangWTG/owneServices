WITH

PkColumn
AS (
	SELECT
		ic.[object_id],
		column_id = min(ic.[column_id])
	FROM
		sys.indexes AS i
		INNER JOIN sys.index_columns AS ic
			ON i.[object_id] = ic.[object_id]
			AND i.[index_id] = ic.[index_id]
	WHERE
		i.[is_primary_key] = 1
	GROUP BY
		ic.[object_id]
),

EligibleColumn
AS (
	SELECT
		[SourceSchema] = s.[name],
		[SourceTable] = t.[name],
		[SourceColumn] = c.[name],
		[DataType] = CASE WHEN dt.[name] <> 'geometry' THEN dt.[name] ELSE 'geography' END,
		[MaxLength] = CASE WHEN dt.[name] IN ('nchar', 'nvarchar') AND c.[max_length] <> -1 THEN c.[max_length]/2 ELSE c.[max_length] END,
		[Precision] = c.[precision],
		[Scale] = c.[scale],
		[Nullable] = c.[is_nullable],
		[IsPrimaryKey] = CAST(CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END AS bit)
	FROM
		sys.schemas AS s
		INNER JOIN sys.tables AS t
			ON t.[schema_id] = s.[schema_id]
		INNER JOIN sys.columns AS c
			ON c.[object_id] = t.[object_id]
		INNER JOIN sys.types AS dt
			ON c.[system_type_id] = dt.[system_type_id]
		LEFT OUTER JOIN PkColumn AS pk
			ON pk.[object_id] = c.[object_id]
			AND pk.[column_id] = c.[column_id]
	WHERE
		t.[is_ms_shipped] = 0
		AND s.[name] NOT LIKE 'db[_]%'
		AND s.[name] NOT IN ('sys', 'cdc', 'guest','INFORMATION_SCHEMA')
		AND t.[name] NOT LIKE 'Dummy%'
		AND t.[name] NOT IN ({0})
		AND dt.[name] NOT IN ({1})
		AND (c.[max_length] > 0 OR
			(c.[max_length] = -1 AND dt.[name] IN ('geometry', 'varbinary', 'xml', 'nvarchar', 'varchar')))
		AND c.[max_length] <= {2}
		AND c.[is_computed] = 0
),

ReferenceColumn
AS (
	SELECT
		SCHEMA_NAME(f.[schema_id]) SourceSchema,
		OBJECT_NAME(f.[parent_object_id]) AS SourceTable,
		COL_NAME(fc.[parent_object_id], fc.[parent_column_id]) AS SourceColumn,
		SCHEMA_NAME(p.[schema_id]) ReferenceSchema,
		OBJECT_NAME (f.[referenced_object_id]) AS ReferenceTable
	FROM
		sys.foreign_keys AS f
		INNER JOIN sys.foreign_key_columns AS fc
			ON f.object_id = fc.constraint_object_id
		INNER JOIN sys.tables AS p
			ON p.object_id = fc.referenced_object_id
	WHERE f.name IN
	(
		SELECT fk.name
		FROM
			sys.foreign_keys AS fk
			INNER JOIN sys.foreign_key_columns AS fc
				ON fk.object_id = fc.constraint_object_id
		GROUP BY fk.name
		HAVING COUNT(fk.name) = 1
	)
)

SELECT 
       col.[SourceSchema],
       col.[SourceTable],
       col.[SourceColumn],
       col.[DataType],
       col.[MaxLength],
       col.[Precision],
       col.[Scale],
       col.[Nullable],
       col.[IsPrimaryKey],
       ref.[ReferenceSchema],
       ref.[ReferenceTable]
FROM
       EligibleColumn AS col 
       LEFT OUTER JOIN ReferenceColumn AS ref
              ON col.[SourceSchema] = ref.[SourceSchema]
              AND col.[SourceTable] = ref.[SourceTable]
              AND col.[SourceColumn] = ref.[SourceColumn]
