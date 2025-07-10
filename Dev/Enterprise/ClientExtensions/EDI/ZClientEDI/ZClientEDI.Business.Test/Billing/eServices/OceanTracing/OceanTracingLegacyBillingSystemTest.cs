using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class OceanTracingLegacyBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestLoadChargeableUsages()
		{
			var periodStart = new ZDateTime(2016, 6, 1);
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD");
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "OCK", periodStart.AddDays(-10), org.LicCompany.PK, 100);

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1), org.PK);
			var billingSystem = new OceanTracingLegacyBillingSystem();
			var systemBills = billingSystem.LoadSystemBills(context);
			AssertEquals(1, systemBills.Length);
			AssertEquals(1, systemBills[0].SystemUsages.Count);
		}

		public void TestSystemCode()
		{
			var billingSystem = new OceanTracingLegacyBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.OceanTracingLegacy, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "OCK", new ZDateTime(2015, 3, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new OceanTracingLegacyBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 3, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as OceanTracingBill;
			AssertEquals(BillingConstants.BillingSystem.OceanTracingLegacy, bill.SystemCode);
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "OCK", new ZDateTime(2016, 6, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "MSG#: 720767", "WGO", null, null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "OCK", new ZDateTime(2016, 6, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "Type: LOD", "1007238", null, null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "OCK", new ZDateTime(2016, 6, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new OceanTracingLegacyBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 6, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "MSG#: 720767", "WGO");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "DDDABCSYD", "1007238", "Type: LOD");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "DDDABCSYD", "", "");
			});

			string expectedCsvResult =
@"""Client ID"",""Message Number"",""Movement Type"",""Message Time (UTC)""
""DDDABCSYD"",""MSG#: 720767"",""WGO"",""01-Jun-16 10:00""
""DDDABCSYD"",""1007238"",""Type: LOD"",""02-Jun-16 10:00""
""DDDABCSYD"","""","""",""30-Jun-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Jun-16 10:00"",""DDDABCSYD"",""B10"",""S10"",""MSG#: 720767 WGO"","""","""",""1""
""02-Jun-16 10:00"",""DDDABCSYD"",""B11"",""S11"",""1007238 Type: LOD"","""","""",""1""
""30-Jun-16 00:00"",""DDDABCSYD"",""B12"",""S12"","""","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string messageNumber, string movementType)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Message Number", messageNumber, summaryLine.Column2);
			AssertEquals("Movement Type", movementType, summaryLine.Column3);
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "OCK", new ZDateTime(2016, 6, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "MSG#: 720767", "WGO", null, null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "OCK", new ZDateTime(2016, 6, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "Type: LOD", "1007238", null, null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "OCK", new ZDateTime(2016, 6, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new OceanTracingLegacyBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 6, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "MSG#: 720767", "WGO");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "ABC", "1007238", "Type: LOD");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "ABC", "", "");
			});

			string expectedCsvResult =
@"""Company Code"",""Message Number"",""Movement Type"",""Message Time (UTC)""
""ABC"",""MSG#: 720767"",""WGO"",""01-Jun-16 10:00""
""ABC"",""1007238"",""Type: LOD"",""02-Jun-16 10:00""
""ABC"","""","""",""30-Jun-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Jun-16 10:00"",""ABC"",""B10"",""S10"",""MSG#: 720767 WGO"","""","""",""1""
""02-Jun-16 10:00"",""ABC"",""B11"",""S11"",""1007238 Type: LOD"","""","""",""1""
""30-Jun-16 00:00"",""ABC"",""B12"",""S12"","""","""","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string messageNumber, string movementType)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Message Number", messageNumber, summaryLine.Column2);
			AssertEquals("Movement Type", movementType, summaryLine.Column3);
		}
	}
}