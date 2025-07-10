using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	public class IndexScriptRunner : MetadataScriptRunner
	{
		public IndexScriptRunner(DbConnection upgConnection, string dbBeingUpgraded, string templateDb, IUpgradeTaskWorkflowLogger taskLogger = null)
			: base(upgConnection, dbBeingUpgraded, templateDb, taskLogger)
		{
		}

		public void SetSelectiveXMLIndexesChangedFilter()
		{
			taskLogger?.StartTask("Setting selective XML indexes changed filter");
			selectiveXMLIndexesChangedFilter = GetSelectiveXMLIndexesChangedFilter(upgConnection, dbBeingUpgraded, templateDb);
		}

		string selectiveXMLIndexesChangedFilter;

		string GetSelectiveXMLIndexesChangedFilter(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
		{
			var sql = GetScriptReplacingDbNames(FormattableString.Invariant($@"
SELECT * FROM(
	SELECT
		NewIndex.SchemaName,
		NewIndex.TableName,
		NewIndex.IndexName,
		CurIndex.ObjectId,
		CurIndex.IndexId,
		CurIndex.NameSpaces  AS CurNameSpaces,
		CurIndex.Paths       AS CurPaths,
		NewIndex.NameSpaces  AS NewNameSpaces,
		NewIndex.Paths       AS NewPaths
	FROM
		({GetDbIndexInfoSql(includeColumnListInfo: false, dbStringFormatItemIndex: 0)}) CurIndex
		INNER JOIN
		({GetDbIndexInfoSql(includeColumnListInfo: true, dbStringFormatItemIndex: 1)}) NewIndex

		-- Index matching on Schema + Table + Name
		ON  CurIndex.SchemaName = NewIndex.SchemaName
		AND CurIndex.TableName = NewIndex.TableName
		AND CurIndex.IndexName = NewIndex.IndexName
	) selectiveXMLIndexes
WHERE CurNameSpaces != NewNameSpaces OR CurPaths != NewPaths
"));
			var selectiveXMLIndexesChanged = DataUtils.GetDataTableFromQuery(upgConnection, sql);

			var filter = string.Empty;
			foreach (DataRow row in selectiveXMLIndexesChanged.Rows)
			{
				filter += FormattableString.Invariant($"OR (CurIndex.ObjectId={row["ObjectId"]} AND CurIndex.IndexId={row["IndexId"]})");
			}
			return filter;
		}

		// Created for testing
		protected virtual void ExecuteNonQuery(string sqlText)
		{
			upgConnection.ExecuteNonQuery(sqlText);
		}

		#region Drop Old Indexes

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- DROP INDEXES THAT HAVE BEEN REMOVED FROM THE SCHEMA
		///	--
		///	-- Note 1: Indexes created for UniqueKey (UK) constraints and PrimaryKey (PK) constraints are ignored here.
		///	--
		/// --           Unique Index                  => is_unique != 0
		/// --           Clustered Index               => type = 1
		/// --           PK Constraint Index           => is_primary_key != 0
		/// --           UNIQUE Constraint Index       => is_unique_constraint != 0
		/// --
		///	-- Note 2: HEAP indexes are also ignored.
		///	--
		/// --         Index TYPE information:
		/// --           0 => Heap - Entry for all tables without a clustered index
		/// --           1 => Clustered
		/// --           2 => Nonclustered
		/// --           3 => XML
		/// --           4 => Spatial
		/// --           5 => Columnstore clustered
		/// --           6 => Columnstore nonclustered
		/// --
		/// -- Note 3: Indexes of tables that are not in the new schema are IGNORED
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		public void DropOldIndexes()
		{
			var selectOldIndexSql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	CurIndex.SchemaName,
	CurIndex.TableName,
	CurIndex.IndexName

FROM
	(
		SELECT
			sch.name SchemaName,
			tab.name TableName,
			ind.name IndexName,
			ind.index_id IndexId
		FROM
			[{0}].sys.tables tab
			INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
			INNER JOIN [{0}].sys.indexes ind ON ind.object_id = tab.object_id
		WHERE
			tab.is_ms_shipped = 0
			AND ind.is_primary_key = 0
			AND ind.is_unique_constraint = 0
			AND ind.type != 0
			-- Ignore TABLES that are not in the new schema
			AND EXISTS (
				SELECT newtab.name 
				FROM [{1}].sys.tables newtab
				WHERE newtab.name = tab.name)
			-- Ignore manually created indexes by WTG
			AND ind.name NOT LIKE '{2}%'
	) CurIndex

	LEFT OUTER JOIN

	(
		SELECT
			sch.name SchemaName,
			tab.name TableName,
			ind.name IndexName
		FROM
			[{1}].sys.tables tab
			INNER JOIN [{1}].sys.schemas sch ON sch.schema_id = tab.schema_id
			INNER JOIN [{1}].sys.indexes ind ON ind.object_id = tab.object_id
		WHERE
			tab.is_ms_shipped = 0
			AND ind.is_primary_key = 0
			AND ind.is_unique_constraint = 0
			AND ind.type != 0
	) NewIndex

	-- Index matching on Schema + Table + Name
	ON  CurIndex.SchemaName = NewIndex.SchemaName
	AND CurIndex.TableName  = NewIndex.TableName
	AND CurIndex.IndexName  = NewIndex.IndexName

WHERE
	NewIndex.TableName IS NULL

-- Drop dependent index first (in case of dependent XML indexes for instance)
ORDER BY
	CurIndex.IndexId DESC
OPTION (RECOMPILE)
"
				, "{0}"
				, "{1}"
				, IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX.Replace("_", "[_]")
				);

			string indexesToDropScript = GetScriptReplacingDbNames(selectOldIndexSql);
			var indexesToDrop = DataUtils.GetDataTableFromQuery(upgConnection, indexesToDropScript);

			foreach (DataRow row in indexesToDrop.Rows)
			{
				string schemaName = row["SchemaName"].ToString().Trim();
				string tableName = row["TableName"].ToString().Trim();
				string indexName = row["IndexName"].ToString().Trim();

				if (taskLogger != null)
				{
					taskLogger.ShowInfoMessage("\t  (-) [" + schemaName + "].[" + tableName + "].[" + indexName + "]");
				}

				DropReferencingFks(schemaName, tableName, indexName);

				string dropScript = "DROP INDEX [" + indexName + "] ON [" + schemaName + "].[" + tableName + "];";
				ExecuteNonQuery(dropScript);
			}
		}

		#endregion

		#region Re-create Modified Indexes

		/// <summary>
		///	------------------------------------------------------------------------------------------------------------
		///	-- RE-CREATE INDEXES THAT HAVE BEEN MODIFIED
		///	--
		///	-- 1/ DROP + CREATE WHEN INDEX TYPE HAS CHANGED (e.g.: CLUSTERED, XML)
		///	-- 2/ CREATE (DROP EXISTING) FOR OTHER CHANGES (e.h.: Change of COLUMN LIST)
		///	--
		///	-- Note 1: Indexes created for UniqueKey (UK) constraints and PrimaryKey (PK) constraints are ignored here.
		///	--
		/// --           Unique Index                  => is_unique != 0
		/// --           Clustered Index               => type = 1
		/// --           PK Constraint Index           => is_primary_key != 0
		/// --           UNIQUE Constraint Index       => is_unique_constraint != 0
		/// --
		///	-- Note 2: HEAP, SPATIAL and COLUMNSTORE indexes are also ignored.
		///	--
		/// --         Index TYPE information:
		/// --           0 => Heap - Entry for all tables without a clustered index
		/// --           1 => Clustered
		/// --           2 => Nonclustered
		/// --           3 => XML
		/// --           4 => Spatial
		/// --           5 => Columnstore clustered
		/// --           6 => Columnstore nonclustered
		/// --
		/// -- Note 3: Advanced index options bellow are not considered.
		/// --         STATISTICS_NORECOMPUTE, IGNORE_DUP_KEY, PAD_INDEX
		/// --         (UnitTests enforce there are no indexes with special settings for these options)
		/// --
		/// -- Note 4: If FILLFACTOR is specified (between 1..99) in the template DB, it is used.
		/// --         Otherwise the value in the DB being upgraded is preserved.
		/// --
		/// -- Note 5: Indexes of tables that are not in the new schema are IGNORED
		/// --
		/// -- Note 6: Index Column (key) sort order is not used when matching (for performance reasons)
		/// --         It is regarded as being always ASCENDING (UnitTest enforced).
		///	------------------------------------------------------------------------------------------------------------
		/// </summary>
		public void RecreateModifiedIndexes()
		{
			RecreateModifiedKeyListIndexes();
			RecreateModifiedAttributeIndexes();
		}

		#region Modified Key (column) List Indexes

		void RecreateModifiedKeyListIndexes()
		{
			string scriptWithDbIndexes = String.Format(
				ModifiedKeyListIndexInfoSql,
				GetDbIndexInfoSql(includeColumnListInfo: true, dbStringFormatItemIndex: 0),
				GetDbIndexInfoSql(includeColumnListInfo: true, dbStringFormatItemIndex: 1)
			);
			RunSynchroniseIndexCommands(GetScriptReplacingDbNames(scriptWithDbIndexes));
		}

		protected string ModifiedKeyListIndexInfoSql = @"
			SELECT
				NewIndex.SchemaName,
				NewIndex.TableName,
				NewIndex.IndexName,
				NewIndex.IndexType,
				cast(
					CASE
						WHEN (CurIndex.ParentXmlIndexName is null AND NewIndex.ParentXmlIndexName is null) THEN 0
						ELSE 1
					END as bit) AS DropAction,
				cast(1 as bit) AS CreateAction,
				cast(1 as bit) AS IsExistingIndex,
				NewIndex.IsUnique,
				CurIndex.IndFillFactor,
				NewIndex.IndexFilter,
				NewIndex.IndexCompression,
				NewIndex.ParentXmlIndexName,
				NewIndex.XmlSecondaryType,
				NewIndex.XmlIndexType,
				NewIndex.NameSpaces,
				NewIndex.Paths,
				NewIndex.KeyColumns,
				NewIndex.IncludeColumns
			FROM
				({0}) CurIndex
				INNER JOIN
				({1}) NewIndex

				-- Index matching on Schema + Table, Name, IsUnique, IndexType, IndexFilter, IndexCompression, FillFactor (regarded only if template fillfactor is specified)
				ON  CurIndex.SchemaName       = NewIndex.SchemaName
				AND CurIndex.TableName        = NewIndex.TableName
				AND CurIndex.IndexName        = NewIndex.IndexName
				AND CurIndex.IsUnique         = NewIndex.IsUnique
				AND CurIndex.IndexType        = NewIndex.IndexType
				AND CurIndex.IndexFilter      = NewIndex.IndexFilter
				AND CurIndex.IndexCompression = NewIndex.IndexCompression
				AND (NewIndex.IndFillFactor = 0 OR CurIndex.IndFillFactor = NewIndex.IndFillFactor)
			WHERE
				(
					CurIndex.KeyColumns != NewIndex.KeyColumns
					OR isnull(CurIndex.IncludeColumns, '') != isnull(NewIndex.IncludeColumns, '')
				)
			OPTION (USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'))";

		#endregion

		#region Modified Attribute Indexes

		void RecreateModifiedAttributeIndexes()
		{
			string scriptWithDbIndexes = String.Format(
				ModifiedAttributeIndexInfoSql,
				GetDbIndexInfoSql(includeColumnListInfo: false, dbStringFormatItemIndex: 0),
				GetDbIndexInfoSql(includeColumnListInfo: true, dbStringFormatItemIndex: 1),
				selectiveXMLIndexesChangedFilter
			);
			RunSynchroniseIndexCommands(GetScriptReplacingDbNames(scriptWithDbIndexes));
		}

		protected string ModifiedAttributeIndexInfoSql = @"
			SELECT
				NewIndex.SchemaName,
				NewIndex.TableName,
				NewIndex.IndexName,
				NewIndex.IndexType,
				cast(
					CASE
						WHEN CurIndex.IndexType = NewIndex.IndexType
						AND CurIndex.ParentXmlIndexName is null
						AND NewIndex.ParentXmlIndexName is null
							THEN 0
						ELSE 1
					END as bit) AS DropAction,
				cast(1 as bit) AS CreateAction,
				cast(1 as bit) AS IsExistingIndex,
				NewIndex.IsUnique,
				(CASE WHEN NewIndex.IndFillFactor > 0 THEN NewIndex.IndFillFactor ELSE CurIndex.IndFillFactor END) AS IndFillFactor,
				NewIndex.IndexFilter,
				NewIndex.IndexCompression,
				NewIndex.ParentXmlIndexName,
				NewIndex.XmlSecondaryType,
				NewIndex.XmlIndexType,
				NewIndex.NameSpaces,
				NewIndex.Paths,
				NewIndex.KeyColumns,
				NewIndex.IncludeColumns
			FROM
				({0}) CurIndex
				INNER JOIN
				({1}) NewIndex

				-- Index matching on Schema + Table + Name
				ON  CurIndex.SchemaName = NewIndex.SchemaName
				AND CurIndex.TableName = NewIndex.TableName
				AND CurIndex.IndexName = NewIndex.IndexName

			WHERE
				(
					CurIndex.IndexType != NewIndex.IndexType
					OR CurIndex.IsUnique != NewIndex.IsUnique
					OR (NewIndex.IndFillFactor > 0 AND CurIndex.IndFillFactor != NewIndex.IndFillFactor)
					OR isnull(CurIndex.XmlSecondaryType, '') != isnull(NewIndex.XmlSecondaryType, '')
					OR isnull(CurIndex.ParentXmlIndexName, '') != isnull(NewIndex.ParentXmlIndexName, '')
					OR CurIndex.IndexFilter != NewIndex.IndexFilter
					OR CurIndex.IndexCompression != NewIndex.IndexCompression
					{2}
				)
			OPTION (USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'))";

		#endregion

		#endregion

		#region Create New Indexes

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- CREATE INDEXES THAT HAVE BEEN ADDED TO THE SCHEMA
		///	--
		///	-- Note 1: Indexes created for UniqueKey (UK) constraints and PrimaryKey (PK) constraints are not in the scope of this class.
		///	--         They're handled in another class.
		///	--
		/// --           Unique Index                  => is_unique != 0
		/// --           Clustered Index               => type = 1
		/// --           PK Constraint Index           => is_primary_key != 0
		/// --           UNIQUE Constraint Index       => is_unique_constraint != 0
		/// --
		///	-- Note 2: HEAP, SPATIAL and COLUMNSTORE indexes are also not in scope here.
		///	--
		/// --         Index TYPE information:
		/// --           0 => Heap - Entry for all tables without a clustered index
		/// --           1 => Clustered
		/// --           2 => Nonclustered
		/// --           3 => XML
		/// --           4 => Spatial
		/// --           5 => Columnstore clustered (handled in another class) 
		/// --           6 => Columnstore nonclustered (handled in another class)
		/// --
		/// -- Note 3: Advanced index options bellow are not considered 
		/// --         STATISTICS_NORECOMPUTE, IGNORE_DUP_KEY, PAD_INDEX
		/// --         (UnitTests enforce there are no indexes with special settings for these options).
		/// --
		/// -- Note 4: PAD_INDEX is not included in our templates,
		/// --         and hence not used to create the new indexes.
		/// --
		/// -- Note 5: Indexes of tables that are not in the new schema are IGNORED
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		public void CreateNewIndexes()
		{
			string scriptWithDbIndexes = String.Format(CultureInfo.InvariantCulture,
				NewIndexInfoSql,
				"{0}",
				GetDbIndexInfoSql(includeColumnListInfo: true, dbStringFormatItemIndex: 1)
			);
			RunSynchroniseIndexCommands(GetScriptReplacingDbNames(scriptWithDbIndexes));
		}

		protected string NewIndexInfoSql = @"
			SELECT
				NewIndex.SchemaName,
				NewIndex.TableName,
				NewIndex.IndexName,
				NewIndex.IndexType,
				cast(0 as bit) AS DropAction,
				cast(1 as bit) AS CreateAction,
				cast(0 as bit) AS IsExistingIndex,
				NewIndex.IsUnique,
				NewIndex.IndFillFactor,
				NewIndex.IndexFilter,
				NewIndex.IndexCompression,
				NewIndex.ParentXmlIndexName,
				NewIndex.XmlSecondaryType,
				NewIndex.XmlIndexType,
				NewIndex.NameSpaces,
				NewIndex.Paths,
				NewIndex.KeyColumns,
				NewIndex.IncludeColumns
			FROM
				(
					SELECT
						sch.name SchemaName,
						tab.name TableName,
						ind.name IndexName
					FROM
						[{0}].sys.tables tab
						INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
						INNER JOIN [{0}].sys.indexes ind ON ind.object_id = tab.object_id
					WHERE
						tab.is_ms_shipped = 0
						AND ind.is_primary_key = 0
						AND ind.is_unique_constraint = 0
						AND ind.type not in (0, 4, 5, 6, 7)
				) CurIndex
				RIGHT OUTER JOIN
				({1}) NewIndex
					-- Index matching on Schema + Table + Name
					ON  CurIndex.SchemaName    = NewIndex.SchemaName
					AND CurIndex.TableName     = NewIndex.TableName
					AND CurIndex.IndexName     = NewIndex.IndexName

			WHERE
				CurIndex.TableName IS NULL
			OPTION (USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'))
			";

		#endregion

		#region On-line indexes

		#region New and modified indexes

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- CREATE NEW / ALTERED INDEXES ON-LINE
		///	--
		///	-- Common rules:
		///	-- 	1. Match indexes by schema, table and index names
		///	-- 	2. Ignore:
		///	-- 		1. Table CLUSTERED changes
		///	-- 			1. Create new
		///	-- 			2. Drop old
		///	-- 			3. Alter existing
		///	-- 			4. Drop old and create new
		///	-- 		2. Unique index (potentially requires transform)
		///	-- 		3. Indexes for PK and UQ constraints (performed by Constraint synchroniser)
		///	-- 		4. Index which contains columns that do not exist in DB being upgraded (new/renamed column)
		///	-- 	3. Match populated columns
		///	-- 		1. Ignore index which contains populated column in filter predicate (it will be dropped by Column synchroniser)
		///	-- 		2. Create new index definition based on populated columns
		///	-- 
		///	-- Candidates to create (NONCLUSTERED indexes only):
		///	-- 	4. Brand new index
		///	-- 	5. Alter index
		///	-- 		1. Alter key_columns list and/or order
		///	-- 			According to our name convention, the altered index name must be different. Therefore it can be treated as brand new index
		///	-- 		2. Alter included_columns list
		///	-- 		3. Alter filter predicate
		///	-- 		4. Alter index options (currently - FILLFACTOR only)
		///	-- 			If FILLFACTOR is specified (between 1..99) in the template DB, it is used. Otherwise the value in the DB being upgraded is preserved
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		public void CreateNewOnLineIndexes()
		{
			taskLogger.StartTask("Create new on-line indexes");

			var runOnline = upgConnection.ServerEdition == DbConnection.SqlServerEdition.EnterpriseDeveloper;
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- CreateNewOnLineIndexes
DECLARE @RunOnline bit = {2};

WITH
	PopulatedColumn AS (
			SELECT
				sch_name = sch.name,
				obj_name = obj.name,
				old_name = old.name,
				new_name = new.name
			FROM
				[{1}].sys.schemas      AS sch
				JOIN [{1}].sys.tables  AS obj ON obj.schema_id = sch.schema_id
				JOIN [{1}].sys.columns AS old ON old.object_id = obj.object_id
				JOIN [{1}].sys.columns AS new ON new.object_id = old.object_id AND new.name LIKE '[_]%[_]' + old.name
			WHERE 1=1
				AND obj.is_ms_shipped = 0
		)
	, MissedColumns AS (
			SELECT
				sch_name     = NewSch.name,
				obj_name     = NewObj.name,
				new_col_name = NewCol.name
			FROM
				[{0}].sys.schemas      AS NewSch
				JOIN [{0}].sys.tables  AS NewObj ON NewObj.schema_id = NewSch.schema_id
				JOIN [{0}].sys.columns AS NewCol ON NewCol.object_id = NewObj.object_id

				JOIN [{1}].sys.schemas      AS CurSch ON 1=1
					AND CurSch.name = NewSch.name
				JOIN [{1}].sys.tables       AS CurObj ON CurObj.schema_id = CurSch.schema_id
					AND CurObj.name = NewObj.name
				LEFT JOIN [{1}].sys.columns AS CurCol ON CurCol.object_id = CurObj.object_id
					AND CurCol.name = NewCol.name
			WHERE 1=1
				AND CurCol.name is NULL
		)
	, TmpObjects AS (
			SELECT
				TmpObjects = '|'

				, sch_name   = NewSch.name
				, obj_name   = NewObj.name
				, new_obj_id = NewObj.object_id
				, cur_obj_id = CurObj.object_id
			FROM
				[{0}].sys.schemas      AS NewSch
				JOIN [{0}].sys.tables  AS NewObj ON NewObj.schema_id = NewSch.schema_id
				JOIN [{0}].sys.indexes AS NewInd ON NewInd.object_id = NewObj.object_id
					AND NewInd.type in (0, 1)

				JOIN [{1}].sys.schemas AS CurSch ON 1=1
					AND CurSch.name = NewSch.name
				JOIN [{1}].sys.tables  AS CurObj ON CurObj.schema_id = CurSch.schema_id
					AND CurObj.name = NewObj.name
				JOIN [{1}].sys.indexes AS CurInd ON CurInd.object_id = CurObj.object_id
					AND CurInd.type in (0, 1)
					-- Exclude Table CLUSTERED changes
					AND CurInd.type = NewInd.type
					AND (CurInd.type = 0 OR CurInd.name = NewInd.name)
			WHERE 1=1
				AND NewObj.is_ms_shipped = 0
				AND CurObj.is_ms_shipped = 0
		)
	, TmpIndexes AS (
			SELECT d.*
				, TmpIndexes = '|'

				, new_ind_id          = NewInd.index_id
				, new_ind_name        = NewInd.name
				, new_ind_type_desc   = CONCAT(''
					, CASE WHEN NewInd.is_unique = 1 THEN 'UNIQUE' ELSE '      ' END
					, CASE WHEN NewInd.type = 2 THEN ' NON' ELSE '    ' END + 'CLUSTERED'
					)
				, new_ind_fill_factor = CASE WHEN CurInd.fill_factor is NULL OR NewInd.fill_factor BETWEEN 1 AND 99 THEN NewInd.fill_factor ELSE CurInd.fill_factor END
				, new_ind_filter      = ISNULL(NewInd.filter_definition, '')
				, new_ind_compression = ISNULL(NewIndPar.data_compression_desc, '')

				, cur_ind_id          = CurInd.index_id
				, cur_ind_name        = CurInd.name
				, cur_ind_fill_factor = CurInd.fill_factor
				, cur_ind_filter      = ISNULL(CurInd.filter_definition, '')
				, cur_ind_compression = ISNULL(CurIndPar.data_compression_desc, '')
			FROM
				TmpObjects                  AS d
				JOIN [{0}].sys.indexes      AS NewInd ON NewInd.object_id = d.new_obj_id
					-- Nonclustered indexes only
					AND NewInd.type = 2
					-- Ignore Unique index (potentially requires transform)
					AND NewInd.is_unique = 0
				LEFT JOIN [{0}].sys.partitions NewIndPar
					ON NewIndPar.object_id = NewInd.object_id
					AND NewIndPar.index_id = NewInd.index_id
					AND NewIndPar.partition_number = 1
				LEFT JOIN
				(
					SELECT
						CurInd.*
					FROM
						[{1}].sys.indexes           AS CurInd
						LEFT JOIN [{1}].sys.indexes AS CreatedInd ON CreatedInd.object_id = CurInd.object_id
							AND CreatedInd.name = @ind_prefix + CurInd.name
					WHERE 1=1
						-- Exclude already created indexes (include/filter/fillfactor)
						AND CreatedInd.name is NULL
				) AS CurInd ON CurInd.object_id = d.cur_obj_id
					AND
					(1=2
						OR CurInd.name = NewInd.name
						-- for checking already created indexes (new/key)
						OR CurInd.name = @ind_prefix + NewInd.name
					)
					-- Nonclustered indexes only
					AND CurInd.type = 2
					-- Ignore Unique index (potentially requires transform)
					AND CurInd.is_unique = 0
				LEFT JOIN [{1}].sys.partitions CurIndPar
					ON CurIndPar.object_id = CurInd.object_id
					AND CurIndPar.index_id = CurInd.index_id
					AND CurIndPar.partition_number = 1
			WHERE 1=2
				OR CurInd.name is NULL
				-- Exclude already created indexes (new/key)
				OR CurInd.name <> @ind_prefix + NewInd.name
		)
	, TmpColumns AS (
			SELECT d.*
				, TmpColumns = '|'

				, new_ind_options =
					CASE
						WHEN new_ind_fill_factor > 0 THEN
							CASE
								WHEN new_ind_compression = '' THEN CONCAT('FILLFACTOR = ', new_ind_fill_factor)
								ELSE CONCAT('FILLFACTOR=', new_ind_fill_factor, ', DATA_COMPRESSION=', new_ind_compression)
							END
						ELSE
							CASE
								WHEN new_ind_compression = '' THEN ''
								ELSE CONCAT('DATA_COMPRESSION=', new_ind_compression)
							END
					END COLLATE database_default

				, new_key_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CAST(QUOTENAME(ISNULL(p.new_name, c.name)) AS nvarchar(max)), ', ')
								WITHIN GROUP (ORDER BY i.key_ordinal)
						FROM
							[{0}].sys.index_columns     AS i
							JOIN [{0}].sys.columns      AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
							-- Match populated columns
							LEFT JOIN PopulatedColumn  AS p ON p.sch_name = d.sch_name AND p.obj_name = d.obj_name AND p.old_name = c.name
						WHERE 1=1
							AND i.object_id = d.new_obj_id
							AND i.index_id = d.new_ind_id
							AND i.is_included_column = 0
					), '')
				, new_included_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CAST(QUOTENAME(ISNULL(p.new_name, c.name)) AS nvarchar(max)), ', ')
								WITHIN GROUP (ORDER BY c.name)
						FROM
							[{0}].sys.index_columns     AS i
							JOIN [{0}].sys.columns      AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
							-- Match populated columns
							LEFT JOIN PopulatedColumn  AS p ON p.sch_name = d.sch_name AND p.obj_name = d.obj_name AND p.old_name = c.name
						WHERE 1=1
							AND i.object_id = d.new_obj_id
							AND i.index_id = d.new_ind_id
							AND i.is_included_column = 1
					), '')
				, cur_key_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CONVERT(nvarchar(4000), QUOTENAME(c.name)), ', ')
								WITHIN GROUP (ORDER BY i.key_ordinal)
						FROM
							[{1}].sys.index_columns AS i
							JOIN [{1}].sys.columns  AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
						WHERE 1=1
							AND i.object_id = d.cur_obj_id
							AND i.index_id = d.cur_ind_id
							AND i.is_included_column = 0
					), '')
				, cur_included_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CONVERT(nvarchar(4000), QUOTENAME(c.name)), ', ')
								WITHIN GROUP (ORDER BY c.name)
						FROM
							[{1}].sys.index_columns AS i
							JOIN [{1}].sys.columns  AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
						WHERE 1=1
							AND i.object_id = d.cur_obj_id
							AND i.index_id = d.cur_ind_id
							AND i.is_included_column = 1
					), '')
			FROM
				TmpIndexes AS d
		)
