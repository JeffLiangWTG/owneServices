using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	public class HostingDataAccessBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var hostingBilling = new HostingDataAccessBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.HostingDataAccess, hostingBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			CreateChargeableUsage(lic, new ZDateTime(2010, 10, 01), 10);
			Factory.Save();

			var hostingBilling = new HostingDataAccessBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));
			AssertEquals(true, hostingBilling.LoadSystemBills(context).First() is HostingBill);
		}

		public void TestLoadOdplUsages()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var licHeader2 = BillingTestHelper.CreateLicence(Factory, "BBB", "SYD", "BBB");
			var licHeader3 = BillingTestHelper.CreateLicence(Factory, "CCC", "SYD", "CCC");
			var testHeader = BillingTestHelper.CreateAnotherDatabase(licHeader3, "DDD");
			testHeader.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			testHeader.Database.LD_LD_ParentDatabase = licHeader3.LA_LD;

			var periodStart = new ZDateTime(2010, 10, 01);

			var chargeableUsage1 = CreateChargeableUsage(licHeader1, periodStart, BillingConstants.Hosting.MBperGB);
			var chargeableUsage2 = CreateChargeableUsage(licHeader2, periodStart, BillingConstants.Hosting.MBperGB + 1);
			var chargeableUsage3 = CreateChargeableUsage(licHeader3, periodStart, BillingConstants.Hosting.MBperGB + 7);
			var testUsage = CreateChargeableUsage(testHeader, periodStart, BillingConstants.Hosting.MBperGB + 17);

			var chargeableUsage4 = CreateChargeableUsage(licHeader1, periodStart.AddMonths(1), BillingConstants.Hosting.MBperGB + 100);
			var chargeableUsage5 = CreateChargeableUsage(licHeader2, periodStart.AddMonths(-1), BillingConstants.Hosting.MBperGB + 200);
			var chargeableUsage6 = CreateChargeableUsage(licHeader3, periodStart.AddMonths(-2), BillingConstants.Hosting.MBperGB + 300);

			var priceHeader1 = licHeader1.Company.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = periodStart.AddYears(-1);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.Hosting.DataAccessCode, BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 16m);
			var priceHeader2 = licHeader2.Company.PriceHeaders.AddNew();
			priceHeader2.L6_RX_NKCurrency = "AUD";
			priceHeader2.L6_ValidFrom = periodStart.AddYears(-1);
			BillingTestHelper.AddPriceItem(priceHeader2, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader2, BillingConstants.Hosting.DataAccessCode, BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 15m);
			var priceHeader3 = licHeader3.Company.PriceHeaders.AddNew();
			priceHeader3.L6_RX_NKCurrency = "AUD";
			priceHeader3.L6_ValidFrom = periodStart.AddYears(-1);
			BillingTestHelper.AddPriceItem(priceHeader3, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader3, BillingConstants.Hosting.DataAccessCode, BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 14m);

			Factory.Save();

			var hostingBilling = new HostingDataAccessBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var allHostingUsages = new List<HostingUsage>();
			foreach (SystemBill bill in hostingBilling.LoadSystemBills(context))
			{
				allHostingUsages.AddRange(bill.SystemUsages.Cast<HostingUsage>());
			}

			AssertEquals("usage count", 3, allHostingUsages.Count);

			var hostingUsage2 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage2.PK));
			var hostingUsage3 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage3.PK));
			var testHostingUsage = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(testUsage.PK));
			var hostingUsage5 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage5.PK));

			AssertEquals("test and production usage are combined", hostingUsage3.PK, testHostingUsage.PK);
			AssertEquals("UnitCount", 1, hostingUsage2.UnitCount);
			AssertEquals("UnitCount", 7 + 17 + BillingConstants.Hosting.MBperGB, hostingUsage3.UnitCount);
			AssertEquals("UnitCount", 200, hostingUsage5.UnitCount);

			AssertEquals(chargeableUsage2.U1_SubCode, hostingUsage2.SubCode);
		}

		public void TestLoadOdplUsages_Preview()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var periodStart = new ZDateTime(2017, 5, 1);

			var chargeableUsage = CreateChargeableUsage(licence, periodStart, BillingConstants.Hosting.MBperGB + 10);

			var priceHeader1 = licence.Company.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = periodStart.AddYears(-1);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.Hosting.DataAccessCode, BillingConstants.FeeType.PerMBPerMonthMin1GB, "", 16m);

			var priceHeaderPreview = licence.Company.PriceHeaders.AddNew();
			priceHeaderPreview.L6_RX_NKCurrency = "AUD";
			priceHeaderPreview.L6_ValidFrom = periodStart.AddYears(-2);

			Factory.Save();

			var hostingBilling = new HostingDataAccessBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.SetPreviewOnly(licence, priceHeaderPreview);

			var bill = hostingBilling.LoadSystemBills(context)[0];
			var usage = bill.SystemUsages[0] as HostingUsage;
			AssertEquals("Should use preview price header", priceHeaderPreview, usage.PriceHeader);
		}

		public void TestLoadRawUsage()
		{
			var lic = CreateRawUsage();

			var billingSystem = new HostingDataAccessBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 11, 1), lic.Company.LC_OH, lic.ClientCompany.PK, lic.Company.PK, lic.Database.PK);

			// ODPL
			{
				var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
				AssertEquals(4, rawUsage.Summary.Lines.Count);
				CombineAssertions(() =>
				{
					AssertRawUsage(rawUsage.Summary.Lines[0], 5, "ref1a", "ref2a", new ZDateTime(2015, 11, 1, 10, 0, 0), "ref3a");
					AssertRawUsage(rawUsage.Summary.Lines[1], 7, "ref1b", "ref2b", new ZDateTime(2015, 11, 2, 10, 0, 0), "ref3b");
					AssertRawUsage(rawUsage.Summary.Lines[2], 11, "ref1c", "ref2c", new ZDateTime(2015, 11, 3, 10, 0, 0), "ref3c");
				});
			}

			// STL
			{
				var rawUsage = billingSystem.LoadStlRawUsage(context);
				AssertEquals(4, rawUsage.Summary.Lines.Count);
				CombineAssertions(() =>
				{
					AssertRawUsage(rawUsage.Summary.Lines[0], 5, "ref1a", "ref2a", new ZDateTime(2015, 11, 1, 10, 0, 0), "ref3a");
					AssertRawUsage(rawUsage.Summary.Lines[1], 7, "ref1b", "ref2b", new ZDateTime(2015, 11, 2, 10, 0, 0), "ref3b");
					AssertRawUsage(rawUsage.Summary.Lines[2], 11, "ref1c", "ref2c", new ZDateTime(2015, 11, 3, 10, 0, 0), "ref3c");
				});
			}

			// CSV
			string expectedCsvResult =
