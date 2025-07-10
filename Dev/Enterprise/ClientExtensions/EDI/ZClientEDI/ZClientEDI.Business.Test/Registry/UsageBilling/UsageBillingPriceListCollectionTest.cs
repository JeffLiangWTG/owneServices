using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageBillingPriceListCollection))]
	internal class UsageBillingPriceListCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UsageBillingPriceListCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override UsageBillingPriceListCollection GetCollectionToTest() => new UsageBillingPriceListCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new UsageBillingPriceList();

		public void TestGetPriceListCodeDescriptionPairList()
		{
			var billingProducts = new UsageBillingPriceListCollection();
			var billingProduct = billingProducts.AddNew();
			billingProduct.ProductCode = "E2E";
			billingProduct.RawUsageCategory = "GBC";
			billingProduct.PriceListCode = "PL0";
			billingProduct.Description = "Price List #0";

			var billingProduct2 = billingProducts.AddNew();
			billingProduct2.ProductCode = "E2E";
			billingProduct.RawUsageCategory = "GBC";
			billingProduct2.PriceListCode = "PL2";
			billingProduct2.Description = "Price List #2";

			var priceListCodeList = billingProducts.GetPriceListCodeDescriptionPairList();
			AssertEquals(2, priceListCodeList.Count);
			AssertEquals("Price List #0", priceListCodeList["PL0"].Description);
			AssertEquals("Price List #2", priceListCodeList["PL2"].Description);
		}

		public void TestContainsProductCode()
		{
			var billingProducts = new UsageBillingPriceListCollection();
			AssertEquals(false, billingProducts.ContainsProductCode("E2E"));

			var billingProduct = billingProducts.AddNew();
			billingProduct.ProductCode = "E2E";
			billingProduct.RawUsageCategory = "GBC";
			billingProduct.PriceListCode = "PL0";
			billingProduct.Description = "Price List #0";
			AssertEquals(true, billingProducts.ContainsProductCode("E2E"));
			AssertEquals(false, billingProducts.ContainsProductCode("E20"));
		}

		public void TestContainsPriceListCode()
		{
			var billingProducts = new UsageBillingPriceListCollection();
			AssertEquals(false, billingProducts.ContainsPriceListCode("PL0"));
			AssertEquals(false, billingProducts.ContainsPriceListCode("PL1"));

			var billingProduct = billingProducts.AddNew();
			billingProduct.ProductCode = "E2E";
			billingProduct.RawUsageCategory = "GBC";
			billingProduct.PriceListCode = "PL0";
			billingProduct.Description = "Price List #0";
			AssertEquals(true, billingProducts.ContainsPriceListCode("PL0"));
			AssertEquals(false, billingProducts.ContainsPriceListCode("PL1"));
		}

		public void TestGetProductCodes()
		{
			var billingProducts = new UsageBillingPriceListCollection();
			AssertEquals(0, billingProducts.GetProductCodes().Count());

			var billingProduct = billingProducts.AddNew();
			billingProduct.ProductCode = "E2E";
			billingProduct.RawUsageCategory = "GBC";
			billingProduct.PriceListCode = "PL0";
			billingProduct.Description = "Price List #0";
			AssertEquals("E2E", billingProducts.GetProductCodes().Single());
		}
	}
}