SELECT
	new_ind_name = CONCAT(QUOTENAME(sch_name), '.', QUOTENAME(obj_name), '.', QUOTENAME(@ind_prefix + new_ind_name))
	, new_ind_short_definition = CONCAT(''
		, QUOTENAME(sch_name), '.', QUOTENAME(obj_name), '.', QUOTENAME(@ind_prefix + new_ind_name)
		, ' (', new_key_columns, ')'
		, CASE WHEN new_included_columns > '' THEN CONCAT(' INCLUDE (', new_included_columns, ')') ELSE '' END
		, CASE WHEN new_ind_filter > '' THEN CONCAT(' WHERE ', new_ind_filter) ELSE '' END
		, CASE WHEN new_ind_options > '' THEN CONCAT(' WITH (', new_ind_options, ')') ELSE '' END
		)
	, new_ind_full_definition = CONCAT(''
		, 'CREATE ', new_ind_type_desc, ' INDEX ', QUOTENAME(@ind_prefix + new_ind_name), ' ON ', QUOTENAME(sch_name), '.', QUOTENAME(obj_name)
		, ' (', new_key_columns, ')'
		, CASE WHEN new_included_columns > '' THEN CONCAT(' INCLUDE (', new_included_columns, ')') ELSE '' END
		, CASE WHEN new_ind_filter > '' THEN CONCAT(' WHERE ', new_ind_filter) ELSE '' END
		-- Index options
		,
			CASE
				WHEN (@RunOnline = 1) THEN
					CONCAT(' WITH (ONLINE=ON', CASE WHEN new_ind_options > '' THEN CONCAT(', ', new_ind_options) ELSE '' END, ')')
				ELSE
					CASE WHEN new_ind_options > '' THEN CONCAT(' WITH (', new_ind_options, ')') ELSE '' END
			END
		, ';'
		)
