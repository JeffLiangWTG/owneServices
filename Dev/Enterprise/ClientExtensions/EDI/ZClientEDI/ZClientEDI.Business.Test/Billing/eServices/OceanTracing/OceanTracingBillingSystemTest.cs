using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class OceanTracingBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new OceanTracingBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.OceanTracing, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.OceanTracing, "OCT", new ZDateTime(2015, 3, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new OceanTracingBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 3, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as OceanTracingBill;
			AssertEquals(BillingConstants.BillingSystem.OceanTracing, bill.SystemCode);
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "6102DF7D-5C9B-48C3-9C87-4C7AC1BDA8B5", "34", "201507011000", "LVNU5080690", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "9B010230-6028-4C29-B40C-299C102C74DC", "44", "201507021000", "CGSU2010764", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "2A65404E-2BF1-4EB5-822F-4A9B991EECA1", "46", "201507031000", "FBXU7380160", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 4, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "B447888F-968A-45BE-A0EB-43A3381CD8F8", "36", "201507041000", "UCLU2517843", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 4, 16, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "190C80F8-CA3E-46AB-BB02-2C9942A0E40E", "28", "201507041600", "SEGU2136042", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new OceanTracingBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 7, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(6, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "34(CODECO)", "201507011000", "LVNU5080690");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "DDDABCSYD", "44(COARRI)", "201507021000", "CGSU2010764");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "DDDABCSYD", "46(COARRI)", "201507031000", "FBXU7380160");
				AssertOdplRawUsage(rawUsage.Summary.Lines[3], "DDDABCSYD", "36(CODECO)", "201507041000", "UCLU2517843");
				AssertOdplRawUsage(rawUsage.Summary.Lines[4], "DDDABCSYD", "28", "201507041600", "SEGU2136042");
				AssertOdplRawUsage(rawUsage.Summary.Lines[5], "DDDABCSYD", "", "", "");
			});

			string expectedCsvResult =
@"""Client ID"",""Event Time"",""Event Type"",""Container Number"",""Message Time (UTC)""
""DDDABCSYD"",""201507011000"",""34(CODECO)"",""LVNU5080690"",""01-Jul-15 10:00""
""DDDABCSYD"",""201507021000"",""44(COARRI)"",""CGSU2010764"",""02-Jul-15 10:00""
""DDDABCSYD"",""201507031000"",""46(COARRI)"",""FBXU7380160"",""03-Jul-15 10:00""
""DDDABCSYD"",""201507041000"",""36(CODECO)"",""UCLU2517843"",""04-Jul-15 10:00""
""DDDABCSYD"",""201507041600"",""28"",""SEGU2136042"",""04-Jul-15 16:00""
""DDDABCSYD"","""","""","""",""30-Jul-15 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Jul-15 10:00"",""DDDABCSYD"",""B10"",""S10"",""201507011000 34(CODECO) LVNU5080690"","""","""",""1""
""02-Jul-15 10:00"",""DDDABCSYD"",""B11"",""S11"",""201507021000 44(COARRI) CGSU2010764"","""","""",""1""
""03-Jul-15 10:00"",""DDDABCSYD"",""B12"",""S12"",""201507031000 46(COARRI) FBXU7380160"","""","""",""1""
""04-Jul-15 10:00"",""DDDABCSYD"",""B13"",""S13"",""201507041000 36(CODECO) UCLU2517843"","""","""",""1""
""04-Jul-15 16:00"",""DDDABCSYD"",""B14"",""S14"",""201507041600 28 SEGU2136042"","""","""",""1""
""30-Jul-15 00:00"",""DDDABCSYD"",""B15"",""S15"","""","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string eventType, string eventTime, string containerNumber)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Event Time", eventTime, summaryLine.Column2);
			AssertEquals("Event Type", eventType, summaryLine.Column3);
			AssertEquals("Container Number", containerNumber, summaryLine.Column4);
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "6102DF7D-5C9B-48C3-9C87-4C7AC1BDA8B5", "34", "201507011000", "LVNU5080690", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "9B010230-6028-4C29-B40C-299C102C74DC", "44", "201507021000", "CGSU2010764", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "2A65404E-2BF1-4EB5-822F-4A9B991EECA1", "46", "201507031000", "FBXU7380160", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 4, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "B447888F-968A-45BE-A0EB-43A3381CD8F8", "36", "201507041000", "UCLU2517843", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 4, 16, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "190C80F8-CA3E-46AB-BB02-2C9942A0E40E", "28", "201507041600", "SEGU2136042", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("OCT", "OCT", new ZDateTime(2015, 7, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new OceanTracingBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 7, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(6, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "34(CODECO)", "201507011000", "LVNU5080690");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "ABC", "44(COARRI)", "201507021000", "CGSU2010764");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "ABC", "46(COARRI)", "201507031000", "FBXU7380160");
				AssertStlRawUsage(rawUsage.Summary.Lines[3], "ABC", "36(CODECO)", "201507041000", "UCLU2517843");
				AssertStlRawUsage(rawUsage.Summary.Lines[4], "ABC", "28", "201507041600", "SEGU2136042");
				AssertStlRawUsage(rawUsage.Summary.Lines[5], "ABC", "", "", "");
			});

			string expectedCsvResult =
@"""Company Code"",""Event Time"",""Event Type"",""Container Number"",""Message Time (UTC)""
""ABC"",""201507011000"",""34(CODECO)"",""LVNU5080690"",""01-Jul-15 10:00""
""ABC"",""201507021000"",""44(COARRI)"",""CGSU2010764"",""02-Jul-15 10:00""
""ABC"",""201507031000"",""46(COARRI)"",""FBXU7380160"",""03-Jul-15 10:00""
""ABC"",""201507041000"",""36(CODECO)"",""UCLU2517843"",""04-Jul-15 10:00""
""ABC"",""201507041600"",""28"",""SEGU2136042"",""04-Jul-15 16:00""
""ABC"","""","""","""",""30-Jul-15 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Jul-15 10:00"",""ABC"",""B10"",""S10"",""201507011000 34(CODECO) LVNU5080690"","""","""",""1""
""02-Jul-15 10:00"",""ABC"",""B11"",""S11"",""201507021000 44(COARRI) CGSU2010764"","""","""",""1""
""03-Jul-15 10:00"",""ABC"",""B12"",""S12"",""201507031000 46(COARRI) FBXU7380160"","""","""",""1""
""04-Jul-15 10:00"",""ABC"",""B13"",""S13"",""201507041000 36(CODECO) UCLU2517843"","""","""",""1""
""04-Jul-15 16:00"",""ABC"",""B14"",""S14"",""201507041600 28 SEGU2136042"","""","""",""1""
""30-Jul-15 00:00"",""ABC"",""B15"",""S15"","""","""","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string eventType, string eventTime, string containerNumber)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Event Time", eventTime, summaryLine.Column2);
			AssertEquals("Event Type", eventType, summaryLine.Column3);
			AssertEquals("Container Number", containerNumber, summaryLine.Column4);
		}
	}
}