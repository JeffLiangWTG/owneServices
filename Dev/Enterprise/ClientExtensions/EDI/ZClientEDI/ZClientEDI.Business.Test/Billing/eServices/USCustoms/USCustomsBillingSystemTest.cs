using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class USCustomsBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new USCustomsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.USCustoms, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "USC", new ZDateTime(2015, 3, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new USCustomsBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 3, 31));

			var bill = billingSystem.LoadSystemBills(context).First() as USCustomsBill;
			AssertEquals(BillingConstants.BillingSystem.USCustoms, bill.SystemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "EEE", "LOL", "MEL");
			var org3 = BillingTestHelper.CreateOrganisation(Factory, "FFF", "III", "BRN");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 7323;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "LOL", db2.PK, org2.PK, "", "");

			var db3 = org3.LicCompany.ActiveOrAllLicDatabases[0];
			db3.LD_DatabaseNumber = 9847;
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "III", db3.PK, org3.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".LOL";
			var clientNumber3 = db3.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			// org1
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "EBA", new ZDateTime(2014, 2, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "B00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "CAR", new ZDateTime(2014, 2, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "S00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "RES", new ZDateTime(2014, 2, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "E00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UJL", new ZDateTime(2014, 2, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "AFJ19206262", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UPL", new ZDateTime(2014, 2, 3, 10, 0, 1), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "UPL UNIQUE", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "URB", new ZDateTime(2014, 2, 3, 10, 0, 2), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "URB UNIQUE", "", "", "", null));
			// org2
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UQT", new ZDateTime(2014, 2, 3, 10, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "054226686   ", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "URR", new ZDateTime(2014, 2, 3, 10, 0, 1), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "286 65204161", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "USO", new ZDateTime(2014, 2, 3, 10, 0, 2), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "FV915078078", "", "", "", null));
			// org3
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UFT", new ZDateTime(2014, 1, 3, 10, 0, 0), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UXT", new ZDateTime(2014, 1, 3, 10, 0, 1), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "X20150311804634", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UFT", new ZDateTime(2014, 2, 3, 10, 0, 0), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "UFT UNIQUE", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UXT", new ZDateTime(2014, 2, 3, 10, 0, 1), "FFFIIIBRN", clientNumber3, db3.DatabaseId, clientCompany3.PK, "X20150311804680", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UXT", new ZDateTime(2014, 2, 28, 0, 0, 0), "FFFIIIBRN", null, db3.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new USCustomsBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org1 lines", 3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("org1: rawUsage.Summary.Lines[0].Column1", "Drawback", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[1].Column1", "Protest", rawUsage.Summary.Lines[1].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[2].Column1", "Recon", rawUsage.Summary.Lines[2].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[0].Column2", "AFJ19206262", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("org1: rawUsage.Summary.Lines[1].Column2", "UPL UNIQUE", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("org1: rawUsage.Summary.Lines[2].Column2", "URB UNIQUE", rawUsage.Summary.Lines[2].Column2);
			});

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org2 lines", 3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("org2: rawUsage.Summary.Lines[0].Column1", "Inbond", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("org2: rawUsage.Summary.Lines[1].Column1", "Import", rawUsage.Summary.Lines[1].Column1);
				AssertEquals("org2: rawUsage.Summary.Lines[2].Column1", "Import", rawUsage.Summary.Lines[2].Column1);
				AssertEquals("org2: rawUsage.Summary.Lines[0].Column2", "054226686   ", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("org2: rawUsage.Summary.Lines[1].Column2", "286 65204161", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("org2: rawUsage.Summary.Lines[2].Column2", "FV915078078", rawUsage.Summary.Lines[2].Column2);
			});

			var licence3 = org3.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), org3.PK, clientCompany3.PK, licence3.Company.PK, licence3.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org3 lines", 3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("org3: rawUsage.Summary.Lines[0].Column1", "Export", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("org3: rawUsage.Summary.Lines[1].Column1", "Export", rawUsage.Summary.Lines[1].Column1);
				AssertEquals("org3: rawUsage.Summary.Lines[0].Column2", "UFT UNIQUE", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("org3: rawUsage.Summary.Lines[1].Column2", "X20150311804680", rawUsage.Summary.Lines[1].Column2);
			});

			string expectedCsvResult =
