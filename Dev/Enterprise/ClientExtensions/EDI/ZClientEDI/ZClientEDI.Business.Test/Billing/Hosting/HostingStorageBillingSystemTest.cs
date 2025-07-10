using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	public class HostingStorageBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			var hostingBilling = new HostingStorageBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.HostingStorage, hostingBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "", new ZDateTime(2010, 10, 01), lic, 10);
			Factory.Save();

			var hostingBilling = new HostingStorageBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));
			AssertEquals(true, hostingBilling.LoadSystemBills(context).First() is HostingBill);
		}

		public void TestLoadOdplUsages()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var licChild1 = BillingTestHelper.CreateDependentLicence(licHeader1, "AA1");
			var licHeader2 = BillingTestHelper.CreateLicence(Factory, "BBB", "SYD", "BBB");
			var licHeader3 = BillingTestHelper.CreateLicence(Factory, "CCC", "SYD", "CCC");

			var periodStart = new ZDateTime(2010, 10, 01);

			var chargeableUsage1 = CreateChargeableUsage(licHeader1, periodStart, "AAA", 10);
			var chargeableUsage2 = CreateChargeableUsage(licHeader1, periodStart, "FFF", 20);
			var chargeableUsage3 = CreateChargeableUsage(licHeader1, periodStart, "CCC", 30);

			var chargeableUsage4 = CreateChargeableUsage(licChild1, periodStart, "AAA", 11);

			var chargeableUsage5 = CreateChargeableUsage(licHeader2, periodStart, "FFF", 22);
			var chargeableUsage6 = CreateChargeableUsage(licHeader2, periodStart, "CCC", 24);

			var priceHeader1 = licHeader1.Company.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 16m);
			var priceHeader2 = licHeader2.Company.PriceHeaders.AddNew();
			priceHeader2.L6_RX_NKCurrency = "AUD";
			priceHeader2.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader2, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader2, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 16m);
			var priceHeader3 = licHeader3.Company.PriceHeaders.AddNew();
			priceHeader3.L6_RX_NKCurrency = "AUD";
			priceHeader3.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader3, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader3, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 16m);
			var chargeableUsage7 = CreateChargeableUsage(licHeader1, new ZDateTime(2015, 5, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB / 2);
			var chargeableUsage8 = CreateChargeableUsage(licHeader2, new ZDateTime(2015, 5, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB - 1);
			var chargeableUsage9 = CreateChargeableUsage(licHeader3, new ZDateTime(2015, 5, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB * 100);
			var chargeableUsage10 = CreateChargeableUsage(licHeader1, new ZDateTime(2015, 4, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB * 100);
			var chargeableUsageHA = CreateChargeableUsage(licHeader1, new ZDateTime(2015, 5, 1), BillingConstants.Hosting.UltraFastStorageCode, BillingConstants.Hosting.MBperGB * 100);

			Factory.Save();

			EDIDataRegistry.Instance.HostingStorageBufferPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			var hostingBilling = new HostingStorageBillingSystem();
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

			// billing by used size from 2015-5-1
			hostingBilling = new HostingStorageBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 5, 31));

			allHostingUsages = new List<HostingUsage>();
			foreach (SystemBill bill in hostingBilling.LoadSystemBills(context))
			{
				allHostingUsages.AddRange(bill.SystemUsages.Cast<HostingUsage>());
			}
			Assert("zero usage is skipped", !allHostingUsages.Any(x => x.ChargeableUsagePKs.Contains(chargeableUsage7.PK)));
			Assert("#HA is skipped", !allHostingUsages.Any(x => x.ChargeableUsagePKs.Contains(chargeableUsageHA.PK)));
			var hostingUsage8 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage8.PK));
			var hostingUsage9 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage9.PK));

			AssertEquals("UnitCount", 1, hostingUsage8.UnitCount);
			AssertEquals("UnitCount", 12, hostingUsage9.UnitCount);

			hostingBilling = new HostingStorageBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 4, 30));

			allHostingUsages = new List<HostingUsage>();
			foreach (SystemBill bill in hostingBilling.LoadSystemBills(context))
			{
				allHostingUsages.AddRange(bill.SystemUsages.Cast<HostingUsage>());
			}
			var hostingUsage10 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage10.PK));
			AssertEquals("UnitCount had no buffer before 2015-5-1", 10, hostingUsage10.UnitCount);
		}

		public void TestLoadOdplUsages_AccumulateUnbilledUsages()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");

			var periodStart = new ZDateTime(2016, 5, 1);
			var priceHeader05 = licHeader.Company.PriceHeaders.AddNew();
			priceHeader05.L6_RX_NKCurrency = "AUD";
			priceHeader05.L6_ValidFrom = new ZDateTime(2016, 5, 1);
			BillingTestHelper.AddPriceItem(priceHeader05, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			var hd05 = BillingTestHelper.AddPriceItem(priceHeader05, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 16m);

			var priceHeader04 = licHeader.Company.PriceHeaders.AddNew();
			priceHeader04.L6_RX_NKCurrency = "AUD";
			priceHeader04.L6_ValidFrom = new ZDateTime(2016, 4, 1);
			BillingTestHelper.AddPriceItem(priceHeader04, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 9m);
			var hd04 = BillingTestHelper.AddPriceItem(priceHeader04, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 15m);

			var chargeableUsage1 = CreateChargeableUsage(licHeader, new ZDateTime(2016, 3, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB * 150);
			var chargeableUsage2 = CreateChargeableUsage(licHeader, new ZDateTime(2016, 5, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB * 50);
			var chargeableUsage3 = CreateChargeableUsage(licHeader, new ZDateTime(2016, 4, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB * 100);

			Factory.Save();

			EDIDataRegistry.Instance.HostingStorageBufferPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			var hostingBilling = new HostingStorageBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 5, 31));

			var allHostingUsages = new List<HostingUsage>();
			foreach (SystemBill bill in hostingBilling.LoadSystemBills(context))
			{
				allHostingUsages.AddRange(bill.SystemUsages.Cast<HostingUsage>());
			}
			Assert("Unbilled usage before last billing period is ignored", !allHostingUsages.Any(x => x.ChargeableUsagePKs.Contains(chargeableUsage1.PK)));
			var hostingUsage2 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage2.PK));
			var hostingUsage3 = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage3.PK));

			AssertEquals("UnitCount", 6, hostingUsage2.UnitCount);
			AssertEquals("UnitCount", 12, hostingUsage3.UnitCount);

			AssertEquals(new ZDateTime(2016, 5, 1), hostingUsage2.PeriodStart);
			AssertEquals(new ZDateTime(2016, 4, 1), hostingUsage3.PeriodStart);
			AssertEquals(priceHeader05, hostingUsage2.PriceHeader);
			AssertEquals(priceHeader04, hostingUsage3.PriceHeader);
			AssertEquals(hd05, hostingUsage2.PriceItem);
			AssertEquals(hd04, hostingUsage3.PriceItem);
		}

		public void TestLoadOdplUsages_AccumulateUnbilledUsages_IgnoreUsagesWithoutPriceHeader()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");

			var periodStart = new ZDateTime(2016, 5, 1);
			var priceHeader05 = licHeader.Company.PriceHeaders.AddNew();
			priceHeader05.L6_RX_NKCurrency = "AUD";
			priceHeader05.L6_ValidFrom = new ZDateTime(2016, 5, 1);
			BillingTestHelper.AddPriceItem(priceHeader05, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			var hd05 = BillingTestHelper.AddPriceItem(priceHeader05, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 16m);

			var chargeableUsage05 = CreateChargeableUsage(licHeader, new ZDateTime(2016, 5, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB * 50);
			var chargeableUsage04 = CreateChargeableUsage(licHeader, new ZDateTime(2016, 4, 1), BillingConstants.Hosting.DataStorageCode, BillingConstants.Hosting.MBperGB * 100);

			Factory.Save();

			EDIDataRegistry.Instance.HostingStorageBufferPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
			var hostingBilling = new HostingStorageBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 5, 31));

			var allHostingUsages = new List<HostingUsage>();
			foreach (SystemBill bill in hostingBilling.LoadSystemBills(context))
			{
				allHostingUsages.AddRange(bill.SystemUsages.Cast<HostingUsage>());
			}

			var hostingUsage05 = allHostingUsages.Single();
			AssertEquals("UnitCount", 6, hostingUsage05.UnitCount);
			AssertEquals(new ZDateTime(2016, 5, 1), hostingUsage05.PeriodStart);
			AssertEquals(priceHeader05, hostingUsage05.PriceHeader);
			AssertEquals(hd05, hostingUsage05.PriceItem);
		}

		[TestDate(2017, 7, 20)]
		public void TestLoadOdplRawUsage()
		{
			EDIDataRegistry.Instance.HostingStorageBufferPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 25);

			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var period1 = new ZDateTime(2015, 4, 1);
			var period2 = new ZDateTime(2015, 5, 1);
			var chargeableUsage1 = CreateChargeableUsage(licHeader1, period1, BillingConstants.Hosting.DataStorageCode, 2000);
			var chargeableUsage2 = CreateChargeableUsage(licHeader1, period1, BillingConstants.Hosting.eDocsStorageCode, 13000);
			var chargeableUsage3 = CreateChargeableUsage(licHeader1, period2, BillingConstants.Hosting.DataStorageCode, 3000);
			var chargeableUsage4 = CreateChargeableUsage(licHeader1, period2, BillingConstants.Hosting.eDocsStorageCode, 17000);
			Factory.Save();

			var hostingBilling = new HostingStorageBillingSystem();
			var context1 = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Company.LC_OH, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			var context2 = new BillingLoadRawUsageContext(Factory, period2, licHeader1.Company.LC_OH, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			var rawUsage1 = (SystemCodeRawUsage)hostingBilling.LoadOdplRawUsage(context1);
			var rawUsage2 = (SystemCodeRawUsage)hostingBilling.LoadOdplRawUsage(context2);
			var line1 = rawUsage1.Summary.Lines[0];
			var line2 = rawUsage1.Summary.Lines[1];
			var line3 = rawUsage2.Summary.Lines[0];
			var line4 = rawUsage2.Summary.Lines[1];

			CombineAssertions(() =>
				{
					AssertEquals("High Speed [AAA]", line1.Column1);
					AssertEquals("Image [AAA]", line2.Column1);
					AssertEquals((chargeableUsage1.U1_UnitCount / BillingConstants.Hosting.MBperGB).ToString("#,##0.000"), line1.Column2);
					AssertEquals((chargeableUsage2.U1_UnitCount / BillingConstants.Hosting.MBperGB).ToString("#,##0.000"), line2.Column2);
					AssertEquals((chargeableUsage3.U1_UnitCount * 125 / 100 / BillingConstants.Hosting.MBperGB).ToString("#,##0.000"), line3.Column2);
					AssertEquals((chargeableUsage4.U1_UnitCount * 125 / 100 / BillingConstants.Hosting.MBperGB).ToString("#,##0.000"), line4.Column2);
				});

			string expectedCsvResult =
@"""Type"",""GB""
""High Speed [AAA]"",""1.953""
""Image [AAA]"",""12.695""
";

			var builder = new ZStringBuilder();
			var loadRawUsageContext = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Company.Header.PK, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			hostingBilling.LoadRawUsageInCsv(loadRawUsageContext, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			hostingBilling.LoadRawUsageInCsv(loadRawUsageContext, false, writer);
			AssertEquals(@"""20-Jul-17 00:00"","""","""","""",""High Speed [AAA] 1.953"","""","""",""1""
""20-Jul-17 00:00"","""","""","""",""Image [AAA] 12.695"","""","""",""2""
", writer.ToString());
		}

		[TestDate(2017, 7, 20)]
		public void TestLoadOdplRawUsage_NonProduction()
		{
			EDIDataRegistry.Instance.HostingStorageBufferPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 25);

			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			licHeader1.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			var period1 = new ZDateTime(2015, 4, 1);
			var period2 = new ZDateTime(2015, 5, 1);
			var chargeableUsage1 = CreateChargeableUsage(licHeader1, period1, BillingConstants.Hosting.NonProductionStorageCode, 2000);
			var chargeableUsage2 = CreateChargeableUsage(licHeader1, period2, BillingConstants.Hosting.NonProductionStorageCode, 3000);
			Factory.Save();

			var hostingBilling = new HostingStorageBillingSystem();
			var context1 = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Company.LC_OH, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			var context2 = new BillingLoadRawUsageContext(Factory, period2, licHeader1.Company.LC_OH, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			var rawUsage1 = (SystemCodeRawUsage)hostingBilling.LoadOdplRawUsage(context1);
			var rawUsage2 = (SystemCodeRawUsage)hostingBilling.LoadOdplRawUsage(context2);
			var line1 = rawUsage1.Summary.Lines[0];
			var line2 = rawUsage2.Summary.Lines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Non-Production [AAA]", line1.Column1);
				AssertEquals("Non-Production [AAA]", line2.Column1);
				AssertEquals((chargeableUsage1.U1_UnitCount / BillingConstants.Hosting.MBperGB).ToString("#,##0.000"), line1.Column2);
				AssertEquals((chargeableUsage2.U1_UnitCount * 125 / 100 / BillingConstants.Hosting.MBperGB).ToString("#,##0.000"), line2.Column2);
			});

			string expectedCsvResult =