FROM
	TmpColumns AS d
WHERE 1=1
	AND
	(1=2
		-- Brand new index
		OR cur_ind_name is NULL
		-- Alter key_columns list and/or order
		OR new_key_columns <> cur_key_columns
		-- Alter included_columns list
		OR new_included_columns <> cur_included_columns
		-- Alter filter predicate
		OR new_ind_filter <> cur_ind_filter
		-- Alter index options
		OR new_ind_fill_factor <> cur_ind_fill_factor
		OR new_ind_compression <> cur_ind_compression
	)

	-- Ignore index which contains populated column in filter predicate
	AND
	(1=2
		OR d.new_ind_filter = ''
		OR NOT EXISTS (SELECT NULL FROM PopulatedColumn WHERE sch_name = d.sch_name AND obj_name = d.obj_name AND CHARINDEX(QUOTENAME(old_name), d.new_ind_filter) > 0)
	)

	-- Ignore index which contains columns that do not exist in target (new/renamed column)
	AND NOT EXISTS
		(
			SELECT NULL FROM MissedColumns WHERE sch_name = d.sch_name AND obj_name = d.obj_name
				AND
				(1=2
					OR CHARINDEX(QUOTENAME(new_col_name), d.new_key_columns) > 0
					OR CHARINDEX(QUOTENAME(new_col_name), d.new_included_columns) > 0
					OR CHARINDEX(QUOTENAME(new_col_name), d.new_ind_filter) > 0
				)
		)
