using System.Collections.Specialized;
using System.Data;
using System.Linq;
using CargoWise.BuildTools;
using CargoWise.Data;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoCodeTest : AutoCodeTestCase
	{
		public void TestAllDbDataTypesUsedInDbAreSupported()
		{
			string sqlText = string.Format(@"
				select	distinct typ.name DbType
				from	sys.types typ
				join	sys.columns col on col.user_type_id = typ.user_type_id
				join	sys.tables tab on tab.object_id = col.object_id
				where	tab.is_ms_shipped = 0
					and tab.Name not in ({0})
					and {1}",
					string.Join(", ", BuildXml.Instance.ExcludedTables.Select(x => "'" + x + "'")),
					string.Join(" and ", BuildXml.Instance.ExcludedColumns.Select(x => $"col.name not like '%{x}'")));

			DataTable table = new DataTable();
			using (var adapter = Db.Connection.Command(sqlText).NewDataAdapter())
			{
				adapter.Fill(table);
			}

			StringCollection generatorSupportedSqlTypes = new StringCollection();
			generatorSupportedSqlTypes.AddRange(SupportedSqlDataTypes);

			foreach (DataRow row in table.Rows)
			{
				AssertEquals("The SQL database type <" + row[0] + "> is new and the generator may need updating to support this type.",
					true, generatorSupportedSqlTypes.Contains(row[0].ToString()));
			}
		}

		public void TestLinesOfCode()
		{
			AutoCode testObject = new AutoCode();
			AssertNotNull("Failed to create AutoV2PropertyList", testObject);

			string result = testObject.LinesOfCode("Line 1", "Line 2", "Line 3");
			AssertEquals("result", "Line 1\r\nLine 2\r\nLine 3", result);

			result = testObject.LinesOfCode("Line 1", null, "Line 3");
			AssertEquals("result", "Line 1\r\nLine 3", result);
		}

		public void TestSchemaOnly()
		{
			var allBusinessObjects = BuildXml.Instance.AllBusinessObjects.Cast<BuildXmlBizOEntry>().Select(b => b.TableName);
			var excludedTables = BuildXml.Instance.ExcludedTables;
			foreach (var schameName in BuildXml.Instance.SchemaOnlyViews)
			{
				AssertCollectionNotContains("ExcludedTables if does not generate schema or Bizo.", schameName, excludedTables);
				AssertCollectionNotContains("If we generate bizo it means is not schema only.", schameName, allBusinessObjects);
			}
		}

		public void TestSchemaOnlyShouldBeView()
		{
			var sqlText = $@"
				select
					count(*) count
				from
					sys.Views
				where
					is_ms_shipped = 0 and
					Name in ({string.Join(", ", BuildXml.Instance.SchemaOnlyViews.Select(s => "'" + s + "'"))})";

			var table = new DataTable();
			var count = (int)Db.Connection.Command(sqlText).ExecuteScalar();
			AssertEquals("Only view is acceptable!", BuildXml.Instance.SchemaOnlyViews.Count(), count);
		}
	}
}
