using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.SupplierBookingLine;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.SupplierBookingLine.Testing
{
	[TestedType(typeof(GetDepotAddress))]
	internal sealed class GetDepotAddressTest : DbCreateScriptTest
	{
		public void TestGetDepotAddress()
		{
			generator.SetupTestData();

			var result = GetDepotAddress("DIR", "DLV", "AU", "3560", "", "");
			AssertEquals(1, result.Length);
			AssertEquals("9dbe20cf-1573-4440-a43e-2a07eccddefe", result[0].ToString());

			result = GetDepotAddress("DIR", "DLV", "AU", "", "Adelaide City", "SA");
			AssertEquals(1, result.Length);
			AssertEquals("9dbe20cf-1573-4440-a43e-2a07eccddefe", result[0].ToString());

			result = GetDepotAddress("DIR", "DLV", "AU", "", "Sydney City", "NSW");
			AssertEquals(1, result.Length);
			AssertEquals("9dbe20cf-1573-4440-a43e-2a07eccddefe", result[0].ToString());

			result = GetDepotAddress("DIR,D2D", "DLV", "AU", "", "Sydney City", "");
			AssertEquals(0, result.Length);
		}

		public void TestGetDepotAddress_WithBlankServiceLevelAndNonMatchingServiceLevel()
		{
			generator.SetupTestData(false, true);

			var result = GetDepotAddress("EXP,XYZ", "PIC", "AU", "3560", "", "");
			AssertEquals(1, result.Length);
			AssertEquals("db4f3821-a996-4cc2-afce-d3ebe01e408c", result[0].ToString());
		}

		public void TestGetDepotAddress_WithBlankServiceLevel()
		{
			generator.SetupTestData(true, false);

			var result = GetDepotAddress("EXP,XYZ", "PIC", "AU", "3560", "", "");
			AssertEquals(1, result.Length);
			AssertEquals("a680dfb8-60f9-4c59-a885-aa1eb15d21e5", result[0].ToString());
		}

		public void TestGetDepotAddress_PassingInBlankServiceLevel()
		{
			generator.SetupTestData(true, false);

			var result = GetDepotAddress(string.Empty, "PIC", "AU", "3560", "", "");
			AssertEquals(1, result.Length);
			AssertEquals("a680dfb8-60f9-4c59-a885-aa1eb15d21e5", result[0].ToString());
		}

		public void TestGetDepotAddress_PassingInNullServiceLevel()
		{
			generator.SetupTestData(true, false);

			var result = GetDepotAddress(null, "PIC", "AU", "3560", "", "");
			AssertEquals(1, result.Length);
			AssertEquals("a680dfb8-60f9-4c59-a885-aa1eb15d21e5", result[0].ToString());
		}

		public void TestGetDepotAddress_WhenZoneTypeInDBIsOPS()
		{
			TestGetDepotAddress_WhenZoneTypeInDBIs("OPS");
		}

		public void TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIsALL()
		{
			TestGetDepotAddress_WhenZoneTypeInDBIs("ALL");
		}

		public void TestGetTransportZoneWithZoneTypeOPS_WhenZoneTypeInDBIsRAT()
		{
			TestGetDepotAddress_WhenZoneTypeInDBIs("RAT", false);
		}

		void TestGetDepotAddress_WhenZoneTypeInDBIs(string zoneType, bool hasResult = true)
		{
			var orgHeader = new OrgHeader("TST").InsertAndReturnObject(TestConnection);
			var orgAddress = new OrgAddress(orgHeader, "TST", "1 TST ST").InsertAndReturnObject(TestConnection);
			var refCityTownPerth = new RefCityTown("Perth City", "WA", "AU").InsertAndReturnObject(TestConnection);
			var refPostCodeHobartFrom = new RefPostCode("7000", "AU").InsertAndReturnObject(TestConnection);
			var refPostCodeHobartTo = new RefPostCode("7100", "AU").InsertAndReturnObject(TestConnection);
			var rateTransportProvider = new RateTransportProvider("AU", null, zoneType).InsertAndReturnObject(TestConnection);
			var rateTransportZoneHobart = new RateTransportZones("Hobart Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZonePerth = new RateTransportZones("Perth Metro", rateTransportProvider).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemHobart = new RateTransportZoneItem("AU", rateTransportZoneHobart, refPostCodeHobartFrom, refPostCodeHobartTo).InsertAndReturnObject(TestConnection);
			var rateTransportZoneItemPerth = new RateTransportZoneItem("AU", rateTransportZonePerth, refCityTownPerth).InsertAndReturnObject(TestConnection);

			var portHubSelection = new PortHubSelection("PIC", "DIR", "ALL", orgAddress).InsertAndReturnObject(TestConnection);
			var portHubZonePivotHobart = new PortHubZonePivot(portHubSelection, rateTransportZoneHobart, "DIR").InsertAndReturnObject(TestConnection);
			var portHubZonePivotPerth = new PortHubZonePivot(portHubSelection, rateTransportZonePerth, "DIR").InsertAndReturnObject(TestConnection);

			var result = GetDepotAddress("", "PIC", "AU", "7001", "", "");
			if (hasResult)
			{
				AssertEquals(1, result.Length);
				AssertEquals(orgAddress.PK, result[0]);
			}
			else
			{
				AssertEquals(0, result.Length);
			}

			result = GetDepotAddress("", "PIC", "AU", "", "Perth City", "WA");
			if (hasResult)
			{
				AssertEquals(1, result.Length);
				AssertEquals(orgAddress.PK, result[0]);
			}
			else
			{
				AssertEquals(0, result.Length);
			}

			result = GetDepotAddress("", "PIC", "AU", "", "Perth City", "");
			AssertEquals(0, result.Length);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			generator = new SupplierBookingGeneratorForTests(TestConnection);
		}
		SupplierBookingGeneratorForTests generator;

		Guid[] GetDepotAddress(string serviceLevelList, string direction, string countryCode, string postCode, string cityName, string stateCode)
		{
			var result = new List<Guid>();
			var sql = @"SELECT DepotAddressPk FROM dbo.GetDepotAddress(@ServiceLevelList, @Direction, @CountryCode, @PostCode, @CityName, @StateCode)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@ServiceLevelList", SqlDbType.VarChar, 8000, (object)serviceLevelList ?? DBNull.Value);
				command.AddParameter("@Direction", SqlDbType.VarChar, 3, direction);
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
		#endregion
	}
}

