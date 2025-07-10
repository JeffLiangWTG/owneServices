using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Schema
{
	class ConstraintScriptRunner : MetadataScriptRunner
	{
		public ConstraintScriptRunner(DbConnection upgConnection, string dbBeingUpgraded, string templateDb, IUpgradeTaskWorkflowLogger taskLogger = null)
			: base(upgConnection, dbBeingUpgraded, templateDb, taskLogger)
		{
		}

		#region SynchroniseAllCheckConstraints

		/// <summary>
		/// -------------------------------------------------------------------------------------------------
		/// -- GENERATE SCRIPTS TO DROP old CHECK Constraints
		/// --
		/// -- Note: Tags {0} and {1} are replaced by the actual
		/// --       DbBeingUpgraded and TemplateDb names at run time
		/// -------------------------------------------------------------------------------------------------
		/// </summary>
		public void DropCheckConstraints()
		{
			var sql = @"-- ConstraintScriptRunner.DropCheckConstraints
SELECT
	CommandText = 'ALTER TABLE [' + CurSch.name + '].[' + CurTab.name + '] DROP CONSTRAINT [' + CurChk.name + '];'
FROM
	[{0}].sys.schemas                AS CurSch
	JOIN [{0}].sys.tables            AS CurTab ON CurTab.schema_id = CurSch.schema_id
	JOIN [{0}].sys.check_constraints AS CurChk ON CurChk.parent_object_id = CurTab.object_id

	JOIN [{1}].sys.schemas                AS NewSch ON 1=1
		AND NewSch.name = CurSch.name
	JOIN [{1}].sys.tables                 AS NewTab ON NewTab.schema_id = NewSch.schema_id
		AND NewTab.name = CurTab.name
	LEFT JOIN [{1}].sys.check_constraints AS NewChk ON NewChk.parent_object_id = NewTab.object_id
		AND NewChk.name = CurChk.name
		AND NewChk.definition = CurChk.definition
WHERE 1=1
	-- Exclude system tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Constraints to drop only
	AND NewChk.name is NULL
ORDER BY
	CurSch.name, CurTab.name
OPTION (RECOMPILE);

";

			new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, GetScriptReplacingDbNames(sql));
		}

		/// <summary>
		/// -------------------------------------------------------------------------------------------------
		/// -- GENERATE SCRIPTS TO CREATE new CHECK Constraints
		/// --
		/// -- Note: Tags {0} and {1} are replaced by the actual
		/// --       DbBeingUpgraded and TemplateDb names at run time
		/// -------------------------------------------------------------------------------------------------
		/// </summary>
		public void CreateCheckConstraints()
		{
			var sql = $@"-- ConstraintScriptRunner.CreateCheckConstraints
SELECT
	CommandText = CONCAT(N''
		, N'ALTER TABLE ', QUOTENAME(NewSch.name), N'.', QUOTENAME(NewTab.name)
		, N' WITH ' , CheckNoCheck, N' ADD'
		, NCHAR(13), NCHAR(10)
		, STRING_AGG(
			CONCAT(N''
				, N'CONSTRAINT ', QUOTENAME(NewChk.name)
				, N' CHECK ', NewChk.definition
				)
			, N',' + NCHAR(13) + NCHAR(10)
			) WITHIN GROUP (ORDER BY NewChk.name)
		),
	LogText = CONCAT(N''
		, N'table ', QUOTENAME(NewSch.name), N'.', QUOTENAME(NewTab.name), N' (with ', CheckNoCheck, N')'
		, NCHAR(13), NCHAR(10)
		, STRING_AGG(
			CONCAT(N''
				, N'    (+) ', QUOTENAME(NewChk.name)
				, N' CHECK ', NewChk.definition
				)
			, NCHAR(13) + NCHAR(10)
			) WITHIN GROUP (ORDER BY NewChk.name)
		)
FROM
	[{templateDb}].sys.schemas                AS NewSch
	JOIN [{templateDb}].sys.objects           AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN [{templateDb}].sys.check_constraints AS NewChk ON NewChk.parent_object_id = NewTab.object_id
	CROSS APPLY
	(
		SELECT CheckNoCheck = IIF(NewChk.name LIKE N'%NoCheck', N'NOCHECK', N'CHECK')
	) AS CA

	JOIN [{dbBeingUpgraded}].sys.schemas                AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN [{dbBeingUpgraded}].sys.objects                AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	LEFT JOIN [{dbBeingUpgraded}].sys.check_constraints AS CurChk ON CurChk.parent_object_id = CurTab.object_id
		AND CurChk.name = NewChk.name
		AND CurChk.definition = NewChk.definition
WHERE 1=1
	AND NewTab.type = 'U'
	AND CurTab.type = 'U'

	-- Exclude system tables
	AND NewTab.is_ms_shipped = 0
	AND CurTab.is_ms_shipped = 0

	-- Constraints to create only
	AND CurChk.name is NULL
GROUP BY
	NewSch.name, NewTab.name, CheckNoCheck
ORDER BY
	NewSch.name, NewTab.name, CheckNoCheck

OPTION (RECOMPILE);";

			new BatchRunner(taskLogger).RunCommandsGeneratedByQuery(upgConnection, sql);
		}
		#endregion

		#region SynchroniseAllDefaultConstraints

		/// <summary>
		/// -------------------------------------------------------------------------------------------------
		/// -- GENERATE SCRIPTS TO DROP old DEFAULT Constraints AND CREATE new ones
		/// --
		/// -- Note: Tags {0} and {1} are replaced by the actual
		/// --       DbBeingUpgraded and TemplateDb names at run time
		/// -------------------------------------------------------------------------------------------------
		/// </summary>
		public void SynchroniseAllDefaultConstraints()
		{
			new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, GetScriptReplacingDbNames(synchroniseAllDefaultsSqlText));
		}

		const string synchroniseAllDefaultsSqlText = @"
			SELECT ISNULL(
				-- ALTER TABLE [SCHEMA].[TABLE] ADD CONSTRAINT [DF_TT_CC] DEFAULT ('') FOR [COLUMN]
				'ALTER TABLE [' + NewDef.SchName + '].[' + NewDef.TabName + '] ADD CONSTRAINT [DF_' + NewDef.TabName + '_' + NewDef.ColName + ']' +
				' DEFAULT ' + NewDef.DefValue + ' FOR [' + NewDef.ColName + ']',
				-- ALTER TABLE [SCHEMA].[TABLE] DROP CONSTRAINT [DF_DDD]
				'ALTER TABLE [' + CurDef.SchName + '].[' + CurDef.TabName + '] DROP CONSTRAINT [' + CurDef.DefName + ']') CommandText
			FROM
				-- Main Database DEFAULTs - View (derived table)
				(
					SELECT
						sch.name SchName, tab.name TabName, def.name DefName, col.name ColName, def.definition DefValue
					FROM
						[{0}].sys.default_constraints def
						INNER JOIN [{0}].sys.tables tab
							ON tab.object_id = def.parent_object_id
						INNER JOIN [{0}].sys.schemas sch
							ON sch.schema_id = tab.schema_id
						INNER JOIN [{0}].sys.columns col
							ON col.object_id = def.parent_object_id AND col.column_id = def.parent_column_id
					WHERE
						tab.is_ms_shipped = 0
						AND def.is_ms_shipped = 0
						-- Ignore TABLES that are not in the schema (unmanaged)
						AND EXISTS (
							SELECT newtab.name
							FROM [{1}].sys.tables newtab
							INNER JOIN [{1}].sys.schemas newsch ON newsch.schema_id = newtab.schema_id
							WHERE newsch.name = sch.name
							AND newtab.name = tab.name)
				) CurDef
				FULL OUTER JOIN
				-- Template Database DEFAULTs - View (derived table)
				(
					SELECT
						sch.name SchName, tab.name TabName, def.name DefName, col.name ColName, def.definition DefValue
					FROM
						[{1}].sys.default_constraints def
						INNER JOIN [{1}].sys.tables tab
							ON tab.object_id = def.parent_object_id
						INNER JOIN [{1}].sys.schemas sch
							ON sch.schema_id = tab.schema_id
						INNER JOIN [{1}].sys.columns col
							ON col.object_id = def.parent_object_id AND col.column_id = def.parent_column_id
					WHERE
						tab.is_ms_shipped = 0
						AND def.is_ms_shipped = 0
				) NewDef
				-- DEFAULTs match if Schemas, Tables, Columns and DefaultValues match
				ON CurDef.SchName = NewDef.SchName
				AND CurDef.TabName = NewDef.TabName
				AND CurDef.ColName = NewDef.ColName
				AND CurDef.DefValue = NewDef.DefValue

			WHERE
				-- Only unmatched DEFAULTs
				(CurDef.TabName IS NULL OR NewDef.TabName IS NULL)

			ORDER BY
				-- First the Drops
				NewDef.TabName
			OPTION (USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'))
			";

		#endregion

		#region SynchroniseAllForeignKeys

		/// <summary>
		/// -------------------------------------------------------------------------------------------------
		/// -- GENERATE SCRIPTS TO DROP old FKs AND CREATE new ones
		/// --
		/// -- Note:
		/// --   1. Tags {0} and {1} are replaced by the actual
		/// --      DbBeingUpgraded and TemplateDb names at run time
		/// --   2. Assumes that no PK (and therefore no FK) is composite (they all have 1 single column)
		/// --      (UnitTest enforced)
		/// -------------------------------------------------------------------------------------------------
		/// </summary>
		public void SynchroniseAllForeignKeys()
		{
			new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, GetScriptReplacingDbNames(synchroniseAllForeignKeysSqlText));
		}

		const string synchroniseAllForeignKeysSqlText = @"
			SELECT ISNULL(
				-- ALTER TABLE [TABLE] ADD CONSTRAINT [FK_XX] FOREIGN KEY (RerColList) REFERENCES (ReeColList)
				'ALTER TABLE [' + NewFKs.fksch + '].[' + NewFKs.fktab + '] ADD CONSTRAINT ' + NewFKs.name +
				' FOREIGN KEY (' + NewFKs.fkey1 +
				') REFERENCES [' + NewFKs.rksch + '].[' + NewFKs.rktab + '] (' + NewFKs.rkey1 + ')' +
				CASE NewFKs.UpdAction WHEN 1 THEN ' ON UPDATE CASCADE' ELSE '' END +
				CASE NewFKs.DelAction WHEN 1 THEN ' ON DELETE CASCADE' ELSE '' END,
				-- ALTER TABLE [TABLE] DROP CONSTRAINT [FK_XX]
				'ALTER TABLE [' + CurFKs.fksch + '].[' + CurFKs.fktab + '] DROP CONSTRAINT ' + CurFKs.name) CommandText

			FROM
				-- Current Database FKs - View (derived table)
				(SELECT
					cnst.name,
					-- Cascade Actions
					cnst.update_referential_action UpdAction,
					cnst.delete_referential_action DelAction,
					-- Schemas
					fkSch.name fksch,
					rkSch.name rksch,
					-- Tables
					fkTab.name fktab,
					rkTab.name rktab,
					-- Columns
					(
						SELECT STRING_AGG(CAST(fkCol1.name AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY cnstCol1.constraint_column_id)
						FROM [{0}].sys.foreign_key_columns cnstCol1
						INNER JOIN [{0}].sys.columns fkCol1
							ON fkCol1.object_id = cnstCol1.parent_object_id
							AND fkCol1.column_id = cnstCol1.parent_column_id
						WHERE cnstCol1.constraint_object_id = cnst.object_id
					) as fkey1,
					(
						SELECT STRING_AGG(CAST(rkCol1.name AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY cnstCol1.constraint_column_id)
						FROM [{0}].sys.foreign_key_columns cnstCol1
						INNER JOIN [{0}].sys.columns rkCol1
							ON rkCol1.object_id = cnstCol1.referenced_object_id
							AND rkCol1.column_id = cnstCol1.referenced_column_id
						WHERE cnstCol1.constraint_object_id = cnst.object_id
					) as rkey1
				FROM 
					[{0}].sys.foreign_keys cnst
					INNER JOIN [{0}].sys.objects fkTab
						ON fkTab.object_id = cnst.parent_object_id
					INNER JOIN [{0}].sys.schemas fkSch
						ON fkSch.schema_id = fkTab.schema_id
					INNER JOIN [{0}].sys.objects rkTab
						ON rkTab.object_id = cnst.referenced_object_id
					INNER JOIN [{0}].sys.schemas rkSch
						ON rkSch.schema_id = rkTab.schema_id
				WHERE
					fkTab.is_ms_shipped = 0
					AND cnst.is_ms_shipped = 0
				) CurFKs
				FULL OUTER JOIN
				-- Template Database FKs - View (derived table)
				(SELECT
					cnst.name,
					-- Cascade Actions
					cnst.update_referential_action UpdAction,
					cnst.delete_referential_action DelAction,
					-- Schemas
					fkSch.name fksch,
					rkSch.name rksch,
					-- Tables
					fkTab.name fktab,
					rkTab.name rktab,
					-- Columns
					(
						SELECT STRING_AGG(CAST(fkCol1.name AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY cnstCol1.constraint_column_id)
						FROM [{1}].sys.foreign_key_columns cnstCol1
						INNER JOIN [{1}].sys.columns fkCol1
							ON fkCol1.object_id = cnstCol1.parent_object_id
							AND fkCol1.column_id = cnstCol1.parent_column_id
						WHERE cnstCol1.constraint_object_id = cnst.object_id
					) as fkey1,
					(
						SELECT STRING_AGG(CAST(rkCol1.name as nvarchar(max)), ',') WITHIN GROUP (ORDER BY cnstCol1.constraint_column_id)
						FROM [{1}].sys.foreign_key_columns cnstCol1
						INNER JOIN [{1}].sys.columns rkCol1
							ON rkCol1.object_id = cnstCol1.referenced_object_id
							AND rkCol1.column_id = cnstCol1.referenced_column_id
						WHERE cnstCol1.constraint_object_id = cnst.object_id
					) as rkey1
				FROM 
					[{1}].sys.foreign_keys cnst
					INNER JOIN [{1}].sys.objects fkTab
						ON fkTab.object_id = cnst.parent_object_id
					INNER JOIN [{1}].sys.schemas fkSch
						ON fkSch.schema_id = fkTab.schema_id
					INNER JOIN [{1}].sys.objects rkTab
						ON rkTab.object_id = cnst.referenced_object_id
					INNER JOIN [{1}].sys.schemas rkSch
						ON rkSch.schema_id = rkTab.schema_id
				WHERE
					fkTab.is_ms_shipped = 0
					AND cnst.is_ms_shipped = 0
				) NewFKs
				-- FKs match if Name, CascadeActions, Tables and KeyColumns match
				ON  CurFKs.name = NewFKs.name
				AND CurFKs.UpdAction = NewFKs.UpdAction
				AND CurFKs.DelAction = NewFKs.DelAction
				AND CurFKs.fksch = NewFKs.fksch
				AND CurFKs.fktab = NewFKs.fktab
				AND CurFKs.rksch = NewFKs.rksch
				AND CurFKs.rktab = NewFKs.rktab
				AND CurFKs.fkey1 = NewFKs.fkey1
				AND CurFKs.rkey1 = NewFKs.rkey1

			WHERE
				-- Only not matching FKs
				(CurFKs.name IS NULL OR NewFKs.name IS NULL)

			ORDER BY
				-- First the Drops
				NewFKs.name
			OPTION (USE HINT ('FORCE_LEGACY_CARDINALITY_ESTIMATION'))
			";

		#endregion

		#region EnsureAllConstraintsAreEnabledAndTrusted

		/// <summary>
		/// -------------------------------------------------------------------------------------------------
		/// -- GENERATE SCRIPTS TO CHECK (WITH CHECK) ANY DISABLED OR NOT TRUSTED CONSTRAINTS
		/// --
		/// -- Note:
		/// --   1. Tags {0} is replaced by the DbBeingUpgraded name at run time
		/// -------------------------------------------------------------------------------------------------
		/// </summary>
		public void EnsureForeignKeyConstraintsAreEnabledAndTrusted()
		{
			var sql = @"-- ConstraintScriptRunner.EnsureForeignKeyConstraintsAreEnabledAndTrusted
SELECT
	'ALTER TABLE [' + sch.name + '].[' + tab.name + '] WITH CHECK CHECK CONSTRAINT [' + fk.name + '];'
FROM
	[{0}].sys.foreign_keys AS fk
	JOIN [{0}].sys.tables  AS tab ON tab.object_id = fk.parent_object_id
	JOIN [{0}].sys.schemas AS sch ON sch.schema_id = tab.schema_id
WHERE
	(fk.is_disabled = 1 OR fk.is_not_trusted = 1)
ORDER BY
	sch.name, tab.name
OPTION (RECOMPILE);
";

			new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, GetScriptReplacingDbNames(sql));
		}

		public void EnsureCheckConstraintsAreEnabled()
		{
			var sql = $@"-- ConstraintScriptRunner.EnsureCheckConstraintsAreEnabled
SELECT
	CommandText = CONCAT(N''
		, N'ALTER TABLE ', QUOTENAME(sch.name), N'.', QUOTENAME(tab.name)
		, N' '
		, IIF(ch.name LIKE N'%NoCheck', N'WITH NOCHECK', N'WITH CHECK')
		, N' CHECK CONSTRAINT ', QUOTENAME(ch.name)
		, N';'
	),
	LogText = CONCAT(N''
		, N'(~) '
		, N'ALTER TABLE ', QUOTENAME(sch.name), N'.', QUOTENAME(tab.name)
		, N' '
		, IIF(ch.name LIKE N'%NoCheck', N'WITH NOCHECK', N'WITH CHECK')
		, N' CHECK CONSTRAINT ', QUOTENAME(ch.name)
	)
FROM
	[{dbBeingUpgraded}].sys.check_constraints AS ch
	JOIN [{dbBeingUpgraded}].sys.tables       AS tab ON tab.object_id = ch.parent_object_id
	JOIN [{dbBeingUpgraded}].sys.schemas      AS sch ON sch.schema_id = tab.schema_id
WHERE 1=2
	OR ch.is_disabled = 1
	OR (1=1
		AND ch.is_not_trusted = 1
		AND ch.name NOT LIKE N'%NoCheck'
	)
ORDER BY
	sch.name, tab.name
OPTION (RECOMPILE);";

			new BatchRunner(taskLogger).RunCommandsGeneratedByQuery(upgConnection, sql);
		}

		#endregion
	}
}
