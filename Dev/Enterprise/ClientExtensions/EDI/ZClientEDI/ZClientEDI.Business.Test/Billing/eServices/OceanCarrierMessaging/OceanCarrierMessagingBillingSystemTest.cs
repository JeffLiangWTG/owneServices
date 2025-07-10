using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class OceanCarrierMessagingBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new OceanCarrierMessagingBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.OceanCarrierMessaging, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.OceanCarrierMessaging, "SHI", new ZDateTime(2016, 3, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new OceanCarrierMessagingBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 3, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as TransactionalSystemBill;
			AssertEquals(BillingConstants.BillingSystem.OceanCarrierMessaging, bill.SystemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";
			PrepareUsages(clientNumber, db.DatabaseId, clientCompany.PK);

			var billingSystem = new OceanCarrierMessagingBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(5, rawUsage.SummarySections.Count);
			CombineAssertions(() =>
			{
				AssertRawUsage(rawUsage.SummarySections[0].Lines[0], "DDDABCSYD", "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "INTTRA_BK", "C00010120", "HLCU");
				AssertRawUsage(rawUsage.SummarySections[1].Lines[0], "DDDABCSYD", "8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2", "CMACGM", "C600605303", "");
				AssertRawUsage(rawUsage.SummarySections[2].Lines[0], "DDDABCSYD", "3d678a1f-30ed-4488-82f0-b2e4b5a6977b", "INTTRA_SI", "C02617703", "NYKS");
				AssertRawUsage(rawUsage.SummarySections[3].Lines[0], "DDDABCSYD", "INTTRA_BK", "C00010120", "AAA", "HLCU");
				AssertRawUsage(rawUsage.SummarySections[3].Lines[1], "DDDABCSYD", "CMACGM", "C600605303", "BBB", "");
				AssertRawUsage(rawUsage.SummarySections[4].Lines[0], "DDDABCSYD", "C00010120", "B0003948", "M034830234", "SENDER A");
				AssertRawUsage(rawUsage.SummarySections[4].Lines[1], "DDDABCSYD", "C02617703", "B0009933", "M002837327", "SENDER B");
			});

			string expectedCsvResult =
@"""Type"",""Client ID"",""Message ID"",""Provider"",""Consol"",""Carrier Code"",""Ref 5"",""Message Time (UTC)""
""Shipping Instruction Sent"",""DDDABCSYD"",""084d9e6a-f687-4eed-87bf-e6d5b3a31a06"",""INTTRA_BK"",""C00010120"",""HLCU"","""",""01-Mar-16 10:00""
""Type"",""Client ID"",""Message ID"",""Provider"",""Consol"",""Carrier Code"",""Ref 5"",""Message Time (UTC)""
""Booking Request Sent"",""DDDABCSYD"",""8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2"",""CMACGM"",""C600605303"","""","""",""02-Mar-16 10:00""
""Type"",""Client ID"",""Message ID"",""Provider"",""Consol"",""Carrier Code"",""Ref 5"",""Message Time (UTC)""
""VGM Sent"",""DDDABCSYD"",""3d678a1f-30ed-4488-82f0-b2e4b5a6977b"",""INTTRA_SI"",""C02617703"",""NYKS"","""",""03-Mar-16 10:00""
""Type"",""Client ID"",""Message ID"",""Provider"",""Job Number"",""Event Type"",""Reference Number"",""Message Time (UTC)""
""Shipping Instruction Received"",""DDDABCSYD"",""INTTRA_BK"",""C00010120"",""AAA"",""HLCU"","""",""01-Mar-16 10:00""
""Shipping Instruction Received"",""DDDABCSYD"",""CMACGM"",""C600605303"",""BBB"","""","""",""02-Mar-16 10:00""
""Type"",""Client ID"",""Container"",""Booking Number"",""Bill Number"",""Sender"",""Ref 5"",""Message Time (UTC)""
""VGM Received"",""DDDABCSYD"",""C00010120"",""B0003948"",""M034830234"",""SENDER A"","""",""01-Mar-16 10:00""
""VGM Received"",""DDDABCSYD"",""C02617703"",""B0009933"",""M002837327"",""SENDER B"","""",""02-Mar-16 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Mar-16 10:00"",""DDDABCSYD"",""B10"",""S10"",""Shipping Instruction Sent 084d9e6a-f687-4eed-87bf-e6d5b3a31a06 INTTRA_BK C00010120 HLCU"",""SHI"","""",""1""
""02-Mar-16 10:00"",""DDDABCSYD"",""B11"",""S11"",""Booking Request Sent 8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2 CMACGM C600605303"",""BRT"","""",""1""
""03-Mar-16 10:00"",""DDDABCSYD"",""B12"",""S12"",""VGM Sent 3d678a1f-30ed-4488-82f0-b2e4b5a6977b INTTRA_SI C02617703 NYKS"",""VGM"","""",""1""
""01-Mar-16 10:00"",""DDDABCSYD"",""B13"",""S13"",""Shipping Instruction Received INTTRA_BK C00010120 AAA HLCU"",""SHR"","""",""1""
""02-Mar-16 10:00"",""DDDABCSYD"",""B14"",""S14"",""Shipping Instruction Received CMACGM C600605303 BBB"",""SHR"","""",""1""
""01-Mar-16 10:00"",""DDDABCSYD"",""B15"",""S15"",""VGM Received C00010120 B0003948 M034830234 SENDER A"",""CMV"","""",""1""
""02-Mar-16 10:00"",""DDDABCSYD"",""B16"",""S16"",""VGM Received C02617703 B0009933 M002837327 SENDER B"",""CMV"","""",""1""
", writer.ToString());
		}

		public void TestLoadOdplRawUsage_UsingRegistryConfig()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var captions = new StlRawUsageReportRefCaptionCollection();
			var caption = captions.AddNew("SHI", "Shipping Instruction Sent", "Message ID", "Provider", "First Consol", "Carrier Code");
			caption.Category = "SHI";
			caption = captions.AddNew("SHO", "Shipping Order Sent", "Message ID", "Provider", "First Consol", "Carrier Code");
			caption.Category = "SHI";
			EDIDataRegistry.Instance.StlRawUsageReportRefCaption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, captions);

			var clientNumber = db.DatabaseId + ".ABC";
			var databaseId = db.DatabaseId;
			var companyPk = clientCompany.PK;

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHI", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "INTTRA_BK", "C00010120", "HLCU", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "BRT", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2", "CMACGM", "C600605303", null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "VGM", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "3d678a1f-30ed-4488-82f0-b2e4b5a6977b", "INTTRA_SI", "C02617703", "NYKS", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHO", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "8DB37BD1-553B-4313-AB14-B4132D8F1A9A", "INTTRA_BK", "C00010120", "HLCU", null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHO", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "B4B46730-DA6D-4E4B-BF79-64F259E3CFFD", "CMACGM", "C600605303", "HLCU", null, null));

			//Usage to ignore - provider is same as clientid
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHI", new ZDateTime(2016, 3, 4, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "9bb2de77-09b5-41fd-a5f4-4a99498d33f6", "DDDABCSYD", "CIN729098", null, null));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new OceanCarrierMessagingBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals(2, rawUsage.SummarySections.Count);
			CombineAssertions(() =>
			{
				AssertRawUsage(rawUsage.SummarySections[0].Lines[0], "DDDABCSYD", "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "INTTRA_BK", "C00010120", "HLCU");
				AssertRawUsage(rawUsage.SummarySections[1].Lines[0], "DDDABCSYD", "8DB37BD1-553B-4313-AB14-B4132D8F1A9A", "INTTRA_BK", "C00010120", "HLCU");
				AssertRawUsage(rawUsage.SummarySections[1].Lines[1], "DDDABCSYD", "B4B46730-DA6D-4E4B-BF79-64F259E3CFFD", "CMACGM", "C600605303", "HLCU");
			});

			string expectedCsvResult =
@"""Type"",""Client ID"",""Message ID"",""Provider"",""First Consol"",""Carrier Code"",""Ref 5"",""Message Time (UTC)""
""Shipping Instruction Sent"",""DDDABCSYD"",""084d9e6a-f687-4eed-87bf-e6d5b3a31a06"",""INTTRA_BK"",""C00010120"",""HLCU"","""",""01-Mar-16 10:00""
""Type"",""Client ID"",""Message ID"",""Provider"",""First Consol"",""Carrier Code"",""Ref 5"",""Message Time (UTC)""
""Shipping Order Sent"",""DDDABCSYD"",""8DB37BD1-553B-4313-AB14-B4132D8F1A9A"",""INTTRA_BK"",""C00010120"",""HLCU"","""",""01-Mar-16 10:00""
""Shipping Order Sent"",""DDDABCSYD"",""B4B46730-DA6D-4E4B-BF79-64F259E3CFFD"",""CMACGM"",""C600605303"",""HLCU"","""",""02-Mar-16 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Mar-16 10:00"",""DDDABCSYD"",""B10"",""S10"",""Shipping Instruction Sent 084d9e6a-f687-4eed-87bf-e6d5b3a31a06 INTTRA_BK C00010120 HLCU"",""SHI"","""",""1""
""01-Mar-16 10:00"",""DDDABCSYD"",""B13"",""S13"",""Shipping Order Sent 8DB37BD1-553B-4313-AB14-B4132D8F1A9A INTTRA_BK C00010120 HLCU"",""SHO"","""",""1""
""02-Mar-16 10:00"",""DDDABCSYD"",""B14"",""S14"",""Shipping Order Sent B4B46730-DA6D-4E4B-BF79-64F259E3CFFD CMACGM C600605303 HLCU"",""SHO"","""",""1""
", writer.ToString());
		}

		void AssertRawUsage(SummaryLine summaryLine, string col1, string col2, string col3, string col4, string col5)
		{
			AssertEquals("Column 1", col1, summaryLine.Column1);
			AssertEquals("Column 2", col2, summaryLine.Column2);
			AssertEquals("Column 3", col3, summaryLine.Column3);
			AssertEquals("Column 4", col4, summaryLine.Column4);
			AssertEquals("Column 5", col5, summaryLine.Column5);
		}

		static void PrepareUsages(string clientNumber, string databaseId, ZGuid companyPk)
		{
			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHI", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "INTTRA_BK", "C00010120", "HLCU", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "BRT", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2", "CMACGM", "C600605303", null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "VGM", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "3d678a1f-30ed-4488-82f0-b2e4b5a6977b", "INTTRA_SI", "C02617703", "NYKS", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHR", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "INTTRA_BK", "C00010120", "AAA", "HLCU", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHR", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "CMACGM", "C600605303", "BBB", null, null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "CMV", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "C00010120", "B0003948", "M034830234", "SENDER A", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "CMV", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", null, databaseId, companyPk, "C02617703", "B0009933", "M002837327", "SENDER B", null));

			//Usage to ignore - provider is same as clientid
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("SHI", "SHI", new ZDateTime(2016, 3, 4, 10, 0, 0), "DDDABCSYD", clientNumber, databaseId, companyPk, "9bb2de77-09b5-41fd-a5f4-4a99498d33f6", "DDDABCSYD", "CIN729098", null, null));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);
		}
	}
}