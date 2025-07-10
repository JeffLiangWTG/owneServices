using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class S8CargoBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestLoadOdplRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("S8C", "S8C", new ZDateTime(2016, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "Enterprise", "SolveRouting", "(ediEnterprise) SSIM AMS ARN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("S8C", "S8C", new ZDateTime(2016, 5, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "Enterprise", "SolveRouting", "(ediEnterprise) SSIM NRT LAX", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("S8C", "S8C", new ZDateTime(2016, 5, 9, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "Enterprise", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new S8CargoBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 5, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "SolveRouting", "(ediEnterprise) SSIM AMS ARN");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "DDDABCSYD", "SolveRouting", "(ediEnterprise) SSIM NRT LAX");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "DDDABCSYD", "", "");
			});

			string expectedCsvResult =
@"""Client ID"",""Message Type"",""Reference"",""Message Time (UTC)""
""DDDABCSYD"",""SolveRouting"",""(ediEnterprise) SSIM AMS ARN"",""01-May-16 10:00""
""DDDABCSYD"",""SolveRouting"",""(ediEnterprise) SSIM NRT LAX"",""02-May-16 10:00""
""DDDABCSYD"","""","""",""09-May-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-May-16 10:00"",""DDDABCSYD"",""B10"",""S10"",""SolveRouting (ediEnterprise) SSIM AMS ARN"","""","""",""1""
""02-May-16 10:00"",""DDDABCSYD"",""B11"",""S11"",""SolveRouting (ediEnterprise) SSIM NRT LAX"","""","""",""1""
""09-May-16 00:00"",""DDDABCSYD"",""B12"",""S12"","""","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string messageType, string reference)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Message Type", messageType, summaryLine.Column2);
			AssertEquals("Reference", reference, summaryLine.Column3);
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("S8C", "S8C", new ZDateTime(2016, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "Enterprise", "SolveRouting", "(ediEnterprise) SSIM AMS ARN", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("S8C", "S8C", new ZDateTime(2016, 5, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "Enterprise", "SolveRouting", "(ediEnterprise) SSIM NRT LAX", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("S8C", "S8C", new ZDateTime(2016, 5, 9, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "Enterprise", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new S8CargoBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 5, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "SolveRouting", "(ediEnterprise) SSIM AMS ARN");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "ABC", "SolveRouting", "(ediEnterprise) SSIM NRT LAX");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "ABC", "", "");
			});

			string expectedCsvResult =
@"""Company Code"",""Message Type"",""Reference"",""Message Time (UTC)""
""ABC"",""SolveRouting"",""(ediEnterprise) SSIM AMS ARN"",""01-May-16 10:00""
""ABC"",""SolveRouting"",""(ediEnterprise) SSIM NRT LAX"",""02-May-16 10:00""
""ABC"","""","""",""09-May-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-May-16 10:00"",""ABC"",""B10"",""S10"",""SolveRouting (ediEnterprise) SSIM AMS ARN"","""","""",""1""
""02-May-16 10:00"",""ABC"",""B11"",""S11"",""SolveRouting (ediEnterprise) SSIM NRT LAX"","""","""",""1""
""09-May-16 00:00"",""ABC"",""B12"",""S12"","""","""","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string messageType, string reference)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Message Type", messageType, summaryLine.Column2);
			AssertEquals("Reference", reference, summaryLine.Column3);
		}
	}
}