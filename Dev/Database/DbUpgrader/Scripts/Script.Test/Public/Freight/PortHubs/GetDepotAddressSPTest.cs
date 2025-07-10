using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.PortHubs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.PortHubs.Testing
{
	[TestedType(typeof(GetDepotAddressSP))]
	internal sealed class GetDepotAddressSPTest : DbCreateScriptTest
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

			using (var command = Db.Connection.Command("GetDepotAddressSP"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@ServiceLevelList", SqlDbType.VarChar, serviceLevelList);
				command.AddParameter("@Direction", SqlDbType.VarChar, direction);
				command.AddParameter("@CountryCode", SqlDbType.VarChar, countryCode);
				command.AddParameter("@PostCode", SqlDbType.VarChar, postCode);
				command.AddParameter("@CityName", SqlDbType.VarChar, cityName);
				command.AddParameter("@StateCode", SqlDbType.VarChar, stateCode);

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
