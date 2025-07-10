using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.SupplierBookingLine;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.SupplierBookingLine.Testing
{
	[TestedType(typeof(UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo))]
	internal sealed class UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfoTest : DbCreateScriptTest
	{
		[DeveloperOnlyTest]
		public void TestUpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo_WithExactMatchByServiceLevel()
		{
			generator.SetupTestData();

			var consignorPk = generator.GenerateAddress();
			var dispatchOrgAddressPK = generator.InsertOrgAddress("DISPORG", "Dispatch Address", "Melbourne", "VIC", "3560", "AUMEL");

			var headerPk1 = generator.NewSupplierBookingHeader("Test1", false, consignorPk, dispatchOrgAddressPK, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var linePk11 = generator.NewSupplierBookingLine(headerPk1, "Test11", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", "Test Address 11", "Melbourne", "VIC", "3560", "AU", "COU");

			var linePk12 = generator.NewSupplierBookingLine(headerPk1, "Test12", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", "Test Address 12", "Adelaide City", "SA", "6024", "AU", "DIR");

			var linePk13 = generator.NewSupplierBookingLine(headerPk1, "Test12", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", "Test Address 11", "Melbourne", "VIC", "3565", "AU", "");

			var headerPk2 = generator.NewSupplierBookingHeader("Test2", false, consignorPk, dispatchOrgAddressPK, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var linePk21 = generator.NewSupplierBookingLine(headerPk2, "Test21", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", "Test Address 21", "Sydney City", "NSW", "", "AU", "DIR");

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk1);

			var linesResults = GetLinesDepotAddressAndCarrierInfo(headerPk1).ToArray();
			AssertEquals(3, linesResults.Length);
			AssertEquals(linePk11, linesResults[0].Item1);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), linesResults[0].Item2);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), linesResults[0].Item3);
			AssertEquals("D2D", linesResults[0].Item4);
			AssertEquals(linePk12, linesResults[1].Item1);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), linesResults[1].Item2);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), linesResults[1].Item3);
			AssertEquals("TSP", linesResults[1].Item4);
			AssertEquals(linePk13, linesResults[2].Item1);
			AssertEquals(string.Empty, linesResults[2].Item2.ToString());
			AssertEquals(string.Empty, linesResults[2].Item3.ToString());
			AssertEquals(string.Empty, linesResults[2].Item4.ToString());

			var headerResults = GetHeaderDepotAddress(headerPk1).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk1, headerResults[0].Item1);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), headerResults[0].Item2);

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk2);

			linesResults = GetLinesDepotAddressAndCarrierInfo(headerPk2).ToArray();
			AssertEquals(1, linesResults.Length);
			AssertEquals(linePk21, linesResults[0].Item1);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), linesResults[0].Item2);
			AssertEquals(new Guid("01B40E8F-D697-4551-AE6B-E5606A86469A"), linesResults[0].Item3);
			AssertEquals("DEF", linesResults[0].Item4);

			headerResults = GetHeaderDepotAddress(headerPk2).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk2, headerResults[0].Item1);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), headerResults[0].Item2);
		}

		[DeveloperOnlyTest]
		public void TestUpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo_WithBlankServiceLevelAndNonMatchingServiceLevel()
		{
			generator.SetupTestData(false, true);

			var consignorPk = generator.GenerateAddress();
			var dispatchOrgAddressPK = generator.InsertOrgAddress("DISPORG", "Dispatch Address", "Melbourne", "VIC", "3560", "AUMEL");

			var headerPk = generator.NewSupplierBookingHeader("Test2", false, consignorPk, dispatchOrgAddressPK, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var linePk = generator.NewSupplierBookingLine(headerPk, "Test21", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", "Test Address 21", "Sydney City", "NSW", "", "AU", "EXP");

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			var linesResults = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, linesResults.Length);
			AssertEquals(linePk, linesResults[0].Item1);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), linesResults[0].Item2);
			AssertEquals(new Guid("01B40E8F-D697-4551-AE6B-E5606A86469A"), linesResults[0].Item3);
			AssertEquals("DEF", linesResults[0].Item4);

			var headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(new Guid("DB4F3821-A996-4CC2-AFCE-D3EBE01E408C"), headerResults[0].Item2);
		}

		[DeveloperOnlyTest]
		public void TestUpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo_WithBlankServiceLevel()
		{
			generator.SetupTestData(true, false);

			var consignorPk = generator.GenerateAddress();
			var dispatchOrgAddressPK = generator.InsertOrgAddress("DISPORG", "Dispatch Address", "Melbourne", "VIC", "3560", "AUMEL");

			var headerPk = generator.NewSupplierBookingHeader("Test2", false, consignorPk, dispatchOrgAddressPK, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var linePk = generator.NewSupplierBookingLine(headerPk, "Test21", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks",
				1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", "Test Address 21", "Sydney City", "NSW", "", "AU", "EXP");

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			var linesResults = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, linesResults.Length);
			AssertEquals(linePk, linesResults[0].Item1);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), linesResults[0].Item2);
			AssertEquals(new Guid("01B40E8F-D697-4551-AE6B-E5606A86469A"), linesResults[0].Item3);
			AssertEquals("DEF", linesResults[0].Item4);

			var headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(new Guid("A680DFB8-60F9-4C59-A885-AA1EB15D21E5"), headerResults[0].Item2);
		}

		public void TestUpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo_LinesWithPackTypeAndDispatchDepotAddressSpecified()
		{
			var generator = new SupplierBookingGeneratorForTests(TestConnection);
			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var dispatchDepotAddress1 = generator.GenerateAddress();
			var dispatchDepotAddress2 = generator.GenerateAddress();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelection(TestConnection, "DIR", "PIC", "ALL", "", dispatchDepotAddress1)
				.AddZone("Z1", carrier1, "D2D")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelection(TestConnection, "EXP", "DLV", "ALL", "AAA", dispatchDepotAddress2)
				.AddZone("Z4", carrier2, "DIR")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var headerPk = generator.NewSupplierBookingHeader("Test2", false, generator.GenerateAddress(), dispatchDepotAddress2, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var linePk = generator.NewSupplierBookingLine(headerPk, "Test21", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks", 1, DateTime.Now, "ABC",
				DateTime.Now, "ABC", "GB", "AU", "Test Address 21", "Kiev", "KO", "", "UA", "EXP", "", "AAA");
			var consignee = generator.InsertJobDocAddress("Test Address 21", "Kiev", "KO", "", "UA", "CEA", linePk);

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			var lines = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, lines.Length);
			AssertEquals(linePk, lines[0].Item1);
			AssertEquals(depotAddress2, lines[0].Item2);
			AssertEquals(carrier3, lines[0].Item3);
			AssertEquals("TSP", lines[0].Item4);
		}

		[DeveloperOnlyTest]
		public void TestUpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo_DoesNotSetOriginDepotWhenNoLinesExist()
		{
			var generator = new SupplierBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();

			var dispatchDepotAddress1 = generator.GenerateAddress();
			var dispatchDepotAddress2 = generator.InsertOrgAddress("DISPORG", "Dispatch Address", "Melbourne", "VIC", "3550", "AUMEL");

			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			var headerPk = generator.NewSupplierBookingHeader("Test2", false, generator.GenerateAddress(), dispatchDepotAddress2, DateTime.Now, "ABC", DateTime.Now, "ABC");

			var headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(DBNull.Value, headerResults[0].Item2);

			var lines = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(0, lines.Length);

			depotAddress1.CreatePortAndDepotSelection(TestConnection, "EXP", "PIC", "ALL", "AAA", dispatchDepotAddress1)
							.AddZone("Z1", carrier1, "EXP")
								.AddZoneItem("AU", 3500, 3600)
								.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelection(TestConnection, "EXP", "DLV", "ALL", "AAA", dispatchDepotAddress2)
				.AddZone("Z4", carrier2, "TSP")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(DBNull.Value, headerResults[0].Item2);

			var linePk = generator.NewSupplierBookingLine(headerPk, "Test21", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks", 1, DateTime.Now, "ABC", DateTime.Now, "ABC", "GB", "AU", "Test Address 21", "Kiev", "KO", "", "UA", "EXP", "", "AAA");

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(depotAddress1, headerResults[0].Item2);

			generator.SetOriginDepot(headerPk, dispatchDepotAddress2);
			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			lines = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, lines.Length);
			AssertEquals(linePk, lines[0].Item1);
			AssertEquals(depotAddress2, lines[0].Item2);
			AssertEquals(carrier3, lines[0].Item3);
			AssertEquals("TSP", lines[0].Item4);
		}

		[DeveloperOnlyTest]
		public void TestUpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo_ResetsExistingValuesButKeepsOriginDepot()
		{
			var generator = new SupplierBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var depotAddress3 = generator.GenerateAddress();
			var depotAddress4 = generator.GenerateAddress();

			var dispatchDepotAddress1 = generator.GenerateAddress();
			var dispatchDepotAddress2 = generator.InsertOrgAddress("DISPORG", "Dispatch Address", "Melbourne", "VIC", "3550", "AUMEL");

			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelection(TestConnection, "DIR", "PIC", "ALL", "", dispatchDepotAddress1)
				.AddZone("Z1", carrier1, "XXX")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelection(TestConnection, "EXP", "DLV", "ALL", "AAA", dispatchDepotAddress2)
				.AddZone("Z4", carrier2, "YYY")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "ZZZ")
					.AddZoneItem("Kiev", "KK", "UA")
					.AddZoneItem("AU", 200, 300);

			var headerPk = generator.NewSupplierBookingHeader("Test2", false, generator.GenerateAddress(), dispatchDepotAddress2, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var linePk = generator.NewSupplierBookingLine(headerPk, "Test21", 100.0m, "AUD", 123.45, "KG", 99.9, "M3", "TestDescription", "TestMarks", 1, DateTime.Now, "ABC", DateTime.Now,
				"ABC", "GB", "AU", "Test Address 21", "Kiev", "KO", "", "UA", "EXP", "", "AAA");

			generator.SetOriginDepot(headerPk, depotAddress3);
			generator.SetDestinationDepotAndCarrierInfo(linePk, depotAddress4, carrier1, "COU");

			var lines = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, lines.Length);
			AssertEquals(linePk, lines[0].Item1);
			AssertEquals(depotAddress4, lines[0].Item2);
			AssertEquals(carrier1, lines[0].Item3);
			AssertEquals("COU", lines[0].Item4);

			var headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(depotAddress3, headerResults[0].Item2);

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			lines = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, lines.Length);
			AssertEquals(linePk, lines[0].Item1);
			AssertEquals(DBNull.Value, lines[0].Item2);
			AssertEquals(DBNull.Value, lines[0].Item3);
			AssertEquals(string.Empty, lines[0].Item4);

			headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(depotAddress3, headerResults[0].Item2);

			generator.ResetOriginDepotToBlank(headerPk);

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			lines = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, lines.Length);
			AssertEquals(linePk, lines[0].Item1);
			AssertEquals(DBNull.Value, lines[0].Item2);
			AssertEquals(DBNull.Value, lines[0].Item3);
			AssertEquals(string.Empty, lines[0].Item4);

			headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(DBNull.Value, headerResults[0].Item2);

			depotAddress1.CreatePortAndDepotSelection(TestConnection, "EXP", "PIC", "ALL", "AAA", dispatchDepotAddress1)
				.AddZone("Z1", carrier1, "EXP")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelection(TestConnection, "EXP", "DLV", "ALL", "AAA", dispatchDepotAddress2)
				.AddZone("Z4", carrier2, "TSP")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			headerResults = GetHeaderDepotAddress(headerPk).ToArray();
			AssertEquals(1, headerResults.Length);
			AssertEquals(headerPk, headerResults[0].Item1);
			AssertEquals(depotAddress1, headerResults[0].Item2);

			generator.SetOriginDepot(headerPk, dispatchDepotAddress2);

			Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(headerPk);

			lines = GetLinesDepotAddressAndCarrierInfo(headerPk).ToArray();
			AssertEquals(1, lines.Length);
			AssertEquals(linePk, lines[0].Item1);
			AssertEquals(depotAddress2, lines[0].Item2);
			AssertEquals(carrier3, lines[0].Item3);
			AssertEquals("TSP", lines[0].Item4);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			generator = new SupplierBookingGeneratorForTests(TestConnection);
		}
		SupplierBookingGeneratorForTests generator;

		static List<Tuple<object, object, object, object>> GetLinesDepotAddressAndCarrierInfo(Guid headerPK)
		{
			var result = new List<Tuple<object, object, object, object>>();

			using (var command = Db.Connection.Command("SELECT DL_PK, DL_OA_DestinationDepot, DL_OH_LastMileCarrier, DL_PL_NKCarrierServiceLevel FROM dbo.SupplierBookingLine WHERE DL_DH_BookingHeader = '" + headerPK + "'"))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var tuple = Tuple.Create(reader.GetValue(0), reader.GetValue(1), reader.GetValue(2), reader.GetValue(3));
						result.Add(tuple);
					}
				}
			}

			return result;
		}

		static List<Tuple<object, object>> GetHeaderDepotAddress(Guid headerPK)
		{
			var result = new List<Tuple<object, object>>();

			using (var command = Db.Connection.Command("SELECT DH_PK, DH_OA_OriginDepot FROM dbo.SupplierBookingHeader WHERE DH_PK = '" + headerPK + "'"))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var tuple = Tuple.Create(reader.GetValue(0), reader.GetValue(1));
						result.Add(tuple);
					}
				}
			}

			return result;
		}

		static void Run_UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo(Guid? headerPK)
		{
			using (var command = Db.Connection.Command("UpdateSupplierBookingLinesWithDepotAddressAndCarrierInfo"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@HeaderPk", SqlDbType.UniqueIdentifier, headerPK);

				command.ExecuteNonQuery();
			}
		}
		#endregion
	}
}
