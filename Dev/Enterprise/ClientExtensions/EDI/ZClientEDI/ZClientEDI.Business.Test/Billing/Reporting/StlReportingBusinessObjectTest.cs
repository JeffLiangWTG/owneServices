using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlReportingBusinessObject))]
	sealed class StlReportingBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOnDemandUsages()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "III");
			var clientCompany1 = lic1.ClientCompany;
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var clientCompany2 = lic2.ClientCompany;
			clientCompany2.LCC_RN_NKCountryCode = "GB";
			var db = lic1.Database;
			db.LD_DatabaseNumber = 342;

			var clientStaff1 = BillingTestHelper.CreateClientStaff(db, "US1", "User One");
			var clientStaff2 = BillingTestHelper.CreateClientStaff(db, "US2", "User Two");
			var clientStaff3 = BillingTestHelper.CreateClientStaff(db, "ZZ", "webuser");

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;

			var priceItem1 = priceHeader.Items.AddNew();
			priceItem1.L7_Description = "  Local Docs - French";
			priceItem1.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItem1.L7_Code = "FR7";

			var priceItem2 = priceHeader.Items.AddNew();
			priceItem2.L7_Description = "  Language - French";
			priceItem2.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItem2.L7_Code = "GFR";
			BillingTestHelper.AddUsageMap(priceHeader, "GFR", "FR8");
			BillingTestHelper.AddUsageMap(priceHeader, "GFR", "FR9");

			var priceItem3 = priceHeader.Items.AddNew();
			priceItem3.L7_Description = "  Local Docs - Spanish";
			priceItem3.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItem3.L7_Code = "ES7";

			var priceItem4 = priceHeader.Items.AddNew();
			priceItem4.L7_Description = "  Language - Spanish";
			priceItem4.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItem4.L7_Code = "GES";
			BillingTestHelper.AddUsageMap(priceHeader, "GES", "ES8");
			BillingTestHelper.AddUsageMap(priceHeader, "GES", "ES9");

			var priceItemMCC = priceHeader.Items.AddNew();
			priceItemMCC.L7_Description = "  Multi Country Customers (Global, Regional, Multi Country Customer)";
			priceItemMCC.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItemMCC.L7_Code = "MCC";

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff2, "ODM", "FR7", new ZDateTime(2017, 8, 5));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff1, "ODM", "FR7", new ZDateTime(2017, 8, 7));

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff2, "ODM", "FR8", new ZDateTime(2017, 8, 18));

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff3, "ODM", "FR9", new ZDateTime(2017, 8, 20));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff3, "ODM", "FR9", new ZDateTime(2017, 8, 25));

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff1, "ODM", "ES7", new ZDateTime(2017, 8, 2));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff2, "ODM", "ES7", new ZDateTime(2017, 8, 3));

			// date out of range
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff3, "ODM", "ES9", new ZDateTime(2017, 7, 30));

			//MCC
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "MCC", new ZDateTime(2017, 8, 1), clientCompany1, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "MCC", new ZDateTime(2017, 8, 1), clientCompany2, 1);

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			var linesFR7 = new List<string>();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2017, 8, 1), lic1.LA_LD, priceItem1.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			report.GetCsvUsageReport((csv) => { linesFR7.Add(csv); });

			var linesGFR = new List<string>();
			report = new StlReportingBusinessObject(Factory, new ZDateTime(2017, 8, 1), lic1.LA_LD, priceItem2.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			report.GetCsvUsageReport((csv) => { linesGFR.Add(csv); });

			var linesES7 = new List<string>();
			report = new StlReportingBusinessObject(Factory, new ZDateTime(2017, 8, 1), lic1.LA_LD, priceItem3.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			report.GetCsvUsageReport((csv) => { linesES7.Add(csv); });

			var linesGES = new List<string>();
			report = new StlReportingBusinessObject(Factory, new ZDateTime(2017, 8, 1), lic1.LA_LD, priceItem4.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			report.GetCsvUsageReport((csv) => { linesGES.Add(csv); });

			var linesMCC = new List<string>();
			report = new StlReportingBusinessObject(Factory, new ZDateTime(2017, 8, 1), lic1.LA_LD, priceItemMCC.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			report.GetCsvUsageReport((csv) => { linesMCC.Add(csv); });

			AssertEquals(3, linesFR7.Count);
			CombineAssertions(() =>
			{
				AssertEquals("0", @"""Usage Type"",""Staff Name (Code) / Country""", linesFR7[0]);
				AssertEquals("1", @"""FR7"",""User One (US1)""", linesFR7[1]);
				AssertEquals("2", @"""FR7"",""User Two (US2)""", linesFR7[2]);
			});
			AssertEquals(3, linesGFR.Count);
			CombineAssertions(() =>
			{
				AssertEquals("0", @"""Usage Type"",""Staff Name (Code) / Country""", linesGFR[0]);
				AssertEquals("1", @"""FR8"",""User Two (US2)""", linesGFR[1]);
				AssertEquals("2", @"""FR9"",""webuser (ZZ)""", linesGFR[2]);
			});
			AssertEquals(3, linesES7.Count);
			CombineAssertions(() =>
			{
				AssertEquals("0", @"""Usage Type"",""Staff Name (Code) / Country""", linesES7[0]);
				AssertEquals("1", @"""ES7"",""User One (US1)""", linesES7[1]);
				AssertEquals("2", @"""ES7"",""User Two (US2)""", linesES7[2]);
			});
			AssertEquals(1, linesGES.Count);
			CombineAssertions(() =>
			{
				AssertEquals("0", @"""Usage Type"",""Staff Name (Code) / Country""", linesGES[0]);
			});
			AssertEquals(3, linesMCC.Count);
			CombineAssertions(() =>
			{
				AssertEquals("0", @"""Usage Type"",""Staff Name (Code) / Country""", linesMCC[0]);
				AssertEquals("1", @"""MCC"",""Australia (AU)""", linesMCC[1]);
				AssertEquals("2", @"""MCC"",""United Kingdom (GB)""", linesMCC[2]);
			});
		}

		public void TestGetCsvUsageReport()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			var priceItem = org1.LicCompany.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "CME";
			priceItem.L7_Category = "STL";

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "CME", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "Campaign Feb", "", null, null, null, "ENT", 23));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "CME", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "Campaign Mar 1", "", null, null, null, "ENT", 189));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "CME", new ZDateTime(2016, 3, 3, 12, 0, 0), "DDDIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "Campaign Mar 2", "", null, null, null, "ENT", 47));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty, BillingConstants.BillingSystem.STL);
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Campaign Name"",""Usage Time (UTC)""
""ABC"",""B11"",""S11"",""CME"",""189"",""Campaign Mar 1"",""01-Mar-16 10:00""
""III"",""B12"",""S12"",""CME"",""47"",""Campaign Mar 2"",""03-Mar-16 12:00""
";

			AssertEquals(expectedCsvResult, builder.ToString());

			string expectedCsvResult2 =
