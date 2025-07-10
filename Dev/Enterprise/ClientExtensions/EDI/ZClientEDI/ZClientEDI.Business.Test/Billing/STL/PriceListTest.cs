using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class PriceListTest : TestCaseWithFactory
	{
		public void TestGetPriceNode_CountryTierCode()
		{
			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = "FHR";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			var mapping2 = new CountryTierPriceCodeMapping();
			mapping2.SystemCode = BillingConstants.PriceHeaderType.ODM;
			mapping2.PriceCode = "FHR";
			mapping2.MappingLines.AddNew("US", "C01");
			mapping2.MappingLines.AddNew("SA", "C02");

			mappingCollection.Add(mapping1);
			mappingCollection.Add(mapping2);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceHeader = stdLicCompany.PriceHeaders.AddNew();
			priceHeader.L6_PricelistVersion = "STL v1";
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var countryTierItem1 = priceHeader.Items.AddNew();
			countryTierItem1.L7_Category = BillingConstants.BillingSystem.STL;
			countryTierItem1.L7_Code = mapping1.PriceCode;
			countryTierItem1.L7_Price = 2.00m;
			countryTierItem1.L7_Order = 1;
			countryTierItem1.L7_FeeType = BillingConstants.FeeType.CountryTier;
			countryTierItem1.L7_CountryTierCode = "C01";
			var countryTierItem2 = priceHeader.Items.AddNew();
			countryTierItem2.L7_Category = BillingConstants.BillingSystem.STL;
			countryTierItem2.L7_Code = mapping1.PriceCode;
			countryTierItem2.L7_Price = 3.00m;
			countryTierItem2.L7_Order = 1;
			countryTierItem2.L7_FeeType = BillingConstants.FeeType.CountryTier;
			countryTierItem2.L7_CountryTierCode = "C02";
			var otherItem1 = priceHeader.Items.AddNew();
			otherItem1.L7_Category = BillingConstants.BillingSystem.STL;
			otherItem1.L7_Code = "FOR";
			otherItem1.L7_Price = 5.00m;
			otherItem1.L7_Order = 1;
			otherItem1.L7_FeeType = BillingConstants.FeeType.Transactional;

			var priceList = new PriceList(priceHeader, new PriceItemSet(priceHeader.Items), Array.Empty<EdiPriceUsageMapping>(), Array.Empty<EdiPriceItemRate>(), new DiscountVersion(string.Empty, Array.Empty<EdiPriceHeaderDiscount>(), Array.Empty<EdiPriceDiscountGroupMember>()));

			var usageKey = new UsageCodeKey("", "FHR");

			var priceNode1a = priceList.GetPriceNode(usageKey, "AU");
			var priceNode1b = priceList.GetPriceNodeFromUsageKey(usageKey, "AU");
			AssertEquals("Country AU should map to countryTierItem1 based on the registry mapping", countryTierItem1, priceNode1a.Item);
			AssertEquals("Country AU should map to countryTierItem1 based on the registry mapping", countryTierItem1, priceNode1b.Item);

			var priceNode2a = priceList.GetPriceNode(usageKey, "NZ");
			var priceNode2b = priceList.GetPriceNodeFromUsageKey(usageKey, "NZ");
			AssertEquals("Country AU should map to countryTierItem2 based on the registry mapping", countryTierItem2, priceNode2a.Item);
			AssertEquals("Country AU should map to countryTierItem2 based on the registry mapping", countryTierItem2, priceNode2b.Item);

			AssertEquals("Country US should not map to anything since the registry mapping is for a different system code", null, priceList.GetPriceNode(usageKey, "US"));
			AssertEquals("Country US should not map to anything since the registry mapping is for a different system code", null, priceList.GetPriceNodeFromUsageKey(usageKey, "US"));
			AssertEquals("Country SA should not map to anything since the registry mapping is for a different system code", null, priceList.GetPriceNode(usageKey, "UK"));
			AssertEquals("Country SA should not map to anything since the registry mapping is for a different system code", null, priceList.GetPriceNodeFromUsageKey(usageKey, "UK"));
		}

		public void TestHasPriceCodeCountryTierRegistryMapping()
		{
			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = "FHR";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			var mapping2 = new CountryTierPriceCodeMapping();
			mapping2.SystemCode = BillingConstants.PriceHeaderType.ODM;
			mapping2.PriceCode = "ALP";
			mapping2.MappingLines.AddNew("US", "C01");
			mapping2.MappingLines.AddNew("SA", "C02");

			mappingCollection.Add(mapping1);
			mappingCollection.Add(mapping2);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceHeader = stdLicCompany.PriceHeaders.AddNew();
			priceHeader.L6_PricelistVersion = "STL v1";
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var priceList = new PriceList(priceHeader, new PriceItemSet(priceHeader.Items), Array.Empty<EdiPriceUsageMapping>(), Array.Empty<EdiPriceItemRate>(), new DiscountVersion(string.Empty, Array.Empty<EdiPriceHeaderDiscount>(), Array.Empty<EdiPriceDiscountGroupMember>()));
			AssertEquals("Should find the mapping for STL FHR in the registry", true, priceList.HasPriceCodeCountryTierRegistryMapping("FHR"));
			AssertEquals("Should not find the mapping for STL ALP in the registry", false, priceList.HasPriceCodeCountryTierRegistryMapping("ALP"));
		}

		public void TestGetCountryTierCode()
		{
			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = "FHR";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			var mapping2 = new CountryTierPriceCodeMapping();
			mapping2.SystemCode = BillingConstants.PriceHeaderType.ODM;
			mapping2.PriceCode = "ALP";
			mapping2.MappingLines.AddNew("US", "C01");
			mapping2.MappingLines.AddNew("SA", "C02");

			mappingCollection.Add(mapping1);
			mappingCollection.Add(mapping2);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceHeader = stdLicCompany.PriceHeaders.AddNew();
			priceHeader.L6_PricelistVersion = "STL v1";
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var priceList = new PriceList(priceHeader, new PriceItemSet(priceHeader.Items), Array.Empty<EdiPriceUsageMapping>(), Array.Empty<EdiPriceItemRate>(), new DiscountVersion(string.Empty, Array.Empty<EdiPriceHeaderDiscount>(), Array.Empty<EdiPriceDiscountGroupMember>()));
			AssertEquals("C01", priceList.GetCountryTierCode("FHR", "AU"));
			AssertEquals("C02", priceList.GetCountryTierCode("FHR", "NZ"));
			AssertEquals("US code is not mapped for FHR", string.Empty, priceList.GetCountryTierCode("FHR", "US"));
			AssertEquals("System code STL means this should not find any values", string.Empty, priceList.GetCountryTierCode("ALP", "US"));
		}

		public void TestHasAnyCountryTierPriceItems()
		{
			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = "FHR";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			var mapping2 = new CountryTierPriceCodeMapping();
			mapping2.SystemCode = BillingConstants.PriceHeaderType.ODM;
			mapping2.PriceCode = "ALP";
			mapping2.MappingLines.AddNew("US", "C01");
			mapping2.MappingLines.AddNew("SA", "C02");

			mappingCollection.Add(mapping1);
			mappingCollection.Add(mapping2);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceHeader = stdLicCompany.PriceHeaders.AddNew();
			priceHeader.L6_PricelistVersion = "STL v1";
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var nonCountryTierItem = priceHeader.Items.AddNew();
			nonCountryTierItem.L7_Category = BillingConstants.BillingSystem.STL;
			nonCountryTierItem.L7_Code = mapping1.PriceCode;
			nonCountryTierItem.L7_Price = 2.00m;
			nonCountryTierItem.L7_Order = 1;
			nonCountryTierItem.L7_FeeType = BillingConstants.FeeType.Country;

			var priceList = new PriceList(priceHeader, new PriceItemSet(priceHeader.Items), Array.Empty<EdiPriceUsageMapping>(), Array.Empty<EdiPriceItemRate>(), new DiscountVersion(string.Empty, Array.Empty<EdiPriceHeaderDiscount>(), Array.Empty<EdiPriceDiscountGroupMember>()));
			AssertEquals("No country tier price items", false, priceList.HasAnyCountryTierPriceItems());

			var countryTierItem1 = priceHeader.Items.AddNew();
			countryTierItem1.L7_Category = BillingConstants.BillingSystem.STL;
			countryTierItem1.L7_Code = mapping1.PriceCode;
			countryTierItem1.L7_Price = 2.00m;
			countryTierItem1.L7_Order = 1;
			countryTierItem1.L7_FeeType = BillingConstants.FeeType.CountryTier;
			countryTierItem1.L7_CountryTierCode = "C01";

			priceList = new PriceList(priceHeader, new PriceItemSet(priceHeader.Items), Array.Empty<EdiPriceUsageMapping>(), Array.Empty<EdiPriceItemRate>(), new DiscountVersion(string.Empty, Array.Empty<EdiPriceHeaderDiscount>(), Array.Empty<EdiPriceDiscountGroupMember>()));
			AssertEquals("Country tier price item exists", true, priceList.HasAnyCountryTierPriceItems());
		}

		public void TestGetMissingCountryTierPriceItems()
		{
			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = "FHR";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");
			mapping1.MappingLines.AddNew("SA", "C01");

			var mapping2 = new CountryTierPriceCodeMapping();
			mapping2.SystemCode = BillingConstants.PriceHeaderType.ODM;
			mapping2.PriceCode = "ALP";
			mapping2.MappingLines.AddNew("US", "C01");

			mappingCollection.Add(mapping1);
			mappingCollection.Add(mapping2);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceHeader = stdLicCompany.PriceHeaders.AddNew();
			priceHeader.L6_PricelistVersion = "STL v1";
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var priceList = new PriceList(priceHeader, new PriceItemSet(priceHeader.Items), Array.Empty<EdiPriceUsageMapping>(), Array.Empty<EdiPriceItemRate>(), new DiscountVersion(string.Empty, Array.Empty<EdiPriceHeaderDiscount>(), Array.Empty<EdiPriceDiscountGroupMember>()));
			var missingCountryTierPriceItems = priceList.GetMissingCountryTierPriceItems();
			AssertEquals(2, missingCountryTierPriceItems.Count());
			AssertNotNull(missingCountryTierPriceItems.First(x => x.Item1 == "FHR" && x.Item2 == "C01"));
			AssertNotNull(missingCountryTierPriceItems.First(x => x.Item1 == "FHR" && x.Item2 == "C02"));

			var countryTierItem1 = priceHeader.Items.AddNew();
			countryTierItem1.L7_Category = BillingConstants.BillingSystem.STL;
			countryTierItem1.L7_Code = mapping1.PriceCode;
			countryTierItem1.L7_Price = 2.00m;
			countryTierItem1.L7_Order = 1;
			countryTierItem1.L7_FeeType = BillingConstants.FeeType.CountryTier;
			countryTierItem1.L7_CountryTierCode = "C01";

			priceList = new PriceList(priceHeader, new PriceItemSet(priceHeader.Items), Array.Empty<EdiPriceUsageMapping>(), Array.Empty<EdiPriceItemRate>(), new DiscountVersion(string.Empty, Array.Empty<EdiPriceHeaderDiscount>(), Array.Empty<EdiPriceDiscountGroupMember>()));
			missingCountryTierPriceItems = priceList.GetMissingCountryTierPriceItems();
			AssertEquals(1, missingCountryTierPriceItems.Count());
			AssertNotNull(missingCountryTierPriceItems.First(x => x.Item1 == "FHR" && x.Item2 == "C02"));
		}
	}
}
