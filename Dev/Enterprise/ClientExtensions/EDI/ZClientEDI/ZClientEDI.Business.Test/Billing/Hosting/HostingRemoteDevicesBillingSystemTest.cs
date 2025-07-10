using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using NUnit.Framework;
using ZClientEDI.Business.Billing;
using static Enterprise.Client.EDI.Billing.Hosting.Test.EdiRemoteDevicesChargeableUsageProviderTest;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	[UseSnapshotProtection]
	public class HostingRemoteDevicesBillingSystemTest : TestCase
	{
		public void TestSystemCode()
		{
			var hostingBilling = new HostingRemoteDevicesBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.HostingRemoteDevices, hostingBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingRemoteDevices, "", new ZDateTime(2010, 10, 01), lic, 10);
			Factory.Save();

			var hostingBilling = new HostingRemoteDevicesBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));
			AssertEquals(true, hostingBilling.LoadSystemBills(context).First() is HostingBill);
		}

		public void TestCreateSystemUsages()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var licChild1 = BillingTestHelper.CreateDependentLicence(licHeader1, "AA1");
			var licHeader2 = BillingTestHelper.CreateLicence(Factory, "BBB", "SYD", "BBB");
			var licHeaderNotLive = BillingTestHelper.CreateLicence(Factory, "CCC", "SYD", "CCC");
			licHeaderNotLive.LA_AgreedLiveDate = new ZDateTime(2015, 1, 1);

			var periodStart = new ZDateTime(2010, 10, 01);

			var chargeableUsage1 = CreateChargeableUsage(licHeader1, periodStart, "AAA", 10);
			var chargeableUsage2 = CreateChargeableUsage(licHeader1, periodStart, "FFF", 20);
			var chargeableUsage3 = CreateChargeableUsage(licHeader1, periodStart, "CCC", 30);

			var chargeableUsage4 = CreateChargeableUsage(licChild1, periodStart, "AAA", 11);

			var chargeableUsage5 = CreateChargeableUsage(licHeader2, periodStart, "FFF", 22);
			var chargeableUsage6 = CreateChargeableUsage(licHeader2, periodStart, "CCC", 24);
			var chargeableUsage7 = CreateChargeableUsage(licHeaderNotLive, periodStart, "CCC", 99);

			Factory.Save();

			var hostingBilling = new HostingRemoteDevicesBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			var allHostingUsages = new List<HostingUsage>();
			foreach (SystemBill bill in hostingBilling.LoadSystemBills(context))
			{
				allHostingUsages.AddRange(bill.SystemUsages.Cast<HostingUsage>());
			}

			AssertEquals("usage count", 6, allHostingUsages.Count);
			AssertHostingUsage(allHostingUsages, chargeableUsage1);
			AssertHostingUsage(allHostingUsages, chargeableUsage2);
			AssertHostingUsage(allHostingUsages, chargeableUsage3);
			AssertHostingUsage(allHostingUsages, chargeableUsage4);
			AssertHostingUsage(allHostingUsages, chargeableUsage5);
			AssertHostingUsage(allHostingUsages, chargeableUsage6);
		}

		ClientChargeableUsage CreateChargeableUsage(LicenceHeader licHeader, ZDateTime periodStart, ZString subCode, ZInt unitCount)
		{
			return BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingRemoteDevices, subCode, periodStart, licHeader, unitCount);
		}

		void AssertHostingUsage(IEnumerable<HostingUsage> allHostingUsages, ClientChargeableUsage chargeableUsage)
		{
			var hostingUsage = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage.PK));
			AssertEquals(chargeableUsage.OrganisationPK, hostingUsage.OrganisationPK);

			AssertEquals(chargeableUsage.U1_SubCode, hostingUsage.SubCode);
			AssertEquals(chargeableUsage.U1_UnitCount.ToZInt(), hostingUsage.UnitCount);
		}

		#region Raw Usages

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 80239;
			db1.LD_HostDBName = "OdysseyABCPRD";
			Factory.Save();

			InsertTransaction("Printer 1", "Server SYD", db1.LD_HostDBName, new DateTime(2016, 2, 1));
			InsertTransaction("Printer 2", "Server SYD", db1.LD_HostDBName, new DateTime(2016, 2, 10));
			InsertTransaction("Printer 3", "Server SYD", db1.LD_HostDBName, new DateTime(2016, 2, 29));
			InsertTransaction("", "Server SYD", db1.LD_HostDBName, new DateTime(2016, 2, 29), @"\\Server SYD\Printer 4");
			InsertTransaction("", "Server SYD", db1.LD_HostDBName, new DateTime(2016, 2, 29), @"\\Server SYD\Printer 5");

			var billingSystem = new HostingRemoteDevicesBillingSystemForTest();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 2, 1), org1.PK, ZGuid.Empty, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org1 lines", 5, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("org1: rawUsage.Summary.Lines[0].Column1", @"\\Server SYD\Printer 4", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[1].Column1", @"\\Server SYD\Printer 5", rawUsage.Summary.Lines[1].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[2].Column1", "Printer 1", rawUsage.Summary.Lines[2].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[3].Column1", "Printer 2", rawUsage.Summary.Lines[3].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[4].Column1", "Printer 3", rawUsage.Summary.Lines[4].Column1);
			});

			string expectedCsvResult =