@"""01-Mar-16 10:00"",""ABC"",""B11"",""S11"",""Campaign Mar 1"",""CME"","""",""189""
""03-Mar-16 12:00"",""III"",""B12"",""S12"",""Campaign Mar 2"",""CME"","""",""47""
";
			var writer = new CsvUsageReportWriterForTest();
			report.GetCsvUsageReport(writer);
			AssertEquals(expectedCsvResult2, writer.ToString());
		}

		public void TestGetCsvUsageReport_Category()
		{
			var captions = new StlRawUsageReportRefCaptionCollection();
			var caption = captions.AddNew("CSM", "ContractUpload2", "User Type", "Carrier", "SCAC");
			caption.Category = "FOO";
			EDIDataRegistry.Instance.StlRawUsageReportRefCaption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, captions);

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			var priceItem = org1.LicCompany.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "CSM";
			priceItem.L7_Category = "FOO";

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FOO", "CSM", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "CS Management", "Carrier 1", "SCAC", null, null, "ENT", 23));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FOO", "CSM", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "CS Management", "Carrier 2", "SCAC", null, null, "ENT", 189));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FOO", "CSM", new ZDateTime(2016, 3, 3, 12, 0, 0), "DDDIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "CS Management", "Carrier 3", "SCAC", null, null, "ENT", 47));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty, "FOO");
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""User Type"",""Carrier"",""SCAC"",""Usage Time (UTC)""
""ABC"","""","""",""CSM"",""189"",""CS Management"",""Carrier 2"",""SCAC"",""01-Mar-16 10:00""
""III"","""","""",""CSM"",""47"",""CS Management"",""Carrier 3"",""SCAC"",""03-Mar-16 12:00""
";

			AssertEquals(expectedCsvResult, builder.ToString());

			string expectedCsvResult2 =
@"""01-Mar-16 10:00"",""ABC"","""","""",""CS Management Carrier 2 SCAC"",""CSM"","""",""189""
""03-Mar-16 12:00"",""III"","""","""",""CS Management Carrier 3 SCAC"",""CSM"","""",""47""
";
			var writer = new CsvUsageReportWriterForTest();
			report.GetCsvUsageReport(writer);
			AssertEquals(expectedCsvResult2, writer.ToString());
		}

		public void TestGetCsvUsageReport_ExcludedSystemCodes()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");

			var priceHeader = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = "STL";
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "PR1";
			priceItem.L7_Category = "STL";
			var mapping1 = BillingTestHelper.AddUsageMap(priceHeader, "STL", "PR1", "CTM");
			mapping1.PUM_UsageCategory = "ABM";

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "PR1", new ZDateTime(2023, 6, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "STL - PR1 - #REF1", "STL - PR1 - #REF2", "STL - PR1 - #REF3", null, null, "ENT", 3));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2023, 6, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "ABM - CTM - #REF1", "ABM - CTM - #REF2", "ABM - CTM - #REF3", null, null, "ENT", 5));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2023, 6, 1), db.PK, priceItem.PK, ZGuid.Empty, "STL,ABM");
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Reference 1"",""Reference 2"",""Reference 3"",""Reference 4"",""Usage Time (UTC)""
""ABC"","""","""",""PR1"",""3"",""STL - PR1 - #REF1"",""STL - PR1 - #REF2"",""STL - PR1 - #REF3"","""",""01-Jun-23 10:00""
""Type"",""Company Code"",""Jurisdiction"",""Department"",""Provider"",""Reference"",""Transactions""
""CustomsWare Messaging"",""ABC"",""ABM - CTM - #REF1"","""",""ABM - CTM - #REF2"",""ABM - CTM - #REF3"",""5""
";

			AssertEquals(expectedCsvResult, builder.ToString());

			string expectedCsvResult2 =
