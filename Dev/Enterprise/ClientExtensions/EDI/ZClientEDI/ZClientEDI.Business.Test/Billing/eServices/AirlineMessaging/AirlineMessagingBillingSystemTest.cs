using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class AirlineMessagingBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new AirlineMessagingBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.AirlineMessaging, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.AirlineMessaging, "W1C", new ZDateTime(2014, 11, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new AirlineMessagingBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2014, 11, 30));

			var bill = billingSystem.LoadSystemBills(context).First() as AirlineMessagingBill;
			AssertEquals(BillingConstants.BillingSystem.AirlineMessaging, bill.SystemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "EEE", "LOL", "MEL");
			var org3 = BillingTestHelper.CreateOrganisation(Factory, "FFF", "III", "BRN");
			var traxonOrg = BillingTestHelper.CreateOrganisation(Factory, "TXN", "FFO", "DDD");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 2544;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 9871;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "LOL", db2.PK, org2.PK, "", "");

			var db3 = org3.LicCompany.ActiveOrAllLicDatabases[0];
			db3.LD_DatabaseNumber = 42342;
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "III", db3.PK, org3.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".LOL";
			var clientNumber3 = db3.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2018, 08, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "US", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2018, 08, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-WB", new ZDateTime(2018, 08, 4, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-HL", new ZDateTime(2018, 08, 5, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FMA", new ZDateTime(2018, 08, 6, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCSJ", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CZ", "CCSJ", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 1, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2018, 08, 2, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONEDP", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2018, 08, 3, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONRCF", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 1, 10, 0, 0), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 2, 10, 0, 0), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 30, 0, 0, 0), "FFFIIIBRN", null, db3.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new AirlineMessagingBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2018, 08, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(3, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals(2, rawUsage.SummarySections[1].Lines.Count);

			var writer1 = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer1);
			AssertEquals(@"""01-Aug-18 10:00"",""DDDABCSYD"",""B10"",""S10"",""CCN CX"",""FWB"","""",""1""
""02-Aug-18 10:00"",""DDDABCSYD"",""B11"",""S11"",""CCN US"",""FHL"","""",""1""
""03-Aug-18 10:00"",""DDDABCSYD"",""B12"",""S12"",""CCN"",""FSU"","""",""1""
""07-Aug-18 10:00"",""DDDABCSYD"",""B16"",""S16"",""CCSJ CX"",""FWB"","""",""1""
""07-Aug-18 10:00"",""DDDABCSYD"",""B17"",""S17"",""CCSJ CZ"",""FWB"","""",""1""
", writer1.ToString());

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2018, 08, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(3, rawUsage.SummarySections[0].Lines.Count);

			var licence3 = org3.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2018, 08, 1), org3.PK, clientCompany3.PK, licence3.Company.PK, licence3.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(2, rawUsage.SummarySections[0].Lines.Count);

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2018, 08, 1), traxonOrg.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(5, rawUsage.SummarySections[0].Lines.Count);

			string expectedCsvResult =
