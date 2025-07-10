using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[UseSnapshotProtection]
	class TablePreAddDbTest : TestCase
	{
		public void TestDatabaseExistsAndSchemaDoesNot()
		{
			TablePreSynchroniser.DropPreAddDb_ForTest();
			TablePreSynchroniser.CreatePreAddDb_ForTest();

			using (var connection = Db.NewAdminConnection())
			{
				using (((ICurrentDbControl)connection).UseDatabase(TablePreSynchroniser.PreAddDb_Exposed))
				{
					connection.ExecuteNonQuery("DROP SCHEMA hrm");
				}

				AssertCollectionNotContains("PRE: The schema should have been removed", "hrm", GetSchemas(connection, TablePreSynchroniser.PreAddDb_Exposed));

				TablePreSynchroniser.CreatePreAddDb_ForTest();
				AssertCollectionContains("The hrm schema should have been created, even if the db already exists", "hrm", GetSchemas(connection, TablePreSynchroniser.PreAddDb_Exposed));
				AssertNoExceptionThrown("Subsequent create calls should not attempt to recreate the hrm schema", TablePreSynchroniser.CreatePreAddDb_ForTest);
			}
		}

		public void TestPreAddDbContainsSchemas()
		{
			TablePreSynchroniser.DropPreAddDb_ForTest();
			TablePreSynchroniser.CreatePreAddDb_ForTest();

			using (var adminConnection = Db.NewAdminConnection())
			{
				var tablesThatDoNotNeedSchemaSynchronisation = new[] { "cdc", "sys" };
				var expected = GetSchemas(adminConnection, Db.DatabaseName).Except(tablesThatDoNotNeedSchemaSynchronisation).ToList();
				var actual = GetSchemas(adminConnection, TablePreSynchroniser.PreAddDb_Exposed).Except(tablesThatDoNotNeedSchemaSynchronisation).ToList();

				AssertContainsExactElementsInAnyOrder("If your schema contains tables that require synchronisation, you must ensure the PreUpgradeDb contains it", expected, actual);
			}
		}

		IEnumerable<string> GetSchemas(DbConnection connection, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				AssertEquals("PRE: We should be on the correct database", dbName, connection.ExecuteScalar<string>("SELECT DB_NAME()"));

				using (var cmd = connection.Command("select name from sys.schemas"))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return reader.GetString(0);
					}
				}
			}
		}

		protected override void TearDown()
		{
			TablePreSynchroniser.DropPreAddDb_ForTest();

			base.TearDown();
		}
	}
}
