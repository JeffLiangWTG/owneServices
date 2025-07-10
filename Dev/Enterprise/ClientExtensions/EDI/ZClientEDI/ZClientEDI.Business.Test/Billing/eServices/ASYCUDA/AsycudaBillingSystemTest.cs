using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class AsycudaBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new AsycudaBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ASYCUDA, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ASYCUDA, "ASC", new ZDateTime(2016, 3, 31), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new AsycudaBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 3, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as TransactionalSystemBill;
			AssertEquals(BillingConstants.BillingSystem.ASYCUDA, bill.SystemCode);
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

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "C00027357", "FJ", "095d5b3d-ba0e-4621-88f3-8f134e65d9b7", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "C00017655", "VU", "436217c2-3f59-497e-bc30-c9d2d0adf260", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "C00001009", "FJ", "bb8d8a7d-a616-491f-854d-a71c1fb36274", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 4, 12, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "C00001009", "PG", "963057c6-1ad6-499e-9872-d9ebfc907969", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db2.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new AsycudaBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(1, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "C00017655", "VU");
			});

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "FFFIIIBRN", "C00001009", "FJ");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "FFFIIIBRN", "C00001009", "PG");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "FFFIIIBRN", "", "");
			});

			string expectedCsvResult =
@"""Client ID"",""Consol"",""Country"",""Message Time (UTC)""
""FFFIIIBRN"",""C00001009"",""FJ"",""03-Mar-16 12:00""
""FFFIIIBRN"",""C00001009"",""PG"",""04-Mar-16 12:00""
""FFFIIIBRN"","""","""",""31-Mar-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""03-Mar-16 12:00"",""FFFIIIBRN"",""B12"",""S12"",""C00001009 FJ"","""","""",""1""
""04-Mar-16 12:00"",""FFFIIIBRN"",""B13"",""S13"",""C00001009 PG"","""","""",""1""
""31-Mar-16 00:00"",""FFFIIIBRN"",""B14"",""S14"","""","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string consol, string country)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Consol", consol, summaryLine.Column2);
			AssertEquals("Country", country, summaryLine.Column3);
		}

		public void TestLoadStlRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;

			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 2, 12, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "C00027357", "FJ", "095d5b3d-ba0e-4621-88f3-8f134e65d9b7", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "C00017655", "VU", "436217c2-3f59-497e-bc30-c9d2d0adf260", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "C00001009", "FJ", "bb8d8a7d-a616-491f-854d-a71c1fb36274", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 4, 12, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "C00001009", "PG", "963057c6-1ad6-499e-9872-d9ebfc907969", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new AsycudaBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "C00017655", "VU");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "III", "C00001009", "FJ");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "III", "C00001009", "PG");
				AssertStlRawUsage(rawUsage.Summary.Lines[3], "III", "", "");
			});

			string expectedCsvResult =
@"""Company Code"",""Consol"",""Country"",""Message Time (UTC)""
""ABC"",""C00017655"",""VU"",""02-Mar-16 10:00""
""III"",""C00001009"",""FJ"",""03-Mar-16 12:00""
""III"",""C00001009"",""PG"",""04-Mar-16 12:00""
""III"","""","""",""31-Mar-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""02-Mar-16 10:00"",""ABC"",""B11"",""S11"",""C00017655 VU"","""","""",""1""
""03-Mar-16 12:00"",""III"",""B12"",""S12"",""C00001009 FJ"","""","""",""1""
""04-Mar-16 12:00"",""III"",""B13"",""S13"",""C00001009 PG"","""","""",""1""
""31-Mar-16 00:00"",""III"",""B14"",""S14"","""","""","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string consol, string country)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Consol", consol, summaryLine.Column2);
			AssertEquals("Country", country, summaryLine.Column3);
		}
	}
}