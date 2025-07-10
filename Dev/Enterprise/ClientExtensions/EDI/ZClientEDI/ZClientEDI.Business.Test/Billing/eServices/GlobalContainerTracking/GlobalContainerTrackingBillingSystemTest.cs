using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class GlobalContainerTrackingBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new GlobalContainerTrackingBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.GlobalContainerTracking, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			SetupPriceList();

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD", true);
			var invoiceDelivery = licence.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.GlobalContainerTracking, "", new ZDateTime(2015, 7, 1), licence, 10);
			Factory.Save();

			var billingSystem = new GlobalContainerTrackingBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 7, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as GlobalContainerTrackingBill;
			AssertEquals(BillingConstants.BillingSystem.GlobalContainerTracking, bill.SystemCode);
		}

		#region Load Usages

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		public void TestLoadUsages()
		{
			SetupPriceList();

			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD", true);
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "BBB", "SYD", true);

			var chargeableUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.GlobalContainerTracking, "", new ZDateTime(2015, 7, 1), licence1, 2000);
			var chargeableUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.GlobalContainerTracking, "", new ZDateTime(2015, 7, 1), licence2, 3000);

			Factory.Save();

			var billingSystem = new GlobalContainerTrackingBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 7, 31));

			var allUsages = new List<GlobalContainerTrackingUsage>();
			foreach (SystemBill bill in billingSystem.LoadSystemBills(context))
			{
				allUsages.AddRange(bill.SystemUsages.Cast<GlobalContainerTrackingUsage>());
			}

			var rowsAsText = string.Join("\r\n", allUsages.Select(x => $"{x.LicenceNineCode} - UnitCount:{x.UnitCount} - UnitPrice:{x.UnitPrice} - Amount:{x.Amount} - L7_UnitBreak:{x.PriceItem.L7_UnitBreak} - L7_Description:{x.PriceItem.L7_Description} - TransactionDescription:{x.TransactionDescription}").OrderBy(x => x));
			AssertEquals(@"DDD-AAA-SYD - UnitCount:2000 - UnitPrice:0.700000 - Amount:1400.00 - L7_UnitBreak:4000 - L7_Description:   4001 - 5000 - TransactionDescription:4001 - 5000 unique container tracked
DDD-BBB-SYD - UnitCount:3000 - UnitPrice:0.700000 - Amount:2100.00 - L7_UnitBreak:4000 - L7_Description:   4001 - 5000 - TransactionDescription:4001 - 5000 unique container tracked", rowsAsText);
		}

		void SetupPriceList()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var ctrPriceList = stdLicCompany.PriceHeaders.AddNew();
			ctrPriceList.L6_PricelistVersion = "V1";
			ctrPriceList.L6_SystemCode = BillingConstants.BillingSystem.GlobalContainerTracking;
			ctrPriceList.L6_RX_NKCurrency = "USD";
			ctrPriceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.95m);
			priceItem1.L7_Description = "   1-250";
			priceItem1.L7_UnitBreak = 0;
			priceItem1.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem2 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.90m);
			priceItem2.L7_Description = "   251 - 1000";
			priceItem2.L7_UnitBreak = 250;
			priceItem2.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem3 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.85m);
			priceItem3.L7_Description = "   1001 - 2000";
			priceItem3.L7_UnitBreak = 1000;
			priceItem3.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem4 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.80m);
			priceItem4.L7_Description = "   2001 - 3000";
			priceItem4.L7_UnitBreak = 2000;
			priceItem4.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem5 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.75m);
			priceItem5.L7_Description = "   3001 - 4000";
			priceItem5.L7_UnitBreak = 3000;
			priceItem5.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem6 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.70m);
			priceItem6.L7_Description = "   4001 - 5000";
			priceItem6.L7_UnitBreak = 4000;
			priceItem6.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem7 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.65m);
			priceItem7.L7_Description = "   5001 - 25000";
			priceItem7.L7_UnitBreak = 5000;
			priceItem7.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem8 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.60m);
			priceItem8.L7_Description = "   25001 - 50000";
			priceItem8.L7_UnitBreak = 25000;
			priceItem8.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem9 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.55m);
			priceItem9.L7_Description = "   50001 - 100000";
			priceItem9.L7_UnitBreak = 50000;
			priceItem9.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var priceItem10 = BillingTestHelper.AddPriceItem(ctrPriceList, "CTR", BillingConstants.FeeType.TransactionalOneVolumeBreak, "", 0.50m);
			priceItem10.L7_Description = "   More than 100000";
			priceItem10.L7_UnitBreak = 100000;
			priceItem10.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);
		}

		#endregion

		#region Load Raw Usages

		public void TestLoadOdplRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 41243;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTR", new ZDateTime(2015, 1, 4, 0, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "BSIU2455731", "AUS1231242342", "FUL", "1ADA2744-BF67-48FE-B669-3045B98D3853", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTR", new ZDateTime(2015, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "GLDU5698560", "HAM150334179", "GIN", "6102DF7D-5C9B-48C3-9C87-4C7AC1BDA8B5", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTR", new ZDateTime(2015, 7, 4, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "YMLU8225624", "QSWB4396236", "FUL", "B447888F-968A-45BE-A0EB-43A3381CD8F8", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTR", new ZDateTime(2015, 7, 4, 16, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "BSIU2455731", "SHA1502AXGN0", "RLS", "776BC945-2F62-450A-B347-9E2775E42B58", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("CTR", "CTR", new ZDateTime(2015, 7, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new GlobalContainerTrackingBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 7, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "GLDU5698560", "HAM150334179");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "DDDABCSYD", "YMLU8225624", "QSWB4396236");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "DDDABCSYD", "BSIU2455731", "SHA1502AXGN0");
				AssertOdplRawUsage(rawUsage.Summary.Lines[3], "DDDABCSYD", "", "");
			});

			string expectedCsvResult =
