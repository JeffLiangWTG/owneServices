using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicencePriceItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPriceCodeList()
		{
			var priceItem = Factory.New<ClientLicencePriceItem>();

			priceItem.L7_Category = BillingConstants.BillingSystem.STL;
			var expected = new CodeDescriptionPairList();
			AssertContainsExactElementsInAnyOrder(expected, priceItem.Lookups.PriceCodeList);

			priceItem.L7_Category = BillingConstants.BillingSystem.ODM;
			expected.AddRange(priceItem.Lookups.ModuleCodeList);
			expected.AddRange(priceItem.Lookups.PriceItemDiscountTypes);
			AssertContainsExactElementsInAnyOrder(expected, priceItem.Lookups.PriceCodeList);

			priceItem.L7_Category = ZString.Empty;
			AssertContainsExactElementsInAnyOrder(expected, priceItem.Lookups.PriceCodeList);
		}

		public void TestFeeTypes()
		{
			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();
			AssertContainsExactElementsInAnyOrder(BillingConstants.GetFeeTypeList(), priceItem.Lookups.FeeTypes);
		}

		public void TestModuleCodeList()
		{
			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();
			AssertContainsExactElementsInAnyOrder(LicenceModuleList.Instance.Names, priceItem.Lookups.ModuleCodeList);
		}

		public void TestProductPropertiesLists()
		{
			var priceItem = Factory.New<ClientLicencePriceItem>();
			AssertContainsExactElementsInAnyOrder(new ClientLicencePriceItemProductAvailabilityPairList(), priceItem.Lookups.ProductAvailabilityPairList);
			AssertContainsExactElementsInAnyOrder(EDIDataRegistry.Instance.ProductDisplayCategories.Value.GetActiveCodeDescriptionPairList(), priceItem.Lookups.ProductDisplayCategories);
		}

		public void TestCountryTierCodeList()
		{
			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mapping1.MappingLines.AddNew("AU", "C01");

			var mapping2 = new CountryTierPriceCodeMapping();
			mapping2.SystemCode = "XXX";
			mapping2.PriceCode = "YYY";
			mapping2.MappingLines.AddNew("CN", "C03");
			mapping2.MappingLines.AddNew("NZ", "C02");
			mapping2.MappingLines.AddNew("US", "C02");

			mappingCollection.Add(mapping1);
			mappingCollection.Add(mapping2);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem = priceHeader.Items.AddNew();
			AssertEquals("Without a system or product code there should be no country tier codes", 0, priceItem.Lookups.CountryTierCodeList.Count);

			priceItem.L7_FeeType = FeeType.CountryTier;

			priceHeader.L6_SystemCode = "STL";
			AssertEquals("Without a product code there should be no country tier codes", 0, priceItem.Lookups.CountryTierCodeList.Count);

			priceItem.L7_Code = "FBL";
			AssertEquals("Should retrieve list from registry", 1, priceItem.Lookups.CountryTierCodeList.Count);
			AssertEquals("C01", priceItem.Lookups.CountryTierCodeList[0].Code);

			priceItem.L7_Code = "YYY";
			AssertEquals("Invalid System Product Code combination should yield no results", 0, priceItem.Lookups.CountryTierCodeList.Count);

			priceHeader.L6_SystemCode = "XXX";
			AssertEquals("Should retrieve list from registry", 2, priceItem.Lookups.CountryTierCodeList.Count);
			AssertEquals("Codes should be listed in alphabetical order", "C02", priceItem.Lookups.CountryTierCodeList[0].Code);
			AssertEquals("Codes should be listed in alphabetical order", "C03", priceItem.Lookups.CountryTierCodeList[1].Code);
		}
	}
}