OPTION (RECOMPILE);
"
				, templateDb
				, dbBeingUpgraded
				, (runOnline) ? "1" : "0"
				);

			var indexesToCreate = new Dictionary<string, string>();

			using (var cmd = upgConnection.Command(sql))
			{
				cmd.AddParameter("@ind_prefix", SqlDbType.NVarChar, 10, IndexInfo.ONLINE_INDEX_PREFIX);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						indexesToCreate.Add(reader["new_ind_short_definition"].ToString(), reader["new_ind_full_definition"].ToString());
					}
				}
			}

			foreach (var index in indexesToCreate.OrderBy(ind => ind.Key, StringComparer.OrdinalIgnoreCase))
			{
				taskLogger.ShowInfoMessage("    (+) " + index.Key);
				ExecuteNonQuery(index.Value);
			}
		}

		/// <summary>
		///--------------------------------------------------------------------------------------------------------
		///	-- CREATE NEW / ALTERED UNIQUE INDEXES ON-LINE
		///	--
		///	-- Common rules:
		///	--	1. Match indexes by schema, table and index names
		///	-- 	2. Ignore:
		///	-- 		1. Clustered indexes
		///	-- 		2. Indexes for PK and UQ constraints (performed by Constraint synchroniser)
		///	-- 		3. Index which contains columns that do not exist in DB being upgraded (new/renamed column)
		///	-- 	3. Match populated columns
		///	-- 		1. Ignore index which contains populated column in filter predicate (it will be dropped by Column synchroniser)
		///	-- 		2. Create new index definition based on populated columns
		///	-- 
		///	-- Candidates to create (NONCLUSTERED indexes only):
		///	--	4. We can confirm uniqueness of candidate index based on existing indexes:
		///	--		1. Key fields of candidate is superset of one of existing unique index keys (or equal to it)
		///	--		2. Candidate has same of more strict filter (currently from nonfiltered to filtered)
		///	--		3. Columns order and ASC/DESC do not affect uniqueness
		///	-- 	5. Alter index
		///	-- 		1. Alter key_columns list and/or order
		///	-- 			According to our name convention, the altered index name must be different. Therefore, it can be treated as brand-new index
		///	-- 		2. Alter included_columns list
		///	-- 		3. Alter filter predicate to more strict
		///	-- 		4. Alter index options (currently - FILLFACTOR only)
		///	-- 			If FILLFACTOR is specified (between 1..99) in the template DB, it is used. Otherwise, the value in the DB being upgraded is preserved
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		public void CreateUniqueIndexesOnline()
		{
			taskLogger.StartTask("Create new unique nonclustered indexes online");

			var runOnline = upgConnection.ServerEdition == DbConnection.SqlServerEdition.EnterpriseDeveloper;
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- CreateNewOnLineIndexes
WITH
	PopulatedColumn AS (
			SELECT
				new_obj_id = NewObj.object_id,
				new_col_id = NewCol.column_id,
				old_name = old.name,
				new_name = new.name
			FROM
				[{1}].sys.schemas      AS CurSch
				JOIN [{1}].sys.tables  AS CurObj ON CurObj.schema_id = CurSch.schema_id
				JOIN [{1}].sys.columns AS old ON old.object_id = CurObj.object_id
				JOIN [{1}].sys.columns AS new ON new.object_id = old.object_id AND new.name LIKE '[_]%[_]' + old.name

				JOIN [{0}].sys.schemas AS NewSch ON NewSch.name = CurSch.name
				JOIN [{0}].sys.tables  AS NewObj ON NewObj.schema_id = NewSch.schema_id AND NewObj.name = CurObj.name
				JOIN [{0}].sys.columns AS NewCol ON NewCol.object_id = NewObj.object_id AND NewCol.name = old.name
			WHERE 1=1
				AND CurObj.type = 'U'
				AND NewObj.type = 'U'
				AND CurObj.is_ms_shipped = 0
		)
	, MissedColumns AS (
			SELECT
				new_obj_id = NewObj.object_id,
				new_col_id = NewCol.column_id,
				new_col_name = NewCol.name
			FROM
				[{0}].sys.schemas      AS NewSch
				JOIN [{0}].sys.tables  AS NewObj ON NewObj.schema_id = NewSch.schema_id
				JOIN [{0}].sys.columns AS NewCol ON NewCol.object_id = NewObj.object_id

				JOIN [{1}].sys.schemas      AS CurSch ON 1=1
					AND CurSch.name = NewSch.name
				JOIN [{1}].sys.tables       AS CurObj ON CurObj.schema_id = CurSch.schema_id
					AND CurObj.name = NewObj.name
				LEFT JOIN [{1}].sys.columns AS CurCol ON CurCol.object_id = CurObj.object_id
					AND CurCol.name = NewCol.name
			WHERE 1=1
				AND CurCol.name is NULL
		)
	, ClusterIndexChange AS (
		SELECT
			new_schema_id = NewSch.schema_id,
			new_object_id = NewObj.object_id,
			name = NewSch.name,
			obj = NewObj.name
		FROM
			[{0}].sys.schemas      AS NewSch
			JOIN [{0}].sys.objects AS NewObj ON NewSch.schema_id = NewObj.schema_id
			LEFT JOIN [{0}].sys.indexes AS NewInd ON NewInd.object_id = NewObj.object_id AND NewInd.type = 1

			JOIN [{1}].sys.schemas      AS CurSch ON CurSch.name = NewSch.name
			JOIN [{1}].sys.objects AS CurObj ON CurSch.schema_id = CurObj.schema_id AND CurObj.name = NewObj.name
			LEFT JOIN [{1}].sys.indexes AS CurInd ON CurInd.object_id = CurObj.object_id AND CurInd.type = 1
		WHERE 1=1
			AND NewObj.type = 'U'
			AND CurObj.type = 'U'
			AND (CurInd.index_id IS NOT NULL OR NewInd.index_id IS NOT NULL)
			AND (1=2
				OR (CurInd.index_id IS NULL AND NewInd.index_id IS NOT NULL)
				OR (CurInd.index_id IS NOT NULL AND NewInd.index_id IS NULL)
				OR (NOT (1=1
					AND CurInd.name = NewInd.name
					AND CurInd.is_unique = NewInd.is_unique
					AND CurInd.has_filter = NewInd.has_filter
					AND (CurInd.has_filter = 0 OR CurInd.filter_definition = NewInd.filter_definition)
					AND NOT EXISTS(
						SELECT NULL
						FROM
							([{0}].sys.index_columns as NewIndCol
							JOIN [{0}].sys.columns as NewCol ON NewCol.column_id = NewIndCol.column_id AND NewIndCol.index_id = NewInd.index_id AND NewIndCol.object_id = NewObj.object_id AND NewCol.object_id = NewObj.object_id)

							FULL OUTER JOIN
							([{1}].sys.index_columns as CurIndCol
							JOIN [{1}].sys.columns as CurCol ON CurCol.column_id = CurIndCol.column_id AND CurIndCol.index_id = CurInd.index_id AND CurIndCol.object_id = CurObj.object_id AND CurCol.object_id = CurObj.object_id)
							ON 1=1
								AND NewCol.name = CurCol.name
								AND NewIndCol.key_ordinal = CurIndCol.key_ordinal
								AND NewIndCol.is_descending_key = CurIndCol.is_descending_key
								AND NewIndCol.is_included_column = CurIndCol.is_included_column
						WHERE
							NewIndCol.column_id IS NULL OR CurIndCol.column_id IS NULL
						)
					))
				)
	)
	, UniqueIndexes as (
			SELECT 
				new_obj_id = NewObj.object_id,
				new_ind_id = NewInd.index_id
			FROM
				[{0}].sys.schemas      AS NewSch
				JOIN [{0}].sys.objects AS NewObj ON NewSch.schema_id = NewObj.schema_id
				JOIN [{0}].sys.indexes AS NewInd ON NewInd.object_id = NewObj.object_id
				LEFT JOIN ClusterIndexChange  AS CluInd ON CluInd.new_schema_id = NewSch.schema_id AND CluInd.new_object_id = NewObj.object_id
			WHERE 1=1
				AND CluInd.new_object_id IS NULL
				AND NewInd.is_unique = 1
				AND NewInd.is_unique_constraint = 0
				AND NewInd.is_primary_key = 0
				AND NewObj.type = 'U'
				AND NewObj.is_ms_shipped = 0

				-- Ignore index which contains populated column in key columns
				AND NOT EXISTS (
					SELECT NULL
					FROM
						[{0}].sys.index_columns AS IndCol
						JOIN PopulatedColumn as PopCol ON IndCol.object_id = PopCol.new_obj_id AND IndCol.column_id = PopCol.new_col_id
					WHERE 1=1
						AND IndCol.object_id = NewObj.object_id
						AND IndCol.index_id = NewInd.index_id
						AND IndCol.is_included_column = 0
				)

				-- Ignore index which contains populated column in filter predicate
				AND (1=2
					OR NewInd.filter_definition IS NULL
					OR NOT EXISTS (
						SELECT NULL
						FROM PopulatedColumn
						WHERE new_obj_id = NewInd.object_id AND CHARINDEX(QUOTENAME(old_name), NewInd.filter_definition) > 0
					))

				-- Ignore index which contains new/renamed columns in key or included columns
				AND NOT EXISTS (
					SELECT NULL
					FROM
						[{0}].sys.index_columns AS IndCol
						JOIN MissedColumns as MisCol ON IndCol.object_id = MisCol.new_obj_id AND IndCol.column_id = MisCol.new_col_id
					WHERE 1=1
						AND IndCol.object_id = NewObj.object_id
						AND IndCol.index_id = NewInd.index_id
				)

				-- Ignore index which contains new/renamed columns in filter
				AND (1=2
					OR NewInd.filter_definition IS NULL
					OR NOT EXISTS (
						SELECT NULL
						FROM MissedColumns
						WHERE new_obj_id = NewInd.object_id AND CHARINDEX(QUOTENAME(new_col_name), NewInd.filter_definition) > 0
					))

				-- Index uniqueness can be proved by existing unique index
				AND EXISTS(
					SELECT NULL
					FROM
						[{1}].sys.schemas      AS CurSch
						JOIN [{1}].sys.objects AS CurObj ON CurSch.schema_id = CurObj.schema_id
						JOIN [{1}].sys.indexes AS CurInd ON CurObj.object_id = CurInd.object_id
					WHERE 1=1
						AND CurInd.is_unique = 1
						AND CurObj.type = 'U'
						AND CurObj.is_ms_shipped = 0
						AND CurSch.name = NewSch.name
						AND CurObj.name = NewObj.name
						AND (CurInd.filter_definition IS NULL OR CurInd.filter_definition = NewInd.filter_definition)
						AND NOT EXISTS (
							SELECT NULL
							FROM (
								SELECT col_name = c.name
								from
									[{1}].sys.schemas      AS s
									JOIN [{1}].sys.objects AS o ON s.schema_id = o.schema_id
									JOIN [{1}].sys.columns AS c ON o.object_id = c.object_id
									JOIN [{1}].sys.index_columns AS ic ON ic.object_id = o.object_id and ic.column_id = c.column_id
								WHERE 1=1
									AND s.schema_id = CurSch.schema_id
									AND o.object_id = CurObj.object_id
									AND ic.index_id = CurInd.index_id
									AND ic.is_included_column = 0
								) AS CurIndexFields
								LEFT JOIN (
									SELECT col_name = c.name
									FROM
										[{0}].sys.schemas            AS s
										JOIN [{0}].sys.objects       AS o ON s.schema_id = o.schema_id
										JOIN [{0}].sys.columns       AS c ON o.object_id = c.object_id
										JOIN [{0}].sys.index_columns AS ic ON ic.object_id = o.object_id and ic.column_id = c.column_id
									WHERE 1=1
										AND s.schema_id = NewSch.schema_id
										AND o.object_id = NewObj.object_id
										AND ic.index_id = NewInd.index_id
										AND ic.is_included_column = 0
								) AS NewIndexFields ON CurIndexFields.col_name = NewIndexFields.col_name
							WHERE NewIndexFields.col_name IS NULL
						)
				)
		)
	, TmpObjects AS (
			SELECT
				TmpObjects = '|'

				, sch_name   = NewSch.name
				, obj_name   = NewObj.name
				, new_obj_id = NewObj.object_id
				, cur_obj_id = CurObj.object_id
			FROM
				[{0}].sys.schemas      AS NewSch
				JOIN [{0}].sys.objects AS NewObj ON NewObj.schema_id = NewSch.schema_id
				JOIN [{0}].sys.indexes AS NewInd ON NewInd.object_id = NewObj.object_id
					AND NewInd.type in (0, 1)

				JOIN [{1}].sys.schemas AS CurSch ON 1=1
					AND CurSch.name = NewSch.name
				JOIN [{1}].sys.tables  AS CurObj ON CurObj.schema_id = CurSch.schema_id
					AND CurObj.name = NewObj.name
				JOIN [{1}].sys.indexes AS CurInd ON CurInd.object_id = CurObj.object_id
					AND CurInd.type in (0, 1)
					-- Exclude Table CLUSTERED changes
					AND CurInd.type = NewInd.type
					AND (CurInd.type = 0 OR CurInd.name = NewInd.name)
			WHERE 1=1
				AND NewObj.is_ms_shipped = 0
				AND CurObj.is_ms_shipped = 0
				AND NewObj.type = 'U'
				AND CurObj.type = 'U'
		)
	, TmpIndexes AS (
			SELECT d.*
				, TmpIndexes = '|'

				, new_ind_id          = NewInd.index_id
				, new_ind_name        = NewInd.name
				, new_ind_type_desc   = CONCAT(''
					, CASE WHEN NewInd.is_unique = 1 THEN 'UNIQUE' ELSE '      ' END
					, CASE WHEN NewInd.type = 2 THEN ' NON' ELSE '    ' END + 'CLUSTERED'
					)
				, new_ind_fill_factor = CASE WHEN CurInd.fill_factor is NULL OR NewInd.fill_factor BETWEEN 1 AND 99 THEN NewInd.fill_factor ELSE CurInd.fill_factor END
				, new_ind_filter      = ISNULL(NewInd.filter_definition, '')
				, new_ind_compression = ISNULL(NewIndPar.data_compression_desc, '')

				, cur_ind_id          = CurInd.index_id
				, cur_ind_name        = CurInd.name
				, cur_ind_fill_factor = CurInd.fill_factor
				, cur_ind_filter      = ISNULL(CurInd.filter_definition, '')
				, cur_ind_compression = ISNULL(CurIndPar.data_compression_desc, '')
			FROM
				TmpObjects                  AS d
				JOIN UniqueIndexes          AS UniInd ON UniInd.new_obj_id = d.new_obj_id
				JOIN [{0}].sys.indexes      AS NewInd ON NewInd.object_id = d.new_obj_id AND UniInd.new_ind_id = NewInd.index_id
					-- Nonclustered indexes only
					AND NewInd.type = 2
				LEFT JOIN [{0}].sys.partitions AS NewIndPar
					ON NewIndPar.object_id = NewInd.object_id
					AND NewIndPar.index_id = NewInd.index_id
					AND NewIndPar.partition_number = 1
				LEFT JOIN
				(
					SELECT
						CurInd.*
					FROM
						[{1}].sys.indexes           AS CurInd
						LEFT JOIN [{1}].sys.indexes AS CreatedInd ON CreatedInd.object_id = CurInd.object_id
							AND CreatedInd.name = @ind_prefix + CurInd.name
					WHERE 1=1
						-- Exclude already created indexes (include/filter/fillfactor)
						AND CreatedInd.name is NULL
				) AS CurInd ON CurInd.object_id = d.cur_obj_id
					AND
					(1=2
						OR CurInd.name = NewInd.name
						-- for checking already created indexes (new/key)
						OR CurInd.name = @ind_prefix + NewInd.name
					)
					-- Nonclustered indexes only
					AND CurInd.type = 2
				LEFT JOIN [{1}].sys.partitions AS CurIndPar
					ON CurIndPar.object_id = CurInd.object_id
					AND CurIndPar.index_id = CurInd.index_id
					AND CurIndPar.partition_number = 1
			WHERE 1=2
				OR CurInd.name is NULL
				-- Exclude already created indexes (new/key)
				OR CurInd.name <> @ind_prefix + NewInd.name
		)
	, TmpColumns AS (
			SELECT d.*
				, TmpColumns = '|'

				, new_ind_options =
					CASE
						WHEN new_ind_fill_factor > 0 THEN
							CASE
								WHEN new_ind_compression = '' THEN CONCAT('FILLFACTOR = ', new_ind_fill_factor)
								ELSE CONCAT('FILLFACTOR=', new_ind_fill_factor, ', DATA_COMPRESSION=', new_ind_compression)
							END
						ELSE
							CASE
								WHEN new_ind_compression = '' THEN ''
								ELSE CONCAT('DATA_COMPRESSION=', new_ind_compression)
							END
					END COLLATE database_default

				, new_key_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CONVERT(nvarchar(4000), QUOTENAME(c.name)) + CASE WHEN i.is_descending_key = 1 THEN ' DESC' ELSE '' END, ', ')
								WITHIN GROUP (ORDER BY i.key_ordinal)
						FROM
							[{0}].sys.index_columns     AS i
							JOIN [{0}].sys.columns      AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
						WHERE 1=1
							AND i.object_id = d.new_obj_id
							AND i.index_id = d.new_ind_id
							AND i.is_included_column = 0
					), '')
				, new_included_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CONVERT(nvarchar(4000), QUOTENAME(ISNULL(p.new_name, c.name))) + CASE WHEN i.is_descending_key = 1 THEN ' DESC' ELSE '' END, ', ')
								WITHIN GROUP (ORDER BY c.name)
						FROM
							[{0}].sys.index_columns     AS i
							JOIN [{0}].sys.columns      AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
							-- Match populated columns
							LEFT JOIN PopulatedColumn  AS p ON p.new_obj_id = d.new_obj_id AND p.new_col_id = c.column_id
						WHERE 1=1
							AND i.object_id = d.new_obj_id
							AND i.index_id = d.new_ind_id
							AND i.is_included_column = 1
					), '')
				, cur_key_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CONVERT(nvarchar(4000), QUOTENAME(c.name)), ', ')
								WITHIN GROUP (ORDER BY i.key_ordinal)
						FROM
							[{1}].sys.index_columns AS i
							JOIN [{1}].sys.columns  AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
						WHERE 1=1
							AND i.object_id = d.cur_obj_id
							AND i.index_id = d.cur_ind_id
							AND i.is_included_column = 0
					), '')
				, cur_included_columns = ISNULL((
						SELECT
							[text()] = STRING_AGG(CONVERT(nvarchar(4000), QUOTENAME(c.name)), ', ')
								WITHIN GROUP (ORDER BY c.name)
						FROM
							[{1}].sys.index_columns AS i
							JOIN [{1}].sys.columns  AS c ON c.object_id = i.object_id AND c.column_id = i.column_id
						WHERE 1=1
							AND i.object_id = d.cur_obj_id
							AND i.index_id = d.cur_ind_id
							AND i.is_included_column = 1
					), '')
			FROM
				TmpIndexes AS d
		)