@"""Provider"",""Client ID"",""Message Type"",""Airline"",""AWB"",""Message Time (UTC)""
""Traxon"",""EEELOLMEL"",""FWB"","""","""",""01-Aug-18 10:00""
""Traxon"",""EEELOLMEL"",""FHL"","""","""",""02-Aug-18 10:00""
""Traxon"",""EEELOLMEL"",""FSU"","""","""",""03-Aug-18 10:00""
""Traxon"",""FFFIIIBRN"",""FWB"","""","""",""01-Aug-18 10:00""
""Traxon"",""FFFIIIBRN"",""FWB"","""","""",""02-Aug-18 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Aug-18 10:00"",""EEELOLMEL"",""B18"",""S18"",""Traxon"",""FWB"","""",""1""
""02-Aug-18 10:00"",""EEELOLMEL"",""B19"",""S19"",""Traxon"",""FHL"","""",""1""
""03-Aug-18 10:00"",""EEELOLMEL"",""B20"",""S20"",""Traxon"",""FSU"","""",""1""
""01-Aug-18 10:00"",""FFFIIIBRN"",""B21"",""S21"",""Traxon"",""FWB"","""",""1""
""02-Aug-18 10:00"",""FFFIIIBRN"",""B22"",""S22"",""Traxon"",""FWB"","""",""1""
", writer.ToString());
		}

		public void TestLoadOdplRawUsagePre201808()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "EEE", "LOL", "MEL");
			var org3 = BillingTestHelper.CreateOrganisation(Factory, "FFF", "III", "BRN");
			var traxonOrg = BillingTestHelper.CreateOrganisation(Factory, "TXN", "FFO", "DDD");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 2544;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 9871;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "LOL", db2.PK, org2.PK, "", "");

			var db3 = org3.LicCompany.ActiveOrAllLicDatabases[0];
			db3.LD_DatabaseNumber = 42342;
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "III", db3.PK, org3.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".LOL";
			var clientNumber3 = db3.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2014, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "US", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2014, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-WB", new ZDateTime(2014, 11, 4, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-HL", new ZDateTime(2014, 11, 5, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FMA", new ZDateTime(2014, 11, 6, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCSJ", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CZ", "CCSJ", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 1, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2014, 11, 2, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONEDP", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2014, 11, 3, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONRCF", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 1, 10, 0, 0), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 2, 10, 0, 0), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 30, 0, 0, 0), "FFFIIIBRN", null, db3.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new AirlineMessagingBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(5, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals(2, rawUsage.SummarySections[1].Lines.Count);

			var writer1 = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer1);
			AssertEquals(@"""01-Nov-14 10:00"",""DDDABCSYD"",""B10"",""S10"",""CCN CX"",""FWB"","""",""1""
""02-Nov-14 10:00"",""DDDABCSYD"",""B11"",""S11"",""CCN US"",""FHL"","""",""1""
""03-Nov-14 10:00"",""DDDABCSYD"",""B12"",""S12"",""CCN"",""FSU"","""",""1""
""04-Nov-14 10:00"",""DDDABCSYD"",""B13"",""S13"",""CCN"",""FNA (FWB)"","""",""-1""
""05-Nov-14 10:00"",""DDDABCSYD"",""B14"",""S14"",""CCN"",""FNA (FHL)"","""",""-1""
""07-Nov-14 10:00"",""DDDABCSYD"",""B16"",""S16"",""CCSJ CX"",""FWB"","""",""1""
""07-Nov-14 10:00"",""DDDABCSYD"",""B17"",""S17"",""CCSJ CZ"",""FWB"","""",""1""
", writer1.ToString());

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(3, rawUsage.SummarySections[0].Lines.Count);

			var licence3 = org3.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), org3.PK, clientCompany3.PK, licence3.Company.PK, licence3.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(2, rawUsage.SummarySections[0].Lines.Count);

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), traxonOrg.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(5, rawUsage.SummarySections[0].Lines.Count);

			string expectedCsvResult =
