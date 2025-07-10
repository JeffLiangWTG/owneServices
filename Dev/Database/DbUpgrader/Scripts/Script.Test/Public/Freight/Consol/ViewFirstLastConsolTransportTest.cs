using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol.Testing
{
	[TestedType(typeof(ViewFirstLastConsolTransport))]
	class ViewFirstLastConsolTransportTest : DbCreateScriptTest
	{
		public void TestTransportVoyageFlightAndSailingPK()
		{
			var helper = new TestDbHelper(Db.Connection);
			var consolPK = helper.InsertConsol("C00012345", "USCHI", "AUSYD");
			var time = DateTime.Today;
			var voyagePK1 = helper.InsertJobVoyage("BOAT1");
			var voyagePK2 = helper.InsertJobVoyage("BOAT2");
			var originPK1 = helper.InsertJobVoyOrigin("USCHI", voyagePK1);
			var destinationPK1 = helper.InsertJobVoyDestination("AUMEL", voyagePK1, time);
			var originPK2 = helper.InsertJobVoyOrigin("AUMEL", voyagePK2);
			var destinationPK2 = helper.InsertJobVoyDestination("AUSYD", voyagePK2, time.AddDays(1));
			var sailingPK1 = helper.InsertJobSailing(originPK1, destinationPK1, time);
			var sailingPK2 = helper.InsertJobSailing(originPK2, destinationPK2, time.AddDays(1));
			var transport1 = helper.InsertJobConsolTransport(consolPK, "CON", sailingPK1, time, time.AddDays(1), 1);
			var transport2 = helper.InsertJobConsolTransport(consolPK, "CON", sailingPK2, time.AddDays(1), time.AddDays(2), 2);

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
					$"SELECT * FROM dbo.ViewFirstLastConsolTransport");

			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("LastTransportVoyageFlight should be [BOAT2]", "BOAT2", result.Rows[0]["LastTransportVoyageFlight"]);
			AssertEquals("FirstTransportVoyageFlight should be [BOAT1]", "BOAT1", result.Rows[0]["FirstTransportVoyageFlight"]);
			AssertEquals($"FirstSailingPK should be [{sailingPK1}]", sailingPK1, result.Rows[0]["FirstSailingPK"]);
			AssertEquals($"LastSailingPK should be [{sailingPK2}]", sailingPK2, result.Rows[0]["LastSailingPK"]);
		}
	}
}

