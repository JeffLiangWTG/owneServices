using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class ShippingPortMessagingBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new ShippingPortMessagingBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ShippingPortMessaging, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ShippingPortMessaging, "SDT", new ZDateTime(2016, 3, 31), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new ShippingPortMessagingBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 3, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as ShippingPortMessagingBill;
			AssertEquals(BillingConstants.BillingSystem.ShippingPortMessaging, bill.SystemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "FFF", "III", "BRN");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 342;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 5555;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "ABC", db2.PK, org2.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".III";

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPA", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "CSG397000-1", "Port of Napier COPARN", "PER", "1283CCE8-91E4-46B8-8640-8385B6632078", null) { BillableCount = 12 });
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPA", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "CSG397022-1", "Port of Napier COPARN", "PER", "3E23C542-6481-4A2A-8378-3311218C2BBC", null) { BillableCount = 11 });

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPE", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "TTNU8015978", "Port of Auckland COREOR", "PIR", "5070F8B6-802C-4DF4-B217-4515385D4962", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPR", new ZDateTime(2016, 3, 10, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "SZLU3635610", "Port of Tauranga CORPAR", "PAR", "778620A6-CDE1-4F76-AF83-34C77AE30A83", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPR", new ZDateTime(2016, 3, 11, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "SZLU3635610", "Port of Tauranga CORPAR", "PAR", "A0DEAB98-694E-4D89-A630-687BAE376024", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPR", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db2.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ShippingPortMessagingBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(1, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "Port of Napier COPARN", "PER", "CSG397022-1", "11");
			});

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "FFFIIIBRN", "Port of Auckland COREOR", "PIR", "TTNU8015978", "1");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "FFFIIIBRN", "Port of Tauranga CORPAR", "PAR", "SZLU3635610", "1");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "FFFIIIBRN", "Port of Tauranga CORPAR", "PAR", "SZLU3635610", "1");
				AssertOdplRawUsage(rawUsage.Summary.Lines[3], "FFFIIIBRN", "", "", "", "1");
			});

			string expectedCsvResult =
@"""Client ID"",""Message Recipient"",""Message Role"",""Release / Container / Shipment"",""Unit Count"",""Message Time (UTC)""
""FFFIIIBRN"",""Port of Auckland COREOR"",""PIR"",""TTNU8015978"",""1"",""03-Mar-16 12:00""
""FFFIIIBRN"",""Port of Tauranga CORPAR"",""PAR"",""SZLU3635610"",""1"",""10-Mar-16 10:00""
""FFFIIIBRN"",""Port of Tauranga CORPAR"",""PAR"",""SZLU3635610"",""1"",""11-Mar-16 10:00""
""FFFIIIBRN"","""","""","""",""1"",""31-Mar-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""03-Mar-16 12:00"",""FFFIIIBRN"",""B12"",""S12"",""Port of Auckland COREOR PIR TTNU8015978"","""","""",""1""
""10-Mar-16 10:00"",""FFFIIIBRN"",""B13"",""S13"",""Port of Tauranga CORPAR PAR SZLU3635610"","""","""",""1""
""11-Mar-16 10:00"",""FFFIIIBRN"",""B14"",""S14"",""Port of Tauranga CORPAR PAR SZLU3635610"","""","""",""1""
""31-Mar-16 00:00"",""FFFIIIBRN"",""B15"",""S15"","""","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string recipient, string role, string number, string unitCount)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Message Recipient", recipient, summaryLine.Column2);
			AssertEquals("Message Role", role, summaryLine.Column3);
			AssertEquals("Release / Container / Shipment Number", number, summaryLine.Column4);
			AssertEquals("Unit Count", unitCount, summaryLine.Column5);
		}

		public void TestLoadStlRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			var priceHeader = org1.LicCompany.PriceHeaders.AddNew();
			var priceItem1 = priceHeader.Items.AddNew();
			priceItem1.L7_Code = "SDT";
			var priceItem2 = priceHeader.Items.AddNew();
			priceItem2.L7_Code = "SPT";

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPA", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "CSG397000-1", "Port of Napier COPARN", "PER", "1283CCE8-91E4-46B8-8640-8385B6632078", null) { BillableCount = 12 });
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPA", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "CSG397022-1", "Port of Napier COPARN", "PER", "3E23C542-6481-4A2A-8378-3311218C2BBC", null) { BillableCount = 11 });

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPE", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "TTNU8015978", "Port of Auckland COREOR", "PIR", "5070F8B6-802C-4DF4-B217-4515385D4962", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPR", new ZDateTime(2016, 3, 10, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "SZLU3635610", "Port of Tauranga CORPAR", "PAR", "778620A6-CDE1-4F76-AF83-34C77AE30A83", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPR", new ZDateTime(2016, 3, 11, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "SZLU3635610", "Port of Tauranga CORPAR", "PAR", "A0DEAB98-694E-4D89-A630-687BAE376024", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SPM", "SPR", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ShippingPortMessagingBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem1.PK, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(2, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "Port of Napier COPARN", "PER", "CSG397022-1", "11");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "III", "Port of Auckland COREOR", "PIR", "TTNU8015978", "1");
			});

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem2.PK, ZGuid.Empty);
			rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "III", "Port of Tauranga CORPAR", "PAR", "SZLU3635610", "1");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "III", "Port of Tauranga CORPAR", "PAR", "SZLU3635610", "1");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "III", "", "", "", "1");
			});

			string expectedCsvResult =
@"""Company Code"",""Message Recipient"",""Message Role"",""Release / Container / Shipment"",""Unit Count"",""Message Time (UTC)""
""III"",""Port of Tauranga CORPAR"",""PAR"",""SZLU3635610"",""1"",""10-Mar-16 10:00""
""III"",""Port of Tauranga CORPAR"",""PAR"",""SZLU3635610"",""1"",""11-Mar-16 10:00""
""III"","""","""","""",""1"",""31-Mar-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""10-Mar-16 10:00"",""III"",""B13"",""S13"",""Port of Tauranga CORPAR PAR SZLU3635610"",""SPT"","""",""1""
""11-Mar-16 10:00"",""III"",""B14"",""S14"",""Port of Tauranga CORPAR PAR SZLU3635610"",""SPT"","""",""1""
""31-Mar-16 00:00"",""III"",""B15"",""S15"","""",""SPT"","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string recipient, string role, string number, string unitCount)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Message Recipient", recipient, summaryLine.Column2);
			AssertEquals("Message Role", role, summaryLine.Column3);
			AssertEquals("Release / Container / Shipment Number", number, summaryLine.Column4);
			AssertEquals("Unit Count", unitCount, summaryLine.Column5);
		}
	}
}