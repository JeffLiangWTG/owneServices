using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiPriceUsageMappingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUsageCode()
		{
			var licCompany = BillingTestHelper.CreateLicencedOrganization(Factory, "ENT", "COM");
			var prices = BillingTestHelper.CreatePriceHeader(licCompany, BillingConstants.PriceHeaderType.STL, "STL V1", "USD", BillingTestHelper.MonthToday);
			var priceItem1 = BillingTestHelper.AddPriceItem(prices, "AAA", BillingConstants.FeeType.Transactional, "", 100);
			var priceItem2 = BillingTestHelper.AddPriceItem(prices, "BBB", BillingConstants.FeeType.Transactional, "", 100);
			var map1 = prices.UsageMaps.AddNew();
			map1.PUM_PriceCategory = "STL";
			map1.PUM_PriceCode = "AAA";
			map1.PUM_UsageCategory = "STL";
			map1.PUM_UsageCode = "AA1";
			var map2 = prices.UsageMaps.AddNew();
			map2.PUM_PriceCategory = "STL";
			map2.PUM_PriceCode = "AAA";
			map2.PUM_UsageCategory = "STL";
			map2.PUM_UsageCode = "AA2";
			AssertNoErrors(map2.PUM_PriceCodeInfo);
			AssertNoErrors(map2.PUM_UsageCodeInfo);

			map2.PUM_UsageCode = "AA1";
			AssertHasErrors("duplicate codes error", map2.PUM_UsageCodeInfo);

			map2.PUM_PriceCode = "BBB";
			map2.Validation.ValidatePUM_UsageCode();
			AssertHasErrors("duplicate codes error", map2.PUM_UsageCodeInfo);

			map2.PUM_UsageCode = "AA2";
			AssertNoErrors(map2.PUM_UsageCodeInfo);

			Factory.Save();

			map2.PUM_UsageCode = "AA1";
			AssertHasErrors("duplicate codes error", map2.PUM_UsageCodeInfo);
		}

		public void TestCategoryCodeValidation()
		{
			var licCompany = BillingTestHelper.CreateLicencedOrganization(Factory, "ENT", "COM");
			var prices = BillingTestHelper.CreatePriceHeader(licCompany, BillingConstants.PriceHeaderType.STL, "STL V1", "USD", BillingTestHelper.MonthToday);
			var priceItem1 = BillingTestHelper.AddPriceItem(prices, "AAA", BillingConstants.FeeType.Transactional, "", 100);
			var priceItem2 = BillingTestHelper.AddPriceItem(prices, "AAA", BillingConstants.FeeType.Transactional, "", 100);
			priceItem2.L7_Category = "USC";
			var map1 = prices.UsageMaps.AddNew();
			map1.PUM_PriceCategory = "";
			map1.PUM_PriceCode = "AAA";
			map1.PUM_UsageCategory = "";
			map1.PUM_UsageCode = "AA1";

			map1.Validation.ValidatePUM_PriceCategory();
			map1.Validation.ValidatePUM_UsageCategory();
			AssertHasErrors("PUM_PriceCategory must be entered", map1.PUM_PriceCategoryInfo);
			AssertHasErrors("PUM_UsageCategory must be entered", map1.PUM_UsageCategoryInfo);

			// STL.AA1 -> STL.AAA
			map1.PUM_PriceCategory = "STL";
			map1.PUM_UsageCategory = "STL";
			AssertNoErrors(map1.PUM_PriceCategoryInfo);
			AssertNoErrors(map1.PUM_UsageCategoryInfo);

			// STL.AA2 -> USC.AAA
			var map2 = prices.UsageMaps.AddNew();
			map2.PUM_PriceCategory = "USC";
			map2.PUM_PriceCode = "AAA";
			map2.PUM_UsageCategory = "STL";
			map2.PUM_UsageCode = "AA2";
			map2.Validation.ValidatePUM_UsageCategory();
			AssertNoErrors("Another map to a different price category, same price code is OK", map2.PUM_UsageCategoryInfo);
		}
	}
}