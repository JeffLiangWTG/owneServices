using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;
using NUnit.Framework;

namespace CargoWise.Bi.Maintenance.Testing
{
	class EdwIndexMaintenanceTransactionedTest : TransactionedTestCase
	{
		public void TestGetReorganizedTableIndexList()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[ModelTableState]
SET IsIndexReorganized = 0

UPDATE [{0}].[TransformTableState]
SET IsIndexReorganized = 0", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				var edwIndexMaintenance = EdwIndexMaintenance.New(TestConnection, new LoggerForTest());
				AssertEquals("There should be no reorganized indexes.", false, edwIndexMaintenance.GetReorganizedTableIndexList().Any());

				sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[ModelTableState]
SET IsIndexReorganized = 1

UPDATE [{0}].[TransformTableState]
SET IsIndexReorganized = 1", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				AssertEquals("All tables should have indexes reorganized.", true, edwIndexMaintenance.GetReorganizedTableIndexList().Any());
			}
		}

		public void TestEnsureIndexNameConvention()
		{
			var sqlText = string.Format(@"
select tc.ModelSchemaName, tc.ModelTableName, i.name
from [{0}].[TransformTableConfiguration] tc
inner join sys.indexes i
	on i.object_id = object_id(tc.ModelSchemaName + '.' + tc.ModelTableName)
where i.type = 5

UNION ALL

select mts.ModelSchemaName, mts.ModelTableName, i.name
from [{0}].[ModelTableState] mts
inner join sys.indexes i
	on i.object_id = object_id(mts.ModelSchemaName + '.' + mts.ModelTableName)
where i.type = 5",
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.EdwDatabaseName))
			using (var cmd = TestConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				CombineAssertions(() =>
				{
					Assert(true);

					while (reader.Read())
					{
						var schema = reader.GetString(0);
						var table = reader.GetString(1);
						var indexName = reader.GetString(2);

						AssertEquals(string.Format("Clustered columnstore index name for [{0}].[{1}]", schema, table), string.Format("cci_{0}_{1}", schema, table), indexName);
					}
				});
			}
		}
	}
}