@"""Provider"",""Client ID"",""Message Type"",""Airline"",""AWB"",""Message Time (UTC)""
""Traxon"",""EEELOLMEL"",""FWB"","""","""",""01-Nov-14 10:00""
""Traxon"",""EEELOLMEL"",""FHL"","""","""",""02-Nov-14 10:00""
""Traxon"",""EEELOLMEL"",""FSU"","""","""",""03-Nov-14 10:00""
""Traxon"",""FFFIIIBRN"",""FWB"","""","""",""01-Nov-14 10:00""
""Traxon"",""FFFIIIBRN"",""FWB"","""","""",""02-Nov-14 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Nov-14 10:00"",""EEELOLMEL"",""B18"",""S18"",""Traxon"",""FWB"","""",""1""
""02-Nov-14 10:00"",""EEELOLMEL"",""B19"",""S19"",""Traxon"",""FHL"","""",""1""
""03-Nov-14 10:00"",""EEELOLMEL"",""B20"",""S20"",""Traxon"",""FSU"","""",""1""
""01-Nov-14 10:00"",""FFFIIIBRN"",""B21"",""S21"",""Traxon"",""FWB"","""",""1""
""02-Nov-14 10:00"",""FFFIIIBRN"",""B22"",""S22"",""Traxon"",""FWB"","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EEE", "LOL", "MEL");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "DDD", "III", "SYD");
			var traxonOrg = BillingTestHelper.CreateLicence(Factory, "TXN", "FFO", "DDD");

			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 2544;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, lic1.Company.LC_OH, "", "");
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "III", db1.PK, lic3.Company.LC_OH, "", "");

			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 9871;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "LOL", db2.PK, lic2.Company.LC_OH, "", "");

			var priceItems = new List<ClientLicencePriceItem>();

			foreach (var lic in new[] { lic1, lic2 })
			{
				var priceHeader = lic.Company.PriceHeaders.AddNew();
				var link = BillingTestHelper.CreatePriceLink(lic.Database, priceHeader, new ZDateTime(2010, 1, 1));
				priceItems.Add(BillingTestHelper.AddPriceItem(priceHeader, new UsageCodeKey(BillingConstants.BillingSystem.AirlineMessaging, "AIR"), "TRA", 10));

				foreach (var usageCode in new[] { "H4C", "S4C", "W3C", "W4C", "WAC", "WUC", "HAC", "SAC" })
				{
					var usageMap = priceHeader.UsageMaps.AddNew();
					usageMap.PUM_PriceCategory = BillingConstants.BillingSystem.AirlineMessaging;
					usageMap.PUM_PriceCode = "AIR";
					usageMap.PUM_UsageCategory = BillingConstants.BillingSystem.AirlineMessaging;
					usageMap.PUM_UsageCode = usageCode;
				}
			}

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".LOL";
			var clientNumber3 = db1.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2018, 08, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "US", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2018, 08, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-WB", new ZDateTime(2018, 08, 4, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-HL", new ZDateTime(2018, 08, 5, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FMA", new ZDateTime(2018, 08, 6, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCSJ", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CZ", "CCSJ", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 1, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2018, 08, 2, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONEDP", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2018, 08, 3, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONRCF", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 1, 10, 0, 0), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 2, 10, 0, 0), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2018, 08, 30, 0, 0, 0), "FFFIIIBRN", null, db1.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new AirlineMessagingBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2018, 08, 1), db1.PK, priceItems[0].PK, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context) as MultiSectionStlRawUsage;
			AssertEquals(3, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals(2, rawUsage.SummarySections[1].Lines.Count);

			var writer1 = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer1);
			AssertEquals(@"""30-Aug-18 00:00"",""III"",""B23"",""S23"","""",""FWB"","""",""1""
