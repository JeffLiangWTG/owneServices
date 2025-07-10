using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.PortHubs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.PortHubs.Testing
{
	[TestedType(typeof(GetTransportZone))]
	internal sealed class GetTransportZoneTest : DbCreateScriptTest
	{
		public void TestGetTransportZoneWithDefaultZoneType()
		{
			generator.SetupTestData();

			var result = GetTransportZoneWithDefaultZoneType("AU", "3560", "", "");
			AssertEquals(1, result.Length);
			AssertEquals("5f3a8cbf-65da-44a3-9100-8cf91e983c0c", result[0].ToString());

			result = GetTransportZoneWithDefaultZoneType("AU", "", "Adelaide City", "SA");
			AssertEquals(1, result.Length);
			AssertEquals("a7a8b7f4-f7b3-4411-98c4-fbb5dcf29a98", result[0].ToString());

			result = GetTransportZoneWithDefaultZoneType("AU", "", "Sydney City", "NSW");
			AssertEquals(1, result.Length);
			AssertEquals("62d0f80c-23a5-4896-a302-df5759efedc1", result[0].ToString());

			result = GetTransportZoneWithDefaultZoneType("AU", "", "Sydney City", "");
			AssertEquals(0, result.Length);
		}

		public void TestGetTransportZoneWithInActiveTransportProvider()
		{
			generator.SetupTestData(includeInActiveTransportProvider: true);

			var result = GetTransportZone("AU", "3750", "", "", "ALL");
			AssertEquals(0, result.Length);
		}

		public void TestGetTransportZoneWithInActiveTransportZones()
		{
			generator.SetupTestData(includeInActiveTransportZones: true);

			var result = GetTransportZone("AU", "3750", "", "", "ALL");
			AssertEquals(1, result.Length);
			AssertEquals("a9eb65ad-828c-4a2b-8963-076f390a4b75", result[0].ToString());
		}

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

		public void TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIsOPS()
		{
			TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIs("OPS");
		}

		public void TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIsALL()
		{
			TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIs("ALL");
		}

		public void TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIsRAT()
		{
			TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIs("RAT", false);
		}

		void TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIs(string zoneType, bool hasResult = true)
		{
			var refCityTownPerth = new RefCityTown("Perth City", "WA", "AU").InsertAndReturnObject(TestConnection);
			var refPostCodeHobartFrom = new RefPostCode("7000", "AU").InsertAndReturnObject(TestConnection);
			var refPostCodeHobartTo = new RefPostCode("7100", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportProvider = new RateTransportProvider("AU", null, zoneType).InsertAndReturnObject(TestConnection);
			var rateTransportZoneHobart = new RateTransportZones("Hobart Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZonePerth = new RateTransportZones("Perth Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemHobart = new RateTransportZoneItem("AU", rateTransportZoneHobart, refPostCodeHobartFrom, refPostCodeHobartTo).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemPerth = new RateTransportZoneItem("AU", rateTransportZonePerth, refCityTownPerth).InsertAndReturnObject(TestConnection);

			var result = GetTransportZone("AU", "7001", "", "", "OPS");
			if (hasResult)
			{
				AssertEquals(1, result.Length);
				AssertEquals(rateTransportZoneHobart.PK, result[0]);
			}
			else
			{
				AssertEquals(0, result.Length);
			}

			result = GetTransportZone("AU", "", "Perth City", "WA", "OPS");
			if (hasResult)
			{
				AssertEquals(1, result.Length);
				AssertEquals(rateTransportZonePerth.PK, result[0]);
			}
			else
			{
				AssertEquals(0, result.Length);
			}

			result = GetTransportZone("AU", "", "Perth City", "", "OPS");
			AssertEquals(0, result.Length);
		}

		public void TestGetTransportZoneWhichHasFromPostCodeButNullToPostCode()
		{
			var refPostCodeCanberraCBD = new RefPostCode("2601", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportProvider = new RateTransportProvider("AU", null, "ALL").InsertAndReturnObject(TestConnection);
			var rateTransportZoneCanberra = new RateTransportZones("Canberra Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemCanberraCBD = new RateTransportZoneItem("AU", rateTransportZoneCanberra, refPostCodeCanberraCBD).InsertAndReturnObject(TestConnection);

			var result = GetTransportZone("AU", "2601", "", "", "ALL");

			AssertEquals(1, result.Length);
			AssertEquals(rateTransportZoneCanberra.PK, result[0]);
		}

		public void TestGetTransportZone_WhenMultipleMatchesExist_ReturnsByMatching_CountryCityStateCode_AndZoneTypeIsOPS()
		{
			var rateTransportProviders = GetTransportProviders(new string[] { "OPS" });
			var refCityTownVictoriaVIC = new RefCityTown("Victoria VIC", "VIC", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportZoneVictoria = CreateTransportZoneWithCountryAndCity(rateTransportProviders[0], refCityTownVictoriaVIC);
			var rateTransportZoneVicPostCodesRange = CreateTransportZoneWithPostCodeRange(rateTransportProviders[0]);

			var result = GetTransportZone("AU", "3220", "Victoria VIC", "VIC", "OPS");

			AssertEquals(1, result.Length);
			AssertEquals(rateTransportZoneVictoria.PK, result[0]);
		}

		public void TestGetTransportZone_WhenMultipleMatchesExist_ReturnsByMatching_PostCodeRange_AndZoneTypeIsOPS()
		{
			var rateTransportProviders = GetTransportProviders(new string[] { "OPS", "ALL" });
			var refCityTownVictoriaVIC = new RefCityTown("Victoria VIC", "VIC", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportZoneVictoria = CreateTransportZoneWithCountryAndCity(rateTransportProviders[1], refCityTownVictoriaVIC);
			var rateTransportZoneVicPostCodesRange = CreateTransportZoneWithPostCodeRange(rateTransportProviders[0]);

			var result = GetTransportZone("AU", "3220", "Victoria VIC", "VIC", "OPS");

			AssertEquals(1, result.Length);
			AssertEquals(rateTransportZoneVicPostCodesRange.PK, result[0]);
		}

		public void TestGetTransportZone_WhenMultipleMatchesExist_ReturnsByMatching_CountryCityStateCode_AndZoneTypeIsAll()
		{
			var rateTransportProviders = GetTransportProviders(new string[] { "ALL" });
			var refCityTownVictoriaVIC = new RefCityTown("Victoria VIC", "VIC", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportZoneVictoria = CreateTransportZoneWithCountryAndCity(rateTransportProviders[0], refCityTownVictoriaVIC);
			var rateTransportZoneVicPostCodesRange = CreateTransportZoneWithPostCodeRange(rateTransportProviders[0]);

			 var result = GetTransportZone("AU", "3220", "Victoria VIC", "VIC", "");

			AssertEquals(1, result.Length);
			AssertEquals(rateTransportZoneVictoria.PK, result[0]);
		}

		public void TestGetTransportZone_WhenMultipleMatchesExist_ReturnsByMatching_PostCodeRange_AndZoneTypeIsAll()
		{
			var rateTransportProviders = GetTransportProviders(new string[] { "ALL" });
			var refCityTownVictoriaVIC = new RefCityTown("Victoria VIC", "VIC", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportZoneVictoria = CreateTransportZoneWithCountryAndCity(rateTransportProviders[0], refCityTownVictoriaVIC);
			var rateTransportZoneVicPostCodesRange = CreateTransportZoneWithPostCodeRange(rateTransportProviders[0]);

			var result = GetTransportZone("AU", "3220", "", "VIC", "");

			AssertEquals(1, result.Length);
			AssertEquals(rateTransportZoneVicPostCodesRange.PK, result[0]);
		}

		public void TestGetTransportZone_WhenMultipleMatchExist_ReturnsByMatching_CountryCityStateCode_AndIgnoresZoneOwner()
		{
			var orgHeader = new OrgHeader("TST").InsertAndReturnObject(TestConnection);
			var rateTransportProvidersWithZoneOwner = GetTransportProviders(new string[] { "OPS" }, zoneOwner: orgHeader);
			var refCityTownVictoriaVIC = new RefCityTown("Victoria VIC", "VIC", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportZone = CreateTransportZoneWithCountryAndCity(rateTransportProvidersWithZoneOwner[0], refCityTownVictoriaVIC);

			var result = GetTransportZone("AU", "3220", "Victoria VIC", "VIC", "OPS");

			AssertEquals(0, result.Length);
		}

		public void TestGetTransportZone_WhenMultipleMatchExist_ReturnsByMatching_PostCodeRange_AndIgnoresZoneOwner()
		{
			var orgHeader = new OrgHeader("TST").InsertAndReturnObject(TestConnection);
			var rateTransportProvidersWithZoneOwner = GetTransportProviders(new string[] { "OPS" }, zoneOwner: orgHeader);
			var rateTransportZoneVicPostCodesRange = CreateTransportZoneWithPostCodeRange(rateTransportProvidersWithZoneOwner[0]);

			var result = GetTransportZone("AU", "3220", "Victoria VIC", "VIC", "OPS");

			AssertEquals(0, result.Length);
		}

		public void TestGetTransportZone_WhenMultipleMatchExist_ReturnsByMatching_CountryCityStateCode_AndIgnoresZoneHub()
		{
			var refCityTownVictoriaVIC = new RefCityTown("Victoria VIC", "VIC", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportProviders = GetTransportProviders(new string[] { "OPS" }, zoneHubLocation: refCityTownVictoriaVIC);
			var rateTransportZone = CreateTransportZoneWithCountryAndCity(rateTransportProviders[0], refCityTownVictoriaVIC);

			var result = GetTransportZone("AU", "3220", "Victoria VIC", "VIC", "OPS");

			AssertEquals(0, result.Length);
		}

		public void TestGetTransportZone_WhenMultipleMatchExist_ReturnsByMatching_PostCodeRange_AndIgnoresZoneHub()
		{
			var refCityTownVictoriaVIC = new RefCityTown("Victoria VIC", "VIC", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportProviders = GetTransportProviders(new string[] { "OPS" }, zoneHubLocation: refCityTownVictoriaVIC);
			var rateTransportZoneVicPostCodesRange = CreateTransportZoneWithPostCodeRange(rateTransportProviders[0]);

			var result = GetTransportZone("AU", "3220", "Victoria VIC", "VIC", "OPS");

			AssertEquals(0, result.Length);
		}
		public void TestGetTransportZone_IsExcludingPostCode_MatchOnCityName()
		{
			var refCityTownHobart = new RefCityTown("New Hobart", "TAS", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportProvider = new RateTransportProvider("AU", null, "ALL").InsertAndReturnObject(TestConnection);
			var rateTransportZoneHobart = new RateTransportZones("Hobart Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemHobart = new RateTransportZoneItem("AU", rateTransportZoneHobart, refCityTownHobart);
			rateTransportZoneItemHobart.TQ_IsExcludingPostCode = true;
			rateTransportZoneItemHobart.InsertAndReturnObject(TestConnection);

			var result = GetTransportZone("AU", "6000", "New Hobart", "TAS", "ALL");

			AssertEquals("Returns ones zone", 1, result.Length);
			AssertEquals("Match on city name", rateTransportZoneHobart.PK, result[0]);
		}

		public void TestGetTransportZone_IsExcludingPostCode_NoMatchOnPostCode()
		{
			var refPostCodeHobartFrom = new RefPostCode("7000", "AU").InsertAndReturnObject(TestConnection);
			var refPostCodeHobartTo = new RefPostCode("7100", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportProvider = new RateTransportProvider("AU", null, "ALL").InsertAndReturnObject(TestConnection);
			var rateTransportZoneHobart = new RateTransportZones("Hobart Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemHobart = new RateTransportZoneItem("AU", rateTransportZoneHobart, refPostCodeHobartFrom, refPostCodeHobartTo);
			rateTransportZoneItemHobart.TQ_IsExcludingPostCode = true;
			rateTransportZoneItemHobart.InsertAndReturnObject(TestConnection);

			var result = GetTransportZone("AU", "7001", "", "", "ALL");

			AssertEquals("No match on post code when IsExcludingPostCode", 0, result.Length);
		}

		RateTransportZones CreateTransportZoneWithCountryAndCity(RateTransportProvider rateTransportProvider, RefCityTown refCityTownVictoriaVIC)
		{
			var rateTransportZoneVictoria = new RateTransportZones("Victoria Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemVictoriaCBD = new RateTransportZoneItem("AU", rateTransportZoneVictoria, refCityTownVictoriaVIC).InsertAndReturnObject(TestConnection);

			return rateTransportZoneVictoria;
		}
		RateTransportZones CreateTransportZoneWithPostCodeRange(RateTransportProvider rateTransportProvider)
		{
			var rateTransportZoneVicPostCodesRange = new RateTransportZones("TEST", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var refPostCodeVicFrom = new RefPostCode("3000", "AU").InsertAndReturnObject(TestConnection);
			var refPostCodeVicTo = new RefPostCode("3300", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportZoneVicPostCodesRangeItem = new RateTransportZoneItem("AU", rateTransportZoneVicPostCodesRange, refPostCodeVicFrom, refPostCodeVicTo).InsertAndReturnObject(TestConnection);

			return rateTransportZoneVicPostCodesRange;
		}

		RateTransportProvider[] GetTransportProviders(string[] zoneTypes, IOrgHeaderSQL zoneOwner = null, RefCityTown zoneHubLocation = null)
		{
			List<RateTransportProvider> providers = new List<RateTransportProvider>();

			foreach (var zoneType in zoneTypes)
			{
				providers.Add(new RateTransportProvider("AU", zoneOwner, zoneType) { TP_R9_ZoneHubLocation = zoneHubLocation }.InsertAndReturnObject(TestConnection));
			}

			return providers.ToArray();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			generator = new SupplierBookingGeneratorForTests(TestConnection);
		}
		SupplierBookingGeneratorForTests generator;

		Guid[] GetTransportZoneWithDefaultZoneType(string countryCode, string postCode, string cityName, string stateCode)
		{
			var result = new List<Guid>();
			var sql = @"SELECT TQ_TZ_DomesticZone FROM dbo.GetTransportZone(@CountryCode, @PostCode, @CityName, @StateCode, default)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CountryCode", SqlDbType.VarChar, 2, countryCode);
				command.AddParameter("@PostCode", SqlDbType.VarChar, 10, postCode);
				command.AddParameter("@CityName", SqlDbType.VarChar, 50, cityName);
				command.AddParameter("@StateCode", SqlDbType.VarChar, 3, stateCode);

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

		Guid[] GetTransportZone(string countryCode, string postCode, string cityName, string stateCode, string zoneType)
		{
			var result = new List<Guid>();
			var sql = @"SELECT TQ_TZ_DomesticZone FROM dbo.GetTransportZone(@CountryCode, @PostCode, @CityName, @StateCode, @ZoneType)";

			using (var command = Db.Connection.Command(sql))
			{
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
