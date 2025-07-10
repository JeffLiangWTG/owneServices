using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class RailincByMessageBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new RailincByMessageBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.RailincByMessage, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.RailincByMessage, "RIC", new ZDateTime(2015, 3, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new RailincByMessageBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 3, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as RailincBill;
			var usage = ((RailincUsage)bill.SystemUsages[0]);
			AssertEquals(BillingConstants.BillingSystem.RailincByMessage, bill.SystemCode);
			AssertEquals("RIC", usage.PriceItemCode);
			AssertEquals("RIC", usage.SubCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 2503;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";
			PrepareRawUsages(clientNumber, db.DatabaseId, clientCompany.PK);

			var billingSystem = new RailincByMessageBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 8, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("C300319197", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("PONU0296346", rawUsage.Summary.Lines[0].Column3);
				AssertEquals(new ZDateTime(2016, 8, 3, 10, 4, 3, 430).ToLongTimeString(), rawUsage.Summary.Lines[0].Column4);

				AssertEquals("C500329600", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("MRKU0368784", rawUsage.Summary.Lines[1].Column3);
				AssertEquals(new ZDateTime(2016, 8, 15, 10, 3, 15, 315).ToLongTimeString(), rawUsage.Summary.Lines[1].Column4);

				AssertEquals("C500329600", rawUsage.Summary.Lines[2].Column2);
				AssertEquals("MRKU0368784", rawUsage.Summary.Lines[2].Column3);
				AssertEquals(new ZDateTime(2016, 8, 20, 10, 4, 20, 420).ToLongTimeString(), rawUsage.Summary.Lines[2].Column4);
			});

			string expectedCsvResult =
@"""Client ID"",""Consol"",""Container"",""Message Time (UTC)""
""DDDABCSYD"",""C300319197"",""PONU0296346"",""03-Aug-16 10:04""
""DDDABCSYD"",""C500329600"",""MRKU0368784"",""15-Aug-16 10:03""
""DDDABCSYD"",""C500329600"",""MRKU0368784"",""20-Aug-16 10:04""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""03-Aug-16 10:04"",""DDDABCSYD"",""B14"",""S14"",""C300319197 PONU0296346"","""","""",""1""
""15-Aug-16 10:03"",""DDDABCSYD"",""B11"",""S11"",""C500329600 MRKU0368784"","""","""",""1""
""20-Aug-16 10:04"",""DDDABCSYD"",""B12"",""S12"",""C500329600 MRKU0368784"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 2503;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";
			PrepareRawUsages(clientNumber, db.DatabaseId, clientCompany.PK);

			var billingSystem = new RailincByMessageBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 8, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("C300319197", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("PONU0296346", rawUsage.Summary.Lines[0].Column3);
				AssertEquals(new ZDateTime(2016, 8, 3, 10, 4, 3, 430).ToLongTimeString(), rawUsage.Summary.Lines[0].Column9);

				AssertEquals("C500329600", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("MRKU0368784", rawUsage.Summary.Lines[1].Column3);
				AssertEquals(new ZDateTime(2016, 8, 15, 10, 3, 15, 315).ToLongTimeString(), rawUsage.Summary.Lines[1].Column9);

				AssertEquals("C500329600", rawUsage.Summary.Lines[2].Column2);
				AssertEquals("MRKU0368784", rawUsage.Summary.Lines[2].Column3);
				AssertEquals(new ZDateTime(2016, 8, 20, 10, 4, 20, 420).ToLongTimeString(), rawUsage.Summary.Lines[2].Column9);
			});

			string expectedCsvResult =
@"""Company Code"",""Consol"",""Container"",""Message Time (UTC)""
""ABC"",""C300319197"",""PONU0296346"",""03-Aug-16 10:04""
""ABC"",""C500329600"",""MRKU0368784"",""15-Aug-16 10:03""
""ABC"",""C500329600"",""MRKU0368784"",""20-Aug-16 10:04""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""03-Aug-16 10:04"",""ABC"",""B14"",""S14"",""C300319197 PONU0296346"","""","""",""1""
""15-Aug-16 10:03"",""ABC"",""B11"",""S11"",""C500329600 MRKU0368784"","""","""",""1""
""20-Aug-16 10:04"",""ABC"",""B12"",""S12"",""C500329600 MRKU0368784"","""","""",""1""
", writer.ToString());

			//null value test
			EServicesBillingTestHelper.DropTable();
			EServicesBillingTestHelper.CreateTable();
			PrepareRawUsages(null, db.DatabaseId, clientCompany.PK, shouldInsertNullRef2: true, shouldInsertNullRef3: true);
			AssertRawUsageWithNullValues(db.PK);
		}

		static void PrepareRawUsages(string clientNumber, string databaseId, ZGuid companyPk, bool shouldInsertNullRef2 = false, bool shouldInsertNullRef3 = false)
		{
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("RIM", "RIC", new ZDateTime(2016, 7, 10, 10, 3, 10, 310), "DDDABCSYD", clientNumber, databaseId, companyPk, "CLM", shouldInsertNullRef2 ? null : "C500329600", shouldInsertNullRef3 ? null : "MRKU0368784", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("RIM", "RIC", new ZDateTime(2016, 8, 15, 10, 3, 15, 315), "DDDABCSYD", clientNumber, databaseId, companyPk, "CLM", shouldInsertNullRef2 ? null : "C500329600", shouldInsertNullRef3 ? null : "MRKU0368784", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("RIM", "RIC", new ZDateTime(2016, 8, 20, 10, 4, 20, 420), "DDDABCSYD", clientNumber, databaseId, companyPk, "CLM", shouldInsertNullRef2 ? null : "C500329600", shouldInsertNullRef3 ? null : "MRKU0368784", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("RIM", "RIC", new ZDateTime(2016, 3, 4, 10, 3, 4, 340), "DDDABCSYD", clientNumber, databaseId, companyPk, "CLM", shouldInsertNullRef2 ? null : "C300319197", shouldInsertNullRef3 ? null : "PONU0296346", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("RIM", "RIC", new ZDateTime(2016, 8, 3, 10, 4, 3, 430), "DDDABCSYD", clientNumber, databaseId, companyPk, "CLM", shouldInsertNullRef2 ? null : "C300319197", shouldInsertNullRef3 ? null : "PONU0296346", "", null));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);
		}

		void AssertRawUsageWithNullValues(ZGuid orgPK)
		{
			var billingSystem = new RailincByMessageBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 8, 1), orgPK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				var lines = rawUsage.Summary.Lines.OfType<SummaryLine>();
				var row1 = lines.First(x => x.Column9 == new ZDateTime(2016, 8, 3, 10, 4, 3, 430).ToLongTimeString());
				var row2 = lines.First(x => x.Column9 == new ZDateTime(2016, 8, 15, 10, 3, 15, 315).ToLongTimeString());
				var row3 = lines.First(x => x.Column9 == new ZDateTime(2016, 8, 20, 10, 4, 20, 420).ToLongTimeString());

				AssertEquals("", row1.Column2);
				AssertEquals("", row1.Column3);

				AssertEquals("", row2.Column2);
				AssertEquals("", row2.Column3);

				AssertEquals("", row3.Column2);
				AssertEquals("", row3.Column3);
			});

			var expectedCsvResult =
@"""Company Code"",""Consol"",""Container"",""Message Time (UTC)""
""ABC"","""","""",""03-Aug-16 10:04""
""ABC"","""","""",""15-Aug-16 10:03""
""ABC"","""","""",""20-Aug-16 10:04""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""03-Aug-16 10:04"",""ABC"",""B14"",""S14"","""","""","""",""1""
""15-Aug-16 10:03"",""ABC"",""B11"",""S11"","""","""","""",""1""
""20-Aug-16 10:04"",""ABC"",""B12"",""S12"","""","""","""",""1""
", writer.ToString());
		}
	}
}