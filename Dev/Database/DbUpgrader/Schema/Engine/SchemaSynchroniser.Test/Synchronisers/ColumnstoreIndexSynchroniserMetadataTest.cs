using System;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnstoreIndexSynchroniserMetadataTest : SchemaSyncTestCase
	{
		/// <summary>
		/// Nonclustered columnstore indexes are not yet supported.
		/// This UnitTest enforces we do not have any nonclustered columnstore indexes.
		/// </summary>
		public void TestThereAreNoNonclusteredColumnstoreIndexes()
		{
			string sqlText = @"
				SELECT
					'Schema: ' + sch.name + ' - Table: ' + tab.name + ' - Index: ' + ind.name
				FROM
					sys.schemas sch
					INNER JOIN sys.tables tab ON tab.schema_id = sch.schema_id
					INNER JOIN sys.indexes ind ON ind.object_id = tab.object_id
				WHERE
					tab.is_ms_shipped = 0
					AND ind.type = 6";

			var indexList = DataUtils.GetListOfValuesFromQuery(TestConnection, sqlText);

			string assertMessage = String.Format(
				"Found {0} nonclustered columnstore indexes:\r\n{1}",
				indexList.Count().ToString(),
				String.Join("\r\n", indexList));
			Assert(assertMessage, !indexList.Any());
		}
	}
}
