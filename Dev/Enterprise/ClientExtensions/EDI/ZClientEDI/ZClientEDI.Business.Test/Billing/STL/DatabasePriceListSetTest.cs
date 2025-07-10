using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class DatabasePriceListSetTest : TestCaseWithFactory
	{
		public void TestConstructorHubExpired()
		{
			var today = ZDateTime.UtcToday.Date;
			var dateToInclusive = today.AddDays(-today.Day);
			var dateFrom = new ZDateTime(dateToInclusive.Year, dateToInclusive.Month, 1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");
			var hubPricesExpired = BillingTestHelper.CreatePriceHeader(lic1.Company, BillingConstants.PriceHeaderType.EHub, "HUB1", "AUD", dateFrom.AddYears(-1));
			hubPricesExpired.L6_ValidTo = dateFrom.AddDays(-1);
			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, dateToInclusive);
			var discountVersionSet = new DiscountVersionSet(Factory);
			var delivery = new UsageOwnerDelivery(new UsageOwner(lic1), lic1.Company.InvoiceDeliveries[0], lic1.Company);
			var priceListSet1 = new DatabasePriceListSet(context, new UsageOwnerDelivery[] { delivery }, BillingConstants.PriceHeaderType.EHub, discountVersionSet);

			AssertNull(priceListSet1.GetPriceListByDatabasePk(lic1.LA_LD.ToGuid()));
		}

		public void TestConstructorStl()
		{
			var today = ZDateTime.UtcToday.Date;
			var dateToInclusive = today.AddDays(-today.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var licWithoutPrices = BillingTestHelper.CreateLicence(Factory, "BBB");
			var priceHeaderWithoutItems = BillingTestHelper.CreateStlPriceList(lic1.Company, Array.Empty<string>());
			var priceLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeaderWithoutItems.PK;
			priceLink.PHL_ValidFrom = today.AddYears(-1);
			priceLink.PHL_RX_NKCurrency = "AUD";
			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, dateToInclusive);
			var discountVersionSet = new DiscountVersionSet(Factory);
			var priceListSet1 = new DatabasePriceListSet(context, new Guid[] { lic1.Database.PK.ToGuid() }, discountVersionSet);
			var priceListSet2 = new DatabasePriceListSet(context, new Guid[] { licWithoutPrices.Database.PK.ToGuid() }, discountVersionSet);

			var prices1 = priceListSet1.GetPriceListByDatabasePk(lic1.LA_LD.ToGuid());
			AssertEquals(priceHeaderWithoutItems.PK, prices1.Header.PK);
			AssertNull(priceListSet2.GetPriceListByDatabasePk(licWithoutPrices.LA_LD.ToGuid()));
		}

		public void TestConstructorStl_RoundedPriceOverBasePrice()
		{
			var today = ZDateTime.UtcToday.Date;
			var dateToInclusive = today.AddDays(-today.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, new[] { "FAX" });

			AssertEquals(1, priceHeader.Items.Count);
			var item1 = priceHeader.Items[0];
			item1.L7_Price = 2.3456m;
			BillingTestHelper.AddPriceItemRate(item1, "USD", 2.51m);

			var priceLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeader.PK;
			priceLink.PHL_ValidFrom = today.AddYears(-1);
			priceLink.PHL_RX_NKCurrency = "AUD";
			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, dateToInclusive);
			var discountVersionSet = new DiscountVersionSet(Factory);
			var priceListSet1 = new DatabasePriceListSet(context, new Guid[] { lic1.Database.PK.ToGuid() }, discountVersionSet);

			var prices1 = priceListSet1.GetPriceListByDatabasePk(lic1.LA_LD.ToGuid());
			AssertEquals(priceHeader.PK, prices1.Header.PK);
			var itemPk2Price = prices1.GetItemPkToPrice("USD");
			AssertEquals(1, itemPk2Price.Count);
			AssertEquals("should be the rounded price", 2.51m, itemPk2Price[item1.PK.ToGuid()]);
		}

		public void TestConstructorStlWithExpiredPriceLink()
		{
			var today = ZDateTime.UtcToday.Date;
			var dateToInclusive = today.AddDays(-today.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var licWithoutPrices = BillingTestHelper.CreateLicence(Factory, "BBB");
			var priceHeaderWithoutItems = BillingTestHelper.CreateStlPriceList(lic1.Company, Array.Empty<string>());
			var priceLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = priceHeaderWithoutItems.PK;
			priceLink.PHL_ValidFrom = today.AddYears(-1);
			priceLink.PHL_ValidTo = today.AddMonths(-2);
			priceLink.PHL_RX_NKCurrency = "AUD";
			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, dateToInclusive);
			var discountVersionSet = new DiscountVersionSet(Factory);
			var priceListSet1 = new DatabasePriceListSet(context, new Guid[] { lic1.Database.PK.ToGuid() }, discountVersionSet);
			var priceListSet2 = new DatabasePriceListSet(context, new Guid[] { licWithoutPrices.Database.PK.ToGuid() }, discountVersionSet);

			var prices1 = priceListSet1.GetPriceListByDatabasePk(lic1.LA_LD.ToGuid());
			AssertNull(prices1);
			AssertNull(priceListSet2.GetPriceListByDatabasePk(licWithoutPrices.LA_LD.ToGuid()));
		}

		public void TestConstructorStlWithExpiredAndNotExpiredPriceLinks()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var priceHeader1 = BillingTestHelper.CreateStlPriceList(lic1.Company, Array.Empty<string>());
			var priceHeader2 = BillingTestHelper.CreateStlPriceList(lic1.Company, Array.Empty<string>());
			var priceLinkOldNotExpired = lic1.Database.PriceHeaderLinks.AddNew();
			priceLinkOldNotExpired.PHL_L6 = priceHeader1.PK;
			priceLinkOldNotExpired.PHL_ValidFrom = periodStart.AddYears(-2);
			priceLinkOldNotExpired.PHL_RX_NKCurrency = "AUD";

			var priceLinkNewExpired = lic1.Database.PriceHeaderLinks.AddNew();
			priceLinkNewExpired.PHL_L6 = priceHeader2.PK;
			priceLinkNewExpired.PHL_ValidFrom = periodStart.AddYears(-1);
			priceLinkNewExpired.PHL_ValidTo = periodStart.AddMonths(-1).AddDays(-1);
			priceLinkNewExpired.PHL_RX_NKCurrency = "AUD";

			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var discountVersionSet = new DiscountVersionSet(Factory);
			var priceListSet1 = new DatabasePriceListSet(context, new Guid[] { lic1.Database.PK.ToGuid() }, discountVersionSet);

			var prices1 = priceListSet1.GetPriceListByDatabasePk(lic1.LA_LD.ToGuid());
			AssertNull("there is no valid pricelist since latest pricelist has expired", prices1);
		}
	}
}