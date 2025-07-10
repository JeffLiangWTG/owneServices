using System.Data;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Schema;

public static class ColumnChangeMetadataHelper
{
	public static ColumnChangeMetadata[] GetTableColumnMetadata(DbConnection connection, string schemaName, string tableName)
	{
		var dt = DataUtils.GetDataTableFromQuery(
			connection,
			@"
SELECT
	TabSchema                 = CurSch.name,
	TabName                   = CurTab.name,
	ColName                   = CurCol.name,
	ColType                   = CurTyp.name,
	ColLength                 = IIF(CurCol.max_length > 0 AND CurTyp.name in (N'nchar', N'nvarchar'), CurCol.max_length / 2, CurCol.max_length),
	ColNullOrNotNull          = IIF(CurCol.is_nullable = 1, 'NULL', 'NOT NULL'),
	ColSparse                 = CurCol.is_sparse,
	ColPrecision              = CurCol.precision,
	ColScale                  = CurCol.scale,
	ColDefault                = CurDefault.definition,
	ColXmlSchema              = CurXmlSchema.name,
	IdentitySeed              = CurIdCol.seed_value,
	IdentityIncrement         = CurIdCol.increment_value,
	IsComputedColumnChange    = CurCol.is_computed,
	ComputedColumnDefinition  = CurCptCol.definition,
	IsPersistedComputedColumn = CurCptCol.is_persisted
FROM
	sys.schemas                           AS CurSch
	JOIN sys.objects                      AS CurTab ON CurTab.type = 'U' AND CurTab.schema_id = CurSch.schema_id
	JOIN sys.columns                      AS CurCol ON CurCol.object_id = CurTab.object_id
	JOIN sys.types                        AS CurTyp ON CurTyp.user_type_id = CurCol.user_type_id
	LEFT JOIN sys.default_constraints     AS CurDefault ON CurDefault.object_id = CurCol.default_object_id
	LEFT JOIN sys.identity_columns        AS CurIdCol ON CurIdCol.object_id = CurCol.object_id AND CurIdCol.column_id = CurCol.column_id
	LEFT JOIN sys.computed_columns        AS CurCptCol ON CurCptCol.object_id = CurCol.object_id AND CurCptCol.column_id = CurCol.column_id
	LEFT JOIN sys.xml_schema_collections  AS CurXmlSchema ON CurXmlSchema.xml_collection_id = CurCol.xml_collection_id AND CurCol.xml_collection_id <> 0
WHERE 1=1
	AND CurSch.name = @schemaName
	AND CurTab.name = @tableName
",
			("@schemaName", SqlDbType.NVarChar, 128, (object)schemaName),
			("@tableName", SqlDbType.NVarChar, 128, (object)tableName));
		return dt.Rows.Cast<DataRow>().Select(row => new ColumnChangeMetadata(row)).ToArray();
	}
}