@"""Client ID"",""Container Number"",""Job Number"",""CBR/MBN"",""Carrier Code"",""Message Time (UTC)""
""DDDABCSYD"",""GLDU5698560"",""HAM150334179"",""GIN"",""6102DF7D-5C9B-48C3-9C87-4C7AC1BDA8B5"",""01-Jul-15 10:00""
""DDDABCSYD"",""YMLU8225624"",""QSWB4396236"",""FUL"",""B447888F-968A-45BE-A0EB-43A3381CD8F8"",""04-Jul-15 10:00""
""DDDABCSYD"",""BSIU2455731"",""SHA1502AXGN0"",""RLS"",""776BC945-2F62-450A-B347-9E2775E42B58"",""04-Jul-15 16:00""
""DDDABCSYD"","""","""","""","""",""30-Jul-15 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Jul-15 10:00"",""DDDABCSYD"",""B11"",""S11"",""GLDU5698560 HAM150334179 GIN 6102DF7D-5C9B-48C3-9C87-4C7AC1BDA8B5"",""CTR"",""Container Automation"",""1""
""04-Jul-15 10:00"",""DDDABCSYD"",""B12"",""S12"",""YMLU8225624 QSWB4396236 FUL B447888F-968A-45BE-A0EB-43A3381CD8F8"",""CTR"",""Container Automation"",""1""
""04-Jul-15 16:00"",""DDDABCSYD"",""B13"",""S13"",""BSIU2455731 SHA1502AXGN0 RLS 776BC945-2F62-450A-B347-9E2775E42B58"",""CTR"",""Container Automation"",""1""
""30-Jul-15 00:00"",""DDDABCSYD"",""B14"",""S14"","""",""CTR"",""Container Automation"",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string containerNumber, string jobNumber)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Container Number", containerNumber, summaryLine.Column2);
			AssertEquals("Job Number", jobNumber, summaryLine.Column3);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
