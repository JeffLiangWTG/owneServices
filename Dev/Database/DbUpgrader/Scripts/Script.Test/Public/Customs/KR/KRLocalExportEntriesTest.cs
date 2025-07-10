using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.Customs.KR.Testing
{
	[TestedType(typeof(KRLocalExportEntries))]
	class KRLocalExportEntriesTest : DbCreateScriptTest
	{
		public string MessageType = "LEX";
		public int clusterKey;
		public string selectQuery = "";

		public void TestCusEntryHeaderAndNumData()
		{
			clusterKey = 70000;

			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);

			var entryHeaderItems1 = GetItemList(new List<string> { "CH_MessageType", "CH_BGMReference" }, new List<object> { "5DP", "01610753524861" });
			var entryHeaderPK1 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems1);
			CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "6N00221000024X", MessageType, "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09), "1");
			CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "6N00221000025X", MessageType, "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09), "2");

			var entryHeaderItems2 = GetItemList(new List<string> { "CH_MessageType", "CH_BGMReference" }, new List<object> { "5DP", "54710753525891" });
			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems2);
			CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "6N00221000026X", MessageType, "CUS", new DateTime(2023, 03, 02), new DateTime(2023, 03, 10), "1");

			var entryHeaderItems3 = GetItemList(new List<string> { "CH_MessageType", "CH_BGMReference" }, new List<object> { "5DQ", "32790753526137" });
			var entryHeaderPK3 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems3);
			CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "6N00221000027X", MessageType, "CUS", new DateTime(2023, 04, 01), new DateTime(2023, 04, 10), "1");
			CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "6N00221000028X", MessageType, "CUS", new DateTime(2023, 04, 01), new DateTime(2023, 04, 10), "2");

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK1)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("6N00221000024X", reader["EntryNumber"]);
					AssertEquals(new DateTime(2023, 02, 24), reader["AcceptedDate"]);

					AssertEquals("5DP", reader["MessageType"]);
					AssertEquals("01610753524861", reader["CustomsConfirmationNo"]);
				}
			}

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("6N00221000026X", reader["EntryNumber"]);
					AssertEquals(new DateTime(2023, 03, 02), reader["AcceptedDate"]);

					AssertEquals("5DP", reader["MessageType"]);
					AssertEquals("54710753525891", reader["CustomsConfirmationNo"]);
				}
			}

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("6N00221000027X", reader["EntryNumber"]);
					AssertEquals(new DateTime(2023, 04, 01), reader["AcceptedDate"]);

					AssertEquals("5DQ", reader["MessageType"]);
					AssertEquals("32790753526137", reader["CustomsConfirmationNo"]);
				}
			}
		}

		public void TestJobDeclarationAndAddInfo()
		{
			clusterKey = 74000;

			var declarationAddInfoDics1 = new Dictionary<string, string>();
			declarationAddInfoDics1.Add("CustomsDivision", "20");
			declarationAddInfoDics1.Add("VoyageDuration", "10");
			declarationAddInfoDics1.Add("NoOfCrew", "7");

			var declarationItems1 = GetItemList(new List<string> { "JE_MessageSubType", "JE_OH_Exporter", "JE_CustomsOffice", "JE_ExportGoodsType",
																	"JE_VesselName", "JE_VoyageFlightNo", "JE_EntryDate",
																	"JE_SubLocationOfGoods", "JE_LocationOtherInformation", "JE_AddInfo" },
												 new List<object> { "09", exporterPK, "010", "11",
																	"VesselName", "KRC1131", new DateTime(2023, 1, 20),
																	"Address", "12312", GenerateAddInfoData(declarationAddInfoDics1) });

			var declarationPK1 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems1);
			var declarationclusterKey1 = clusterKey;
			var entryHeaderPK = CreateCusEntryHeader(declarationPK1, clusterKey);
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, clusterKey);

			var invoiceHeaderAddInfoDics1 = new Dictionary<string, string>();
			invoiceHeaderAddInfoDics1.Add("DRWApplicantType", "1");
			var invoiceHeader1Items1 = GetItemList(new List<string> { "JZ_AddInfo" }, new List<object> { GenerateAddInfoData(invoiceHeaderAddInfoDics1) });
			var invoiceHeader1PK = CreateJobComInvoiceHeader(declarationPK1, clusterKey, invoiceHeader1Items1);
			CreateJobComInvoiceLine(invoiceHeader1PK, clusterKey, entryLine1PK);

			clusterKey = 74100;
			var declarationItems3 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_VoyageFlightNo", "JE_LocationOtherInformation" }, new List<object> { "01", "", "KRC1131", "15103" });
			var declarationPK3 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems3);
			var declarationclusterKey3 = clusterKey;
			CreateCusEntryHeader(declarationPK3, clusterKey);

			clusterKey = 74200;
			var declarationItems4 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_VoyageFlightNo" }, new List<object> { "17", "", "" });
			var declarationPK4 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems4);
			var declarationclusterKey4 = clusterKey;
			CreateCusEntryHeader(declarationPK4, clusterKey);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey1)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("09", reader["DeclarationType"]);
					AssertEquals("ALOVERPUS", reader["ExporterCode"]);
					AssertEquals("1234567890123", reader["ExporterBusinessNumber"]);
					AssertEquals("010", reader["CustomsOffice"]);
					AssertEquals("11", reader["ExportGoodsType"]);
					AssertEquals("VesselName", reader["VesselNameOrFlightNo"]);
					AssertEquals(new DateTime(2023, 1, 20), (DateTime)reader["DeclarationDate"]);
					AssertEquals("Address", reader["BondedAreaName"]);
					AssertEquals("", reader["BondedAreaCode"]);
					AssertEquals("20", reader["Department"]);
					AssertEquals(10, reader["ScheduledSailingDays"]);
					AssertEquals(7, reader["CrewCount"]);
					AssertEquals("1", reader["DrawbackApplicantType"]);
				}
			}

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("15103", reader["BondedAreaCode"]);
					AssertEquals("KRC1131", reader["VesselNameOrFlightNo"]);
				}
			}

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey4)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("", reader["VesselNameOrFlightNo"]);
				}
			}
		}

		public void TestJobComInvoiceHeaderData()
		{
			clusterKey = 71000;

			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, clusterKey);
			var entryLine2PK = CreateCusEntryLine(entryHeaderPK, clusterKey);

			var invoiceHeader1Items = GetItemList(new List<string> { "JZ_Weight", "JZ_WeightUQ", "JZ_NoOfPacks" }, new List<object> { 5000, "G", 1000 });
			var invoiceHeader2Items = GetItemList(new List<string> { "JZ_Weight", "JZ_WeightUQ", "JZ_NoOfPacks" }, new List<object> { 100, "KG", 50 });

			var invoiceHeader1PK = CreateJobComInvoiceHeader(declarationPK, clusterKey, invoiceHeader1Items);
			var invoiceHeader2PK = CreateJobComInvoiceHeader(declarationPK, clusterKey, invoiceHeader2Items);
			CreateJobComInvoiceLine(invoiceHeader1PK, clusterKey, entryLine1PK);
			CreateJobComInvoiceLine(invoiceHeader2PK, clusterKey, entryLine2PK);
			CreateJobComInvoiceLine(invoiceHeader2PK, clusterKey, entryLine2PK);

			using (var command = TestConnection.Command(string.Format(selectQuery, companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(105m, reader["TotalGrossWeightKG"]);
					AssertEquals(1050m, reader["TotalPackages"]);
				}
			}
		}

		public void TestCusEntryLineData()
		{
			clusterKey = 72000;

			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);

			var entryLineItems = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 1000 });
			var entryHeaderPK1 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "6N00223000001", MessageType, "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09), "1");
			CreateCusEntryLine(entryHeaderPK1, clusterKey, entryLineItems);
			CreateCusEntryLine(entryHeaderPK1, clusterKey, entryLineItems);

			entryLineItems = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 1200 });
			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "6N00223000002", MessageType, "CUS", new DateTime(2023, 03, 02), new DateTime(2023, 03, 10), "1");
			CreateCusEntryLine(entryHeaderPK2, clusterKey, entryLineItems);
			CreateCusEntryLine(entryHeaderPK2, clusterKey, entryLineItems);
			CreateCusEntryLine(entryHeaderPK2, clusterKey, entryLineItems);

			var entryHeaderPK3 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "6N00223000003", MessageType, "CUS", new DateTime(2023, 04, 01), new DateTime(2023, 04, 10), "1");
			entryLineItems = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 1200 });
			CreateCusEntryLine(entryHeaderPK3, clusterKey, entryLineItems);
			entryLineItems = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 1500 });
			CreateCusEntryLine(entryHeaderPK3, clusterKey, entryLineItems);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK1)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(2000m, reader["TotalCustomsValueKRW"]);
				}
			}

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3600m, reader["TotalCustomsValueKRW"]);
				}
			}

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(2700m, reader["TotalCustomsValueKRW"]);
				}
			}
		}

		public void TestJobDeclarationOtherData()
		{
			CreateRefVessel("MARIO SHIP", "5P9VD");
			CreateRefVessel("LUIGI AIR", "");

			clusterKey = 73000;
			var declarationAddInfoDics1 = new Dictionary<string, string>();
			declarationAddInfoDics1.Add("MRNType", "1");
			var declarationItems1 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_AddInfo" }, new List<object> { "07", "", GenerateAddInfoData(declarationAddInfoDics1) });
			var declarationPK1 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems1);
			var declarationclusterKey1 = clusterKey;
			CreateJobDecRefs(declarationPK1, clusterKey, "A0123456789", "MRN");
			CreateCusEntryHeader(declarationPK1, clusterKey);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey1)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader["VesselRadioCallSign"]);
					AssertEquals("A0123456789", reader["MRNNo"]);
				}
			}

			clusterKey = 73100;
			var declarationAddInfoDics2 = new Dictionary<string, string>();
			declarationAddInfoDics2.Add("MRNType", "2");
			var declarationItems2 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_AddInfo" }, new List<object> { "09", "", GenerateAddInfoData(declarationAddInfoDics2) });
			var declarationPK2 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems2);
			var declarationclusterKey2 = clusterKey;
			CreateCusEntryHeader(declarationPK2, clusterKey);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader["VesselRadioCallSign"]);
					AssertEquals(DateTime.Today.ToString("yy") + "ZZZZZZZZZ", reader["MRNNo"]);
				}
			}

			clusterKey = 73200;
			var declarationAddInfoDics3 = new Dictionary<string, string>();
			declarationAddInfoDics3.Add("MRNType", "2");
			var declarationItems3 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_AddInfo" }, new List<object> { "17", "", GenerateAddInfoData(declarationAddInfoDics3) });
			var declarationPK3 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems3);
			var declarationclusterKey3 = clusterKey;

			var entryPK = CreateCusEntryHeader(declarationPK3, clusterKey);
			CreateCusEntryNum(entryPK, "CusEntryHeader", "6N00223000004", MessageType, "CUS", new DateTime(2024, 10, 10), new DateTime(2025, 10, 10), "1");

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey3)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader["VesselRadioCallSign"]);
					AssertEquals("24ZZZZZZZZZ", reader["MRNNo"]);
				}
			}

			clusterKey = 73300;
			var declarationAddInfoDics4 = new Dictionary<string, string>();
			declarationAddInfoDics4.Add("MRNType", "3");
			var declarationItems4 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_AddInfo" }, new List<object> { "07", "MARIO SHIP", GenerateAddInfoData(declarationAddInfoDics4) });
			var declarationPK4 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems4);
			var declarationclusterKey4 = clusterKey;
			CreateCusEntryHeader(declarationPK4, clusterKey);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey4)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("5P9VD", reader["VesselRadioCallSign"]);
					AssertEquals("35P9VD", reader["MRNNo"]);
				}
			}

			clusterKey = 73400;
			var declarationAddInfoDics5 = new Dictionary<string, string>();
			declarationAddInfoDics5.Add("MRNType", "3");
			var declarationItems5 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_AddInfo" }, new List<object> { "08", "MARIO SHIP", GenerateAddInfoData(declarationAddInfoDics5) });
			var declarationPK5 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems5);
			var declarationclusterKey5 = clusterKey;
			CreateCusEntryHeader(declarationPK5, clusterKey);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey5)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader["VesselRadioCallSign"]);
					AssertEquals("", reader["MRNNo"]);
				}
			}

			clusterKey = 73500;
			var declarationAddInfoDics6 = new Dictionary<string, string>();
			declarationAddInfoDics6.Add("MRNType", "4");
			var declarationItems6 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_AddInfo" }, new List<object> { "09", "LUIGI AIR", GenerateAddInfoData(declarationAddInfoDics6) });
			var declarationPK6 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems6);
			var declarationclusterKey6 = clusterKey;
			CreateCusEntryHeader(declarationPK6, clusterKey);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey6)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader["VesselRadioCallSign"]);
					AssertEquals("4", reader["MRNNo"]);
				}
			}

			clusterKey = 73600;
			var declarationItems7 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName" }, new List<object> { "09", "LUIGI AIR" });
			var declarationPK7 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems7);
			var declarationclusterKey7 = clusterKey;
			CreateCusEntryHeader(declarationPK7, clusterKey);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE ClusterKey = '{0}'", declarationclusterKey7)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader["VesselRadioCallSign"]);
					AssertEquals("", reader["MRNNo"]);
				}
			}
		}

		public void TestOrganizationData()
		{
			clusterKey = 70001;

			var supplierPK = TestDataCreator.CreateOrganisation("KRSupplier", "KR Supplier");
			TestDataCreator.CreateOrgCusCode(supplierPK, "06", "Supplier1234567", "KR");
			TestDataCreator.CreateOrgCusCode(supplierPK, "GBR", "1234567890", "KR");

			var declarationItems = GetItemList(new List<string> { "JE_OH_Supplier" }, new List<object> { supplierPK });
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems);

			var entryHeaderItems1 = GetItemList(new List<string> { "CH_MessageType", "CH_BGMReference" }, new List<object> { "5DP", "54710753525891" });
			var entryHeaderPK1 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems1);
			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK1, clusterKey);

			#region Manufacturer1
			var manufacturerPK1 = TestDataCreator.CreateOrganisation("KRManu", "KR Manufacturer");
			TestDataCreator.CreateOrgCusCode(manufacturerPK1, "06", "Manufacturer1234567", "KR");
			TestDataCreator.CreateOrgCusCode(manufacturerPK1, "GBR", "0987654321", "KR");
			#endregion

			#region Importer1
			var importerPK1 = TestDataCreator.CreateOrganisation("KRImporter", "KR Importer");
			var address = TestDataCreator.CreateAddress(importerPK1, "Test", "Address1", "Address2", "Seoul", "14", "12345");
			TestDataCreator.CreateOrgAddressCapability(address, "OFC", true);
			KRTestDataCreator.CreateContact(importerPK1, "Kim", "010-0000-0000", true);
			TestDataCreator.CreateOrgCusCode(importerPK1, "GBR", "7418529630", "KR");
			#endregion

			var invoiceHeaderItems1 = GetItemList(new List<string> { "JZ_OH_Manufacturer", "JZ_OH_Buyer" },
				new List<object> { manufacturerPK1, importerPK1 });
			var invoiceHeaderPK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey, invoiceHeaderItems1);
			CreateJobComInvoiceLine(invoiceHeaderPK1, clusterKey, entryLinePK1);

			var entryHeaderItems2 = GetItemList(new List<string> { "CH_MessageType", "CH_BGMReference" }, new List<object> { "5DQ", "32790753526137" });
			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems2);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK2, clusterKey);

			#region Manufacturer2
			var manufacturerPK2 = TestDataCreator.CreateOrganisation("ManuKR", "KR Manufacturer2");
			TestDataCreator.CreateOrgCusCode(manufacturerPK2, "06", "Manufacturer21234567", "KR");
			TestDataCreator.CreateOrgCusCode(manufacturerPK2, "GBR", "8754219630", "KR");
			#endregion

			#region Importer2
			var importerPK2 = TestDataCreator.CreateOrganisation("ImporterKR", "KR Importer2");
			var address1 = TestDataCreator.CreateAddress(importerPK2, "Test2_1", "Address9_1", "Address8_1", "Seoul", "14", "65432");
			TestDataCreator.CreateOrgAddressCapability(address1, "PAD", true);
			var address2 = TestDataCreator.CreateAddress(importerPK2, "Test2_2", "Address9_2", "Address8_2", "Seoul", "14", "85312");
			TestDataCreator.CreateOrgAddressCapability(address2, "OFC", true);
			KRTestDataCreator.CreateContact(importerPK2, "Park", "010-1111-1111", true);
			TestDataCreator.CreateOrgCusCode(importerPK2, "GBR", "6587491230", "KR");
			#endregion

			var invoiceHeaderItems2 = GetItemList(new List<string> { "JZ_OH_Manufacturer", "JZ_OH_Buyer" },
				new List<object> { manufacturerPK2, importerPK2 });

			var invoiceHeaderPK2 = CreateJobComInvoiceHeader(declarationPK, clusterKey, invoiceHeaderItems2);
			CreateJobComInvoiceLine(invoiceHeaderPK2, clusterKey, entryLinePK2);

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK1)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("5DP", reader["MessageType"]);
					AssertEquals("54710753525891", reader["CustomsConfirmationNo"]);

					AssertEquals("KRSupplier", reader["SupplierCode"]);
					AssertEquals("Supplier1234567", reader["SupplierUnipassID"]);
					AssertEquals("1234567890", reader["SupplierBusinessNumber"]);

					AssertEquals("KRManu", reader["ManufacturerCode"]);
					AssertEquals("Manufacturer1234567", reader["ManufacturerUnipassID"]);
					AssertEquals("0987654321", reader["ManufacturerBusinessNumber"]);

					AssertEquals("KRImporter", reader["ImporterCode"]);
					AssertEquals("7418529630", reader["ImporterBusinessNumber"]);
					AssertEquals("KR Importer", reader["ImporterCompanyName"]);
					AssertEquals("Kim", reader["ImporterRepresentativeName"]);
					AssertEquals("12345", reader["ImporterPostCode"]);
					AssertEquals("Address1", reader["ImporterAddressLine1"]);
					AssertEquals("Address2", reader["ImporterAddressLine2"]);

					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["EntryPK"], new object[] { reader["SupplierCode"], reader["ManufacturerCode"], reader["ImporterCode"] });
					}

					AssertEquals(0, result.Count);
				}
			}

			using (var command = TestConnection.Command(selectQuery + string.Format(" WHERE EntryPK = '{0}'", entryHeaderPK2)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					AssertEquals("5DQ", reader["MessageType"]);
					AssertEquals("32790753526137", reader["CustomsConfirmationNo"]);

					AssertEquals("KRSupplier", reader["SupplierCode"]);
					AssertEquals("Supplier1234567", reader["SupplierUnipassID"]);
					AssertEquals("1234567890", reader["SupplierBusinessNumber"]);

					AssertEquals("ManuKR", reader["ManufacturerCode"]);
					AssertEquals("Manufacturer21234567", reader["ManufacturerUnipassID"]);
					AssertEquals("8754219630", reader["ManufacturerBusinessNumber"]);

					AssertEquals("ImporterKR", reader["ImporterCode"]);
					AssertEquals("6587491230", reader["ImporterBusinessNumber"]);
					AssertEquals("KR Importer2", reader["ImporterCompanyName"]);
					AssertEquals("Park", reader["ImporterRepresentativeName"]);
					AssertEquals("85312", reader["ImporterPostCode"]);
					AssertEquals("Address9_2", reader["ImporterAddressLine1"]);
					AssertEquals("Address8_2", reader["ImporterAddressLine2"]);

					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["EntryPK"], new object[] { reader["SupplierCode"], reader["ManufacturerCode"], reader["ImporterCode"] });
					}

					AssertEquals(0, result.Count);
				}
			}
		}

		public void TestProcessStatus()
		{
			clusterKey = 13;
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);

			var entryHeaderItems1 = GetItemList(new List<string> { "CH_MessageType" }, new List<object> { "5DP" });
			var entryHeaderPK1 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems1);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "6N00221000022X", MessageType, "CUS", "KR", new DateTime(2023, 02, 24), "1");

			var entryHeaderItems2 = GetItemList(new List<string> { "CH_MessageType" }, new List<object> { "5DP" });
			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems2);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK2, "CusEntryHeader", "6N00221000023X", MessageType, "CUS", "KR", null, "1");

			var entryHeaderItems3 = GetItemList(new List<string> { "CH_MessageType" }, new List<object> { "5DQ" });
			var entryHeaderPK3 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems3);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK3, "CusEntryHeader", "6N00221000024X", MessageType, "CUS", "KR", null, "1");

			var entryHeaderItems4 = GetItemList(new List<string> { "CH_Status" }, new List<object> { "CAP" });
			var entryHeaderPK4 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems4);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK4, "CusEntryHeader", "6N00221000025X", MessageType, "CUS", "KR", null, "1");

			var entryHeaderItems5 = GetItemList(new List<string> { "CH_Status" }, new List<object> { "CAB" });
			var entryHeaderPK5 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems5);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK5, "CusEntryHeader", "6N00221000026X", MessageType, "CUS", "KR", null, "1");

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRLocalExportEntries]('{0}', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["RowCount"]);
				}
			}

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRLocalExportEntries]('{0}', 'A')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
				}
			}
		}

		protected override bool RequiresSchemaBinding => false;
		
		protected override void SetUp()
		{
			TestDataCreator.CreateRefDatabaseRefDataGrouping("KR", "KR is your country code");

			companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
			branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRSEL");

			var orgHeaderPK = TestDataCreator.CreateOrganisation("RDKOR", "READY KOREA");
			CreateOrgAddress(orgHeaderPK, "TEST", true, "CST", GetItemList(new List<string> { "OA_Address1", "OA_RN_NKCountryCode", "OA_RL_NKRelatedPortCode" }, new List<object> { "Street 1st", "KR", "KRSEL" }));
			CreateStmData(companyPK, "UNIPASSDeclarantID", "12345");
			CreateContact(orgHeaderPK, "Kim", "010-0000-0000", true);
			CreateContact(orgHeaderPK, "Lee", "010-1234-1234", false);
			TestDataCreator.CreateOrgCusCode(orgHeaderPK, "06", "RK000000", "KR");
			var branchSQL = @"UPDATE dbo.GlbBranch SET GB_OH_OrgProxy = @orgHeaderPK, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_PK = @branchPK";
			using (var command = Db.Connection.Command(branchSQL))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.ExecuteNonQuery();
			}

			exporterPK = TestDataCreator.CreateOrganisation("ALOVERPUS", "ALOE VERA KOREA PUSAN", "KRPUS");
			TestDataCreator.CreateOrgCusCode(exporterPK, "GBR", "1234567890123", "KR");

			selectQuery = string.Format("SELECT * FROM [dbo].[KRLocalExportEntries]('{0}', '')", companyPK);
		}
		Guid companyPK;
		Guid branchPK;
		Guid exporterPK;
	}
}
