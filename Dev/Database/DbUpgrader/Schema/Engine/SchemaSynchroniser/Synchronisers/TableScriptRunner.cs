using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Gets/Runs metadata scripts to synchronise database objects based on the TemplateDb.
	/// Tags {0} and {1} are replaced by the actual DbBeingUpgraded and TemplateDb names.
	/// </summary>
	public class TableScriptRunner : MetadataScriptRunner
	{
		public TableScriptRunner(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
			: base(upgConnection, dbBeingUpgraded, templateDb)
		{
		}

		#region Select Tables in unsupported schemas

		public DataTable GetListOfTablesFromUnsupportedSchemas()
		{
			string unsupportedSchemaTableSql = String.Format(SelectUnsupportedSchemaTables, "{0}", "{1}", String.Join("','", Db.SqlReservedSchemas));
			return GetDataTableFromQueryReplacingDbNames(unsupportedSchemaTableSql);
		}

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- SELECT ALL TABLES WHICH BELONG TO AN UNSUPPORTED SCHEMA
		///	--
		///	-- Note 1: Tables which names start with [Client] are reserved for client-specific tables.
		///	--         NO tables with this naming convetion should be shipped with Enterprise.
		///	--         This is enforced by [TestNoEnterpriseTableNamesStartWithClient] unit test
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		protected string SelectUnsupportedSchemaTables = @"
			SELECT
				CurSch.name SchemaName,
				CurTab.name TableName
			FROM
				[{0}].sys.tables CurTab
				INNER JOIN [{0}].sys.schemas CurSch ON CurSch.schema_id = CurTab.schema_id
				LEFT JOIN [{1}].sys.schemas NewSch ON NewSch.name = CurSch.name
			WHERE
				-- Ignore microsoft-shipped tables
				CurTab.is_ms_shipped = 0
				-- Ignore client-specific tables
				AND (CurTab.name NOT LIKE 'Client%' AND CurTab.name NOT LIKE 'RptDt%')
				-- Exclude tables in the dbo schema and system ones
				AND CurSch.name not in ('dbo', '{2}')
				-- Only if the table schema is not in the template database
				AND NewSch.name is null
			ORDER BY
				CurSch.name,
				CurTab.name";

		#endregion

		#region Select Old Tables

		public DataTable GetOldTablesDataTable()
		{
			string oldTablesSchema = String.Format(SelectOldTables, "{0}", "{1}", String.Join("','", Db.SqlReservedSchemas));
			return GetDataTableFromQueryReplacingDbNames(oldTablesSchema);
		}

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- SELECT ALL TABLES THAT HAVE BEEN REMOVED FROM THE SCHEMA
		///	--
		///	-- Note 1: Tables which names start with [Client] are reserved for client-specific tables.
		///	--         NO tables with this naming convetion should be shipped with Enterprise.
		///	--         This is enforced by [TestNoEnterpriseTableNamesStartWithClient] unit test
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		protected string SelectOldTables = @"
			SELECT
				SchemaName = CurSch.name,
				TableName = CurTab.name
			FROM
				[{0}].sys.tables CurTab
				INNER JOIN [{0}].sys.schemas CurSch ON CurSch.schema_id = CurTab.schema_id
				LEFT JOIN (
					[{1}].sys.tables NewTab
					INNER JOIN [{1}].sys.schemas NewSch ON NewSch.schema_id = NewTab.schema_id
				) ON CurSch.name = NewSch.name AND CurTab.name = NewTab.name
			WHERE
				-- Ignore microsoft-shipped tables
				CurTab.is_ms_shipped = 0
				-- Ignore client-specific tables
				AND (CurTab.name NOT LIKE 'Client%' AND CurTab.name NOT LIKE 'RptDt%')
				AND CurSch.name not in ('{2}')
				AND NewTab.name IS NULL
			ORDER BY
				CurSch.name,
				CurTab.name";

		#endregion

		#region Select New Tables

		public DataTable GetNewTablesDataTable()
		{
			var result = GetDataTableFromQueryReplacingDbNames(SelectNewTables);
			return result;
		}

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- SELECT ALL TABLES THAT HAVE BEEN ADDED TO THE SCHEMA
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		protected string SelectNewTables = @"
			SELECT
				SchemaName = NewSch.name,
				TableName = NewTab.name
			FROM
				[{0}].sys.tables CurTab
				INNER JOIN [{0}].sys.schemas CurSch ON CurSch.schema_id = CurTab.schema_id
				RIGHT JOIN (
					[{1}].sys.tables NewTab
					INNER JOIN [{1}].sys.schemas NewSch ON NewSch.schema_id = NewTab.schema_id
				) ON CurSch.name = NewSch.name AND CurTab.name = NewTab.name
			WHERE
				-- Ignore microsoft-shipped tables
				NewTab.is_ms_shipped = 0
				AND CurTab.name IS NULL
			ORDER BY
				NewSch.name,
				NewTab.name";

		#endregion

		#region Select Modified Tables
#if DEBUG

		public DataTable GetModifiedTablesDataTable()
		{
			var result = GetDataTableFromQueryReplacingDbNames(SelectModifiedTables);
			return result;
		}

		const string SelectTableInfo = @"
			SELECT
				sch.name SchName, tab.name TabName, col.name ColName,
				col.system_type_id, col.max_length, col.precision, col.scale,
				ISNULL(col.is_nullable,0) is_nullable, col.column_id
			FROM
				[(=DB=)].sys.columns col
				INNER JOIN [(=DB=)].sys.tables tab ON tab.object_id = col.object_id
				INNER JOIN [(=DB=)].sys.schemas sch ON sch.schema_id = tab.schema_id
			WHERE
				-- Ignore microsoft-shipped tables
				tab.is_ms_shipped = 0
			";

		const string TableInfoJoinConditionCaseSensitive = @"
			ON  CurCol.SchName        = NewCol.SchName COLLATE SQL_Latin1_General_CP1_CS_AS
			AND CurCol.TabName        = NewCol.TabName COLLATE SQL_Latin1_General_CP1_CS_AS
			AND CurCol.ColName        = NewCol.ColName COLLATE SQL_Latin1_General_CP1_CS_AS
			AND CurCol.system_type_id = NewCol.system_type_id
			AND CurCol.is_nullable    = NewCol.is_nullable
			AND CurCol.max_length     = NewCol.max_length
			AND CurCol.precision      = NewCol.precision
			AND CurCol.scale          = NewCol.scale
			AND CurCol.column_id      = NewCol.column_id
			";

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- SELECT ALL TABLES THAT HAVE BEEN MODIFIED IN THE SCHEMA
		///	-- For REGEN purposes => Uses case sensitive collation to ensure name casing
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		protected string SelectModifiedTables = @"
			SELECT
				SchemaName = CurCol.SchName,
				TableName = CurCol.TabName
			FROM
				(" + SelectTableInfo.Replace("(=DB=)", "{0}") + @") CurCol
				LEFT JOIN (" + SelectTableInfo.Replace("(=DB=)", "{1}") + @" ) NewCol" + TableInfoJoinConditionCaseSensitive + @"
			WHERE NewCol.TabName IS NULL
			-- Case-sensitive Collation added here to ensure the NewCol.TableName (as opposed to CurCol.TabName)
			-- will be returned by the UNION operation when the names differ only by their character-case
			AND EXISTS (
				SELECT null
				FROM [{1}].sys.schemas s
				INNER JOIN [{1}].sys.tables t ON t.schema_id = s.schema_id
				WHERE s.name = CurCol.SchName COLLATE SQL_Latin1_General_CP1_CS_AS
				AND t.name = CurCol.TabName COLLATE SQL_Latin1_General_CP1_CS_AS)
			GROUP BY
				CurCol.SchName,
				CurCol.TabName

			UNION

			SELECT
				NewCol.SchName,
				NewCol.TabName
			FROM
				(" + SelectTableInfo.Replace("(=DB=)", "{0}") + @") CurCol
				RIGHT JOIN (" + SelectTableInfo.Replace("(=DB=)", "{1}") + @") NewCol" + TableInfoJoinConditionCaseSensitive + @"
			WHERE CurCol.TabName IS NULL
			AND EXISTS (
				SELECT null
				FROM [{0}].sys.schemas s
				INNER JOIN [{0}].sys.tables t ON t.schema_id = s.schema_id
				WHERE s.name = NewCol.SchName
				AND t.name = NewCol.TabName)
			GROUP BY
				NewCol.SchName,
				NewCol.TabName

			ORDER BY
				SchemaName,
				TableName";

#endif
		#endregion

		#region Select Tables Which All Existing Columns Are Being Removed

		public DataTable GetTablesWhichAllExistingColumnsAreBeingRemovedDataTable()
		{
			string tablesToDropSql = String.Format(CultureInfo.InvariantCulture,
				SelectTablesWhichAllExistingColumnsAreBeingRemoved,
				"{0}", "{1}", ColumnChangeRetriever.WtgPreservedColumnPrefix);
			var result = GetDataTableFromQueryReplacingDbNames(tablesToDropSql);
			return result;
		}

		/// <summary>
		/// Select tables which none of the existing columns are in the template database
		/// </summary>
		const string SelectTablesWhichAllExistingColumnsAreBeingRemoved = @"
			SELECT
				SchemaName = CurSch.name,
				TableName = CurTab.name
			FROM
				[{0}].sys.schemas CurSch
				INNER JOIN [{0}].sys.tables CurTab ON CurTab.schema_id = CurSch.schema_id
				INNER JOIN [{1}].sys.schemas NewSch ON NewSch.name = CurSch.name
				INNER JOIN [{1}].sys.tables NewTab ON NewTab.schema_id = NewSch.schema_id AND NewTab.name = CurTab.name
				LEFT JOIN (
					[{0}].sys.columns CurCol
					INNER JOIN [{1}].sys.columns NewCol ON NewCol.name = CurCol.name
				) ON CurCol.object_id = CurTab.object_id AND NewCol.object_id = NewTab.object_id
				LEFT JOIN [{0}].sys.columns PsrvdCol ON PsrvdCol.object_id = CurTab.object_id AND PsrvdCol.name like '{2}%'
			WHERE
				-- With no current colunms matching the new ones
				CurCol.name IS NULL
				-- With no CW preserved columns
				AND PsrvdCol.name IS NULL
				-- Ignore microsoft-shipped tables
				AND CurTab.is_ms_shipped = 0
			ORDER BY
				CurSch.name,
				CurTab.name";

		#endregion

		#region Select Tables to FIX CASE (EXISTING tables with different name casing)

		public DataTable GetTablesToFixCaseDataTable()
		{
			var result = GetDataTableFromQueryReplacingDbNames(selectTablesToFixCase);
			return result;
		}

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- SELECT ALL TABLES WHICH NAME CASING IS DIFFERENT IN THE NEW SCHEMA
		///	--------------------------------------------------------------------------------------------------------
		/// </summary>
		protected string selectTablesToFixCase = @"
			SELECT
				SchemaName = NewSch.name,
				OldTableName = CurTab.name,
				NewTableName = NewTab.name
			FROM
				[{0}].sys.schemas CurSch
				INNER JOIN [{0}].sys.tables CurTab ON CurTab.schema_id = CurSch.schema_id
				INNER JOIN [{1}].sys.schemas NewSch ON NewSch.name = CurSch.name
				INNER JOIN [{1}].sys.tables NewTab ON NewTab.schema_id = NewSch.schema_id AND NewTab.name = CurTab.name
			WHERE
				-- Ignore microsoft-shipped tables
				CurTab.is_ms_shipped = 0
				AND NewTab.is_ms_shipped = 0
				-- Only tables with a different name casing
				AND CurTab.name != NewTab.name COLLATE SQL_Latin1_General_CP1_CS_AS
			ORDER BY
				NewSch.name,
				NewTab.name";

		public void RenameTable(string schemaName, string curTableName, string newTableName)
		{
			DropSchemaBoundReferencingObjects(schemaName, curTableName);
			DbObjectCreator.RenameTable(upgConnection, dbBeingUpgraded, schemaName, curTableName, newTableName);
		}

		#endregion

		#region Table Lock Escalation Settings

		public void SynchroniseLockEscalationSettings()
		{
			new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, GetScriptReplacingDbNames(SynchroniseLockEscalationQuery));
		}

		/// <summary>
		///	--------------------------------------------------------------------------------------------------------
		///	-- GENERATE COMMANDS TO CHANGE UNMATCHING LOCK ESCALATION SETTINGS
		///	--------------------------------------------------------------------------------------------------------
		///	LOCK_ESCALATION:
		///	  0 = TABLE
		///	  1 = DISABLE
		///	  2 = AUTO
		/// </summary>
		protected string SynchroniseLockEscalationQuery = @"
			SELECT
				'ALTER TABLE [' + SCHEMA_NAME(NewTables.schema_id)  + '].[' + NewTables.name + '] SET (LOCK_ESCALATION = ' +
				CASE NewTables.lock_escalation WHEN 1 THEN 'DISABLE' WHEN 2 THEN 'AUTO' ELSE 'TABLE' END + ');'
			FROM [{0}].sys.tables CurTables
				INNER JOIN [{1}].sys.tables NewTables
				ON CurTables.name = NewTables.name AND CurTables.schema_id = NewTables.schema_id
			WHERE
				-- Ignore microsoft-shipped tables
				NewTables.is_ms_shipped = 0
				AND CurTables.lock_escalation != NewTables.lock_escalation
			ORDER BY NewTables.name";

		#endregion

		/// <summary>
		/// Create new table based on the Template DB (structure and default constraints)
		/// Note: Uses ".dbo." to make sure the table will be created in the DBOwner schema
		/// </summary>
		public void CreateNewTable(string schemaName, string tableName)
		{
			string columnDeclaration = GetNewTableColumnDeclaration(schemaName, tableName);
			string sqlText = String.Format("CREATE TABLE [{0}].[{1}].[{2}] ({3});", dbBeingUpgraded, schemaName, tableName, columnDeclaration);
			upgConnection.ExecuteNonQuery(sqlText);
		}

		string GetNewTableColumnDeclaration(string schemaName, string tableName)
		{
			var newTableColumnsSql = Invariant($@"
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
WHERE 1=1
	AND NewSch.name = @SchemaName
	AND NewTab.name = @TableName
ORDER BY
	NewCol.column_id

");

			var columnDeclarationList = new StringBuilder();
			DataTable newTableColumns = null;

			using (var cmd = upgConnection.Command(newTableColumnsSql))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, 128, schemaName);
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, tableName);

				newTableColumns = DataUtils.GetDataTableFromCommand(cmd);
			}

			foreach (DataRow row in newTableColumns.Rows)
			{
				var columnMetadata = new ColumnChangeMetadata(row);
				columnDeclarationList.Append(columnMetadata.FullAddColumnDeclaration);
				columnDeclarationList.AppendLine(",");
			}

			return columnDeclarationList.ToString();
		}

		/// <summary>
		/// Drop FKs which reference a table, before it (or its PK) can be dropped.
		/// </summary>
		public void DropReferencingFKs(string referencedTableSchema, string referencedTable)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				SELECT 'ALTER TABLE [{0}].[' + FkTabSchema.name + '].[' + FkTab.name + '] DROP CONSTRAINT [' + FkConst.name + ']'
				FROM [{0}].sys.foreign_keys FkConst
				INNER JOIN [{0}].sys.tables FkTab ON FkTab.object_id = FkConst.parent_object_id
				INNER JOIN [{0}].sys.schemas FkTabSchema ON FkTabSchema.schema_id = FkTab.schema_id
				INNER JOIN [{0}].sys.tables PkTab ON PkTab.object_id = FkConst.referenced_object_id
				INNER JOIN [{0}].sys.schemas PkTabSchema ON PkTabSchema.schema_id = PkTab.schema_id
				WHERE PkTabSchema.name = @SchemaName
				AND PkTab.name = @TableName",
				dbBeingUpgraded);

			using (var cmd = upgConnection.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, referencedTableSchema);
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, referencedTable);
				new BatchRunner().RunCommandsGeneratedByCommand(cmd);
			}
		}

		/// <summary>
		/// Drop Primary XML indexes of a table, before it (or its PK) can be dropped.
		/// </summary>
		public void DropPrimaryXmlIndexes(string schemaName, string tableName)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				SELECT
					'DROP INDEX [' + ind.name + '] ON [{0}].[' + sch.name + '].[' + tab.name + ']'
				FROM
					[{0}].sys.xml_indexes ind
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = ind.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
				WHERE
					sch.name = @SchemaName
					AND tab.name = @TableName
					AND using_xml_index_id is null",
				dbBeingUpgraded);

			using (var cmd = upgConnection.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, schemaName);
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, tableName);
				new BatchRunner().RunCommandsGeneratedByCommand(cmd);
			}
		}

		/// <summary>
		/// Drop Spatial indexes of a table, before it (or its PK) can be dropped.
		/// </summary>
		public void DropSpatialIndexes(string schemaName, string tableName)
		{
			using (((ICurrentDbControl)upgConnection).UseDatabase(dbBeingUpgraded))
			{
				var indexesToDrop = SpatialIndexLoader.Load(upgConnection, schemaName, tableName, null);
				foreach (var index in indexesToDrop)
				{
					upgConnection.ExecuteNonQuery(index.SQL_Drop);
				}
			}
		}

		public void DropSchemaBoundReferencingObjects(string schemaName, string tableName)
		{
			var sql = UpgraderUtils.GetSchemaBoundObjectsToDropSql(schemaName, tableName);
			new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, sql);
		}

		public DataTable GetSensitivityClassificationsToDrop()
		{
			var getSensitivityClassificationsToDrop = $@"-- {nameof(GetSensitivityClassificationsToDrop)}
SELECT
	TabSchema = CurSch.name,
	TabName   = CurTab.name,
	ColName   = CurCol.name
FROM
	{dbBeingUpgraded.QuoteName()}.sys.schemas      AS CurSch
	JOIN {dbBeingUpgraded.QuoteName()}.sys.objects AS CurTab on CurTab.schema_id = CurSch.schema_id -- table
	JOIN {dbBeingUpgraded.QuoteName()}.sys.columns AS CurCol on CurCol.object_id = CurTab.object_id -- column
	JOIN {dbBeingUpgraded.QuoteName()}.sys.sensitivity_classifications AS CurCf ON CurCf.major_id = CurCol.object_id AND CurCf.minor_id = CurCol.column_id

	JOIN {templateDb.QuoteName()}.sys.schemas AS NewSch ON 1=1
		AND NewSch.name = CurSch.name
	JOIN {templateDb.QuoteName()}.sys.objects AS NewTab ON NewTab.schema_id = NewSch.schema_id
		AND NewTab.name = CurTab.name -- table
	JOIN {templateDb.QuoteName()}.sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
		AND NewCol.name = CurCol.name -- column
	LEFT JOIN {templateDb.QuoteName()}.sys.sensitivity_classifications AS NewCf ON NewCf.major_id = NewCol.object_id AND NewCf.minor_id = NewCol.column_id
WHERE 1=1
	-- exclude system tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0
	AND CurTab.type = 'U'
	AND NewTab.type = 'U'
	AND
	(1=2
		-- old and modified
		OR ISNULL(NULLIF(NewCf.label, CurCf.label), NULLIF(CurCf.label, NewCf.label)) is NOT NULL
		OR ISNULL(NULLIF(NewCf.label_id, CurCf.label_id), NULLIF(CurCf.label_id, NewCf.label_id)) is NOT NULL
		OR ISNULL(NULLIF(NewCf.information_type, CurCf.information_type), NULLIF(CurCf.information_type, NewCf.information_type)) is NOT NULL
		OR ISNULL(NULLIF(NewCf.information_type_id, CurCf.information_type_id), NULLIF(CurCf.information_type_id, NewCf.information_type_id)) is NOT NULL
		OR ISNULL(NULLIF(NewCf.rank_desc, CurCf.rank_desc), NULLIF(CurCf.rank_desc, NewCf.rank_desc)) is NOT NULL
	)
ORDER BY
	CurSch.name,
	CurTab.name,
	CurCol.name
";

			return DataUtils.GetDataTableFromQuery(upgConnection, getSensitivityClassificationsToDrop);
		}

		public DataTable GetSensitivityClassificationsToCreate()
		{
			var getSensitivityClassificationsToCreate = $@"-- {nameof(GetSensitivityClassificationsToCreate)}
SELECT
	TabSchema  = NewSch.name,
	TabName    = NewTab.name,
	ColName    = NewCol.name,
	Label      = ISNULL(CONVERT(NVARCHAR(128), NewCf.label), N''),
	LabelId    = ISNULL(CONVERT(NVARCHAR(128), NewCf.label_id), N''),
	InfoType   = ISNULL(CONVERT(NVARCHAR(128), NewCf.information_type), N''),
	InfoTypeId = ISNULL(CONVERT(NVARCHAR(128), NewCf.information_type_id), N''),
	Rank       = ISNULL(CONVERT(NVARCHAR(128), NewCf.rank_desc), N'')
FROM
	{templateDb.QuoteName()}.sys.schemas      AS NewSch
	JOIN {templateDb.QuoteName()}.sys.objects AS NewTab on NewTab.schema_id = NewSch.schema_id -- table
	JOIN {templateDb.QuoteName()}.sys.columns AS NewCol on NewCol.object_id = NewTab.object_id -- column
	JOIN {templateDb.QuoteName()}.sys.sensitivity_classifications AS NewCf ON NewCf.major_id = NewCol.object_id AND NewCf.minor_id = NewCol.column_id

	JOIN {dbBeingUpgraded.QuoteName()}.sys.schemas AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN {dbBeingUpgraded.QuoteName()}.sys.objects AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name -- table
	JOIN {dbBeingUpgraded.QuoteName()}.sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
		AND CurCol.name = NewCol.name -- column
	LEFT JOIN {dbBeingUpgraded.QuoteName()}.sys.sensitivity_classifications AS CurCf ON CurCf.major_id = CurCol.object_id AND CurCf.minor_id = CurCol.column_id
WHERE 1 = 1
	-- exclude system tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0
	AND CurTab.type = 'U'
	AND NewTab.type = 'U'
	-- new and modified
	AND CurCf.major_id is NULL
ORDER BY
	NewSch.name,
	NewTab.name,
	NewCol.name
";

			return DataUtils.GetDataTableFromQuery(upgConnection, getSensitivityClassificationsToCreate);
		}
	}
}