@"""Printer Name"",""Print Server Name""
""\\Server SYD\Printer 4"",""Server SYD""
""\\Server SYD\Printer 5"",""Server SYD""
""Printer 1"",""Server SYD""
""Printer 2"",""Server SYD""
""Printer 3"",""Server SYD""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""29-Feb-16 00:00"","""","""","""",""\\Server SYD\Printer 4 Server SYD"","""","""",""1""
""29-Feb-16 00:00"","""","""","""",""\\Server SYD\Printer 5 Server SYD"","""","""",""1""
""01-Feb-16 00:00"","""","""","""",""Printer 1 Server SYD"","""","""",""1""
""10-Feb-16 00:00"","""","""","""",""Printer 2 Server SYD"","""","""",""1""
""29-Feb-16 00:00"","""","""","""",""Printer 3 Server SYD"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 80239;
			db1.LD_HostDBName = "OdysseyABCPRD";
			Factory.Save();

			InsertTransaction("Printer 1", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 1));
			InsertTransaction("Printer 2", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 10));
			InsertTransaction("Printer 3", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 29));
			InsertTransaction("", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 29), @"\\Server SYD\Printer 4");
			InsertTransaction("", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 29), @"\\Server SYD\Printer 5");

			var billingSystem = new HostingRemoteDevicesBillingSystemForTest();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 2, 1), db1.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org1 lines", 5, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("org1: rawUsage.Summary.Lines[0].Column1", @"\\Server SYD\Printer 4", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[1].Column1", @"\\Server SYD\Printer 5", rawUsage.Summary.Lines[1].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[2].Column1", "Printer 1", rawUsage.Summary.Lines[2].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[3].Column1", "Printer 2", rawUsage.Summary.Lines[3].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[4].Column1", "Printer 3", rawUsage.Summary.Lines[4].Column1);
			});

			string expectedCsvResult =