@"""01-Jun-23 10:00"",""ABC"","""","""",""STL - PR1 - #REF1 STL - PR1 - #REF2 STL - PR1 - #REF3"",""PR1"","""",""3""
""02-Jun-23 10:00"",""ABC"","""","""",""CustomsWare Messaging ABM - CTM - #REF1  ABM - CTM - #REF2 ABM - CTM - #REF3"",""PR1"",""ABM Customs"",""5""
";
			var writer = new CsvUsageReportWriterForTest();
			report.GetCsvUsageReport(writer);
			AssertEquals(expectedCsvResult2, writer.ToString());
		}

		public void TestGetCsvUsageReport_ExcludedSystemCodes_PMG()
		{
			var captions = new StlRawUsageReportRefCaptionCollection();
			var caption = captions.AddNew("POZ", "Data Record / Original Entry", "Consol", "Message Type", "Submission Type", "Message ID");
			caption.Category = "PMG";
			EDIDataRegistry.Instance.StlRawUsageReportRefCaption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, captions);

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");

			var priceHeader = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = "STL";
			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "PM1";
			priceItem.L7_Category = "PMG";
			var mapping1 = BillingTestHelper.AddUsageMap(priceHeader, "PMG", "PM1", "POZ");
			mapping1.PUM_UsageCategory = "PMG";

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "POZ", new ZDateTime(2023, 6, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "ABM - CTM - #REF1", "ABM - CTM - #REF2", "ABM - CTM - #REF3", null, null, "ENT", 5));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2023, 6, 1), db.PK, priceItem.PK, ZGuid.Empty, "PMG");
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Consol"",""Message Type"",""Submission Type"",""Message ID"",""Usage Time (UTC)""
""ABC"","""","""",""POZ"",""5"",""ABM - CTM - #REF1"",""ABM - CTM - #REF2"",""ABM - CTM - #REF3"","""",""02-Jun-23 10:00""
";

			AssertEquals(expectedCsvResult, builder.ToString());

			string expectedCsvResult2 =
@"""02-Jun-23 10:00"",""ABC"","""","""",""ABM - CTM - #REF1 ABM - CTM - #REF2 ABM - CTM - #REF3"",""POZ"","""",""5""
";
			var writer = new CsvUsageReportWriterForTest();
			report.GetCsvUsageReport(writer);
			AssertEquals(expectedCsvResult2, writer.ToString());
		}

		public void TestGetCsvUsageReport_CategoryACC()
		{
			var periodStart = new ZDateTime(2019, 1, 1);

			var captions = new StlRawUsageReportRefCaptionCollection();
			var caption1 = captions.AddNew("IT1", "e-Invoicing", "Invoice#");
			caption1.Category = "ACC";
			var caption2 = captions.AddNew("GTS", "Golden Tax Invoice", "Transaction Num", "Golden Tax Invoice Num", "Registration Code", "Print By", "Method");
			caption2.Category = "ACC";
			EDIDataRegistry.Instance.StlRawUsageReportRefCaption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, captions);

			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("INV", "E-Invoicing");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = "ACC";
			usageCodes1.PriceItemCode = "IT1";
			usageCodes1.PriceHeaderCode = "INV";
			var usageCodes2 = billingCodes.AddNew();
			usageCodes2.Category = "ACC";
			usageCodes2.PriceItemCode = "GTS";
			usageCodes2.PriceHeaderCode = BillingConstants.PriceHeaderType.GoldenTax;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdLicCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			var goldenTaxPrices = BillingTestHelper.CreateGoldenTaxPriceList(stdLicCompany);
			var gtsPriceItem = goldenTaxPrices.Items.FindByCode("GTS");
			var eInvoicingPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "IT1");
			eInvoicingPrices.L6_SystemCode = "INV";
			var eInvoicePriceItem = eInvoicingPrices.Items[0];
			eInvoicePriceItem.L7_Category = "ACC";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var clientCompany1 = lic1.ClientCompany;
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var db = lic1.Database;
			db.LD_DatabaseNumber = 342;

			BillingTestHelper.CreatePriceLink(db, stlPrices, periodStart);
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "GTS", periodStart, lic1, "Inv 12345", "Ref2", "Ref3", "Ref4", "Ref5"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "IT1", periodStart, lic1, "Inv 9000", "Ref2", "Ref3", "Ref4"));

			// Should be ignored...
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ACC", "INV", periodStart, lic1, "N/A", "Ref2", "Ref3", "Ref4"));

			EServicesBillingTestHelper.AddTransactions(infoList);

			var builderGoldenTax = new ZStringBuilder();
			var reportGoldenTax = new StlReportingBusinessObject(Factory, periodStart, db.PK, gtsPriceItem.PK, ZGuid.Empty, "ACC");
			reportGoldenTax.GetCsvUsageReport((csv) => { builderGoldenTax.AppendLine(csv); });

			var builderEInv = new ZStringBuilder();
			var reportEInv = new StlReportingBusinessObject(Factory, periodStart, db.PK, eInvoicePriceItem.PK, ZGuid.Empty, "ACC");
			reportEInv.GetCsvUsageReport((csv) => { builderEInv.AppendLine(csv); });

			string expectedCsvResultGoldenTax =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Transaction Num"",""Golden Tax Invoice Num"",""Registration Code"",""Print By"",""Usage Time (UTC)""
""ABC"","""","""",""GTS"",""1"",""Inv 12345"",""Ref2"",""Ref3"",""Ref4"",""01-Jan-19 00:00""
";

			string expectedCsvResultEInv =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Invoice#"",""Usage Time (UTC)""
""ABC"","""","""",""IT1"",""1"",""Inv 9000"",""01-Jan-19 00:00""
";

			AssertEquals(expectedCsvResultGoldenTax, builderGoldenTax.ToString());
			AssertEquals(expectedCsvResultEInv, builderEInv.ToString());
		}

		public void TestGetCsvUsageReport_AdjustedUnitCount()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			var priceItem = org1.LicCompany.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "WTU";
			priceItem.L7_Category = "STL";

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "PackageID", "JobID", "PkgPackageHeader.PK#1", "1", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "PackageID", "JobID", "PkgPackageHeader.PK#1", "2", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2016, 3, 3, 12, 0, 0), "DDDIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "PackageID", "JobID", "PkgPackageHeader.PK#1", "3", null, "ENT", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty, BillingConstants.BillingSystem.STL);
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Reference 1"",""Reference 2"",""Reference 3"",""Reference 4"",""Usage Time (UTC)"",""Adjusted Unit Count""
""ABC"",""B11"",""S11"",""WTU"",""1"",""PackageID"",""JobID"",""PkgPackageHeader.PK#1"",""2"",""01-Mar-16 10:00"",""0.5000""
""III"",""B12"",""S12"",""WTU"",""1"",""PackageID"",""JobID"",""PkgPackageHeader.PK#1"",""3"",""03-Mar-16 12:00"",""0.3300""
";

			AssertEquals(expectedCsvResult, builder.ToString());

			string expectedCsvResult2 =
@"""01-Mar-16 10:00"",""ABC"",""B11"",""S11"",""PackageID JobID PkgPackageHeader.PK#1 2"",""WTU"","""",""1"",""0.5000""
""03-Mar-16 12:00"",""III"",""B12"",""S12"",""PackageID JobID PkgPackageHeader.PK#1 3"",""WTU"","""",""1"",""0.3300""
";
			var writer = new CsvUsageReportWriterForTest();
			report.GetCsvUsageReport(writer);
			AssertEquals(expectedCsvResult2, writer.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReports_TenantID()
		{
			BillingTestHelper.LoadClientSpecificDocuments();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "AA1", "SYD");
			var lic1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var db1 = lic1.Database;
			db1.LD_Product = "SMF";
			db1.LD_DatabaseNumber = 2000;
			db1.LD_TenantID = "1900";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "AA2");
			var db2 = lic2.Database;
			db2.LD_Product = "SMF";
			db2.LD_DatabaseNumber = 2001;
			db2.LD_TenantID = "1901";

			var lic3 = BillingTestHelper.CreateAnotherDatabase(lic1, "AA3");
			var db3 = lic3.Database;
			db3.LD_Product = "SMF";
			db3.LD_DatabaseNumber = 2002;
			db3.LD_TenantID = "1902";

			var priceItem = org1.LicCompany.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "P01";
			priceItem.L7_Category = "SMF";
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SMF", "P01", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAA1SYD", "", db1.DatabaseId, ZGuid.Empty, "DB1", "", "", "", null, billableCount: 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SMF", "P01", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAA2SYD", "", db2.DatabaseId, ZGuid.Empty, "DB2", "", "", "", null, billableCount: 2));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SMF", "P01", new ZDateTime(2021, 1, 1, 10, 0, 0), "DDDAA3SYD", "", db3.DatabaseId, ZGuid.Empty, "DB3", "", "", "", null, billableCount: 3));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			Db.Connection.ExecuteNonQuery(
$@"INSERT INTO EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202101, '{db1.PK}', '{db1.PK}');

