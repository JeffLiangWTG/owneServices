using System;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion48VarMaxTypeTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertColumnDataType("~BlobTable~", "ColChar", "varchar");
			AssertColumnDataType("~BlobTable~", "ColNchar", "nvarchar");
			AssertColumnDataType("~BlobTable~", "ColBinary", "varbinary");
			AssertColumnDataType("USCChapter", "UK_Description", "varchar");
			AssertColumnDefault("~BlobTable~", "ColChar", "('')");
			AssertColumnDefault("~BlobTable~", "ColNchar", null);
			AssertColumnDefault("~BlobTable~", "ColBinary", null);
			AssertColumnDefault("USCChapter", "UK_Description", "('')");
		}

		protected override int LatestVersionNumber
		{
			get { return 48; }
		}

		protected override void PrepareTestData(DbConnection conn)
		{
			conn.ExecuteNonQuery("CREATE TABLE [~BlobTable~] (ColChar text not null default '', ColNchar ntext null, ColBinary image not null)");
			AssertColumnDataType("~BlobTable~", "ColChar", "text");
			AssertColumnDataType("~BlobTable~", "ColNchar", "ntext");
			AssertColumnDataType("~BlobTable~", "ColBinary", "image");
			AssertColumnDefault("~BlobTable~", "ColChar", "('')");
			AssertColumnDefault("~BlobTable~", "ColNchar", null);
			AssertColumnDefault("~BlobTable~", "ColBinary", null);
		}

		void AssertColumnDataType(string tableName, string columnName, string expectedType)
		{
			var sqlText = string.Format(@"
				SELECT typ.name
				FROM [{0}].sys.tables tab
				INNER JOIN [{0}].sys.columns col ON col.object_id = tab.object_id
				INNER JOIN [{0}].sys.types typ ON typ.system_type_id = col.system_type_id
				WHERE tab.name = '{1}' AND col.name = '{2}'",
				refDbUpgrader.DbName, tableName, columnName);
			var actualType = testConnection.ExecuteScalar(sqlText).ToString();

			Assert(
				string.Format("Column [{0}].[{1}] data type: (expected){2} != (actual){3}", tableName, columnName, expectedType, actualType),
				string.Equals(expectedType, actualType, StringComparison.OrdinalIgnoreCase));
		}

		void AssertColumnDefault(string tableName, string columnName, string expectedDefault)
		{
			var sqlText = string.Format(@"
				SELECT def.definition
				FROM [{0}].sys.tables tab
				INNER JOIN [{0}].sys.columns col ON col.object_id = tab.object_id
				INNER JOIN [{0}].sys.default_constraints def ON def.parent_object_id = col.object_id AND def.object_id = col.default_object_id
				WHERE tab.name = '{1}' AND col.name = '{2}'",
				refDbUpgrader.DbName, tableName, columnName);
			var actualDefault = testConnection.ExecuteScalar(sqlText);
			AssertEquals(string.Format("Column [{0}].[{1}] default:", tableName, columnName), expectedDefault, actualDefault);
		}
	}
}