""01-Aug-18 10:00"",""ABC"",""B10"",""S10"",""CCN CX"",""FWB"","""",""1""
""02-Aug-18 10:00"",""ABC"",""B11"",""S11"",""CCN US"",""FHL"","""",""1""
""03-Aug-18 10:00"",""ABC"",""B12"",""S12"",""CCN"",""FSU"","""",""1""
""07-Aug-18 10:00"",""ABC"",""B16"",""S16"",""CCSJ CX"",""FWB"","""",""1""
""07-Aug-18 10:00"",""ABC"",""B17"",""S17"",""CCSJ CZ"",""FWB"","""",""1""
""01-Aug-18 10:00"",""III"",""B21"",""S21"",""Traxon"",""FWB"","""",""1""
""02-Aug-18 10:00"",""III"",""B22"",""S22"",""Traxon"",""FWB"","""",""1""
", writer1.ToString());

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2018, 08, 1), db2.PK, priceItems[1].PK, ZGuid.Empty);
			rawUsage = billingSystem.LoadStlRawUsage(context) as MultiSectionStlRawUsage;
			AssertEquals(3, rawUsage.SummarySections[0].Lines.Count);

			string expectedCsvResult =
@"""Provider"",""Company Code"",""Message Type"",""Airline"",""AWB"",""Message Time (UTC)""
""Traxon"",""LOL"",""FWB"","""","""",""01-Aug-18 10:00""
""Traxon"",""LOL"",""FHL"","""","""",""02-Aug-18 10:00""
""Traxon"",""LOL"",""FSU"","""","""",""03-Aug-18 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Aug-18 10:00"",""LOL"",""B18"",""S18"",""Traxon"",""FWB"","""",""1""
""02-Aug-18 10:00"",""LOL"",""B19"",""S19"",""Traxon"",""FHL"","""",""1""
""03-Aug-18 10:00"",""LOL"",""B20"",""S20"",""Traxon"",""FSU"","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsagePre201808()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EEE", "LOL", "MEL");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "DDD", "III", "SYD");
			var traxonOrg = BillingTestHelper.CreateLicence(Factory, "TXN", "FFO", "DDD");

			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 2544;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, lic1.Company.LC_OH, "", "");
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "III", db1.PK, lic3.Company.LC_OH, "", "");

			var db2 = lic2.Database;
			db2.LD_DatabaseNumber = 9871;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "LOL", db2.PK, lic2.Company.LC_OH, "", "");

			var priceItems = new List<ClientLicencePriceItem>();

			foreach (var lic in new[] { lic1, lic2 })
			{
				var priceHeader = lic.Company.PriceHeaders.AddNew();
				var link = BillingTestHelper.CreatePriceLink(lic.Database, priceHeader, new ZDateTime(2010, 1, 1));
				priceItems.Add(BillingTestHelper.AddPriceItem(priceHeader, new UsageCodeKey(BillingConstants.BillingSystem.AirlineMessaging, "AIR"), "TRA", 10));

				foreach (var usageCode in new[] { "H4C", "S4C", "W3C", "W4C", "WAC", "WUC", "HAC", "SAC" })
				{
					var usageMap = priceHeader.UsageMaps.AddNew();
					usageMap.PUM_PriceCategory = BillingConstants.BillingSystem.AirlineMessaging;
					usageMap.PUM_PriceCode = "AIR";
					usageMap.PUM_UsageCategory = BillingConstants.BillingSystem.AirlineMessaging;
					usageMap.PUM_UsageCode = usageCode;
				}
			}

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".LOL";
			var clientNumber3 = db1.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2014, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "US", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2014, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-WB", new ZDateTime(2014, 11, 4, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "-HL", new ZDateTime(2014, 11, 5, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FMA", new ZDateTime(2014, 11, 6, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "", "CCN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CX", "CCSJ", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 7, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "", "", "CZ", "CCSJ", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 1, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FHL", new ZDateTime(2014, 11, 2, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONEDP", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FSU", new ZDateTime(2014, 11, 3, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "", "", "", "TRAXONRCF", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 1, 10, 0, 0), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 2, 10, 0, 0), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "", "", "", "TRAXON", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo(BillingConstants.BillingSystem.AirlineMessaging, "FWB", new ZDateTime(2014, 11, 30, 0, 0, 0), "FFFIIIBRN", null, db1.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new AirlineMessagingBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), db1.PK, priceItems[0].PK, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context) as MultiSectionStlRawUsage;
			AssertEquals(5, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals(2, rawUsage.SummarySections[1].Lines.Count);

			var writer1 = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer1);
			AssertEquals(@"""30-Nov-14 00:00"",""III"",""B23"",""S23"","""",""FWB"","""",""1""
""01-Nov-14 10:00"",""ABC"",""B10"",""S10"",""CCN CX"",""FWB"","""",""1""
""02-Nov-14 10:00"",""ABC"",""B11"",""S11"",""CCN US"",""FHL"","""",""1""
""03-Nov-14 10:00"",""ABC"",""B12"",""S12"",""CCN"",""FSU"","""",""1""
""04-Nov-14 10:00"",""ABC"",""B13"",""S13"",""CCN"",""FNA (FWB)"","""",""-1""
""05-Nov-14 10:00"",""ABC"",""B14"",""S14"",""CCN"",""FNA (FHL)"","""",""-1""
""07-Nov-14 10:00"",""ABC"",""B16"",""S16"",""CCSJ CX"",""FWB"","""",""1""
""07-Nov-14 10:00"",""ABC"",""B17"",""S17"",""CCSJ CZ"",""FWB"","""",""1""
""01-Nov-14 10:00"",""III"",""B21"",""S21"",""Traxon"",""FWB"","""",""1""
""02-Nov-14 10:00"",""III"",""B22"",""S22"",""Traxon"",""FWB"","""",""1""
", writer1.ToString());

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), db2.PK, priceItems[1].PK, ZGuid.Empty);
			rawUsage = billingSystem.LoadStlRawUsage(context) as MultiSectionStlRawUsage;
			AssertEquals(3, rawUsage.SummarySections[0].Lines.Count);

			string expectedCsvResult =
