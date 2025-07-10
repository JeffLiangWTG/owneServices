using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.NZ.Testing
{
	sealed class UpgradeToVersion15_DropFlightsAndVessels_Test : NZTariffReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 15;

		protected override void PrepareTestData(DbConnection conn)
		{
			if (!DbObjectCreator.TableExists(conn, NZTariffReferenceDbUpgrader.FlightsAndVesselsTableName))
			{
				var script = "CREATE TABLE " + NZTariffReferenceDbUpgrader.FlightsAndVesselsTableName
					+ " ( [Q3_PK] [uniqueidentifier] NOT NULL DEFAULT (newid()) )";
				conn.Command(script).ExecuteNonQuery();
			}
		}

		protected override void AssertUpgradeResult()
		{
			Assert(!DbObjectCreator.TableExists(Db.Connection, NZTariffReferenceDbUpgrader.FlightsAndVesselsTableName));
		}
	}
}