INSERT INTO EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202101, '{db2.PK}', '{db1.PK}');

INSERT INTO EdiLicenceDatabaseConsolidationHistory(EDH_Period, EDH_LD, EDH_LD_ConsolidatedDatabase)
VALUES (202101, '{db3.PK}', '{db1.PK}');
");

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("SMF", "SMF Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
			categories.AddPair("SMF", "SMF");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "SMF";
			priceList1.RawUsageCategory = "SMF";
			priceList1.PriceListCode = "SMF";
			priceList1.Description = "SMF - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var consolSettings = new ConsolidatedBillingSettingCollection();
			var consolSetting = consolSettings.AddNew();
			consolSetting.ProductCode = "SMF";
			consolSetting.Description = "SMF";
			EDIDataRegistry.Instance.ConsolidatedBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, consolSettings);

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2021, 1, 1), db1.PK, priceItem.PK, ZGuid.Empty, "SMF");
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Reference 1"",""Reference 2"",""Reference 3"",""Reference 4"",""Usage Time (UTC)"",""Tenant ID""
""   "",""B10"",""S10"",""P01"",""1"",""DB1"","""","""","""",""01-Jan-21 10:00"",""1900""
""   "",""B11"",""S11"",""P01"",""2"",""DB2"","""","""","""",""01-Jan-21 10:00"",""1901""
""   "",""B12"",""S12"",""P01"",""3"",""DB3"","""","""","""",""01-Jan-21 10:00"",""1902""
";

			AssertEquals(expectedCsvResult, builder.ToString());

			string expectedCsvResult2 =
@"""01-Jan-21 10:00"",""   "",""B10"",""S10"",""DB1"",""P01"","""",""1"",""1900""
""01-Jan-21 10:00"",""   "",""B11"",""S11"",""DB2"",""P01"","""",""2"",""1901""
""01-Jan-21 10:00"",""   "",""B12"",""S12"",""DB3"",""P01"","""",""3"",""1902""
";
			var writer = new CsvUsageReportWriterForTest();
			report.GetCsvUsageReport(writer);
			AssertEquals(expectedCsvResult2, writer.ToString());

			var report2 = new StlReportingBusinessObjectForTest(Factory, new ZDateTime(2021, 1, 1), db1.PK, priceItem.PK, ZGuid.Empty, "SMF");
			var expectedCSV = @" Server SYD Jan 2021 
 Dates and times are in Universal Coordinated Time (UTC) 
 Usage Summary 
 Company Code Usage Type Usage Count Reference 1 Reference 2 Reference 3 Reference 4 Tenant ID Usage Time (UTC) 
 P01 1 DB1 1900 01-Jan-21 10:00 
 P01 2 DB2 1901 01-Jan-21 10:00 
 P01 3 DB3 1902 01-Jan-21 10:00 
";
			AssertEquals(expectedCSV, report2.GetUsageReportForDocTemplateTest());
		}

		public void TestWarehouseUsageNewRefFormat()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");

			var priceItem = org1.LicCompany.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "WOL";
			priceItem.L7_Category = "STL";
			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WOL", new ZDateTime(2023, 9, 10, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "BRI - W00032370", "#9.0", null, null, null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WOL", new ZDateTime(2023, 9, 15, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "MEL - N", "#2.0", "W00032380", null, null, "ENT", 2));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2023, 9, 1), db.PK, priceItem.PK, ZGuid.Empty, BillingConstants.BillingSystem.STL);
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			var expectedCsvResult =
@"""Company Code"",""Branch Code"",""Staff Code"",""Usage Type"",""Usage Count"",""Reference 1"",""Reference 2"",""Reference 3"",""Reference 4"",""Usage Time (UTC)""
""ABC"",""B10"",""S10"",""WOL"",""1"",""BRI - W00032370"",""#9.0"","""","""",""10-Sep-23 10:00""
""ABC"",""B11"",""S11"",""WOL"",""2"",""MEL - N - W00032380"",""#2.0"","""","""",""15-Sep-23 10:00""
";
			AssertEquals(expectedCsvResult, builder.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocTemplate()
		{
			BillingTestHelper.LoadClientSpecificDocuments();

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "III");
			var clientCompany1 = lic1.ClientCompany;
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var clientCompany2 = lic2.ClientCompany;
			clientCompany2.LCC_RN_NKCountryCode = "GB";
			var db = lic1.Database;
			db.LD_DatabaseNumber = 342;

			var clientStaff1 = BillingTestHelper.CreateClientStaff(db, "US1", "User One");
			var clientStaff2 = BillingTestHelper.CreateClientStaff(db, "US2", "User Two");
			var clientStaff3 = BillingTestHelper.CreateClientStaff(db, "ZZ", "webuser");

			var priceHeader = lic1.Company.PriceHeaders.AddNew();

			var priceItem1 = priceHeader.Items.AddNew();
			priceItem1.L7_Description = "  Local Docs - French";
			priceItem1.L7_Code = "FR7";

			var priceItem2 = priceHeader.Items.AddNew();
			priceItem2.L7_Description = "  Language - French";
			priceItem2.L7_Code = "GFR";
			var usageMap1 = priceHeader.UsageMaps.AddNew();
			usageMap1.PUM_UsageCode = "FR8";
			usageMap1.PUM_PriceCode = "GFR";
			var usageMap2 = priceHeader.UsageMaps.AddNew();
			usageMap2.PUM_UsageCode = "FR9";
			usageMap2.PUM_PriceCode = "GFR";

			var priceItem3 = priceHeader.Items.AddNew();
			priceItem3.L7_Description = "  Local Docs - Spanish";
			priceItem3.L7_Code = "ES7";

			var priceItem4 = priceHeader.Items.AddNew();
			priceItem4.L7_Description = "  Language - Spanish";
			priceItem4.L7_Code = "GES";
			var usageMap3 = priceHeader.UsageMaps.AddNew();
			usageMap3.PUM_UsageCode = "ES8";
			usageMap3.PUM_PriceCode = "GES";
			var usageMap4 = priceHeader.UsageMaps.AddNew();
			usageMap4.PUM_UsageCode = "ES9";
			usageMap4.PUM_PriceCode = "GES";

			var priceItemMCC = priceHeader.Items.AddNew();
			priceItemMCC.L7_Description = "  Multi Country Customers (Global, Regional, Multi Country Customer)";
			priceItemMCC.L7_Code = "MCC";

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff2, "ODM", "FR7", new ZDateTime(2017, 8, 5));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff1, "ODM", "FR7", new ZDateTime(2017, 8, 7));

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff2, "ODM", "FR8", new ZDateTime(2017, 8, 18));

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff3, "ODM", "FR9", new ZDateTime(2017, 8, 20));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff3, "ODM", "FR9", new ZDateTime(2017, 8, 25));

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff1, "ODM", "ES7", new ZDateTime(2017, 8, 2));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff2, "ODM", "ES7", new ZDateTime(2017, 8, 3));

			// date out of range
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff3, "ODM", "ES9", new ZDateTime(2017, 7, 30));

			//MCC
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "MCC", new ZDateTime(2017, 8, 1), clientCompany1, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "MCC", new ZDateTime(2017, 8, 1), clientCompany2, 1);

			Factory.Save();
			var report = new StlReportingBusinessObjectForTest(Factory, new ZDateTime(2017, 8, 1), lic1.LA_LD, priceItem1.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			var expectedCSV = @" Server SYD Aug 2017 
 Local Docs - French 
 Dates and times are in Universal Coordinated Time (UTC) 
 Usage Summary 
 Usage Type Staff Name (Code) / Country 
 FR7 User One (US1) 
 FR7 User Two (US2) 
";
			AssertEquals(expectedCSV, report.GetUsageReportForDocTemplateTest());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocTemplate_AdjustedUnitCount()
		{
			BillingTestHelper.LoadClientSpecificDocuments();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			var priceItem = org1.LicCompany.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "WTU";
			priceItem.L7_Category = "STL";

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "PackageID", "JobID", "PK#1", "1", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "PackageID", "JobID", "PK#1", "2", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "WTU", new ZDateTime(2016, 3, 3, 12, 0, 0), "DDDIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "PackageID", "JobID", "PK#1", "3", null, "ENT", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var report = new StlReportingBusinessObjectForTest(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty, BillingConstants.BillingSystem.STL);

			string expectedCSV = @" Server SYD Mar 2016 
 Dates and times are in Universal Coordinated Time (UTC) 
 Usage Summary 
 Company Code Usage Type Usage Count Reference 1 Reference 2 Reference 3 Reference 4 Adjusted Usage Count Usage Time (UTC) 
 ABC WTU 1 PackageID JobID PK#1 2 0.5000 01-Mar-16 10:00 
 III WTU 1 PackageID JobID PK#1 3 0.3300 03-Mar-16 12:00 
";
			AssertEquals(expectedCSV, report.GetUsageReportForDocTemplateTest());
		}

		public void TestSendDeveloperNotificationIfUsageReportIsNotLoaded()
		{
			var lines = new List<string>();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2017, 8, 1), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "TST");
			report.GetCsvUsageReport((csv) => { lines.Add(csv); });
			AssertEquals("can not load usage report for systemCode: TST", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StlReportingBusinessObject(Factory, new ZDateTime(2015, 9, 1), ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid(), "");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			EServicesBillingTestHelper.CreateTable();
		}

		public override void RunBare()
		{
			base.RunBare();
			EServicesBillingTestHelper.DropTable();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		public class StlReportingBusinessObjectForTest : StlReportingBusinessObject
		{
			public StlReportingBusinessObjectForTest(BusinessObjectFactory factory, ZDateTime periodStart, ZGuid databasePk, ZGuid priceItemPk, ZGuid companyPk, ZString systemCodes)
				: base(factory, periodStart, databasePk, priceItemPk, companyPk, systemCodes)
			{
			}

			public string GetUsageReportForDocTemplateTest()
			{
				var docWrapper = DocStlRawUsage.New(GetRawUsage(), Factory);

				(var rawDocument, _) = BillingInvoicingHelper.GetRawDocumentInExcel(UsageDocTemplate, docWrapper);
				var excelBytes = new ZBlob(rawDocument);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(excelBytes);
					using (var tempDir = new TempDirectory())
					{
						var csvFile = Path.Combine(tempDir.DirectoryName, "doc.csv");
						excelInterface.SaveToFile(csvFile);
						var csvContent = File.ReadAllText(csvFile);
						csvContent = csvContent.Replace("\t", " ");
						csvContent = Regex.Replace(csvContent, "[ ]{2,}", " ", RegexOptions.None);
						csvContent = Regex.Replace(csvContent, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
						csvContent = Regex.Replace(csvContent, @"\r\n|\n\r|\n|\r", "\r\n");
						return csvContent;
					}
				}
			}
		}

		#endregion
	}
}
