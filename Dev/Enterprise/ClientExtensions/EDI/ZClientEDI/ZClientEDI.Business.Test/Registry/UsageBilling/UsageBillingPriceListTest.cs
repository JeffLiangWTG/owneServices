using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageBillingPriceList))]
	internal class UsageBillingPriceListTest : RegistryBusinessObjectTemplateTestCase<UsageBillingPriceList>
	{
		public void TestGetClone()
		{
			var priceList = NewPopulatedBusinessObject();
			priceList.ProductCode = "E2E";
			priceList.RawUsageCategory = "E01";
			priceList.PriceListCode = "EX1";

			var clone = (UsageBillingPriceList)priceList.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals("E2E", clone.ProductCode);
			AssertEquals("E01", clone.RawUsageCategory);
			AssertEquals("EX1", clone.PriceListCode);
		}

		public void TestValidation()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var collection = new UsageBillingPriceListCollection();
			var product1 = collection.AddNew();
			product1.RunPreSaveValidation();
			AssertHasError(product1.ProductCodeInfo, "Please enter a Product Code.");
			product1.ProductCode = "XXX";
			AssertHasError(product1.ProductCodeInfo, "Enter a valid Product Code.");
			product1.ProductCode = "ABC";
			AssertNoErrors(product1.ProductCodeInfo);

			AssertHasError(product1.PriceListCodeInfo, "Please enter a Price List Code.");
			product1.PriceListCode = "E#1";
			AssertNoErrors(product1.PriceListCodeInfo);

			var product2 = collection.AddNew();
			product2.ProductCode = "ABC";
			product2.PriceListCode = "E#1";
			AssertHasError(product2.PriceListCodeInfo, "The price list system code must be unique.");
			product2.PriceListCode = "E#2";
			AssertNoErrors(product2.PriceListCodeInfo);

			product2.PriceListCode = "WCU";
			AssertHasError(product2.PriceListCodeInfo, "The price list system code must be unique.");
			product2.PriceListCode = "E#3";
			AssertNoErrors(product2.PriceListCodeInfo);

			product1.RawUsageCategory = "SAT";
			product2.RawUsageCategory = "XXX";
			AssertHasError(product2.RawUsageCategoryInfo, "Enter a valid Raw Usage Category.");
			product2.RawUsageCategory = "";
			AssertHasError(product2.RawUsageCategoryInfo, "Please enter a Raw Usage Category.");
			product2.RawUsageCategory = "SAT";
			AssertHasError(product2.RawUsageCategoryInfo, "The raw usage category must be unique.");
			product2.RawUsageCategory = "3GT";
			AssertNoErrors(product2.RawUsageCategoryInfo);
		}

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;

		protected override UsageBillingPriceList GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override UsageBillingPriceList GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		UsageBillingPriceList NewPopulatedBusinessObject() => new UsageBillingPriceList(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
	}
}
