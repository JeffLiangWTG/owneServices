using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class PriceItemSetTest : TestCaseWithFactory
	{
		public void TestCountryTierCodeToPriceNode()
		{
			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = "FHR";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceList = stdLicCompany.PriceHeaders.AddNew();
			priceList.L6_PricelistVersion = "STL v1";
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var countryTierItem1 = priceList.Items.AddNew();
			countryTierItem1.L7_Category = BillingConstants.BillingSystem.STL;
			countryTierItem1.L7_Code = mapping1.PriceCode;
			countryTierItem1.L7_Price = 2.00m;
			countryTierItem1.L7_Order = 1;
			countryTierItem1.L7_FeeType = BillingConstants.FeeType.CountryTier;
			countryTierItem1.L7_CountryTierCode = "C01";
			var countryTierItem2 = priceList.Items.AddNew();
			countryTierItem2.L7_Category = BillingConstants.BillingSystem.STL;
			countryTierItem2.L7_Code = mapping1.PriceCode;
			countryTierItem2.L7_Price = 3.00m;
			countryTierItem2.L7_Order = 1;
			countryTierItem2.L7_FeeType = BillingConstants.FeeType.CountryTier;
			countryTierItem2.L7_CountryTierCode = "C02";
			var otherItem1 = priceList.Items.AddNew();
			otherItem1.L7_Category = BillingConstants.BillingSystem.STL;
			otherItem1.L7_Code = "FOR";
			otherItem1.L7_Price = 5.00m;
			otherItem1.L7_Order = 1;
			otherItem1.L7_FeeType = BillingConstants.FeeType.Transactional;

			var priceItemSet = new PriceItemSet(priceList.Items);
			AssertEquals("Should contain only the country tier items", 2, priceItemSet.CountryTierCodeToPriceNode.Count);
			AssertEquals("Should map the country tier codes to the price items", countryTierItem1, priceItemSet.CountryTierCodeToPriceNode["C01"].Item);
			AssertEquals("Should map the country tier codes to the price items", countryTierItem2, priceItemSet.CountryTierCodeToPriceNode["C02"].Item);
		}
	}
}
