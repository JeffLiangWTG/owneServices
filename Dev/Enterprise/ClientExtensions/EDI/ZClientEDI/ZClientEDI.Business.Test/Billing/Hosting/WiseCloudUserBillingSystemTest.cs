using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	public class WiseCloudUserBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			var billing = new WiseCloudUserBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.WiseCloudUser, billing.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var priceList = BillingTestHelper.CreatePriceHeader(lic.Company, BillingConstants.PriceHeaderType.ODM, "ODM1", "AUD", new ZDateTime(2018, 1, 1), false);
			BillingTestHelper.AddPriceItem(priceList, BillingConstants.Hosting.WiseCloudUserFeeCode, BillingConstants.FeeType.NamedUser, "", 7.0m);
			CreateChargeableUsage(lic.ClientCompany, new ZDateTime(2018, 10, 1), 10);
			Factory.Save();

			var billing = new WiseCloudUserBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2018, 10, 31));
			AssertEquals(true, billing.LoadSystemBills(context).First() is HostingBill);
		}

		public void TestLoadSystemBills()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var licHeader2NoPriceItem = BillingTestHelper.CreateLicence(Factory, "BBB", "CO2", "PRD");

			var periodStart = new ZDateTime(2018, 10, 1);

			var chargeableUsage1 = CreateChargeableUsage(licHeader1.ClientCompany, periodStart, 50);
			var chargeableUsageTooOld = CreateChargeableUsage(licHeader1.ClientCompany, periodStart.AddMonths(-1), 40);
			var chargeableUsageTooNew = CreateChargeableUsage(licHeader1.ClientCompany, periodStart.AddMonths(1), 60);

			var chargeableUsage2 = CreateChargeableUsage(licHeader2NoPriceItem.ClientCompany, periodStart, 50);

			var priceHeader1 = licHeader1.Company.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = periodStart.AddYears(-1);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 44m);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.Hosting.WiseCloudUserFeeCode, BillingConstants.FeeType.NamedUser, "", 7m);

			var priceHeader2NoPriceItem = licHeader2NoPriceItem.Company.PriceHeaders.AddNew();
			priceHeader2NoPriceItem.L6_RX_NKCurrency = "AUD";
			priceHeader2NoPriceItem.L6_ValidFrom = periodStart.AddYears(-1);
			BillingTestHelper.AddPriceItem(priceHeader2NoPriceItem, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 44m);

			Factory.Save();

			var billing = new WiseCloudUserBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var allUsages = new List<HostingUsage>();
			foreach (SystemBill bill in billing.LoadSystemBills(context))
			{
				allUsages.AddRange(bill.SystemUsages.Cast<HostingUsage>());
			}

			AssertEquals("usage count", 1, allUsages.Count);

			var hostingUsage1 = allUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsage1.PK));
			AssertEquals("UnitCount", 50, hostingUsage1.UnitCount);

			AssertEquals(chargeableUsage1.U1_SubCode, hostingUsage1.SubCode);
			AssertEquals(chargeableUsage1.U1_LCC, hostingUsage1.User.ClientCompanyPK);
			AssertEquals(BillingConstants.BillingSystem.WiseCloudUser, hostingUsage1.SystemCode);
		}

		public void TestLoadSystemBills_Preview()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var periodStart = new ZDateTime(2018, 10, 1);

			var chargeableUsage = CreateChargeableUsage(licence.ClientCompany, periodStart, 50);

			var priceHeader1 = licence.Company.PriceHeaders.AddNew();
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_ValidFrom = periodStart.AddYears(-1);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader1, BillingConstants.Hosting.WiseCloudUserFeeCode, BillingConstants.FeeType.NamedUser, "", 16m);

			var priceHeaderPreview = licence.Company.PriceHeaders.AddNew();
			priceHeaderPreview.L6_RX_NKCurrency = "AUD";
			priceHeaderPreview.L6_ValidFrom = periodStart.AddYears(-2);
			BillingTestHelper.AddPriceItem(priceHeaderPreview, BillingConstants.Hosting.WiseCloudUserFeeCode, BillingConstants.FeeType.NamedUser, "", 7m);

			Factory.Save();

			var billing = new WiseCloudUserBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.SetPreviewOnly(licence, priceHeaderPreview);

			var bill = billing.LoadSystemBills(context)[0];
			var usage = bill.SystemUsages[0] as HostingUsage;
			AssertEquals("Should use preview price header", priceHeaderPreview, usage.PriceHeader);
		}

		ClientChargeableUsage CreateChargeableUsage(ClientCompany clientCompany, ZDateTime periodStart, ZInt unitCount)
		{
			return BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.Hosting.WiseCloudUserFeeCode, periodStart, clientCompany, unitCount);
		}
	}
}
