using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating
{
	[TestedType(typeof(GetZonesOfUnlocoAndCountry))]
	class GetZonesOfUnlocoAndCountryTest : DbCreateScriptTest
	{
		public void TestAUSYD_ReturnsZonesForAustraliaAlso()
		{
			var expectedPK = new[]
			{
				"b97aef8b-dbd4-4b20-b07f-13202f73436b",
				"c432fb9e-bdc3-4d3b-b35c-1a8b8282c26c",
				"51c7c487-c58d-4134-9210-c9ff469f6b01",
			};
			var observedPKs = new List<string>();
			using (var reader = TestConnection.Command(@"SELECT ZonePK FROM GetZonesOfUnlocoAndCountry('AD87872E-96B8-4C9A-BC0B-6A4088247A64')").ExecuteReader())
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