SELECT
	new_ind_short_definition = CONCAT(''
		, QUOTENAME(sch_name), '.', QUOTENAME(obj_name), '.', QUOTENAME(@ind_prefix + new_ind_name)
		, ' (', new_key_columns, ')'
		, CASE WHEN new_included_columns > '' THEN CONCAT(' INCLUDE (', new_included_columns, ')') ELSE '' END
		, CASE WHEN new_ind_filter > '' THEN CONCAT(' WHERE ', new_ind_filter) ELSE '' END
		, CASE WHEN new_ind_options > '' THEN CONCAT(' WITH (', new_ind_options, ')') ELSE '' END
		)
	, new_ind_full_definition = CONCAT(''
		, 'CREATE ', new_ind_type_desc, ' INDEX ', QUOTENAME(@ind_prefix + new_ind_name), ' ON ', QUOTENAME(sch_name), '.', QUOTENAME(obj_name)
		, ' (', new_key_columns, ')'
		, CASE WHEN new_included_columns > '' THEN CONCAT(' INCLUDE (', new_included_columns, ')') ELSE '' END
		, CASE WHEN new_ind_filter > '' THEN CONCAT(' WHERE ', new_ind_filter) ELSE '' END
		-- Index options
		,
			CASE
				WHEN (@RunOnline = 1) THEN
					CONCAT(' WITH (ONLINE=ON', CASE WHEN new_ind_options > '' THEN CONCAT(', ', new_ind_options) ELSE '' END, ')')
				ELSE
					CASE WHEN new_ind_options > '' THEN CONCAT(' WITH (', new_ind_options, ')') ELSE '' END
			END
		, ';'
		)
