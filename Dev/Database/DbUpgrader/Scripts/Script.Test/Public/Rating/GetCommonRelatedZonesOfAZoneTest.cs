using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating
{
	[TestedType(typeof(GetCommonRelatedZonesOfAZone))]
	class GetCommonRelatedZonesOfAZoneTest : DbCreateScriptTest
	{
		public void TestZoneDoesNotReturnsItself()
		{
			AssertEquals("Test USAR", 0, TestConnection.ExecuteScalar(@"SELECT Count(ZonePK) FROM GetCommonRelatedZonesOfAZone('ca88e9cc-b130-4895-820a-c100c182ab4a')"));
			AssertEquals("Test AUSR", 0, TestConnection.ExecuteScalar(@"SELECT Count(ZonePK) FROM GetCommonRelatedZonesOfAZone('B97AEF8B-DBD4-4B20-B07F-13202F73436B')"));
		}
		public void TestZoneReturnsCommonRelatedZones()
		{
			var expectedPK = new[]
			{
				"df278d0c-5b09-4ebc-b135-9d0151e9c5d1",
				"51c7c487-c58d-4134-9210-c9ff469f6b01",
			};
			var observedPKs = new List<string>();
			using (var reader = TestConnection.Command(@"SELECT ZonePK FROM GetCommonRelatedZonesOfAZone('EC98BB3F-A886-4218-A55D-3CE91166FAB8')").ExecuteReader())
			{
				while (reader.Read())
				{
					var currentPK = reader.GetGuid(0).ToString();
					observedPKs.Add(currentPK);
				}
			}
			AssertContainsExactElementsInAnyOrder(expectedPK, observedPKs);
		}
	}
}

