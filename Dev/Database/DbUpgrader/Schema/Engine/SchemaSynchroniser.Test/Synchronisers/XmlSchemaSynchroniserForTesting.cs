using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	public class XmlSchemaSynchroniserForTesting : XmlSchemaSynchroniser
	{
		public XmlSchemaSynchroniserForTesting(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
			: base(upgConnection, new DummyUpgradeManager(), dbBeingUpgraded, templateDb)
		{
		}

		public static string GetColumnXmlSchema(DbConnection conn, string dbName, string tableName, string columnName)
		{
			string sqlText = String.Format(@"
				SELECT xsd.name
				FROM
					[{0}].sys.tables tab
					INNER JOIN [{0}].sys.columns col ON col.object_id = tab.object_id
					INNER JOIN [{0}].sys.xml_schema_collections xsd ON xsd.xml_collection_id = col.xml_collection_id
				WHERE
					tab.name = '{1}'
					AND col.name like '{2}';",
				dbName, tableName, columnName);

			object scalarResult = conn.ExecuteScalar(sqlText);
			string xmlSchema = (scalarResult == null || scalarResult == DBNull.Value) ? null : scalarResult.ToString();
			return xmlSchema;
		}

		public static int GetRowCountWithGivenXmlValue(DbConnection conn, string dbName, string tableName, string columnName, string expectedXmlValue)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}]..[{1}]
				WHERE convert(varchar(max), {2}) = '{3}'",
				dbName, tableName, columnName, expectedXmlValue);

			int qtyRows = Convert.ToInt32(conn.ExecuteScalar(sqlText));
			return qtyRows;
		}
	}
}
