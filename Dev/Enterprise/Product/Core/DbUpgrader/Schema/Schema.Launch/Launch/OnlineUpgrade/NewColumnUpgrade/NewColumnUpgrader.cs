using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	class NewColumnUpgrader
	{
		public NewColumnUpgrader(IUpgradeManager manager, string targetDb, string templateDb)
		{
			TemplateDB = templateDb;
			TargetDb = targetDb;
			Manager = manager;
		}

		const string StatusName = nameof(NewColumnUpgrader);
		const string WatermarkName = nameof(NewColumnUpgrader);

		public string TemplateDB { get; }
		public string TargetDb { get; }
		public IUpgradeManager Manager { get; private set; }

		public void Run()
		{
			Manager.StartTask("Creating new columns");

			var columnsToAdd = GetColumnsToAdd();
			var columns = columnsToAdd.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row));

			foreach (var schema in columns.GroupBy(col => col.TableSchema, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(s => s.Key))
			{
				foreach (var table in schema.GroupBy(s => s.TableName, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(t => t.Key))
				{
					Manager.ShowInfoMessage($"{schema.Key}.{table.Key}:");

					foreach (var column in table.OrderBy(c => c.IsComputedColumnChange).ThenBy(c => c.ColumnName))
					{
						Manager.ShowInfoMessage($"    (+) {column.FullAddColumnDeclaration}");

						try
						{
							AddColumn(column);
						}
						catch (SqlException ex)
						{
							Manager.ShowInfoMessage($"        Warning: {ex.Message}");
						}
					}

					TablePreSynchroniser.RefreshDependentScripts(Db.Connection, TargetDb, schema.Key, table.Key, Manager);
				}
			}
		}

		#region Implementation

		void AddColumn(ColumnChangeMetadata column)
		{
			Db.Connection.ExecuteNonQuery($"ALTER TABLE {TargetDb.QuoteName()}.{column.TableSchema.QuoteName()}.{column.TableName.QuoteName()} ADD {column.FullAddColumnDeclaration}");
		}

		DataTable GetColumnsToAdd()
		{
			var renamedColumns = new ColumnCreatorWithValueFactory(Manager).GetPreUpgradeTransformationRenamedColumnList();
			var renamedJoin = "";
			var renamedColumn = "";
			if (!string.IsNullOrWhiteSpace(renamedColumns))
			{
				renamedJoin = string.Format(CultureInfo.InvariantCulture, @"
	LEFT JOIN
	(
		VALUES
			{0}
	) AS Renamed(schemaName, tableName, oldName, newName) ON Renamed.schemaName = NewSch.name AND Renamed.tableName = NewTab.name AND Renamed.newName = NewCol.name"
					, renamedColumns
					);

				renamedColumn = ", Renamed.oldName";
			}
			var reservedSchemas = string.Join("','", Db.SqlReservedSchemas);
			var sql = Invariant($@"-- Get list of the columns to populate
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
	[{TemplateDB}].sys.schemas     AS NewSch
	JOIN[{TemplateDB}].sys.tables  AS NewTab ON NewTab.schema_id = NewSch.schema_id
	JOIN[{TemplateDB}].sys.columns AS NewCol ON NewCol.object_id = NewTab.object_id
	JOIN[{TemplateDB}].sys.types   AS NewTyp ON NewTyp.user_type_id = NewCol.user_type_id
	LEFT JOIN[{TemplateDB}].sys.default_constraints AS NewDefault ON NewDefault.object_id = NewCol.default_object_id
{renamedJoin}

	LEFT JOIN[{TemplateDB}].sys.identity_columns AS NewIdCol ON NewIdCol.object_id = NewCol.object_id AND NewIdCol.column_id = NewCol.column_id
	LEFT JOIN[{TemplateDB}].sys.computed_columns AS NewCptCol ON NewCptCol.object_id = NewCol.object_id AND NewCptCol.column_id = NewCol.column_id
	LEFT JOIN[{TemplateDB}].sys.xml_schema_collections AS NewXmlSchema ON NewXmlSchema.xml_collection_id = NewCol.xml_collection_id
		AND NewCol.xml_collection_id <> 0

	JOIN[{TargetDb}].sys.schemas      AS CurSch ON 1=1
		AND CurSch.name = NewSch.name
	JOIN[{TargetDb}].sys.tables       AS CurTab ON CurTab.schema_id = CurSch.schema_id
		AND CurTab.name = NewTab.name
	LEFT JOIN[{TargetDb}].sys.columns AS CurCol ON CurCol.object_id = CurTab.object_id
		AND CurCol.name in (NewCol.name{renamedColumn})
WHERE 1=1
	-- Ignore microsoft-shipped tables
	AND CurTab.is_ms_shipped = 0
	AND NewTab.is_ms_shipped = 0

	-- Exclude tables in system schemas
	AND CurSch.name NOT in ('{reservedSchemas}')

	-- Exclude xml, identity type
	AND NewXmlSchema.name is NULL
	AND NewCol.is_identity <> 1

	-- Only columns added in the new schema and
	-- is nullable or has a default value
	AND CurCol.name is NULL
	AND
	(1=2
		OR NewCol.is_nullable = 1
		OR NewDefault.definition is NOT NULL
		OR NewCol.is_computed = 1
	)

	-- Ignore Offline-only operations
	-- NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation
	AND NOT
	(
		NewCol.is_nullable = 0
		AND
		(1=2
			OR
			(
				NewTyp.name in (N'varchar', N'nvarchar', N'varbinary')
				AND NewCol.max_length = -1
			)
			OR NewTyp.name in (N'xml', N'geography')
		)
	)

	-- Ignore not supported Offline-only types
	-- NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation
	AND NewTyp.name NOT in (N'text', N'ntext', N'image', N'hierarchyid', N'geometry')

");
			return Utilities.GetDataTableFromQuery(sql);
		}

		#endregion // Implementation
	}
}
