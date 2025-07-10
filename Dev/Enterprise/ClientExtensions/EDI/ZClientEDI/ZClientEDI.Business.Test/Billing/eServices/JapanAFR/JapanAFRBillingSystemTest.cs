using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class JapanAFRBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new JapanAFRBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.JapanAFR, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.JapanAFR, "AFR", new ZDateTime(2014, 12, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new JapanAFRBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2014, 12, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as JapanAFRBill;
			AssertEquals(BillingConstants.BillingSystem.JapanAFR, bill.SystemCode);
			AssertEquals("AFR", ((JapanAFRUsage)bill.SystemUsages[0]).PriceItemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 9871;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");
			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "B00003001", "B00003001", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "S00003002", "S00003002", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "E00003003", "E00003003", "", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 11, 30, 21, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "D00002001", "D00002001", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 30, 0, 0, 0), "DDDABCSYD", null, db1.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new JapanAFRBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 12, 1), org1.PK, clientCompany.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("B00003001", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("S00003002", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("E00003003", rawUsage.Summary.Lines[2].Column2);
				AssertEquals("", rawUsage.Summary.Lines[3].Column2);
			});

			string expectedCsvResult =
@"""Client ID"",""MBOL"",""HBOL"",""Message Time (UTC)""
""DDDABCSYD"",""B00003001"",""B00003001"",""01-Dec-14 10:00""
""DDDABCSYD"",""S00003002"",""S00003002"",""02-Dec-14 10:00""
""DDDABCSYD"",""E00003003"",""E00003003"",""03-Dec-14 10:00""
""DDDABCSYD"","""","""",""30-Dec-14 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Dec-14 10:00"",""DDDABCSYD"",""B10"",""S10"",""B00003001 B00003001"","""","""",""1""
""02-Dec-14 10:00"",""DDDABCSYD"",""B11"",""S11"",""S00003002 S00003002"","""","""",""1""
""03-Dec-14 10:00"",""DDDABCSYD"",""B12"",""S12"",""E00003003 E00003003"","""","""",""1""
""30-Dec-14 00:00"",""DDDABCSYD"",""B14"",""S14"","""","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 9871;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");
			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "B00003001", "B00003001", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "S00003002", "S00003002", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "E00003003", "E00003003", "", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 11, 30, 21, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany.PK, "D00002001", "D00002001", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("JPC", "AFR", new ZDateTime(2014, 12, 30, 0, 0, 0), "DDDABCSYD", null, db1.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new JapanAFRBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 12, 1), db1.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("B00003001", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("S00003002", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("E00003003", rawUsage.Summary.Lines[2].Column2);
				AssertEquals("", rawUsage.Summary.Lines[3].Column2);
			});

			string expectedCsvResult =
@"""Company Code"",""MBOL"",""HBOL"",""Message Time (UTC)""
""ABC"",""B00003001"",""B00003001"",""01-Dec-14 10:00""
""ABC"",""S00003002"",""S00003002"",""02-Dec-14 10:00""
""ABC"",""E00003003"",""E00003003"",""03-Dec-14 10:00""
""ABC"","""","""",""30-Dec-14 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Dec-14 10:00"",""ABC"",""B10"",""S10"",""B00003001 B00003001"","""","""",""1""
""02-Dec-14 10:00"",""ABC"",""B11"",""S11"",""S00003002 S00003002"","""","""",""1""
""03-Dec-14 10:00"",""ABC"",""B12"",""S12"",""E00003003 E00003003"","""","""",""1""
""30-Dec-14 00:00"",""ABC"",""B14"",""S14"","""","""","""",""1""
", writer.ToString());
		}
	}
}