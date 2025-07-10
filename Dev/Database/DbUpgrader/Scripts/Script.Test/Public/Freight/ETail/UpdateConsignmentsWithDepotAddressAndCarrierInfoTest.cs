using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using Enterprise.Build.Database.Script.Public.Glow.TestHelpers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	[TestedType(typeof(UpdateConsignmentsWithDepotAddressAndCarrierInfo))]
	sealed class UpdateConsignmentBookingWithDepotAddressAndCarrierInfoTest : DbCreateScriptTest
	{
		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_BlankDispatchDepot_ExactUndgClass_ExactServiceLevel_MatchFromPostCodeAndToPostCode()
		{
			generator.SetupTestData();

			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");
			var shipmentPK = generator.GenerateJobShipment("M00000001", "COU", originDepotPK);

			var cluster = 1;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00001");
			var consignmentPK = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment1", "Waybill1", "ShipRef1", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK, consignmentsResults[0].PK);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("D2D", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("13579", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_BlankDispatchDepot_ExactUndgClass_ExactServiceLevel_MatchFromPostCodeOnly()
		{
			generator.SetupTestData();

			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");
			var shipmentPK = generator.GenerateJobShipment("M00000001", "COU", originDepotPK);

			var cluster = 1;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00001");
			var consigneePostCodeNotMatchTransportZoneFromPostCodeConsignmentPK = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment1", "Waybill1", "ShipRef1", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "", "VIC", "4200", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consigneePostCodeNotMatchTransportZoneFromPostCodeConsignmentPK });

			var consigneePostCodeNotMatchTransportZoneFromPostCodeConsignment = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK)
				.First(c => (Guid)c.PK == consigneePostCodeNotMatchTransportZoneFromPostCodeConsignmentPK);

			AssertEquals("last mile carrier not populated when transport zone FromPostcode not matched", System.DBNull.Value, consigneePostCodeNotMatchTransportZoneFromPostCodeConsignment.LastMileCarrier);

			var consigneePostCodeMatchTransportZoneFromPostCodeConsignmentPK = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment2", "Waybill2", "ShipRef2", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "", "VIC", "4500", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consigneePostCodeMatchTransportZoneFromPostCodeConsignmentPK });
			var consigneePostCodeMatchTransportZoneFromPostCodeConsignment = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK)
				.First(c => (Guid)c.PK == consigneePostCodeMatchTransportZoneFromPostCodeConsignmentPK);

			CombineAssertions("last mile carrier details populated when transport zone FromPostcode matched", () =>
			{
				AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consigneePostCodeMatchTransportZoneFromPostCodeConsignment.DestinationDepot);
				AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consigneePostCodeMatchTransportZoneFromPostCodeConsignment.LastMileCarrier);
				AssertEquals("D2D", consigneePostCodeMatchTransportZoneFromPostCodeConsignment.LastMileCarrierServiceLevel);
				AssertEquals("13579", consigneePostCodeMatchTransportZoneFromPostCodeConsignment.CarrierAccountNumber);
				AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consigneePostCodeMatchTransportZoneFromPostCodeConsignment.LastMileCarrierBookingAgent);
			});
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_BlankDispatchDepot_ExactUndgClass_ExactServiceLevel_MatchCityTown()
		{
			generator.SetupTestData();

			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");
			var shipmentPK = generator.GenerateJobShipment("M00000001", "DIR", originDepotPK);

			var cluster = 2;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00001");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment2", "Waybill2", "ShipRef2", 1, "Books", "2", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Adelaide City", "SA", "", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("TSP", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("13579", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_BlankDispatchDepot_ExactUndgClass_BlankServiceLevel()
		{
			generator.SetupTestData();

			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");
			var shipmentPK = generator.GenerateJobShipment("M00000003", "CNF", originDepotPK);

			var cluster = 3;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00003");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment3", "Waybill3", "ShipRef3", 1, "Books", "3", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("4ED2BF6C-A797-4946-9EC7-01D4DBF7A001"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_BlankDispatchDepot_AllUndgClass_ExactServiceLevel()
		{
			generator.SetupTestData();

			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");
			var shipmentPK = generator.GenerateJobShipment("M00000004", "STD", originDepotPK);

			var cluster = 4;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00004");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment4", "Waybill4", "ShipRef4", 1, "Books", "ALL", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("4ED2BF6C-A797-4946-9EC7-01D4DBF7A001"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_BlankDispatchDepot_AllUndgClass_BlankServiceLevel()
		{
			generator.SetupTestData();

			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");
			var shipmentPK = generator.GenerateJobShipment("M00000005", "", originDepotPK);

			var cluster = 5;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00005");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment5", "Waybill5", "ShipRef5", 1, "Books", "ALL", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("4ED2BF6C-A797-4946-9EC7-01D4DBF7A001"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_ExactDispatchDepot_ExactUndgClass_ExactServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var shipmentPK = generator.GenerateJobShipment("M00000006", "EXP", originDepot2);

			var cluster = 6;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_ExactDispatchDepot_ExactUndgClass_BlankServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var shipmentPK = generator.GenerateJobShipment("M00000006", "EXP", originDepot2);

			var cluster = 6;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_ExactDispatchDepot_AllUndgClass_ExactServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "ALL", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var shipmentPK = generator.GenerateJobShipment("M00000006", "EXP", originDepot2);

			var cluster = 6;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_ExactDispatchDepot_AllUndgClass_BlankServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "", "DLV", "ALL", "AAA", originDepot2, "ALL", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var shipmentPK = generator.GenerateJobShipment("M00000006", "EXP", originDepot2);

			var cluster = 6;
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_ExactDispatchDepot_AllUndgClass_BlankAndExactServiceLevel_ForMultipleRowNumbers()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var originDepotPK = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "", "DLV", "ALL", "AAA", originDepotPK, "ALL", agent1)
				.AddZone("Z1", carrier1, "DIR", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3700);

			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepotPK, "ALL", agent2)
				.AddZone("Z2", carrier2, "EXP", accountNumber: "24680")
					.AddZoneItem("Melbourne Metro", "VIC", "AU");

			var cluster = 6;
			var shipmentPK = generator.GenerateJobShipment("M00000006", "EXP", originDepotPK);
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef50", 1, "Books", "6", "CNF",
				"DDD", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("EXP", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_ExactDispatchDepot_NotMatchUndgClass_NotMatchServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "COU", "DLV", "ALL", "AAA", originDepot2, "1", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var cluster = 6;
			var shipmentPK = generator.GenerateJobShipment("M00000006", "EXP", originDepot2);
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(DBNull.Value, consignmentsResults[0].DestinationDepot);
			AssertEquals(DBNull.Value, consignmentsResults[0].LastMileCarrier);
			AssertEquals(string.Empty, consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(DBNull.Value, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_MultipleConsignments()
		{
			#region Setup

			generator.SetupTestData();

			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var cluster = 1;
			var shipmentPK = generator.GenerateJobShipment("M00000001", "COU", originDepotPK);
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment1", "Waybill1", "ShipRef1", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			var consignmentPK2 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment2", "Waybill2", "ShipRef2", 1, "Books", "1", "CNF",
				"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			var consignmentPK3 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment3", "Waybill3", "ShipRef3", 1, "Books", "1", "CNF",
				"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			var consignmentPK4 = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment4", "Waybill4", "ShipRef4", 1, "Books", "1", "CNF",
				"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			#endregion

			var consignmentsToUpdate = new List<Guid>() { consignmentPK1, consignmentPK2, consignmentPK3 };
			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(consignmentsToUpdate);

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();

			AssertEquals(4, consignmentsResults.Length);

			for (int i = 0; i < consignmentsToUpdate.Count; i++)
			{
				CombineAssertions(delegate
				{
					AssertEquals(consignmentsToUpdate[i], consignmentsResults[i].PK);
					AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consignmentsResults[i].DestinationDepot);
					AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[i].LastMileCarrier);
					AssertEquals("D2D", consignmentsResults[i].LastMileCarrierServiceLevel);
					AssertEquals("13579", consignmentsResults[i].CarrierAccountNumber);
					AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consignmentsResults[i].LastMileCarrierBookingAgent);
				});
			}

			CombineAssertions("Should not have processed because not in list which was passed to the procedure", delegate
			{
				AssertEquals(consignmentPK4, consignmentsResults[3].PK);
				AssertEquals(DBNull.Value, consignmentsResults[3].DestinationDepot);
				AssertEquals(DBNull.Value, consignmentsResults[3].LastMileCarrier);
				AssertEquals(string.Empty, consignmentsResults[3].LastMileCarrierServiceLevel);
				AssertEquals(DBNull.Value, consignmentsResults[3].LastMileCarrierBookingAgent);
			});
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_WhenOnlyPopulateEmptyLastMileCarrierAndDepotDetailsIsTrue_ThenDoNotOverwriteLastMileCarrierAndDepotDetails()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot1, "6", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600);

			var cluster = 7;
			var shipmentPK = generator.GenerateJobShipment("M00000007", "EXP", originDepot2);
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00006");
			var consignmentPK = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Precondition:", depotAddress2, consignmentsResults[0].DestinationDepot);
				AssertEquals("Precondition:", carrier2, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Precondition:", "DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Precondition:", "24680", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Precondition:", agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
			});

			generator.UpdateJobShipmentExportReceivingDepot(shipmentPK, originDepot1);
			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK }, true);
			consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();

			CombineAssertions("Expected the following fields not to be overwritten.", () =>
			{
				AssertEquals("ConsigmentPK:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Depot Address:", depotAddress2, consignmentsResults[0].DestinationDepot);
				AssertEquals("Last Mile Carrier:", carrier2, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Service Level:", "DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Carrier Account Number:", "24680", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Booking Agent:", agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
			});
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromConsignmentHeader_WhenOnlyPopulateEmptyLastMileCarrierAndDepotDetailsIsFalse_ThenOverwriteLastMileCarrierAndDepotDetails()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot1, "6", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600);

			var cluster = 7;
			var shipmentPK = generator.GenerateJobShipment("M00000007", "EXP", originDepot2);
			var consignmentHeaderPK = generator.GenerateHVLVConsignmentHeader(cluster, shipmentPK, DateTime.Now, "ABC", DateTime.Now, "ABC", "HCH00007");
			var consignmentPK = generator.GenerateHvlvConsignment_WithConsignmentHeader(cluster, consignmentHeaderPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Precondition:", depotAddress2, consignmentsResults[0].DestinationDepot);
				AssertEquals("Precondition:", carrier2, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Precondition:", "DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Precondition:", "24680", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Precondition:", agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
			});

			generator.UpdateJobShipmentExportReceivingDepot(shipmentPK, originDepot1);
			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK }, false);
			consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(consignmentHeaderPK).ToArray();

			CombineAssertions("Expected the following fields to be overwritten.", () =>
			{
				AssertEquals("ConsigmentPK:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Depot Address:", depotAddress1, consignmentsResults[0].DestinationDepot);
				AssertEquals("Last Mile Carrier:", carrier1, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Service Level:", "D2D", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Carrier Account Number:", "13579", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Booking Agent:", agent1, consignmentsResults[0].LastMileCarrierBookingAgent);
			});
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_BlankDispatchDepot_ExactUndgClass_ExactServiceLevel_MatchFromPostCodeAndToPostCode()
		{
			generator.SetupTestData();

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var cluster = 1;
			var headerPk1 = generator.NewHvlvBookingHeader(cluster, "M00000001", billToPartyPK, "COU", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, headerPk1, "Consignment1", "Waybill1", "ShipRef1", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("D2D", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("13579", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_BlankDispatchDepot_ExactUndgClass_ExactServiceLevel_MatchFromPostCodeOnly()
		{
			generator.SetupTestData();

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var cluster = 1;
			var bookingHeaderPK = generator.NewHvlvBookingHeader(cluster, "M00000001", billToPartyPK, "COU", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consigneePostCodeNotMatchTransportZoneFromPostCodeConsignmentPK = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, bookingHeaderPK, "Consignment1", "Waybill1", "ShipRef1", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "4200", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consigneePostCodeNotMatchTransportZoneFromPostCodeConsignmentPK });
			var consigneePostCodeNotMatchTransportZoneFromPostCodeConsignment = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(bookingHeaderPK)
				.First(c => (Guid)c.PK == consigneePostCodeNotMatchTransportZoneFromPostCodeConsignmentPK);

			AssertEquals("last mile carrier not populated when transport zone FromPostcode not matched", System.DBNull.Value, consigneePostCodeNotMatchTransportZoneFromPostCodeConsignment.LastMileCarrier);

			var consigneePostCodeMatchTransportZoneFromPostCodeConsignmentPK = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, bookingHeaderPK, "Consignment2", "Waybill2", "ShipRef2", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "4500", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consigneePostCodeMatchTransportZoneFromPostCodeConsignmentPK });
			var consigneePostCodeMatchTransportZoneFromPostCodeConsignment = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(bookingHeaderPK)
				.First(c => (Guid)c.PK == consigneePostCodeMatchTransportZoneFromPostCodeConsignmentPK);

			CombineAssertions("last mile carrier details populated when transport zone FromPostcode matched", () =>
			{
				AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consigneePostCodeMatchTransportZoneFromPostCodeConsignment.DestinationDepot);
				AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consigneePostCodeMatchTransportZoneFromPostCodeConsignment.LastMileCarrier);
				AssertEquals("D2D", consigneePostCodeMatchTransportZoneFromPostCodeConsignment.LastMileCarrierServiceLevel);
				AssertEquals("13579", consigneePostCodeMatchTransportZoneFromPostCodeConsignment.CarrierAccountNumber);
				AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consigneePostCodeMatchTransportZoneFromPostCodeConsignment.LastMileCarrierBookingAgent);
			});
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_BlankDispatchDepot_ExactUndgClass_ExactServiceLevel_MatchCityTown()
		{
			generator.SetupTestData();

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var cluster = 2;
			var headerPk1 = generator.NewHvlvBookingHeader(cluster, "M00000002", billToPartyPK, "DIR", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, headerPk1, "Consignment2", "Waybill2", "ShipRef2", 1, "Books", "2", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Adelaide City", "SA", "", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("TSP", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("13579", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_BlankDispatchDepot_ExactUndgClass_BlankServiceLevel()
		{
			generator.SetupTestData();

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var clusterKey = 3;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000003", billToPartyPK, "CNF", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment3", "Waybill3", "ShipRef3", 1, "Books", "3", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("4ED2BF6C-A797-4946-9EC7-01D4DBF7A001"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_BlankDispatchDepot_AllUndgClass_ExactServiceLevel()
		{
			generator.SetupTestData();

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var clusterKey = 4;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000004", billToPartyPK, "STD", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment4", "Waybill4", "ShipRef4", 1, "Books", "ALL", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("4ED2BF6C-A797-4946-9EC7-01D4DBF7A001"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_BlankDispatchDepot_AllUndgClass_BlankServiceLevel()
		{
			generator.SetupTestData();

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var clusterKey = 5;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000005", billToPartyPK, "", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment5", "Waybill5", "ShipRef5", 1, "Books", "ALL", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(new Guid("0E50FD22-8FCE-4EE8-93EC-B8FE81C806DA"), consignmentsResults[0].DestinationDepot);
			AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(new Guid("4ED2BF6C-A797-4946-9EC7-01D4DBF7A001"), consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_ExactDispatchDepot_ExactUndgClass_ExactServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var billToPartyPK = generator.GenerateAddress();

			var clusterKey = 6;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000006", billToPartyPK, "EXP", originDepot2, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_ExactDispatchDepot_ExactUndgClass_BlankServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var billToPartyPK = generator.GenerateAddress();

			var clusterKey = 6;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000006", billToPartyPK, "EXP", originDepot2, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_ExactDispatchDepot_AllUndgClass_ExactServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "ALL", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var billToPartyPK = generator.GenerateAddress();

			var clusterKey = 6;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000006", billToPartyPK, "EXP", originDepot2, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_ExactDispatchDepot_AllUndgClass_BlankServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "", "DLV", "ALL", "AAA", originDepot2, "ALL", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var billToPartyPK = generator.GenerateAddress();

			var clusterKey = 6;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000006", billToPartyPK, "EXP", originDepot2, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_ExactDispatchDepot_AllUndgClass_BlankAndExactServiceLevel_ForMultipleRowNumbers()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "", "DLV", "ALL", "AAA", originDepotPK, "ALL", agent1)
				.AddZone("Z1", carrier1, "DIR", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3700);

			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepotPK, "ALL", agent2)
				.AddZone("Z2", carrier2, "EXP", accountNumber: "24680")
					.AddZoneItem("Melbourne Metro", "VIC", "AU");

			var clusterKey = 6;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000050", billToPartyPK, "EXP", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment6", "Waybill6", "ShipRef50", 1, "Books", "6", "CNF",
				"DDD", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(depotAddress2, consignmentsResults[0].DestinationDepot);
			AssertEquals(carrier2, consignmentsResults[0].LastMileCarrier);
			AssertEquals("EXP", consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals("24680", consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_ExactDispatchDepot_NotMatchUndgClass_NotMatchServiceLevel()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();
			var carrier3 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "DIR", "PIC", "ALL", "", originDepot1, "", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 1500, 3500);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "COU", "DLV", "ALL", "AAA", originDepot2, "1", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600)
					.AddZoneItem("AU", 3600, 3700)
				.AddZone("Z5", carrier3, "TSP", accountNumber: "99999")
					.AddZoneItem("Kiev", "KO", "UA")
					.AddZoneItem("AU", 200, 300);

			var billToPartyPK = generator.GenerateAddress();

			var clusterKey = 6;
			var headerPk1 = generator.NewHvlvBookingHeader(clusterKey, "M00000006", billToPartyPK, "EXP", originDepot2, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPk1, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK1 });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			AssertEquals(consignmentPK1, consignmentsResults[0].PK);
			AssertEquals(DBNull.Value, consignmentsResults[0].DestinationDepot);
			AssertEquals(DBNull.Value, consignmentsResults[0].LastMileCarrier);
			AssertEquals(string.Empty, consignmentsResults[0].LastMileCarrierServiceLevel);
			AssertEquals(string.Empty, consignmentsResults[0].CarrierAccountNumber);
			AssertEquals(DBNull.Value, consignmentsResults[0].LastMileCarrierBookingAgent);
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_MultipleConsignments()
		{
			#region Setup

			generator.SetupTestData();

			var billToPartyPK = generator.GenerateAddress();
			var originDepotPK = generator.InsertOrgAddress("DISPORG", "Origin Depot", "Melbourne", "VIC", "3560", "AUMEL");

			var cluster = 1;
			var headerPk1 = generator.NewHvlvBookingHeader(cluster, "M00000001", billToPartyPK, "COU", originDepotPK, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK1 = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, headerPk1, "Consignment1", "Waybill1", "ShipRef1", 1, "Books", "1", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			var consignmentPK2 = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, headerPk1, "Consignment2", "Waybill2", "ShipRef2", 1, "Books", "1", "CNF",
				"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			var consignmentPK3 = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, headerPk1, "Consignment3", "Waybill3", "ShipRef3", 1, "Books", "1", "CNF",
				"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			var consignmentPK4 = generator.GenerateHvlvConsignment_WithBookingHeader(cluster, headerPk1, "Consignment4", "Waybill4", "ShipRef4", 1, "Books", "1", "CNF",
				"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
				DateTime.Now, "ABC", DateTime.Now, "ABC");

			#endregion

			var consignmentsToUpdate = new List<Guid>() { consignmentPK1, consignmentPK2, consignmentPK3 };
			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(consignmentsToUpdate);

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPk1).ToArray();

			AssertEquals(4, consignmentsResults.Length);

			for (int i = 0; i < consignmentsToUpdate.Count; i++)
			{
				CombineAssertions(delegate
				{
					AssertEquals(consignmentsToUpdate[i], consignmentsResults[i].PK);
					AssertEquals(new Guid("9DBE20CF-1573-4440-A43E-2A07ECCDDEFE"), consignmentsResults[i].DestinationDepot);
					AssertEquals(new Guid("77EE15A4-849A-48D5-A7E8-9313840E729E"), consignmentsResults[i].LastMileCarrier);
					AssertEquals("D2D", consignmentsResults[i].LastMileCarrierServiceLevel);
					AssertEquals("13579", consignmentsResults[i].CarrierAccountNumber);
					AssertEquals(new Guid("C4C74E6E-0780-47CF-A701-C0E9DBB93B7A"), consignmentsResults[i].LastMileCarrierBookingAgent);
				});
			}

			CombineAssertions("Should not have processed because not in list which was passed to the procedure", delegate
			{
				AssertEquals(consignmentPK4, consignmentsResults[3].PK);
				AssertEquals(DBNull.Value, consignmentsResults[3].DestinationDepot);
				AssertEquals(DBNull.Value, consignmentsResults[3].LastMileCarrier);
				AssertEquals(string.Empty, consignmentsResults[3].LastMileCarrierServiceLevel);
				AssertEquals(DBNull.Value, consignmentsResults[3].LastMileCarrierBookingAgent);
			});
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_WhenOnlyPopulateEmptyLastMileCarrierAndDepotDetailsIsTrue_ThenDoNotOverwriteLastMileCarrierAndDepotDetails()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot1, "6", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600);

			var billToPartyPK = generator.GenerateAddress();

			var clusterKey = 7;
			var headerPK = generator.NewHvlvBookingHeader(clusterKey, "M00000006", billToPartyPK, "EXP", originDepot2, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Precondition:", depotAddress2, consignmentsResults[0].DestinationDepot);
				AssertEquals("Precondition:", carrier2, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Precondition:", "DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Precondition:", "24680", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Precondition:", agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
			});

			generator.UpdateHvlvBookingHeaderOriginDepot(headerPK, clusterKey, originDepot1);
			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK }, true);
			consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPK).ToArray();

			CombineAssertions("Expected the following fields not to be overwritten.", () =>
			{
				AssertEquals("ConsigmentPK:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Depot Address:", depotAddress2, consignmentsResults[0].DestinationDepot);
				AssertEquals("Last Mile Carrier:", carrier2, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Service Level:", "DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Carrier Account Number:", "24680", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Booking Agent:", agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
			});
		}

		public void TestUpdateConsignmentBookingWithDepotAddressAndCarrierInfoFromBookingHeader_WhenOnlyPopulateEmptyLastMileCarrierAndDepotDetailsIsFalse_ThenOverwriteLastMileCarrierAndDepotDetails()
		{
			var generator = new ConsignmentBookingGeneratorForTests(TestConnection);

			var depotAddress1 = generator.GenerateAddress();
			var depotAddress2 = generator.GenerateAddress();
			var originDepot1 = generator.GenerateAddress();
			var originDepot2 = generator.GenerateAddress();
			var agent1 = generator.GenerateOrganisation();
			var agent2 = generator.GenerateOrganisation();
			var carrier1 = generator.GenerateOrganisation();
			var carrier2 = generator.GenerateOrganisation();

			depotAddress1.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot1, "6", agent1)
				.AddZone("Z1", carrier1, "D2D", accountNumber: "13579")
					.AddZoneItem("AU", 3500, 3600);
			depotAddress2.CreatePortAndDepotSelectionWithUndgClass(TestConnection, "EXP", "DLV", "ALL", "AAA", originDepot2, "6", agent2)
				.AddZone("Z4", carrier2, "DIR", accountNumber: "24680")
					.AddZoneItem("AU", 3500, 3600);

			var billToPartyPK = generator.GenerateAddress();

			var clusterKey = 7;
			var headerPK = generator.NewHvlvBookingHeader(clusterKey, "M00000006", billToPartyPK, "EXP", originDepot2, 1, DateTime.Now, "ABC", DateTime.Now, "ABC");
			var consignmentPK = generator.GenerateHvlvConsignment_WithBookingHeader(clusterKey, headerPK, "Consignment6", "Waybill6", "ShipRef6", 1, "Books", "6", "CNF",
					"Jack", "Test Address 11", "Test Address 22", "Melbourne Metro", "VIC", "3560", "AU",
					DateTime.Now, "ABC", DateTime.Now, "ABC");

			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK });

			var consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPK).ToArray();
			AssertEquals(1, consignmentsResults.Length);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Precondition:", depotAddress2, consignmentsResults[0].DestinationDepot);
				AssertEquals("Precondition:", carrier2, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Precondition:", "DIR", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Precondition:", "24680", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Precondition:", agent2, consignmentsResults[0].LastMileCarrierBookingAgent);
			});

			generator.UpdateHvlvBookingHeaderOriginDepot(headerPK, clusterKey, originDepot1);
			Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(new List<Guid>() { consignmentPK }, false);
			consignmentsResults = GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(headerPK).ToArray();

			CombineAssertions("Expected the following fields to be overwritten.", () =>
			{
				AssertEquals("ConsigmentPK:", consignmentPK, consignmentsResults[0].PK);
				AssertEquals("Depot Address:", depotAddress1, consignmentsResults[0].DestinationDepot);
				AssertEquals("Last Mile Carrier:", carrier1, consignmentsResults[0].LastMileCarrier);
				AssertEquals("Service Level:", "D2D", consignmentsResults[0].LastMileCarrierServiceLevel);
				AssertEquals("Carrier Account Number:", "13579", consignmentsResults[0].CarrierAccountNumber);
				AssertEquals("Booking Agent:", agent1, consignmentsResults[0].LastMileCarrierBookingAgent);
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			generator = new ConsignmentBookingGeneratorForTests(TestConnection);
		}

		ConsignmentBookingGeneratorForTests generator;

		static List<(object PK, object DestinationDepot, object LastMileCarrier, object LastMileCarrierServiceLevel, object LastMileCarrierBookingAgent, object CarrierAccountNumber)> GetConsignmentDepotAddressAndCarrierInfoFromBookingHeaderPK(Guid headerPK)
		{
			var result = new List<(object, object, object, object, object, object)>();

			using (var command = Db.Connection.Command("SELECT HVC_PK, HVC_OA_DestinationDepot, HVC_OH_LastMileCarrier, HVC_PL_NKLastMileCarrierServiceLevel, HVC_OH_LastMileCarrierBookingAgent, HVC_CarrierAccountNumber FROM dbo.HVLVConsignment WHERE HVC_HVH_BookingHeader = '" + headerPK + "'"))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var tuple = (reader.GetValue(0), reader.GetValue(1), reader.GetValue(2), reader.GetValue(3), reader.GetValue(4), reader.GetValue(5));
						result.Add(tuple);
					}
				}
			}

			return result;
		}

		static List<(object PK, object DestinationDepot, object LastMileCarrier, object LastMileCarrierServiceLevel, object LastMileCarrierBookingAgent, object CarrierAccountNumber)> GetConsignmentDepotAddressAndCarrierInfoFromConsignmentHeaderPK(Guid consignmentHeaderPK)
		{
			var result = new List<(object, object, object, object, object, object)>();

			using (var command = Db.Connection.Command("SELECT HVC_PK, HVC_OA_DestinationDepot, HVC_OH_LastMileCarrier, HVC_PL_NKLastMileCarrierServiceLevel, HVC_OH_LastMileCarrierBookingAgent, HVC_CarrierAccountNumber FROM dbo.HVLVConsignment WHERE HVC_HCH_Header = '" + consignmentHeaderPK + "'"))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var tuple = (reader.GetValue(0), reader.GetValue(1), reader.GetValue(2), reader.GetValue(3), reader.GetValue(4), reader.GetValue(5));
						result.Add(tuple);
					}
				}
			}

			return result;
		}

		static void Run_UpdateConsignmentsWithDepotAddressAndCarrierInfo(List<Guid> consignmentPKs, bool onlyPopulateEmptyLastMileCarrierAndDepotDetails = false)
		{
			using (var command = Db.Connection.Command("UpdateConsignmentsWithDepotAddressAndCarrierInfo"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddTableValuedParameter("@ConsignmentPks", "dbo.TVP_uniqueidentifier", consignmentPKs);
				command.AddParameter("@OnlyPopulatesEmptyLMCDepotDetails", SqlDbType.Bit, onlyPopulateEmptyLastMileCarrierAndDepotDetails);
				command.ExecuteNonQuery();
			}
		}
		#endregion
	}

	static class UpdateConsignmentWithDepotAddressAndCarrierInfoExtensions
	{
		public static Dictionary<string, object> CreatePortAndDepotSelectionWithUndgClass(this Guid guid, DbConnection connection, string serviceLevel, string direction, string ratingFreightMode, string packType, Guid dispatchDepotAddress, string undgClass, Guid carrierBookingAgent)
		{
			var pk = Guid.NewGuid();
			var res = new Dictionary<string, object>();
			res["connection"] = connection;
			res["portDepotSelection"] = pk;

			var query = string.Format(
				"INSERT INTO dbo.PortHubSelection (TY_PK, TY_Direction, TY_RS_NKServiceLevel, TY_RatingFreightMode, TY_OA_DepotAddress, TY_F3_NKPackType, TY_OA_DispatchDepotAddress, TY_UndgClass, TY_OH_CarrierBookingAgent) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, '{7}', {8})",
				pk,
				direction,
				serviceLevel,
				ratingFreightMode,
				guid,
				packType,
				dispatchDepotAddress == Guid.Empty ? "NULL" : string.Format("'{0}'", dispatchDepotAddress.ToString()),
				undgClass,
				carrierBookingAgent == Guid.Empty ? "NULL" : string.Format("'{0}'", carrierBookingAgent.ToString())
				);
			connection.ExecuteNonQuery(query);

			return res;
		}
	}
}
