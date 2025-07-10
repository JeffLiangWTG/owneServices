using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class FlightStatsBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new FlightStatsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.FlightStats, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.FlightStats, "FMS", new ZDateTime(2015, 2, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new FlightStatsBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 2, 28));

			var bill = billingSystem.LoadSystemBills(context).First();
			AssertEquals(BillingConstants.BillingSystem.FlightStats, bill.SystemCode);
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
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db2.PK, org2.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "MAWB#001", "JOB#001", "Orig-Dest1", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "MAWB#002", "JOB#002", "Orig-Dest2", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "MAWB#003", "JOB#003", "Orig-Dest3", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 3, 13, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "MAWB#004", "JOB#004", "Orig-Dest4", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 4, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "MAWB#005", "JOB#005", "Orig-Dest5", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 9, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "MAWB#006", "JOB#006", "Orig-Dest6", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db2.DatabaseId, clientCompany2.PK, "MAWB#007", "JOB#007", "Orig-Dest7", "", DateTime.Now, "ENT"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new FlightStatsBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			var usagesAsText = string.Join("\r\n", new[] { rawUsage.Summary.Header.Code }.Concat(rawUsage.Summary.Lines.OfType<SummaryLine>().Select(x => x.Code).OrderBy(x => x)));
			AssertEquals(@",Client ID,MAWB,,Job Num,,Origin-Destination,Message Time (UTC),,,,,,,
,DDDABCSYD,MAWB#001,,JOB#001,,Orig-Dest1,01-Mar-16 10:00:00,,,,,,,
,DDDABCSYD,MAWB#002,,JOB#002,,Orig-Dest2,02-Mar-16 10:00:00,,,,,,,
,DDDABCSYD,MAWB#003,,JOB#003,,Orig-Dest3,03-Mar-16 10:00:00,,,,,,,", usagesAsText);

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			usagesAsText = string.Join("\r\n", new[] { rawUsage.Summary.Header.Code }.Concat(rawUsage.Summary.Lines.OfType<SummaryLine>().Select(x => x.Code).OrderBy(x => x)));
			AssertEquals(@",Client ID,MAWB,,Job Num,,Origin-Destination,Message Time (UTC),,,,,,,
,FFFIIIBRN,MAWB#004,,JOB#004,,Orig-Dest4,03-Mar-16 13:00:00,,,,,,,
,FFFIIIBRN,MAWB#005,,JOB#005,,Orig-Dest5,04-Mar-16 10:00:00,,,,,,,
,FFFIIIBRN,MAWB#006,,JOB#006,,Orig-Dest6,09-Mar-16 10:00:00,,,,,,,
,FFFIIIBRN,MAWB#007,,JOB#007,,Orig-Dest7,31-Mar-16 00:00:00,,,,,,,", usagesAsText);

			string expectedCsvResult =
@"""Client ID"",""MAWB"",""Job Num"",""Origin-Destination"",""Message Time (UTC)""
""FFFIIIBRN"",""MAWB#004"",""JOB#004"",""Orig-Dest4"",""03-Mar-16 13:00:00""
""FFFIIIBRN"",""MAWB#005"",""JOB#005"",""Orig-Dest5"",""04-Mar-16 10:00:00""
""FFFIIIBRN"",""MAWB#006"",""JOB#006"",""Orig-Dest6"",""09-Mar-16 10:00:00""
""FFFIIIBRN"",""MAWB#007"",""JOB#007"",""Orig-Dest7"",""31-Mar-16 00:00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""03-Mar-16 13:00"",""FFFIIIBRN"",""B13"",""S13"",""MAWB#004 JOB#004 Orig-Dest4"",""FMS"",""Air Waybill Automation"",""1""
""04-Mar-16 10:00"",""FFFIIIBRN"",""B14"",""S14"",""MAWB#005 JOB#005 Orig-Dest5"",""FMS"",""Air Waybill Automation"",""1""
""09-Mar-16 10:00"",""FFFIIIBRN"",""B15"",""S15"",""MAWB#006 JOB#006 Orig-Dest6"",""FMS"",""Air Waybill Automation"",""1""
""31-Mar-16 00:00"",""FFFIIIBRN"",""B16"",""S16"",""MAWB#007 JOB#007 Orig-Dest7"",""FMS"",""Air Waybill Automation"",""1""
", writer.ToString());
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 1, 10, 0, 1), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "MAWB#001", "JOB#001", "Orig-Dest1", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "MAWB#002", "JOB#002", "Orig-Dest2", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "MAWB#003", "JOB#003", "Orig-Dest3", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 3, 13, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "MAWB#004", "JOB#004", "Orig-Dest4", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 4, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "MAWB#005", "JOB#005", "Orig-Dest5", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 9, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "MAWB#006", "JOB#006", "Orig-Dest6", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("FMS", "FMS", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db.DatabaseId, clientCompany2.PK, "MAWB#007", "JOB#007", "Orig-Dest7", "", DateTime.Now, "ENT"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new FlightStatsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			var usagesAsText = string.Join("\r\n", new[] { rawUsage.Summary.Header.Code }.Concat(rawUsage.Summary.Lines.OfType<SummaryLine>().Select(x => x.Code).OrderBy(x => x)));
			AssertEquals(@",Client ID,MAWB,,Job Num,Message Time (UTC),Origin-Destination,,,,,,,,
,ABC,MAWB#001,,JOB#001,01-Mar-16 10:00:01,Orig-Dest1,,,,,,,,
,ABC,MAWB#002,,JOB#002,02-Mar-16 10:00:00,Orig-Dest2,,,,,,,,
,ABC,MAWB#003,,JOB#003,03-Mar-16 10:00:00,Orig-Dest3,,,,,,,,
,III,MAWB#004,,JOB#004,03-Mar-16 13:00:00,Orig-Dest4,,,,,,,,
,III,MAWB#005,,JOB#005,04-Mar-16 10:00:00,Orig-Dest5,,,,,,,,
,III,MAWB#006,,JOB#006,09-Mar-16 10:00:00,Orig-Dest6,,,,,,,,
,III,MAWB#007,,JOB#007,31-Mar-16 00:00:00,Orig-Dest7,,,,,,,,", usagesAsText);

			string expectedCsvResult =
@"""Company Code"",""MAWB"",""Job Num"",""Origin-Destination"",""Message Time (UTC)""
""ABC"",""MAWB#001"",""JOB#001"",""Orig-Dest1"",""01-Mar-16 10:00:01""
""ABC"",""MAWB#002"",""JOB#002"",""Orig-Dest2"",""02-Mar-16 10:00:00""
""ABC"",""MAWB#003"",""JOB#003"",""Orig-Dest3"",""03-Mar-16 10:00:00""
""III"",""MAWB#004"",""JOB#004"",""Orig-Dest4"",""03-Mar-16 13:00:00""
""III"",""MAWB#005"",""JOB#005"",""Orig-Dest5"",""04-Mar-16 10:00:00""
""III"",""MAWB#006"",""JOB#006"",""Orig-Dest6"",""09-Mar-16 10:00:00""
""III"",""MAWB#007"",""JOB#007"",""Orig-Dest7"",""31-Mar-16 00:00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(
@"""01-Mar-16 10:00"",""ABC"",""B10"",""S10"",""MAWB#001 JOB#001 Orig-Dest1"",""FMS"",""Air Waybill Automation"",""1""
""02-Mar-16 10:00"",""ABC"",""B11"",""S11"",""MAWB#002 JOB#002 Orig-Dest2"",""FMS"",""Air Waybill Automation"",""1""
""03-Mar-16 10:00"",""ABC"",""B12"",""S12"",""MAWB#003 JOB#003 Orig-Dest3"",""FMS"",""Air Waybill Automation"",""1""
""03-Mar-16 13:00"",""III"",""B13"",""S13"",""MAWB#004 JOB#004 Orig-Dest4"",""FMS"",""Air Waybill Automation"",""1""
""04-Mar-16 10:00"",""III"",""B14"",""S14"",""MAWB#005 JOB#005 Orig-Dest5"",""FMS"",""Air Waybill Automation"",""1""
""09-Mar-16 10:00"",""III"",""B15"",""S15"",""MAWB#006 JOB#006 Orig-Dest6"",""FMS"",""Air Waybill Automation"",""1""
""31-Mar-16 00:00"",""III"",""B16"",""S16"",""MAWB#007 JOB#007 Orig-Dest7"",""FMS"",""Air Waybill Automation"",""1""
", writer.ToString());
		}
	}
}