@"""Provider"",""Company Code"",""Message Type"",""Airline"",""AWB"",""Message Time (UTC)""
""Traxon"",""LOL"",""FWB"","""","""",""01-Nov-14 10:00""
""Traxon"",""LOL"",""FHL"","""","""",""02-Nov-14 10:00""
""Traxon"",""LOL"",""FSU"","""","""",""03-Nov-14 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Nov-14 10:00"",""LOL"",""B18"",""S18"",""Traxon"",""FWB"","""",""1""
""02-Nov-14 10:00"",""LOL"",""B19"",""S19"",""Traxon"",""FHL"","""",""1""
""03-Nov-14 10:00"",""LOL"",""B20"",""S20"",""Traxon"",""FSU"","""",""1""
", writer.ToString());
		}

		public void TestCheckUnmappedUsages()
		{
			var mailGroup = Factory.New<GlbGroup>();
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "teststaff@edi.com.au";
			mailGroup.GG_Code = "TXB";
			mailGroup.Staff.Add(currentStaff);
			Factory.Save();
			EDIDataRegistry.Instance.InternalNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mailGroup.PK.ToGuid());

			var periodStart = new ZDateTime(2025, 4, 1);
			var logger = new SimpleLogger();
			AirlineMessagingBillingSystem.CheckUnmappedUsages(null, periodStart);
			AirlineMessagingBillingSystem.CheckUnmappedUsages(logger, periodStart);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.CreateChargeableUsage(Factory, "AMG", "C01", periodStart, lic, 99);
			BillingTestHelper.CreateChargeableUsage(Factory, "AMG", "WUC", periodStart.AddMonths(1), lic, 99);
			Factory.Save();
			AirlineMessagingBillingSystem.CheckUnmappedUsages(logger, periodStart);
			AssertEquals("", logger.ToString());

			BillingTestHelper.CreateChargeableUsage(Factory, "AMG", "WUC", periodStart, lic, 15);
			Factory.Save();
			AirlineMessagingBillingSystem.CheckUnmappedUsages(logger, periodStart);
			AssertEquals(@"Error: AMG Usage Mapping Error Detected – Action Required
Error: Some AMG usages are not mapping correctly. Please check and resolve. Ref:WI00894811,AirlineMessagingBillingSystem.CheckUnmappedUsages()
", logger.ToString());

			AssertEquals("email count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("AMG Usage Mapping Error Detected – Action Required", email.Subject);
			AssertEquals("recipients", 1, email.Recipients.Count);
			AssertEquals("to", "teststaff@edi.com.au", email.Recipients[0].Email);
		}
	}
}
