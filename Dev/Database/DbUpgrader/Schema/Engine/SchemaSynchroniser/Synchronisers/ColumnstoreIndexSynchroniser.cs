namespace Enterprise.DbUpgrader.Schema
{
	using CargoWise.Data;
	using CargoWise.DbUpgrader.Foundation;

	class ColumnstoreIndexSynchroniser : MetadataScriptRunner
	{
		public ColumnstoreIndexSynchroniser(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
			: base(upgConnection, dbBeingUpgraded, templateDb)
		{
		}

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		/// -- Run scripts to DROP old and CREATE new clustered columnstore indexes
		/// --
		/// -- Note 1: Tags {0} and {1} are replaced by the actual
		/// --         DbBeingUpgraded and TemplateDb names at run time
		///	--
		///	-- Note 2: Only handles CLUSTERED COLUMNSTORE indexes.
		///	--
		/// --         Index TYPE information:
		/// --           5 => Clustered Columnstore
		/// --
		/// -- Note 3: Indexes of tables that are not in the new schema are IGNORED
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		public void SynchroniseAll()
		{
			new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, GetScriptReplacingDbNames(synchroniseAllClusteredColumnstoreIndexes));
		}

		const string synchroniseAllClusteredColumnstoreIndexes = @"
			SELECT
				CommandText = ISNULL(
					-- CREATE CLUSTERED COLUMNSTORE INDEX [index] ON [schema].[table];
					'CREATE CLUSTERED COLUMNSTORE INDEX [' + NewInd.IndexName + '] ON [' + NewInd.SchemaName + '].[' + NewInd.TableName + '];',
					-- DROP INDEX [schema].[table].[index];
					'DROP INDEX [' + CurInd.IndexName + '] ON [' + CurInd.SchemaName + '].[' + CurInd.TableName + '];'
				)

			FROM
				-- Main Database indexes
				(
					SELECT
						SchemaName = sch.name, TableName = tab.name, IndexName = ind.name
					FROM
						[{0}].sys.tables tab
						INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
						INNER JOIN [{0}].sys.indexes ind ON ind.object_id = tab.object_id
					WHERE
						tab.is_ms_shipped = 0
						AND ind.type = 5
						-- Ignore TABLES that are not in the schema (unmanaged)
						AND EXISTS (
							SELECT newtab.name
							FROM [{1}].sys.tables newtab
							WHERE newtab.name = tab.name)
				) CurInd
				FULL OUTER JOIN
				-- Template Database indexes
				(
					SELECT
						SchemaName = sch.name, TableName = tab.name, IndexName = ind.name
					FROM
						[{1}].sys.tables tab
						INNER JOIN [{1}].sys.schemas sch ON sch.schema_id = tab.schema_id
						INNER JOIN [{1}].sys.indexes ind ON ind.object_id = tab.object_id
					WHERE
						tab.is_ms_shipped = 0
						AND ind.type = 5
				) NewInd
				-- Indexes match if their schemas, tables and names
				ON CurInd.SchemaName = NewInd.SchemaName
				AND CurInd.TableName = NewInd.TableName
				AND CurInd.IndexName = NewInd.IndexName

			WHERE
				-- Only unmatching indexes
				(CurInd.IndexName IS NULL OR NewInd.IndexName IS NULL)

			ORDER BY
				-- Drop commands first
				NewInd.IndexName
			";
	}
}