@"""Type"",""GB""
""Non-Production [AAA]"",""1.953""
";

			var builder = new ZStringBuilder();
			var loadRawUsageContext = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Company.Header.PK, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			hostingBilling.LoadRawUsageInCsv(loadRawUsageContext, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			hostingBilling.LoadRawUsageInCsv(loadRawUsageContext, false, writer);
			AssertEquals(@"""20-Jul-17 00:00"","""","""","""",""Non-Production [AAA] 1.953"","""","""",""1""
", writer.ToString());
		}

		[TestDate(2017, 7, 20)]
		public void TestLoadRawUsage_UsagesFilteredByContext()
		{
			EDIDataRegistry.Instance.HostingStorageBufferPercentage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 25);

			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var period1 = new ZDateTime(2015, 4, 1);
			CreateChargeableUsage(licHeader1, period1, BillingConstants.Hosting.DataStorageCode, 2000);
			CreateChargeableUsage(licHeader1, period1, BillingConstants.Hosting.eDocsStorageCode, 13000);
			CreateChargeableUsage(licHeader1, period1, BillingConstants.Hosting.UltraFastStorageCode, 17000);

			Factory.Save();

			var hostingBilling = new HostingStorageBillingSystem();
			var context1 = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Company.LC_OH, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);

			var builder = new ZStringBuilder();
			var loadRawUsageContext = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Company.Header.PK, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			hostingBilling.LoadRawUsageInCsv(loadRawUsageContext, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(@"""Type"",""GB""