@"""Client ID"",""Message Type"",""Unique Job Identifier"",""Port Of Entry"",""First Transmit Time""
""FFFIIIBRN"",""Export"",""UFT UNIQUE"","""",""03-Feb-14 10:00""
""FFFIIIBRN"",""Export"",""X20150311804680"","""",""03-Feb-14 10:00""
""FFFIIIBRN"",""Export"","""","""",""28-Feb-14 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""03-Feb-14 10:00"",""FFFIIIBRN"",""B21"",""S21"",""Export UFT UNIQUE"","""","""",""1""
""03-Feb-14 10:00"",""FFFIIIBRN"",""B22"",""S22"",""Export X20150311804680"","""","""",""1""
""28-Feb-14 00:00"",""FFFIIIBRN"",""B23"",""S23"",""Export"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "EEE", "LOL", "MEL");
			var org3 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 80239;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");
			var clientCompany3 = ClientCompany.FindOrCreate(Factory, "III", db1.PK, org3.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 7323;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "LOL", db2.PK, org2.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".LOL";
			var clientNumber3 = db1.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			// org1
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "EBA", new ZDateTime(2014, 2, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "B00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "CAR", new ZDateTime(2014, 2, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "S00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "RES", new ZDateTime(2014, 2, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "E00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UJL", new ZDateTime(2014, 2, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "AFJ19206262", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UPL", new ZDateTime(2014, 2, 3, 10, 0, 1), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "UPL UNIQUE", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "URB", new ZDateTime(2014, 2, 3, 10, 0, 2), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "URB UNIQUE", "", "", "", null));
			// org2
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UQT", new ZDateTime(2014, 2, 3, 11, 0, 0), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "054226686   ", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "URR", new ZDateTime(2014, 2, 3, 11, 0, 1), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "286 65204161", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "USO", new ZDateTime(2014, 2, 3, 11, 0, 2), "EEELOLMEL", clientNumber2, db2.DatabaseId, clientCompany2.PK, "FV915078078", "", "", "", null));
			// org3
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UFT", new ZDateTime(2014, 1, 3, 12, 0, 0), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UXT", new ZDateTime(2014, 1, 3, 12, 0, 1), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "X20150311804634", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UFT", new ZDateTime(2014, 2, 3, 12, 0, 0), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "UFT UNIQUE", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UXT", new ZDateTime(2014, 2, 3, 12, 0, 1), "DDDIIISYD", clientNumber3, db1.DatabaseId, clientCompany3.PK, "X20150311804680", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "UXT", new ZDateTime(2014, 2, 28, 0, 0, 0), "DDDIIISYD", null, db1.DatabaseId, clientCompany3.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new USCustomsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), db1.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals("db1 lines", 6, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("db1: rawUsage.Summary.Lines[0].Column1", "Drawback", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("db1: rawUsage.Summary.Lines[1].Column1", "Protest", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("db1: rawUsage.Summary.Lines[2].Column1", "Recon", rawUsage.Summary.Lines[2].Column2);
				AssertEquals("db1: rawUsage.Summary.Lines[0].Column1", "Export", rawUsage.Summary.Lines[3].Column2);
				AssertEquals("db1: rawUsage.Summary.Lines[1].Column1", "Export", rawUsage.Summary.Lines[4].Column2);

				AssertEquals("db1: rawUsage.Summary.Lines[0].Column2", "AFJ19206262", rawUsage.Summary.Lines[0].Column3);
				AssertEquals("db1: rawUsage.Summary.Lines[1].Column2", "UPL UNIQUE", rawUsage.Summary.Lines[1].Column3);
				AssertEquals("db1: rawUsage.Summary.Lines[2].Column2", "URB UNIQUE", rawUsage.Summary.Lines[2].Column3);
				AssertEquals("db1: rawUsage.Summary.Lines[0].Column2", "UFT UNIQUE", rawUsage.Summary.Lines[3].Column3);
				AssertEquals("db1: rawUsage.Summary.Lines[1].Column2", "X20150311804680", rawUsage.Summary.Lines[4].Column3);
			});

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), db2.PK, ZGuid.Empty, ZGuid.Empty);
			rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals("db2 lines", 3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("db2: rawUsage.Summary.Lines[0].Column1", "Inbond", rawUsage.Summary.Lines[0].Column2);
				AssertEquals("db2: rawUsage.Summary.Lines[1].Column1", "Import", rawUsage.Summary.Lines[1].Column2);
				AssertEquals("db2: rawUsage.Summary.Lines[2].Column1", "Import", rawUsage.Summary.Lines[2].Column2);

				AssertEquals("db2: rawUsage.Summary.Lines[0].Column2", "054226686   ", rawUsage.Summary.Lines[0].Column3);
				AssertEquals("db2: rawUsage.Summary.Lines[1].Column2", "286 65204161", rawUsage.Summary.Lines[1].Column3);
				AssertEquals("db2: rawUsage.Summary.Lines[2].Column2", "FV915078078", rawUsage.Summary.Lines[2].Column3);
			});

			string expectedCsvResult =
@"""Company Code"",""Message Type"",""Unique Job Identifier"",""Port Of Entry"",""First Transmit Time""
""LOL"",""Inbond"",""054226686   "","""",""03-Feb-14 11:00""
""LOL"",""Import"",""286 65204161"","""",""03-Feb-14 11:00""
""LOL"",""Import"",""FV915078078"","""",""03-Feb-14 11:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""03-Feb-14 11:00"",""LOL"",""B16"",""S16"",""Inbond 054226686"","""","""",""1""
""03-Feb-14 11:00"",""LOL"",""B17"",""S17"",""Import 286 65204161"","""","""",""1""
""03-Feb-14 11:00"",""LOL"",""B18"",""S18"",""Import FV915078078"","""","""",""1""
", writer.ToString());
		}
	}
}