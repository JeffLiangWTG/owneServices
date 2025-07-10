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
	[TestedType(typeof(KRExportEntryInvoiceLines))]
	class KRExportEntryInvoiceLinesTest : DbCreateScriptTest
	{
		public void TestDefaultQuery()
		{
			var declarationDic1 = new Dictionary<string, string>();
			declarationDic1.Add("ExporterType", "A");
			var declarationPK1 = CreateJobDeclaration(11, "EXP", branchPK, companyPK, GetItemList(new List<string> { "JE_AddInfo" }, new List<object> { GenerateAddInfoData(declarationDic1) }));

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK1, 11);
			var invoicePK2 = CreateJobComInvoiceHeader(declarationPK1, 11);

			var entryPK1 = CreateCusEntryHeader(declarationPK1, 11);

			var entryLinePK1 = CreateCusEntryLine(entryPK1, 11);
			var invLineDic1 = new Dictionary<string, string>();
			invLineDic1.Add("NoOfPacks", "11");
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, 11, entryLinePK1, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic1) }));
			CreateCusSupportingInfo(invLinePK1, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "Y" }));
			var invLineDic2 = new Dictionary<string, string>();
			invLineDic2.Add("NoOfPacks", "12");
			var invLinePK2 = CreateJobComInvoiceLine(invoicePK1, 11, entryLinePK1, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic2) }));
			CreateCusSupportingInfo(invLinePK2, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "N" }));

			var entryLinePK2 = CreateCusEntryLine(entryPK1, 11);
			var invLineDic3 = new Dictionary<string, string>();
			invLineDic3.Add("NoOfPacks", "13");
			var invLinePK3 = CreateJobComInvoiceLine(invoicePK2, 11, entryLinePK2, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic3) }));
			CreateCusSupportingInfo(invLinePK3, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "N" }));
			var invLineDic4 = new Dictionary<string, string>();
			invLineDic4.Add("NoOfPacks", "14");
			var invLinePK4 = CreateJobComInvoiceLine(invoicePK2, 11, entryLinePK2, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic4) }));
			CreateCusSupportingInfo(invLinePK4, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "Y" }));

			var declarationDic2 = new Dictionary<string, string>();
			declarationDic2.Add("ExporterType", "A");
			var declarationPK2 = CreateJobDeclaration(12, "IMP", branchPK, companyPK, GetItemList(new List<string> { "JE_AddInfo" }, new List<object> { GenerateAddInfoData(declarationDic2) }));

			var invoicePK3 = CreateJobComInvoiceHeader(declarationPK2, 12);
			var invoicePK4 = CreateJobComInvoiceHeader(declarationPK2, 12);

			var entryPK2 = CreateCusEntryHeader(declarationPK2, 12);

			var entryLinePK3 = CreateCusEntryLine(entryPK2, 12);
			var invLineDic5 = new Dictionary<string, string>();
			invLineDic5.Add("NoOfPacks", "15");
			var invLinePK5 = CreateJobComInvoiceLine(invoicePK3, 12, entryLinePK3, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic5) }));
			CreateCusSupportingInfo(invLinePK5, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "N" }));
			var invLineDic6 = new Dictionary<string, string>();
			invLineDic6.Add("NoOfPacks", "16");
			var invLinePK6 = CreateJobComInvoiceLine(invoicePK3, 12, entryLinePK3, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic6) }));
			CreateCusSupportingInfo(invLinePK6, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "N" }));

			var entryLinePK4 = CreateCusEntryLine(entryPK2, 12);
			var invLineDic7 = new Dictionary<string, string>();
			invLineDic7.Add("NoOfPacks", "17");
			var invLinePK7 = CreateJobComInvoiceLine(invoicePK4, 12, entryLinePK4, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic7) }));
			CreateCusSupportingInfo(invLinePK7, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "Y" }));
			var invLineDic8 = new Dictionary<string, string>();
			invLineDic8.Add("NoOfPacks", "18");
			var invLinePK8 = CreateJobComInvoiceLine(invoicePK4, 12, entryLinePK4, GetItemList(new List<string> { "JI_AddInfo" }, new List<object> { GenerateAddInfoData(invLineDic8) }));
			CreateCusSupportingInfo(invLinePK8, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "Y" }));

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["ExporterType"], reader["EntryLinePackQty"], reader["COOIssuedCode"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 4, result.Count);

						AssertEquals("A", result[invLinePK1][0]);
						AssertEquals(23, result[invLinePK1][1]);
						AssertEquals("Y", result[invLinePK1][2]);

						AssertEquals("A", result[invLinePK2][0]);
						AssertEquals(23, result[invLinePK2][1]);
						AssertEquals("N", result[invLinePK2][2]);

						AssertEquals("A", result[invLinePK3][0]);
						AssertEquals(27, result[invLinePK3][1]);
						AssertEquals("N", result[invLinePK3][2]);

						AssertEquals("A", result[invLinePK4][0]);
						AssertEquals(27, result[invLinePK4][1]);
						AssertEquals("Y", result[invLinePK4][2]);
					});
				}
			}
		}

		public void TestCusEntryLineQuery()
		{
			int clusterKey = 13;

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);
			CreateRefExchangeRate(companyPK, "USD", 1230.5m, "CUE", new DateTime(2023, 02, 01), new DateTime(2023, 02, 28));

			var entryPK1 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryPK1, "CusEntryHeader", "6N00221000024X", "EXP", "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

			var entryLineItems1 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff", "CL_CustomsValue" }, new List<object> { 1, "9404210010", 1000000 });
			var entryLinePK1 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems1);

			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff", "CL_CustomsValue" }, new List<object> { 2, "5804109010", 300000 });
			var entryLinePK2 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems2);

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK1);
			var invLinePK2 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK2);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["EntryLineNo"], reader["Tariff"], reader["EntryLineCustomsValueKRW"], reader["EntryLineCustomsValueUSD"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 2, result.Count);

						AssertEquals("1", result[invLinePK1][0].ToString());
						AssertEquals("9404210010", result[invLinePK1][1]);
						AssertEquals(1000000m, result[invLinePK1][2]);
						AssertEquals(813m, result[invLinePK1][3]);

						AssertEquals("2", result[invLinePK2][0].ToString());
						AssertEquals("5804109010", result[invLinePK2][1]);
						AssertEquals(300000m, result[invLinePK2][2]);
						AssertEquals(244m, result[invLinePK2][3]);
					});
				}
			}
		}

		public void TestCusEntryLineQueryExchangeRateHasNo()
		{
			int clusterKey = 14;

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);
			CreateRefExchangeRate(companyPK, "USD", 1230.5m, "CUE", new DateTime(2023, 02, 01), new DateTime(2023, 02, 08));

			var entryPK1 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryPK1, "CusEntryHeader", "6N00221000024X", "EXP", "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

			var entryLineItems1 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff", "CL_CustomsValue" }, new List<object> { 1, "9404210010", 1000000 });
			var entryLinePK1 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems1);

			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff", "CL_CustomsValue" }, new List<object> { 2, "5804109010", 300000 });
			var entryLinePK2 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems2);

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK1);
			var invLinePK2 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK2);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["EntryLineNo"], reader["Tariff"], reader["EntryLineCustomsValueKRW"], reader["EntryLineCustomsValueUSD"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 2, result.Count);

						AssertEquals("1", result[invLinePK1][0].ToString());
						AssertEquals("9404210010", result[invLinePK1][1]);
						AssertEquals(1000000m, result[invLinePK1][2]);
						AssertEquals(0m, result[invLinePK1][3]);

						AssertEquals("2", result[invLinePK2][0].ToString());
						AssertEquals("5804109010", result[invLinePK2][1]);
						AssertEquals(300000m, result[invLinePK2][2]);
						AssertEquals(0m, result[invLinePK2][3]);
					});
				}
			}
		}

		public void TestCooAndTotalQuery()
		{
			int clusterKey = 14;

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);

			var entryPK1 = CreateCusEntryHeader(declarationPK, clusterKey);

			var entryLinePK1 = CreateCusEntryLine(entryPK1, clusterKey);
			var entryLinePK2 = CreateCusEntryLine(entryPK1, clusterKey);

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey);

			var invLineDic1 = new Dictionary<string, string>();
			invLineDic1.Add("PackType", "CT");
			invLineDic1.Add("NoOfPacks", "10");
			var invLine1Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_CustomsQuantity", "JI_AddInfo" },
				new List<object> { 1000, "G", 500, GenerateAddInfoData(invLineDic1) });
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK1, invLine1Items);
			CreateCusSupportingInfo(invLinePK1, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "N" }));

			var invLineDic2 = new Dictionary<string, string>();
			invLineDic2.Add("PackType", "CT");
			invLineDic2.Add("NoOfPacks", "5");
			var invLine2Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_CustomsQuantity", "JI_AddInfo" },
				new List<object> { 10, "KG", 500, GenerateAddInfoData(invLineDic2) });
			var invLinePK2 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK1, invLine2Items);
			CreateCusSupportingInfo(invLinePK2, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "N" }));

			var invLineDic3 = new Dictionary<string, string>();
			invLineDic3.Add("PackType", "BA");
			invLineDic3.Add("NoOfPacks", "2");
			var invLine3Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_CustomsQuantity", "JI_AddInfo" },
				new List<object> { 1500, "G", 3000, GenerateAddInfoData(invLineDic3) });
			var invLinePK3 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK2, invLine3Items);
			CreateCusSupportingInfo(invLinePK3, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "Y" }));

			var invLineDic4 = new Dictionary<string, string>();
			invLineDic4.Add("PackType", "BA");
			invLineDic4.Add("NoOfPacks", "8");
			var invLine4Items = GetItemList(new List<string> { "JI_NetWeight", "JI_NetWeightUQ", "JI_CustomsQuantity", "JI_AddInfo" },
				new List<object> { 10, "KG", 1500, GenerateAddInfoData(invLineDic4) });
			var invLinePK4 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK2, invLine4Items);
			CreateCusSupportingInfo(invLinePK4, "JI", "COO", GetItemList(new List<string> { "CSI_Code" }, new List<object> { "Y" }));

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["EntryLineNetWeight"], reader["EntryLineCustomsQty"], reader["EntryLinePackQty"], reader["PackType"], reader["COOIssuedCode"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 4, result.Count);

						AssertEquals(11m, result[invLinePK1][0]);
						AssertEquals(1000m, result[invLinePK1][1]);
						AssertEquals(15, result[invLinePK1][2]);
						AssertEquals("CT", result[invLinePK1][3]);
						AssertEquals("N", result[invLinePK1][4]);

						AssertEquals(11m, result[invLinePK2][0]);
						AssertEquals(1000m, result[invLinePK2][1]);
						AssertEquals(15, result[invLinePK2][2]);
						AssertEquals("CT", result[invLinePK2][3]);
						AssertEquals("N", result[invLinePK2][4]);

						AssertEquals(11.5m, result[invLinePK3][0]);
						AssertEquals(4500m, result[invLinePK3][1]);
						AssertEquals(10, result[invLinePK3][2]);
						AssertEquals("BA", result[invLinePK3][3]);
						AssertEquals("Y", result[invLinePK3][4]);

						AssertEquals(11.5m, result[invLinePK4][0]);
						AssertEquals(4500m, result[invLinePK4][1]);
						AssertEquals(10, result[invLinePK4][2]);
						AssertEquals("BA", result[invLinePK4][3]);
						AssertEquals("Y", result[invLinePK4][4]);
					});
				}
			}
		}
		public void TestTariffDescription()
		{
			int clusterKey = 16;

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);
			var tariffTypePK = CreateRefCusTariffType();

			var tariffItems1 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "28415090900", "Other chromates and dichromates; peroxochromates", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems1);

			var tariffItems2 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "63064010009", "Pneumatic mattresses, of cotton", new DateTime(2023, 01, 01), new DateTime(2023, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems2);

			var entryPK1 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryPK1, "CusEntryHeader", "6N00221000024X", "EXP", "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

			var entryLineItems1 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 1, "28415090900" });
			var entryLinePK1 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems1);

			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 2, "63064010009" });
			var entryLinePK2 = CreateCusEntryLine(entryPK1, clusterKey, entryLineItems2);

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			var invLinePK1 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK1);
			var invLinePK2 = CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK2);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["EntryLineNo"], reader["Tariff"], reader["TariffDescription"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 2, result.Count);

						AssertEquals("1", result[invLinePK1][0].ToString());
						AssertEquals("28415090900", result[invLinePK1][1]);
						AssertEquals("Other chromates and dichromates; peroxochromates", result[invLinePK1][2]);

						AssertEquals("2", result[invLinePK2][0].ToString());
						AssertEquals("63064010009", result[invLinePK2][1]);
						AssertEquals("Pneumatic mattresses, of cotton", result[invLinePK2][2]);
					});
				}
			}
		}

		public void TestJobComInvoiceLineData()
		{
			var declarationPK = CreateJobDeclaration(18, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 18);
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, 18);

			var invoiceHeaderItems = GetItemList(new List<string> { "JZ_InvoiceNumber", "JZ_ValuationDateOverride" },
				new List<object> { "899999999", DateTime.Today });
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 18, invoiceHeaderItems);

			var invoiceLineAddInfoDics = new Dictionary<string, string>();
			invoiceLineAddInfoDics.Add("SequenceNumber", "1");
			var invoiceLineNAddInfoDics = new Dictionary<string, string>();
			invoiceLineNAddInfoDics.Add("Ingredient", "성분1");
			invoiceLineNAddInfoDics.Add("LotNumber", "Lot번호");

			var invoiceLineItems = GetItemList(new List<string> { "JI_Tariff", "JI_Model", "JI_BrandName", "JI_PreviousEntryNumber", "JI_PreviousEntryLineNumber", "JI_CountryOfOrigin", "JI_PartNo", "JI_Description", "JI_InvoiceQuantity", "JI_InvoiceUQ", "JI_LinePrice", "JI_AddInfo", "JI_CustomsUnitQty" },
				new List<object> { "8429521022", "HYUNDAI ROBEX3000LC-7A", "Brand Name", "A", 2, "KR", "KR_PRODUCT", "2007 N81011141", 6m, "U", 27670m, GenerateAddInfoData(invoiceLineAddInfoDics), "KG" });

			invoiceLineItems.Add(new QueryItem("JI_NAddInfo", SqlDbType.NVarChar, GenerateAddInfoData(invoiceLineNAddInfoDics)));

			CreateJobComInvoiceLine(invoiceHeaderPK, 18, entryLine1PK, invoiceLineItems);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("01", reader["SequenceNumber"]);
					AssertEquals("HYUNDAI ROBEX3000LC-7A", reader["ModelTradeName"]);
					AssertEquals("899999999", reader["InvoiceNo"]);
					AssertEquals("Brand Name", reader["BrandName"]);
					AssertEquals("A", reader["ImportDeclarationNo"]);
					AssertEquals((short)2, reader["ImportEntryLineNo"]);
					AssertEquals("KR", reader["GoodsOrigin"]);
					AssertEquals("KR_PRODUCT", reader["ProductCode"]);
					AssertEquals("2007 N81011141", reader["GoodsDescription"]);
					AssertEquals("성분1", reader["Ingredient"]);
					AssertEquals(6m, reader["InvoiceQty"]);
					AssertEquals("U", reader["InvoiceUQ"]);
					AssertEquals(4611.666667m, reader["UnitPrice"]);
					AssertEquals(27670m, reader["Price"]);
					AssertEquals("Lot번호", reader["LotNumber"]);
					AssertEquals("KG", reader["CustomsUnitQty"]);
				}
			}
		}

		public void TestProductCode()
		{
			int clusterKey = 16;

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);
			var entryPK1 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryPK1, "CusEntryHeader", "6N00221000024X", "EXP", "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));
			var entryLinePK1 = CreateCusEntryLine(entryPK1, clusterKey);
			var entryLinePK2 = CreateCusEntryLine(entryPK1, clusterKey);
			var entryLinePK3 = CreateCusEntryLine(entryPK1, clusterKey);
			var entryLinePK4 = CreateCusEntryLine(entryPK1, clusterKey);
			var entryLinePK5 = CreateCusEntryLine(entryPK1, clusterKey);

			var invoicePK = CreateJobComInvoiceHeader(declarationPK, clusterKey);
			var invoiceLineItems1 = GetItemList(new List<string> { "JI_PartNo" }, new List<object> { "A Product Code" });
			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK1, invoiceLineItems1);
			var invoiceLineItems2 = GetItemList(new List<string> { "JI_PartNo" }, new List<object> { "B Product Code" });
			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK2, invoiceLineItems2);
			var invoiceLineItems3 = GetItemList(new List<string> { "JI_PartNo" }, new List<object> { "C Product Code" });
			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK3, invoiceLineItems3);
			var invoiceLineItems4 = GetItemList(new List<string> { "JI_PartNo" }, new List<object> { "D Product Code" });
			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK4, invoiceLineItems4);
			var invoiceLineItems5 = GetItemList(new List<string> { "JI_PartNo" }, new List<object> { "E Product Code" });
			CreateJobComInvoiceLine(invoicePK, clusterKey, entryLinePK5, invoiceLineItems5);

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(5, reader["RowCount"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(ProductCode) AS ProductCode FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', 'A', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("A Product Code", reader["ProductCode"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(ProductCode) AS ProductCode FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', 'B', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("B Product Code", reader["ProductCode"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(ProductCode) AS ProductCode FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', 'C', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("C Product Code", reader["ProductCode"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(ProductCode) AS ProductCode FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', 'D', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("D Product Code", reader["ProductCode"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(ProductCode) AS ProductCode FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', 'E', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("E Product Code", reader["ProductCode"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', 'A', '', 'C', '', 'E', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["RowCount"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', 'B', '', 'D', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(2, reader["RowCount"]);
				}
			}
		}

		public void TestTariff()
		{
			int clusterKey = 16;

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);
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

			var entryPK1 = CreateCusEntryHeader(declarationPK, clusterKey);
			CreateCusEntryNum(entryPK1, "CusEntryHeader", "6N00221000024X", "EXP", "CUS", new DateTime(2023, 02, 24), new DateTime(2023, 03, 09));

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

			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(5, reader["RowCount"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '1', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("1AAAAAAAAA", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '2', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("2BBBBBBBBB", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '3', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("3CCCCCCCCC", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '4', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("4DDDDDDDDD", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount', MAX(Tariff) AS Tariff FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '5')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(1, reader["RowCount"]);
					AssertEquals("5EEEEEEEEE", reader["Tariff"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '1', '', '3', '', '5')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(3, reader["RowCount"]);
				}
			}
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '2', '', '4', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(2, reader["RowCount"]);
				}
			}
		}

		public void TestSequenceNumber()
		{
			var declarationPK = CreateJobDeclaration(18, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, 18);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, 18);
			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, 18);

			var invLinePK1 = CreateJobComInvoiceLine(invoiceHeaderPK, 18, entryLinePK);

			var invoiceLineAddInfoDics = new Dictionary<string, string>();
			invoiceLineAddInfoDics.Add("SequenceNumber", "1");
			var invoiceLineItems = GetItemList(new List<string> { "JI_AddInfo" },
				new List<object> { GenerateAddInfoData(invoiceLineAddInfoDics) });
			var invLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK, 18, entryLinePK, invoiceLineItems);

			invoiceLineAddInfoDics = new Dictionary<string, string>();
			invoiceLineAddInfoDics.Add("SequenceNumber", "99");
			invoiceLineItems = GetItemList(new List<string> { "JI_AddInfo" },
				new List<object> { GenerateAddInfoData(invoiceLineAddInfoDics) });
			var invLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK, 18, entryLinePK, invoiceLineItems);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["SequenceNumber"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 3, result.Count);

						AssertEquals("00", result[invLinePK1][0]);
						AssertEquals("01", result[invLinePK2][0]);
						AssertEquals("99", result[invLinePK3][0]);
					});
				}
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

			var tariffItems3 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "2402209000", "Other", new DateTime(2015, 01, 01), new DateTime(2020, 12, 31) });
			var tariffPK3 = CreateRefCusTariff(tariffTypePK, tariffItems3);
			CreateRefCusTariffAttribute(tariffPK3, "InvoiceQuantity in CU1");

			var declarationPK = CreateJobDeclaration(clusterKey, "EXP", branchPK, companyPK);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, clusterKey);
			var entryLineItems1 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 1, "2402201000" });
			var entryLine1PK = CreateCusEntryLine(entryHeaderPK, clusterKey, entryLineItems1);

			var entryLineItems2 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 2, "25010010" });
			var entryLine2PK = CreateCusEntryLine(entryHeaderPK, clusterKey, entryLineItems2);

			var entryLineItems3 = GetItemList(new List<string> { "CL_LineNumber", "CL_AdValoremTariff" }, new List<object> { 3, "2402209000" });
			var entryLine3PK = CreateCusEntryLine(entryHeaderPK, clusterKey, entryLineItems3);

			var invoiceHeaderPK = CreateJobComInvoiceHeader(declarationPK, clusterKey);

			var invoiceLineItems1 = GetItemList(new List<string> { "JI_InvoiceQuantity", "JI_InvoiceUQ", "JI_CustomsQuantity", "JI_CustomsUnitQty", "JI_LinePrice" },
				new List<object> { 2m, "EA", 3m, "U", 6000m });

			var invLinePK1 = CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine1PK, invoiceLineItems1);

			var invoiceLineItems2 = GetItemList(new List<string> { "JI_InvoiceQuantity", "JI_InvoiceUQ", "JI_CustomsQuantity", "JI_CustomsUnitQty", "JI_LinePrice" },
				new List<object> { 4m, "PC", 5m, "KG", 4000m });

			var invLinePK2 = CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine2PK, invoiceLineItems2);

			var invoiceLineItems3 = GetItemList(new List<string> { "JI_InvoiceQuantity", "JI_InvoiceUQ", "JI_CustomsQuantity", "JI_CustomsUnitQty", "JI_LinePrice" },
				new List<object> { 6m, "GR", 7m, "G", 10000m });

			var invLinePK3 = CreateJobComInvoiceLine(invoiceHeaderPK, clusterKey, entryLine3PK, invoiceLineItems3);

			using (var command = TestConnection.Command(string.Format("SELECT * FROM [dbo].[KRExportEntryInvoiceLines]('{0}', '', '', '', '', '', '', '', '', '', '', '')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["InvoiceLinePK"], new object[] { reader["InvoiceQty"], reader["InvoiceUQ"], reader["UnitPrice"] });
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 3, result.Count);

						AssertEquals(3m, result[invLinePK1][0]);
						AssertEquals("U", result[invLinePK1][1]);
						AssertEquals(2000m, result[invLinePK1][2]);

						AssertEquals(4m, result[invLinePK2][0]);
						AssertEquals("PC", result[invLinePK2][1]);
						AssertEquals(1000m, result[invLinePK2][2]);

						AssertEquals(6m, result[invLinePK3][0]);
						AssertEquals("GR", result[invLinePK3][1]);
						AssertEquals(1666.666667m, result[invLinePK3][2]);
					});
				}
			}
		}

		Guid companyPK;
		Guid branchPK;

		protected override bool RequiresSchemaBinding => false;
		protected override void SetUp()
		{
			TestDataCreator.CreateRefDatabaseRefDataGrouping("KR", "KR is your country code");

			companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
			branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRSEL");

			var orgHeaderPK = TestDataCreator.CreateOrganisation("RDKOR", "READY KOREA");
			TestDataCreator.CreateContact(orgHeaderPK, "Kim", "010 -0000-0000");
			TestDataCreator.CreateOrgCusCode(orgHeaderPK, "06", "RK000000", "KR");
			var branchSQL = @"UPDATE dbo.GlbBranch SET GB_OH_OrgProxy = @orgHeaderPK, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_PK = @branchPK";
			using (var command = Db.Connection.Command(branchSQL))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@orgHeaderPK", SqlDbType.UniqueIdentifier, orgHeaderPK);
				command.ExecuteNonQuery();
			}
		}
	}
}
