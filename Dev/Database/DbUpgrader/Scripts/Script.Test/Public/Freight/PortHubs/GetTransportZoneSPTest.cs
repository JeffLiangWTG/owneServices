using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.PortHubs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.PortHubs.Testing
{
	[TestedType(typeof(GetTransportZoneSP))]
	internal sealed class GetTransportZoneSPTest : DbCreateScriptTest
	{
		public void TestGetTransportZoneWithZoneTypeAll()
		{
			generator.SetupTestData();

			var result = GetTransportZone("AU", "3560", "", "", "ALL");
			AssertEquals(1, result.Length);
			AssertEquals("5f3a8cbf-65da-44a3-9100-8cf91e983c0c", result[0].ToString());

			result = GetTransportZone("AU", "", "Adelaide City", "SA", "ALL");
			AssertEquals(1, result.Length);
			AssertEquals("a7a8b7f4-f7b3-4411-98c4-fbb5dcf29a98", result[0].ToString());

			result = GetTransportZone("AU", "", "Sydney City", "NSW", "ALL");
			AssertEquals(1, result.Length);
			AssertEquals("62d0f80c-23a5-4896-a302-df5759efedc1", result[0].ToString());

			result = GetTransportZone("AU", "", "Sydney City", "", "ALL");
			AssertEquals(0, result.Length);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			generator = new SupplierBookingGeneratorForTests(TestConnection);
		}
		SupplierBookingGeneratorForTests generator;

		Guid[] GetTransportZone(string countryCode, string postCode, string cityName, string stateCode, string zoneType)
		{
			var result = new List<Guid>();

			using (var command = Db.Connection.Command("GetTransportZoneSP"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@CountryCode", SqlDbType.VarChar, 2, countryCode);
				command.AddParameter("@PostCode", SqlDbType.VarChar, 10, postCode);
				command.AddParameter("@CityName", SqlDbType.VarChar, 50, cityName);
				command.AddParameter("@StateCode", SqlDbType.VarChar, 3, stateCode);
				command.AddParameter("@ZoneType", SqlDbType.VarChar, 3, zoneType);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetGuid(0));
					}
				}
			}

			return result.ToArray();
		}
		#endregion
	}
}
