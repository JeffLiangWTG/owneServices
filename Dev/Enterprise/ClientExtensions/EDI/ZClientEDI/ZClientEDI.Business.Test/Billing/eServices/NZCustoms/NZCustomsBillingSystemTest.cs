using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class NZCustomsBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new NZCustomsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.NZCustoms, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.NZCustoms, "EBA", new ZDateTime(2014, 11, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new NZCustomsBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2014, 11, 30));

			var bill = billingSystem.LoadSystemBills(context).First() as NZCustomsBill;
			AssertEquals(BillingConstants.BillingSystem.NZCustoms, bill.SystemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "EEE", "LOL", "MEL");
			var org3 = BillingTestHelper.CreateOrganisation(Factory, "FFF", "III", "BRN");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 10301;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 21412;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "ABC", db2.PK, org2.PK, "", "");

			var db3 = org3.LicCompany.ActiveOrAllLicDatabases[0];
			db3.LD_DatabaseNumber = 7312;
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "ABC", db3.PK, org3.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".LOL";
			var clientNumber3 = db3.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "EBA", new ZDateTime(2014, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "B00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "CAR", new ZDateTime(2014, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "S00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "RES", new ZDateTime(2014, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "E00003001", "", "", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "CAR", new ZDateTime(2014, 11, 1, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "S00004001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "NZM", new ZDateTime(2014, 11, 2, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "A00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "NZC", new ZDateTime(2014, 11, 3, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "A00003001", "", "", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "CAR", new ZDateTime(2014, 10, 15, 12, 0, 0), "DDDIIISYD", clientNumber3, db3.DatabaseId, clientCompany3.PK, "A00001001", "", "", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "DEC", new ZDateTime(2014, 11, 1, 11, 0, 0), "DDDIIISYD", clientNumber3, db3.DatabaseId, clientCompany3.PK, "B00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "DEC", new ZDateTime(2014, 11, 30, 0, 0, 0), "DDDIIISYD", null, db3.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new NZCustomsBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(1, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CUSCAR", rawUsage.Summary.Lines[0].Column2);
			});

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(1, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CUSCAR", rawUsage.Summary.Lines[0].Column2);
			});

			var licence3 = org3.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), org3.PK, clientCompany3.PK, licence3.Company.PK, licence3.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(2, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CUSDEC", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("CUSDEC", rawUsage.Summary.Lines[1].Column2);
			});

			string expectedCsvResult =
@"""Client ID"",""Message Type"",""Job ID"",""Message Time (UTC)""
""DDDIIISYD"",""CUSDEC"",""B00003001"",""01-Nov-14 11:00""
""DDDIIISYD"",""CUSDEC"","""",""30-Nov-14 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Nov-14 11:00"",""DDDIIISYD"",""B17"",""S17"",""CUSDEC B00003001"","""","""",""1""
""30-Nov-14 00:00"",""DDDIIISYD"",""B18"",""S18"",""CUSDEC"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "EEE", "LOL", "MEL");
			var org3 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 10301;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "III", db1.PK, org3.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 21412;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "LOL", db2.PK, org2.PK, "", "");

			Factory.Save();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "EBA", new ZDateTime(2014, 11, 1, 10, 0, 0), "DDDABCSYD", db1.DatabaseId + ".ABC", db1.DatabaseId, clientCompany1.PK, "B00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "CAR", new ZDateTime(2014, 11, 2, 10, 0, 0), "DDDABCSYD", db1.DatabaseId + ".ABC", db1.DatabaseId, clientCompany1.PK, "S00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "RES", new ZDateTime(2014, 11, 3, 10, 0, 0), "DDDABCSYD", db1.DatabaseId + ".ABC", db1.DatabaseId, clientCompany1.PK, "E00003001", "", "", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "CAR", new ZDateTime(2014, 11, 1, 10, 0, 0), "EEELOLMEL", db2.DatabaseId + ".LOL", db2.DatabaseId, clientCompany2.PK, "S00004001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "NZM", new ZDateTime(2014, 11, 2, 10, 0, 0), "EEELOLMEL", db2.DatabaseId + ".LOL", db2.DatabaseId, clientCompany2.PK, "A00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "NZC", new ZDateTime(2014, 11, 3, 10, 0, 0), "EEELOLMEL", db2.DatabaseId + ".LOL", db2.DatabaseId, clientCompany2.PK, "A00003001", "", "", "", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "CAR", new ZDateTime(2014, 10, 15, 12, 0, 0), "DDDIIISYD", db1.DatabaseId + ".III", db1.DatabaseId, clientCompany3.PK, "A00001001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "DEC", new ZDateTime(2014, 11, 1, 11, 0, 0), "DDDIIISYD", db1.DatabaseId + ".III", db1.DatabaseId, clientCompany3.PK, "B00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("NZC", "DEC", new ZDateTime(2014, 11, 30, 0, 0, 0), "DDDIIISYD", null, db1.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new NZCustomsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), db1.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("III", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("ABC", rawUsage.Summary.Lines[1].Column1);
				AssertEquals("III", rawUsage.Summary.Lines[2].Column1);

				AssertEquals("CUSDEC", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("CUSCAR", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("CUSDEC", rawUsage.Summary.Lines[2].Column2);
			});

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 11, 1), db2.PK, ZGuid.Empty, ZGuid.Empty);
			rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(1, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("LOL", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("CUSCAR", rawUsage.Summary.Lines[0].Column2);
			});

			string expectedCsvResult =
@"""Company Code"",""Message Type"",""Job ID"",""Message Time (UTC)""
""LOL"",""CUSCAR"",""S00004001"",""01-Nov-14 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Nov-14 10:00"",""LOL"",""B13"",""S13"",""CUSCAR S00004001"","""","""",""1""
", writer.ToString());
		}
	}
}