@"""MB"",""Source IP"",""Destination IP"",""Start Time (UTC)"",""End Time (UTC)""
""5"",""ref1a"",""ref2a"",""01-Nov-15 10:00"",""ref3a""
""7"",""ref1b"",""ref2b"",""02-Nov-15 10:00"",""ref3b""
""11"",""ref1c"",""ref2c"",""03-Nov-15 10:00"",""ref3c""
""1"","""","""",""30-Nov-15 00:00"",""""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Nov-15 10:00"","""",""B10"",""S10"",""ref1a ref2a ref3a"","""","""",""5""
""02-Nov-15 10:00"","""",""B11"",""S11"",""ref1b ref2b ref3b"","""","""",""7""
""03-Nov-15 10:00"","""",""B12"",""S12"",""ref1c ref2c ref3c"","""","""",""11""
""30-Nov-15 00:00"","""",""B13"",""S13"","""","""","""",""1""
", writer.ToString());
		}

		void AssertRawUsage(SummaryLine summaryLine, int mb, string ip1, string ip2, ZDateTime time, string ref3)
		{
			AssertEquals("MB", mb.ToString(CultureInfo.InvariantCulture), summaryLine.Column1);
			AssertEquals("IP1", ip1, summaryLine.Column2);
			AssertEquals("IP2", ip2, summaryLine.Column3);
			AssertEquals("Time", time.ToLongTimeString(), summaryLine.Column4);
			AssertEquals("Ref3", ref3, summaryLine.Column5);
		}

		ClientChargeableUsage CreateChargeableUsage(LicenceHeader licHeader, ZDateTime periodStart, ZInt unitCount)
		{
			return BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingDataAccess, BillingConstants.Hosting.DataAccessCode, periodStart, licHeader, unitCount);
		}

		LicenceHeader CreateRawUsage()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = lic.Database;
			var clientCompany = lic.ClientCompany;
			db.LD_DatabaseNumber = 4809;

			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("HOS", BillingConstants.Hosting.DataAccessCode, new ZDateTime(2015, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1a", "ref2a", "ref3a", "ref4a", null, "MSC", 5));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("HOS", BillingConstants.Hosting.DataAccessCode, new ZDateTime(2015, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1b", "ref2b", "ref3b", "ref4b", null, "MSC", 7));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("HOS", BillingConstants.Hosting.DataAccessCode, new ZDateTime(2015, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1c", "ref2c", "ref3c", "ref4c", null, "MSC", 11));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("HOS", BillingConstants.Hosting.DataAccessCode, new ZDateTime(2015, 11, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			return lic;
		}
	}
}