FROM
	TmpColumns AS d
WHERE 1=1
	AND
	(1=2
		-- Brand new index
		OR cur_ind_name is NULL
		-- Alter key_columns list and/or order
		OR new_key_columns <> cur_key_columns
		-- Alter included_columns list
		OR new_included_columns <> cur_included_columns
		-- Alter filter predicate
		OR new_ind_filter <> cur_ind_filter
		-- Alter index options
		OR new_ind_fill_factor <> cur_ind_fill_factor
		OR new_ind_compression <> cur_ind_compression
	)
OPTION (RECOMPILE);
"
				, templateDb
				, dbBeingUpgraded
				);

			var indexesToCreate = new Dictionary<string, string>();

			using (var cmd = upgConnection.Command(sql))
			{
				cmd.AddParameter("@ind_prefix", SqlDbType.NVarChar, 10, IndexInfo.ONLINE_INDEX_PREFIX);
				cmd.AddParameter("@RunOnline", SqlDbType.Bit, runOnline);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						indexesToCreate.Add(reader["new_ind_short_definition"].ToString(), reader["new_ind_full_definition"].ToString());
					}
				}
			}

			foreach (var index in indexesToCreate.OrderBy(ind => ind.Key, StringComparer.OrdinalIgnoreCase))
			{
				taskLogger.ShowInfoMessage("    (+) " + index.Key);
				// Sometimes, the creation of a unique index could fail if the table is heavily updated.
				// from https://learn.microsoft.com/en-us/sql/relational-databases/indexes/guidelines-for-online-index-operations?view=sql-server-ver16 :
				// 
				// "When you create or rebuild a UNIQUE index online, the index builder and a concurrent user transaction
				// might try to insert the same key, therefore violating uniqueness. If a row entered by a user is inserted
				// into the new index (target) before the original row from the source table is moved to the new index,
				// the online index operation fails."
				//
				// If index creation fails during the online phase, we try once again.
				// If it fails again, we postpone it to the offline phase when the database will not be in active use.
				var triesLeft = 2;
				while (triesLeft > 0)
				{
					try
					{
						ExecuteNonQuery(index.Value);
						triesLeft = 0;
					}
					catch (SqlException ex)
					{
						// 1505 - The CREATE UNIQUE INDEX statement terminated because a duplicate key was found for the object name '%.*ls' and the index name '%.*ls'. The duplicate key value is %ls.
						// 21203 - Duplicate rows found in %s. Unique index not created.
						if (ex.Number == 1505 || ex.Number == 21203)
						{
							triesLeft--;
							if (triesLeft > 0)
							{
								taskLogger.ShowInfoMessage("        Retrying to create index due to " + ex.Message);
							}
							else
							{
								taskLogger.ShowInfoMessage("        Skipped due to " + ex.Message);
							}
						}
						else
						{
							triesLeft = 0;
							taskLogger.ShowInfoMessage("        Skipped due to " + ex.Message);
							ErrorReporter.ReportOnce("IndexScriptRunner_CreateUniqueIndexesOnline", ex);
						}
					}
				}
			}
		}

		public void SynchroniseOnlineIndexes()
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- SynchroniseOnlineIndexes
SELECT
	ind_name    = CONCAT(QUOTENAME(Sch.name), '.', QUOTENAME(Obj.name), '.', QUOTENAME(REPLACE(NewInd.name, @ind_prefix, ''))),
	stmt_drop   = CASE WHEN OldInd.name is NULL THEN '' ELSE CONCAT('DROP INDEX ', QUOTENAME(OldInd.name), ' ON ', QUOTENAME(Sch.name), '.', QUOTENAME(Obj.name), ';') END,
	stmt_rename = CONCAT('EXEC sys.sp_rename'
		, ' N''', QUOTENAME(Sch.name), '.', QUOTENAME(Obj.name), '.', QUOTENAME(NewInd.name), ''''
		, ', ''', REPLACE(NewInd.name, @ind_prefix, ''), ''''
		, ', ''INDEX'''
		, ';')
