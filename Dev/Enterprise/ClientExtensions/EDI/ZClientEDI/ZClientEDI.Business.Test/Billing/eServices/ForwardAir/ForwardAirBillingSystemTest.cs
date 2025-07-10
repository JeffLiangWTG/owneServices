using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class ForwardAirBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new ForwardAirBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ForwardAir, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDCOMSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = "DDD";

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.AddPriceItem(priceHeader, "ECI", BillingConstants.FeeType.Transactional, "", 0.10m).L7_Description = "Airlines Messaging (FWB/FHL/FSU/FNA/FMA)";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ForwardAir, "FWA", new ZDateTime(2015, 11, 1), org.LicCompany.PK, 30);
			Factory.Save();

			var billingSystem = new ForwardAirBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 11, 30));

			var bill = billingSystem.LoadSystemBills(context).First();
			AssertEquals(BillingConstants.BillingSystem.ForwardAir, bill.SystemCode);

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(1, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Forward Air - 30 Transactions at AUD 0.10 per Transaction", lines[0].Description);
			});
		}

		public void TestLoadOdplRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");

			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "C00373196", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2", "LAX00075063", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "3d678a1f-30ed-4488-82f0-b2e4b5a6977b", "C00169295", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ForwardAirBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 11, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "C00373196");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "DDDABCSYD", "LAX00075063");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "DDDABCSYD", "C00169295");
				AssertOdplRawUsage(rawUsage.Summary.Lines[3], "DDDABCSYD", "");
			});

			string expectedCsvResult =
@"""Client ID"",""Consol"",""Message Time (UTC)""
""DDDABCSYD"",""C00373196"",""01-Nov-15 10:00""
""DDDABCSYD"",""LAX00075063"",""02-Nov-15 10:00""
""DDDABCSYD"",""C00169295"",""03-Nov-15 10:00""
""DDDABCSYD"","""",""30-Nov-15 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Nov-15 10:00"",""DDDABCSYD"",""B10"",""S10"",""C00373196"","""","""",""1""
""02-Nov-15 10:00"",""DDDABCSYD"",""B11"",""S11"",""LAX00075063"","""","""",""1""
""03-Nov-15 10:00"",""DDDABCSYD"",""B12"",""S12"",""C00169295"","""","""",""1""
""30-Nov-15 00:00"",""DDDABCSYD"",""B13"",""S13"","""","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string consol)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Consol", consol, summaryLine.Column2);
		}

		public void TestLoadStlRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");

			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "C00373196", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2", "LAX00075063", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "3d678a1f-30ed-4488-82f0-b2e4b5a6977b", "C00169295", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FWA", "FWA", new ZDateTime(2015, 11, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ForwardAirBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 11, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "C00373196");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "ABC", "LAX00075063");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "ABC", "C00169295");
				AssertStlRawUsage(rawUsage.Summary.Lines[3], "ABC", "");
			});

			string expectedCsvResult =
@"""Company Code"",""Consol"",""Message Time (UTC)""
""ABC"",""C00373196"",""01-Nov-15 10:00""
""ABC"",""LAX00075063"",""02-Nov-15 10:00""
""ABC"",""C00169295"",""03-Nov-15 10:00""
""ABC"","""",""30-Nov-15 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Nov-15 10:00"",""ABC"",""B10"",""S10"",""C00373196"","""","""",""1""
""02-Nov-15 10:00"",""ABC"",""B11"",""S11"",""LAX00075063"","""","""",""1""
""03-Nov-15 10:00"",""ABC"",""B12"",""S12"",""C00169295"","""","""",""1""
""30-Nov-15 00:00"",""ABC"",""B13"",""S13"","""","""","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string consol)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Consol", consol, summaryLine.Column2);
		}
	}
}