""Ultra Fast [AAA]"",""16.602""
""High Speed [AAA]"",""1.953""
""Image [AAA]"",""12.695""
", builder.ToString());

			var priceHeader1 = licHeader1.Company.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			var hd = BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 16m);
			var he = BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.Hosting.eDocsStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 25m);
			Factory.Save();

			builder = new ZStringBuilder();
			loadRawUsageContext = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Database.PK, hd.PK, licHeader1.ClientCompany.PK);
			hostingBilling.LoadRawUsageInCsv(loadRawUsageContext, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(@"""Company Code"",""Type"",""GB""
""SYD"",""High Speed [AAA]"",""1.953""
", builder.ToString());

			builder = new ZStringBuilder();
			loadRawUsageContext = new BillingLoadRawUsageContext(Factory, period1, licHeader1.Company.Header.PK, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			hostingBilling.LoadRawUsageInCsv(loadRawUsageContext, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(@"""Type"",""GB""
""High Speed [AAA]"",""1.953""
""Image [AAA]"",""12.695""
", builder.ToString());
		}

		ClientChargeableUsage CreateChargeableUsage(LicenceHeader licHeader, ZDateTime periodStart, ZString subCode, ZInt unitCount)
		{
			return BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, subCode, periodStart, licHeader, unitCount);
		}

		void AssertHostingUsage(IEnumerable<HostingUsage> allHostingUsages, ClientChargeableUsage chargeableUsage)
		{
			var hostingUsage = allHostingUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage.PK));
			AssertEquals(chargeableUsage.OrganisationPK, hostingUsage.OrganisationPK);

			AssertEquals(chargeableUsage.U1_SubCode, hostingUsage.SubCode);
			AssertEquals(chargeableUsage.U1_UnitCount.ToZInt(), hostingUsage.UnitCount);
		}
	}
}