FROM
	sys.schemas      AS Sch
	JOIN sys.tables  AS Obj ON Obj.schema_id = Sch.schema_id

	JOIN sys.indexes      AS NewInd ON NewInd.object_id = Obj.object_id
	LEFT JOIN sys.indexes AS OldInd ON OldInd.object_id = NewInd.object_id
		AND @ind_prefix + OldInd.name = NewInd.name
WHERE 1=1
	AND NewInd.name LIKE REPLACE(@ind_prefix, '_', '[_]') + '%'
OPTION (RECOMPILE)
"
				);

			var indexesToUpdate = new Dictionary<string, Tuple<string, string>>();

			using (var cmd = upgConnection.Command(sql))
			{
				cmd.AddParameter("@ind_prefix", SqlDbType.NVarChar, 10, IndexInfo.ONLINE_INDEX_PREFIX);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var ind_name = (string)reader["ind_name"];
						var stmt_drop = (string)reader["stmt_drop"];
						var stmt_rename = (string)reader["stmt_rename"];

						indexesToUpdate.Add(ind_name, new Tuple<string, string>(stmt_drop, stmt_rename));
					}
				}
			}

			foreach (var index in indexesToUpdate.OrderBy(ind => ind.Key, StringComparer.OrdinalIgnoreCase))
			{
				taskLogger.ShowInfoMessage("\t    (~) " + index.Key);

				if (!String.IsNullOrWhiteSpace(index.Value.Item1))
				{
					// drop old index
					ExecuteNonQuery(index.Value.Item1);
				}

				// rename new index
				ExecuteNonQuery(index.Value.Item2);
			}
		}

		#endregion // New and modified indexes

		#endregion // On-line indexes

		#region Index Options

		public void SynchroniseIndexOptions()
		{
			var sql = FormattableString.Invariant(@$" -- SynchroniseIndexOptions
SELECT 
	schema_name      = NewSch.name,
	object_name      = NewObj.name,
	index_name       = NewInd.name,
	allow_row_locks  = NewInd.allow_row_locks,
	allow_page_locks = NewInd.allow_page_locks
FROM
	     [{templateDb}].sys.indexes      AS NewInd
	JOIN [{templateDb}].sys.objects      AS NewObj ON NewObj.object_id = NewInd.object_id
	JOIN [{templateDb}].sys.schemas      AS NewSch ON NewSch.schema_id = NewObj.schema_id
	JOIN [{dbBeingUpgraded}].sys.indexes AS CurInd ON CurInd.name      = NewInd.name
	JOIN [{dbBeingUpgraded}].sys.objects AS CurObj ON CurObj.object_id = CurInd.object_id AND CurObj.name = NewObj.name
	JOIN [{dbBeingUpgraded}].sys.schemas AS CurSch ON CurSch.schema_id = CurObj.schema_id AND CurSch.name = NewSch.name
WHERE 1 = 1
	AND NewObj.is_ms_shipped = 0
	AND NewObj.type = 'U'
	AND CurObj.is_ms_shipped = 0
	AND CurObj.type = 'U'
	AND NewSch.name NOT IN ({string.Join(", ", Db.SqlReservedSchemas.Select(schema => FormattableString.Invariant($"N{schema.QuoteName('\'')}")))})
	AND NewInd.index_id > 0
	AND CurInd.is_disabled = 0
	AND 
	( 1 = 0		
		OR NULLIF(NewInd.allow_row_locks, CurInd.allow_row_locks) IS NOT NULL
		OR NULLIF(NewInd.allow_page_locks, CurInd.allow_page_locks) IS NOT NULL
	)");

			var alterIndexes = new List<IndexInfo>();
			upgConnection.ExecuteReader(sql, reader =>
			{
				var schemaName = (string)reader["schema_name"];
				var objectName = (string)reader["object_name"];
				var indexName = (string)reader["index_name"];
				var allowRowLocks = (bool)reader["allow_row_locks"];
				var allowPageLocks = (bool)reader["allow_page_locks"];

				var index = IndexInfo.Builder
					.New(schemaName, objectName, indexName)
					.Key("PK") // Key is not important for this operation
					.Option(IndexOptions.ALLOW_PAGE_LOCKS, allowPageLocks)
					.Option(IndexOptions.ALLOW_ROW_LOCKS, allowRowLocks)
					.GetInfo();

				alterIndexes.Add(index);
			});

			foreach (var index in alterIndexes.OrderBy(i => i.ComparableDefinition))
			{
				var alterCommand = index.SQL_SET;
				taskLogger.ShowInfoMessage($"    (~) {alterCommand}");
				upgConnection.ExecuteNonQuery(alterCommand);
			}
		}

		#endregion

		#region Run Synchronise Index Commands

		string GetDbIndexInfoSql(bool includeColumnListInfo, int dbStringFormatItemIndex)
		{
			const string SelectIndexInfoSql = @"
				SELECT
					sch.[name]                            AS SchemaName,
					tab.[name]                            AS TableName,
					ind.[name]                            AS IndexName,
					ind.[type]                            AS IndexType,
					ind.is_unique                         AS IsUnique,
					CASE ind.fill_factor
						WHEN 100 THEN 0
						ELSE ind.fill_factor
					END                                   AS IndFillFactor,
					isnull(ind.filter_definition, '')     AS IndexFilter,
					isnull(par.data_compression_desc, '') AS IndexCompression,
					parentxmlind.name                     AS ParentXmlIndexName,
					xmlind.secondary_type_desc            AS XmlSecondaryType,
					xmlind.object_id                      AS ObjectId,
					xmlind.index_id                       AS IndexId,
					xmlind.xml_index_type                 AS XmlIndexType,
					(
						SELECT STRING_AGG(CAST(CONCAT('''', uri, ''' AS ', prefix) AS nvarchar(max)), ', ')
							WITHIN GROUP (ORDER BY prefix) AS [text()]
						FROM [(=DB=)].sys.selective_xml_index_namespaces xin
						WHERE xmlind.object_id = xin.object_id AND xmlind.index_id = xin.index_id
					) AS NameSpaces,
					(
						SELECT STRING_AGG(CAST(CONCAT(xip.name, ' = ''', path ,'''' ,
								CASE WHEN path_type = 1 
									THEN CONCAT(' AS SQL ', UPPER(types.name), '(' , xip.max_length , ')', CASE WHEN is_singleton = 1 THEN ' SINGLETON' ELSE '' END)
								ELSE 
									CASE WHEN is_node = 1 THEN ' AS XQUERY ''node()''' ELSE '' END
								END) AS nvarchar(max)) COLLATE DATABASE_DEFAULT, ', ')
							WITHIN GROUP (ORDER BY path_id) AS [text()]
						FROM [(=DB=)].sys.selective_xml_index_paths xip
						LEFT JOIN [(=DB=)].sys.types
							ON xip.user_type_id = types.user_type_id
						WHERE xmlind.object_id = xip.object_id AND xmlind.index_id = xip.index_id
					) AS Paths
					{0}
				FROM
					[(=DB=)].sys.objects tab
					INNER JOIN [(=DB=)].sys.schemas sch
						ON sch.schema_id = tab.schema_id
					INNER JOIN [(=DB=)].sys.indexes ind
						ON ind.object_id = tab.object_id
					LEFT JOIN [(=DB=)].sys.partitions par
						ON par.object_id = ind.object_id
						AND par.index_id = ind.index_id
						AND par.partition_number = 1
						AND ind.type in (1, 2)
					LEFT JOIN [(=DB=)].sys.xml_indexes xmlind
						ON xmlind.object_id = ind.object_id
						AND xmlind.index_id = ind.index_id
					LEFT JOIN [(=DB=)].sys.xml_indexes parentxmlind
						ON parentxmlind.object_id = xmlind.object_id
						AND parentxmlind.index_id = xmlind.using_xml_index_id
				WHERE
					tab.is_ms_shipped = 0
					AND tab.type = 'U'
					AND ind.is_primary_key = 0
					AND ind.is_unique_constraint = 0
					AND ind.type not in (0, 4, 5, 6, 7)
				";

			const string ColumnListSubquery = @"
				, (
					SELECT STRING_AGG(CAST(col.name AS nvarchar(max)), ',')
						WITHIN GROUP (ORDER BY ikey.key_ordinal, col.name)
						AS [text()]
					FROM
						[(=DB=)].sys.index_columns ikey
						INNER JOIN [(=DB=)].sys.columns col
							ON col.object_id = ikey.object_id
							AND col.column_id = ikey.column_id
					WHERE
						ikey.object_id = tab.object_id
						AND ikey.index_id = ind.index_id
						AND ikey.partition_ordinal = 0
						AND ikey.is_included_column = {0}
					) AS {1}";
			string keyColumnListSubquery = String.Format(ColumnListSubquery, "0", "KeyColumns");
			string includeColumnListSubquery = String.Format(ColumnListSubquery, "1", "IncludeColumns");

			string result = String.Format(
				SelectIndexInfoSql,
				(includeColumnListInfo) ? (keyColumnListSubquery + includeColumnListSubquery) : ""
			).Replace("(=DB=)", "{" + dbStringFormatItemIndex.ToString() + "}");

			return result;
		}

		void RunSynchroniseIndexCommands(string newIndexInfoSql)
		{
			var dropFkActionList = new List<Action>();
			var sqlCommands = new SortedList<int, KeyValuePair<string, string>>(new AllowDuplicateComparer());

			using (var reader = upgConnection.Command(newIndexInfoSql).ExecuteReader())
			{
				while (reader.Read())
				{
					//
					// Read returned metadata values
					bool isExistingIndex = (bool)reader["IsExistingIndex"];
					bool dropAction = (bool)reader["DropAction"];
					bool createAction = (bool)reader["CreateAction"];

					string schemaName = reader["SchemaName"].ToString();
					string tableName = reader["TableName"].ToString();
					string indexName = reader["IndexName"].ToString();
					int indexType = Convert.ToInt32(reader["IndexType"]);

					string parentXmlIndexName = GetStringFromNullableDbValue(reader["ParentXmlIndexName"]);
					string xmlSecondaryType = GetStringFromNullableDbValue(reader["XmlSecondaryType"]);
					var xmlIndexType = GetStringFromNullableDbValue(reader["XmlIndexType"]);
					var nameSpaces = GetStringFromNullableDbValue(reader["NameSpaces"]);
					var paths = GetStringFromNullableDbValue(reader["Paths"]);

					//
					// If dropping existing index...
					if (isExistingIndex && dropAction)
					{
						// Add to drop FK reference action list
						dropFkActionList.Add(() => DropReferencingFks(schemaName, tableName, indexName));

						// Assemble DROP command
						string sqlDropCommand = String.Format("DROP INDEX [{0}] ON [{1}].[{2}];", indexName, schemaName, tableName);
						string dropInfoMessage = String.Format("(-) [{0}].[{1}].[{2}]", schemaName, tableName, indexName);

						// XML, then other types
						int dropOrder =
							(indexType == 3) ?
								(xmlIndexType == "0" ? 2 : 1) :
								((indexType == 1) ? 4 : 3);
						sqlCommands.Add(dropOrder, new KeyValuePair<string, string>(dropInfoMessage, sqlDropCommand));
					}

					//
					// If creating or re-creating index...
					if (createAction)
					{
						// Read more returned metadata values
						bool isUnique = (bool)reader["IsUnique"];
						int fillFactor = Convert.ToInt32(reader["IndFillFactor"]);
						string indexFilter = reader["IndexFilter"].ToString();
						string indexCompression = reader["IndexCompression"].ToString();

						string keyColumns = reader["KeyColumns"].ToString();
						string includeColumns = reader["IncludeColumns"].ToString();

						//
						// Assemble CREATE command
						string uniqueClause = (isUnique ? "UNIQUE" : "");

						string typeClause =
							(indexType == 1) ?
								"CLUSTERED" :
								((indexType == 3) ?
									(xmlIndexType == "2" ? "SELECTIVE XML" : (xmlIndexType == "1" ? "XML" : "PRIMARY XML")) :
									"NONCLUSTERED"
								);

						string includeColumnsOrSecondaryXmlClause =
							(indexType == 3 && !String.IsNullOrEmpty(xmlSecondaryType)) ?
								String.Format("USING XML INDEX [{0}] FOR {1}", parentXmlIndexName, xmlSecondaryType) :
								(String.IsNullOrEmpty(includeColumns) ? "" : String.Format("INCLUDE ({0})", includeColumns));

						if (indexType == 3 && xmlIndexType == "2")
						{
							if (!string.IsNullOrEmpty(nameSpaces))
							{
								includeColumnsOrSecondaryXmlClause += FormattableString.Invariant($@"
WITH XMLNAMESPACES
(
{nameSpaces}
)");
							}

							if (!string.IsNullOrEmpty(paths))
							{
								includeColumnsOrSecondaryXmlClause += FormattableString.Invariant($@"
FOR
(
{paths}
)");
							}
						}

						string filterClause = String.IsNullOrEmpty(indexFilter) ? "" : ("WHERE " + indexFilter);

						string withClause = ComposeWithClause((isExistingIndex && !dropAction), fillFactor, indexCompression);

						string sqlCreateCommand = String.Format(
							"CREATE {0} {1} INDEX [{2}] ON [{3}].[{4}] ({5}) {6} {7} {8};",
							uniqueClause,
							typeClause,
							indexName,
							schemaName,
							tableName,
							keyColumns,
							includeColumnsOrSecondaryXmlClause,
							filterClause,
							withClause
						);

						string createInfoMessage = String.Format("({0}) [{1}].[{2}]", ((isExistingIndex && !dropAction) ? "~" : "+"), tableName, indexName);

						// CLUSTERED, then PRIMARY XML, then other types
						int createOrder = (indexType == 1) ? 11 : ((indexType == 3 && xmlIndexType == "0") ? 12 : 13);
						sqlCommands.Add(createOrder, new KeyValuePair<string, string>(createInfoMessage, sqlCreateCommand));
					}
				}
			}

			// Drop Referencing FKs
			foreach (var dropRefFkAction in dropFkActionList)
			{
				dropRefFkAction();
			}

			new BatchRunner(taskLogger).RunCollectionOfSqlCommands(upgConnection, sqlCommands.Values);
		}

		class AllowDuplicateComparer : IComparer<int>
		{
			int IComparer<int>.Compare(int x, int y)
			{
				int comp = x.CompareTo(y);
				return (comp == 0) ? 1 : comp;
			}
		}

		string GetStringFromNullableDbValue(object dbValue)
		{
			return (dbValue == null || dbValue == DBNull.Value) ? null : dbValue.ToString();
		}

		string ComposeWithClause(bool dropExistingOption, int fillFactor, string indexCompression)
		{
			var withClause = new StringBuilder();

			if (dropExistingOption)
			{
				withClause.Append("DROP_EXISTING=ON");
			}

			if (fillFactor > 0)
			{
				if (withClause.Length > 0)
				{
					withClause.Append(",");
				}

				withClause.Append(String.Format(CultureInfo.InvariantCulture, "FILLFACTOR={0}", fillFactor));
			}

			if (!String.IsNullOrWhiteSpace(indexCompression))
			{
				if (withClause.Length > 0)
				{
					withClause.Append(",");
				}

				withClause.Append(String.Format(CultureInfo.InvariantCulture, "DATA_COMPRESSION={0}", indexCompression));
			}

			if (withClause.Length > 0)
			{
				withClause.Insert(0, "WITH (").Append(")");
			}

			return withClause.ToString();
		}

		/// <summary>
		/// Drop FKs which reference an index, before it can be dropped.
		/// </summary>
		void DropReferencingFks(string referencedSchema, string referencedTable, string referencedIndex)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				SELECT 'ALTER TABLE [{0}].[' + FkTabSchema.name + '].[' + FkTab.name + '] DROP CONSTRAINT [' + FkConst.name + '];'
				FROM [{0}].sys.foreign_keys FkConst
				INNER JOIN [{0}].sys.tables FkTab ON FkTab.object_id = FkConst.parent_object_id
				INNER JOIN [{0}].sys.schemas FkTabSchema ON FkTabSchema.schema_id = FkTab.schema_id
				INNER JOIN [{0}].sys.tables ParentTab ON ParentTab.object_id = FkConst.referenced_object_id
				INNER JOIN [{0}].sys.schemas ParentTabSchema ON ParentTabSchema.schema_id = ParentTab.schema_id
				INNER JOIN [{0}].sys.indexes ParentIndex ON ParentIndex.object_id = ParentTab.object_id AND ParentIndex.index_id = FkConst.key_index_id
				WHERE ParentTabSchema.name = @SchemaName
				AND ParentTab.name = @TableName
				AND ParentIndex.name = @IndexName",
				dbBeingUpgraded);

			using (var cmd = upgConnection.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, referencedSchema);
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, referencedTable);
				cmd.AddParameter("@IndexName", SqlDbType.NVarChar, referencedIndex);
				new BatchRunner().RunCommandsGeneratedByCommand(cmd);
			}
		}

		#endregion
	}
}