@"""Printer Name"",""Print Server Name""
""\\Server SYD\Printer 4"",""Server SYD""
""\\Server SYD\Printer 5"",""Server SYD""
""Printer 1"",""Server SYD""
""Printer 2"",""Server SYD""
""Printer 3"",""Server SYD""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""29-Feb-16 00:00"","""","""","""",""\\Server SYD\Printer 4 Server SYD"","""","""",""1""
""29-Feb-16 00:00"","""","""","""",""\\Server SYD\Printer 5 Server SYD"","""","""",""1""
""01-Feb-16 00:00"","""","""","""",""Printer 1 Server SYD"","""","""",""1""
""10-Feb-16 00:00"","""","""","""",""Printer 2 Server SYD"","""","""",""1""
""29-Feb-16 00:00"","""","""","""",""Printer 3 Server SYD"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage_AfterCutover062022()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 80239;
			var prices = BillingTestHelper.CreatePriceList(lic1.Company);
			var serverPriceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.PrintServersCode, BillingConstants.FeeType.PerDevicePerMonth, "", 1m);
			var printerPriceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.RemoteDevicesCode, BillingConstants.FeeType.PerDevicePerMonth, "", 1m);
			Factory.Save();

			var lcc = lic1.ClientCompany;
			InsertTxTransaction(lcc, "PRS", "PGSERVER", new ZDate(2022, 6, 1));
			InsertTxTransaction(lcc, "PRT", "FujiXerox C3373 (A)", new ZDate(2022, 6, 10));
			InsertTxTransaction(lcc, "PRS", "COMPUTER_BT213A", new ZDate(2022, 6, 15));
			InsertTxTransaction(lcc, "PRT", "Brother VC-500W", new ZDate(2022, 6, 22));
			InsertTxTransaction(lcc, "PRS", "BACKUP-MELW10", new ZDate(2022, 6, 29));

			var billingSystem = new HostingRemoteDevicesBillingSystemForTest();
			var contextHS = new BillingLoadRawUsageContext(Factory, new ZDateTime(2022, 6, 1), db1.PK, serverPriceItem.PK, ZGuid.Empty);
			var contextHR = new BillingLoadRawUsageContext(Factory, new ZDateTime(2022, 6, 1), db1.PK, printerPriceItem.PK, ZGuid.Empty);

			CombineAssertions(() =>
			{
				var builder = new ZStringBuilder();
				billingSystem.LoadRawUsageInCsv(contextHS, false, (csv) => { builder.AppendLine(csv); });
				AssertEquals(@"""Printer Name"",""Print Server Name""
"""",""PGSERVER""
"""",""COMPUTER_BT213A""
"""",""BACKUP-MELW10""
", builder.ToString());

				builder.Clear();
				billingSystem.LoadRawUsageInCsv(contextHR, false, (csv) => { builder.AppendLine(csv); });
				AssertEquals(@"""Printer Name"",""Print Server Name""
""FujiXerox C3373 (A)"",""""
""Brother VC-500W"",""""
", builder.ToString());

				var writer = new CsvUsageReportWriterForTest();
				billingSystem.LoadRawUsageInCsv(contextHS, false, writer);
				AssertEquals(@"""01-Jun-22 00:00"","""","""","""",""PGSERVER"",""#HS"","""",""1""
""15-Jun-22 00:00"","""","""","""",""COMPUTER_BT213A"",""#HS"","""",""1""
""29-Jun-22 00:00"","""","""","""",""BACKUP-MELW10"",""#HS"","""",""1""
", writer.ToString());

				writer.Clear();
				billingSystem.LoadRawUsageInCsv(contextHR, false, writer);
				AssertEquals(@"""10-Jun-22 00:00"","""","""","""",""FujiXerox C3373 (A)"",""#HR"","""",""1""
""22-Jun-22 00:00"","""","""","""",""Brother VC-500W"",""#HR"","""",""1""
", writer.ToString());
			});
		}

		public void TestLoadRawUsageInCsv_ServerPriceCode()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", false);
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 80239;
			db1.LD_HostDBName = "OdysseyABCPRD";
			var prices = BillingTestHelper.CreatePriceList(lic1.Company);
			var serverPriceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.PrintServersCode, BillingConstants.FeeType.PerDevicePerMonth, "", 1m);
			var printerPriceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.RemoteDevicesCode, BillingConstants.FeeType.PerDevicePerMonth, "", 1m);
			Factory.Save();

			InsertTransaction("Printer 1", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28));
			InsertTransaction("Printer 2", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28));
			InsertTransaction("Printer 3", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28));
			InsertTransaction("", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28), @"\\Server SYD\Printer 4");
			InsertTransaction("", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28), @"\\Server SYD\Printer 5");

			var billingSystem = new HostingRemoteDevicesBillingSystemForTest();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 2, 1), db1.PK, serverPriceItem.PK, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			var actual = string.Join("\r\n", rawUsage.Summary.Lines.Cast<SummaryLine>().Select(x => x.Column1 + ", " + x.Column2));
			AssertEquals(@", Server SYD", actual);

			string expectedCsvResult =
@"""Printer Name"",""Print Server Name""
"""",""Server SYD""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""28-Feb-16 00:00"","""","""","""",""Server SYD"",""#HS"","""",""1""
", writer.ToString());
		}

		public void TestLoadRawUsageInCsv_PrinterPriceCode()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", false);
			var db1 = lic1.Database;
			db1.LD_DatabaseNumber = 80239;
			db1.LD_HostDBName = "OdysseyABCPRD";
			var prices = BillingTestHelper.CreatePriceList(lic1.Company);
			var serverPriceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.PrintServersCode, BillingConstants.FeeType.PerDevicePerMonth, "", 1m);
			var printerPriceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.RemoteDevicesCode, BillingConstants.FeeType.PerDevicePerMonth, "", 1m);
			Factory.Save();

			InsertTransaction("Printer 1", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28));
			InsertTransaction("Printer 2", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28));
			InsertTransaction("Printer 3", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28));
			InsertTransaction("", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28), @"\\Server SYD\Printer 4");
			InsertTransaction("", "Server SYD", "OdysseyABCPRD", new DateTime(2016, 2, 28), @"\\Server SYD\Printer 5");

			var billingSystem = new HostingRemoteDevicesBillingSystemForTest();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 2, 1), db1.PK, printerPriceItem.PK, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org1 lines", 5, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("org1: rawUsage.Summary.Lines[0].Column1", @"\\Server SYD\Printer 4", rawUsage.Summary.Lines[0].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[1].Column1", @"\\Server SYD\Printer 5", rawUsage.Summary.Lines[1].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[2].Column1", "Printer 1", rawUsage.Summary.Lines[2].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[3].Column1", "Printer 2", rawUsage.Summary.Lines[3].Column1);
				AssertEquals("org1: rawUsage.Summary.Lines[4].Column1", "Printer 3", rawUsage.Summary.Lines[4].Column1);
			});

			string expectedCsvResult =
@"""Printer Name"",""Print Server Name""
""\\Server SYD\Printer 4"",""""
""\\Server SYD\Printer 5"",""""
""Printer 1"",""""
""Printer 2"",""""
""Printer 3"",""""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""28-Feb-16 00:00"","""","""","""",""\\Server SYD\Printer 4"",""#HR"","""",""1""
""28-Feb-16 00:00"","""","""","""",""\\Server SYD\Printer 5"",""#HR"","""",""1""
""28-Feb-16 00:00"","""","""","""",""Printer 1"",""#HR"","""",""1""
""28-Feb-16 00:00"","""","""","""",""Printer 2"",""#HR"","""",""1""
""28-Feb-16 00:00"","""","""","""",""Printer 3"",""#HR"","""",""1""
", writer.ToString());
		}

		#endregion

		#region Implementation

		class HostingRemoteDevicesBillingSystemForTest : HostingRemoteDevicesBillingSystem
		{
			protected override ExternalChargeableUsageProvider GetUsageProvider() => new EdiRemoteDevicesChargeableUsageProviderForTest();
		}

		static void CreateTable()
		{
			RemoteDevicesUsageTestHelper.CreateUsageTable();
		}

		static void InsertTransaction(string displayName, string serverName, ZString dbName, DateTime dateCaptured, string queueName = "")
		{
			RemoteDevicesUsageTestHelper.InsertUsageTable(dateCaptured, dbName, displayName, queueName, serverName);
		}

		static void InsertTxTransaction(ClientCompany clientCompany, string priceItemCode, string reference1, ZDateTime requestUtc)
		{
			var clientNumber = clientCompany.Database.DatabaseId + '.' + clientCompany.LCC_Code;
			var record = new EServicesBillingTestHelper.RawUsageInfo(
				category: "STL",
				priceItemCode: priceItemCode,
				messageTimeUTC: requestUtc,
				clientID: clientCompany.LicenceCode,
				clientNumber: clientNumber,
				systemId: clientCompany.Database.DatabaseId,
				companyPk: clientCompany.PK,
				reference1: reference1,
				reference2: "",
				reference3: "",
				reference4: "");

			EServicesBillingTestHelper.InsertTransaction(record);
		}

		BusinessObjectFactory Factory;

		protected override void SetUp()
		{
			TestCaseHelper.RunClientDbCreateScripts();
			Factory = new BusinessObjectFactory();
			base.SetUp();
			CreateTable();
			EServicesBillingTestHelper.CreateTable();
		}

		#endregion
	}
}
