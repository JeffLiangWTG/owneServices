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
	[TestedType(typeof(KRLocalExportEntryInvoiceLines))]
	class KRLocalExportEntryInvoiceLinesTest : DbCreateScriptTest
	{
		public string MessageType = "LEX";
		public int clusterKey = 100;
		public void TestCusEntryLineQuery()
		{
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLineItems = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff", "CL_Description", "CL_CustomsValue" }, new List<object> { 1, "9404210010", "Mattress", 1000000 });
			var entryLinePK = CreateCusEntryLine(entryPK, clusterKey, entryLineItems);

			var invoicePK = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK);

			using (var command = TestConnection.Command(selectQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("1", reader["EntryLineNo"].ToString());
					AssertEquals("9404210010", reader["Tariff"]);
					AssertEquals("Mattress", reader["InvoiceDescription"]);
					AssertEquals(1000000m, reader["EntryLineCustomsValueKRW"]);
				}
			}
		}

		public void TestInvoiceLineSumQuery()
		{
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLinePK1 = CreateCusEntryLine(entryPK, clusterKey);
			var entryLinePK2 = CreateCusEntryLine(entryPK, clusterKey);
			var entryLinePK3 = CreateCusEntryLine(entryPK, clusterKey);
			var entryLinePK4 = CreateCusEntryLine(entryPK, clusterKey);

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			var invoicePK2 = CreateJobComInvoiceHeader(declarationPK, clusterKey);

			var invLineDic1 = new Dictionary<string, string>();
			invLineDic1.Add("PackType", "CT");
			invLineDic1.Add("NoOfPacks", "10");
			var invLine1Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_InvoiceQuantity", "JI_AddInfo" }, new List<object> { 1100, "G", 1000, GenerateAddInfoData(invLineDic1) });
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK1, invLine1Items);

			var invLineDic2 = new Dictionary<string, string>();
			invLineDic2.Add("PackType", "CT");
			invLineDic2.Add("NoOfPacks", "20");
			var invLine2Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_InvoiceQuantity", "JI_AddInfo" }, new List<object> { 2, "KG", 2200, GenerateAddInfoData(invLineDic2) });
			var invLinePK2 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK2, invLine2Items);

			var invLineDic3 = new Dictionary<string, string>();
			invLineDic3.Add("PackType", "CT");
			invLineDic3.Add("NoOfPacks", "300");
			var invLine3Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_InvoiceQuantity", "JI_AddInfo" }, new List<object> { 530, "G", 4040, GenerateAddInfoData(invLineDic3) });
			var invLinePK3 = CreateJobComInvoiceLine(invoicePK2, clusterKey, entryLinePK3, invLine3Items);

			var invLineDic4 = new Dictionary<string, string>();
			invLineDic4.Add("PackType", "CT");
			invLineDic4.Add("NoOfPacks", "4000");
			var invLine4Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_InvoiceQuantity", "JI_AddInfo" }, new List<object> { 2004, "G", 5005, GenerateAddInfoData(invLineDic4) });
			var invLinePK4 = CreateJobComInvoiceLine(invoicePK2, clusterKey, entryLinePK4, invLine4Items);

			using (var command = TestConnection.Command(selectQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["NetWeight"], reader["InvoiceQty"], reader["Packages"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 4, result.Count);

						AssertEquals(1.1m, result[invLinePK1][0]);
						AssertEquals(1000m, result[invLinePK1][1]);
						AssertEquals(10, result[invLinePK1][2]);

						AssertEquals(2m, result[invLinePK2][0]);
						AssertEquals(2200m, result[invLinePK2][1]);
						AssertEquals(20, result[invLinePK2][2]);

						AssertEquals(0.530m, result[invLinePK3][0]);
						AssertEquals(4040m, result[invLinePK3][1]);
						AssertEquals(300, result[invLinePK3][2]);

						AssertEquals(2.004m, result[invLinePK4][0]);
						AssertEquals(5005m, result[invLinePK4][1]);
						AssertEquals(4000, result[invLinePK4][2]);
					});
				}
			}
		}

		public void TestNoExistCusSupportingInfoData()
		{
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLinePK = CreateCusEntryLine(entryPK, clusterKey);
			var invoicePK = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK);

			using (var command = TestConnection.Command(selectQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("", reader["SupportingDocumentNo"]);
					AssertEquals("", reader["SupportingDocumentType"]);
				}
			}
		}
		public void TestExistCusSupportingInfoData()
		{
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLinePK = CreateCusEntryLine(entryPK, clusterKey);
			var invoicePK = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			var invoiceLinePK = CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK);
			var cusSupportingInfoItems = GetItemList(new List<string> { "CSI_Code", "CSI_ReferenceNumber" }, new List<object> { "01", "L172770925459" });
			CreateCusSupportingInfo(invoiceLinePK, "JI", "SUP", cusSupportingInfoItems);

			using (var command = TestConnection.Command(selectQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("01", reader["SupportingDocumentType"]);
					AssertEquals("L172770925459", reader["SupportingDocumentNo"]);
				}
			}
		}
		public void TestExistCusSupportingInfoTwoData()
		{
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLinePK = CreateCusEntryLine(entryPK, clusterKey);
			var invoicePK = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			var invoiceLinePK = CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK);
			var cusSupportingInfoItems1 = GetItemList(new List<string> { "CSI_Code", "CSI_ReferenceNumber" }, new List<object> { "99", "L172770925459" });
			var cusSupportingInfoItems2 = GetItemList(new List<string> { "CSI_Code", "CSI_ReferenceNumber" }, new List<object> { "01", "L172770925458" });
			CreateCusSupportingInfo(invoiceLinePK, "JI", "SUP", cusSupportingInfoItems1);
			CreateCusSupportingInfo(invoiceLinePK, "JI", "SUP", cusSupportingInfoItems2);

			using (var command = TestConnection.Command(selectQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["SupportingDocumentType"], reader["SupportingDocumentNo"] });
					}

					AssertEquals("Record Count", 1, result.Count);
					AssertEquals("99", result[invoiceLinePK][0]);
					AssertEquals("L172770925459", result[invoiceLinePK][1]);
				}
			}
		}

		public void TestJobComInvoiceLineData()
		{
			clusterKey = 110;
			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLinePK = CreateCusEntryLine(entryPK, clusterKey);

			var invoicePK = CreateJobComInvoiceHeader(declarationPK, clusterKey);

			var invoiceLineAddInfoDics = new Dictionary<string, string>
			{
				{ "PackType", "BG" },
				{ "InboundDate", "2023-10-16" },
				{ "OriginalStateDocType", "01" }
			};
			var invoiceLineItems = GetItemList(new List<string> { "JI_PartNo", "JI_InvoiceUQ", "JI_PreviousEntryNumber", "JI_AddInfo" },
				new List<object> { "KR_PRODUCT", "U", "A", GenerateAddInfoData(invoiceLineAddInfoDics) });

			var invoiceLineNAddInfoDics = new Dictionary<string, string>
			{
				{ "Ingredient", "AAA" },
				{ "SerialNumber", "010101010" }
			};
			invoiceLineItems.Add(new QueryItem("JI_NAddInfo", SqlDbType.NVarChar, GenerateAddInfoData(invoiceLineNAddInfoDics)));

			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK, invoiceLineItems);

			using (var command = TestConnection.Command(selectQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KR_PRODUCT", reader["ProductCode"]);
					AssertEquals("U", reader["InvoiceUQ"]);
					AssertEquals("BG", reader["PackType"]);
					AssertEquals("2023-10-16", reader["InboundDate"]);
					AssertEquals("01", reader["PreviousDocumentType"]);
					AssertEquals("A", reader["PreviousDocumentNo"]);
					AssertEquals("AAA", reader["MaterialCode"]);
					AssertEquals("010101010", reader["ItemID"]);
				}
			}
		}

		public void TestEntryDatas()
		{
			clusterKey = 70001;
			var exporterPK = TestDataCreator.CreateOrganisation("ALOVERPUS", "ALOE VERA KOREA PUSAN", "KRPUS");
			TestDataCreator.CreateOrgCusCode(exporterPK, "GBR", "0147852369", "KR");

			#region Supplier1
			var supplierPK1 = TestDataCreator.CreateOrganisation("KRSupplier", "KR Supplier");
			TestDataCreator.CreateOrgCusCode(supplierPK1, "06", "Supplier1234567", "KR");
			TestDataCreator.CreateOrgCusCode(supplierPK1, "GBR", "1234567890", "KR");
			#endregion

			var declarationAddInfoDics = new Dictionary<string, string>();
			declarationAddInfoDics.Add("CustomsDivision", "20");
			declarationAddInfoDics.Add("VoyageDuration", "10");
			declarationAddInfoDics.Add("NoOfCrew", "7");
			declarationAddInfoDics.Add("MRNType", "3");
			var declarationItems1 = GetItemList(new List<string> { "JE_MessageSubType", "JE_OH_Exporter", "JE_OH_Supplier", "JE_CustomsOffice",
																	"JE_ExportGoodsType", "JE_VesselName", "JE_VoyageFlightNo", "JE_EntryDate",
																	"JE_SubLocationOfGoods", "JE_LocationOtherInformation", "JE_AddInfo" },
												new List<object> { "09", exporterPK, supplierPK1, "010",
																	"11", "MARIO SHIP", "KRC1131", new DateTime(2023, 1, 20),
																	"Address", "12312", GenerateAddInfoData(declarationAddInfoDics) });

			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems1);

			var entryHeaderItems1 = GetItemList(new List<string> { "CH_MessageType", "CH_BGMReference" }, new List<object> { "5DP", "54710753525891" });
			var entryHeaderPK1 = CreateCusEntryHeader(declarationPK, clusterKey, entryHeaderItems1);
			CreateCusEntryNum(entryHeaderPK1, "CusEntryHeader", "6N00221000024X", MessageType, "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09), "1");
			var entryLineItems = GetItemList(new List<string> { "CL_CustomsValue" }, new List<object> { 1000 });
			CreateCusEntryLine(entryHeaderPK1, clusterKey, entryLineItems);
			CreateCusEntryLine(entryHeaderPK1, clusterKey, entryLineItems);

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

			CreateRefVessel("MARIO SHIP", "5P9VD");
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK1, clusterKey);

			var invoiceHeaderAddInfoDics1 = new Dictionary<string, string>();
			invoiceHeaderAddInfoDics1.Add("DRWApplicantType", "1");
			var invoiceHeader1Items1 = GetItemList(new List<string> { "JZ_OH_Manufacturer", "JZ_OH_Buyer", "JZ_Weight", "JZ_WeightUQ", "JZ_NoOfPacks", "JZ_AddInfo" }, new List<object> { manufacturerPK1, importerPK1, 5000, "G", 1000, GenerateAddInfoData(invoiceHeaderAddInfoDics1) });
			var invoiceHeader1PK = CreateJobComInvoiceHeader(declarationPK, clusterKey, invoiceHeader1Items1);
			var invoiceLine1PK = CreateJobComInvoiceLine(invoiceHeader1PK, clusterKey, entryLine1PK);

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '', '', '', '') WHERE InvoiceLinePK = '{1}'", companyPK, invoiceLine1PK)))
			{
				using (var reader = command.ExecuteReader())
				{
					AssertDatas(reader);
				}
			}

				clusterKey = 74100;
			var declarationItems2 = GetItemList(new List<string> { "JE_MessageSubType", "JE_VesselName", "JE_VoyageFlightNo", "JE_LocationOtherInformation" }, new List<object> { "01", "", "KRC1131", "15103" });
			var declarationPK2 = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK, declarationItems2);
			var entryHeaderPK2 = CreateCusEntryHeader(declarationPK2, clusterKey, entryHeaderItems1);
			var entryLine2PK = CreateCusEntryLine(entryHeaderPK2, clusterKey);
			var invoiceLine2PK = CreateJobComInvoiceLine(invoiceHeader1PK, clusterKey, entryLine2PK);

			using (var command = TestConnection.Command(string.Format("SELECT TOP 1 * FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '', '', '', '') WHERE InvoiceLinePK = '{1}'", companyPK, invoiceLine2PK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("15103", reader["BondedAreaCode"]);
				}
			}

			void AssertDatas(IDataReader reader)
			{
				reader.Read();

				AssertEquals("6N00221000024X", reader["EntryNumber"]);
				AssertEquals("5DP", reader["MessageType"]);
				AssertEquals("09", reader["DeclarationType"]);
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

				AssertEquals("ALOVERPUS", reader["ExporterCode"]);
				AssertEquals("0147852369", reader["ExporterBusinessNumber"]);

				AssertEquals("010", reader["CustomsOffice"]);
				AssertEquals("20", reader["Department"]);
				AssertEquals("11", reader["ExportGoodsType"]);
				AssertEquals("MARIO SHIP", reader["VesselNameOrFlightNo"]);
				AssertEquals("5P9VD", reader["VesselRadioCallSign"]);
				AssertEquals("35P9VD", reader["MRNNo"]);
				AssertEquals(10, reader["ScheduledSailingDays"]);
				AssertEquals(new DateTime(2023, 1, 20), (DateTime)reader["DeclarationDate"]);
				AssertEquals(7, reader["CrewCount"]);
				AssertEquals("Address", reader["BondedAreaName"]);
				AssertEquals("1", reader["DrawbackApplicantType"]);
				AssertEquals(2000m, reader["TotalCustomsValueKRW"]);
				AssertEquals(5m, reader["TotalGrossWeightKG"]);
				AssertEquals(1000m, reader["TotalPackages"]);
				AssertEquals(new DateTime(2023, 02, 24), (DateTime)reader["AcceptedDate"]);
			}
		}

		public void TestMappingAccordingToHSCode()
		{
			int clusterKey = 16;

			var tariffTypePK = CreateRefCusTariffType();

			var tariffItems1 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "2402201000", "Filter tip cigarettes", new DateTime(2015, 01, 01), new DateTime(2079, 01, 01) });
			var tariffPK = CreateRefCusTariff(tariffTypePK, tariffItems1);
			CreateRefCusTariffAttribute(tariffPK, "InvoiceQuantity in CU1");

			var tariffItems2 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "25010010", "Rock salt and sea salt made by the heat of the sun, not refined", new DateTime(2016, 01, 01), new DateTime(2020, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems2);

			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLineItems1 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 1, "2402201000" });
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, clusterKey, entryLineItems1);

			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 2, "25010010" });
			var entryLine2PK = CreateCusEntryLine(entryHeaderPK, clusterKey, entryLineItems2);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, clusterKey);

			var invoiceLineItems1 = GetItemList(new List<string> { "JI_InvoiceQuantity", "JI_InvoiceUQ", "JI_CustomsQuantity", "JI_CustomsUnitQty" },
				new List<object> { 2m, "EA", 3m, "U" });

			var invLinePK1 = CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine1PK, invoiceLineItems1);

			var invoiceLineItems2 = GetItemList(new List<string> { "JI_InvoiceQuantity", "JI_InvoiceUQ", "JI_CustomsQuantity", "JI_CustomsUnitQty" },
				new List<object> { 4m, "PC", 5m, "KG" });

			var invLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine2PK, invoiceLineItems2);

			using (var command = TestConnection.Command(selectQuery))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["InvoiceQty"], reader["InvoiceUQ"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 2, result.Count);

						AssertEquals(3m, result[invLinePK1][0]);
						AssertEquals("U", result[invLinePK1][1]);

						AssertEquals(4m, result[invLinePK2][0]);
						AssertEquals("PC", result[invLinePK2][1]);
					});
				}
			}
		}

		public void TestTariff()
		{
			int clusterKey = 16;

			var tariffTypePK = CreateRefCusTariffType();
			var tariffItems1 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "1AAAAAAAAA", "Tariff Star With 1 Test Data", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems1);

			var tariffItems2 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "2BBBBBBBBB", "Tariff Star With 2 Test Data", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems2);

			var tariffItems3 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "3CCCCCCCCC", "Tariff Star With 3 Test Data", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems3);

			var tariffItems4 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "4DDDDDDDDD", "Tariff Star With 4 Test Data", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems4);

			var tariffItems5 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "5EEEEEEEEE", "Tariff Star With 5 Test Data", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems5);

			var declarationPK = CreateJobDeclaration(clusterKey, MessageType, branchPK, companyPK);
			var entryPK1 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryPK1, "CusEntryHeader", "6N00221000024X", "LEX", "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

			var entryLineItems1 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 1, "1AAAAAAAAA" });
			var entryLinePK1 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems1);

			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 2, "2BBBBBBBBB" });
			var entryLinePK2 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems2);

			var entryLineItems3 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 3, "3CCCCCCCCC" });
			var entryLinePK3 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems3);

			var entryLineItems4 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 4, "4DDDDDDDDD" });
			var entryLinePK4 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems4);

			var entryLineItems5 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 5, "5EEEEEEEEE" });
			var entryLinePK5 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems5);

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK1);
			CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK2);
			CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK3);
			CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK4);
			CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK5);

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(5, reader["RowCount"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '1', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("1AAAAAAAAA", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '2', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("2BBBBBBBBB", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '', '3', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("3CCCCCCCCC", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '', '', '4', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("4DDDDDDDDD", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '', '', '', '5')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("5EEEEEEEEE", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '1', '', '3', '', '5')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["RowCount"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '2', '', '4', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(2, reader["RowCount"]);
				}
			}
		}

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
			selectQuery = string.Format("SELECT * FROM [dbo].[KRLocalExportEntryInvoiceLines]('{0}', '', '', '', '', '', '')", companyPK);
		}
		string selectQuery;
		Guid companyPK;
		Guid branchPK;
	}
}
