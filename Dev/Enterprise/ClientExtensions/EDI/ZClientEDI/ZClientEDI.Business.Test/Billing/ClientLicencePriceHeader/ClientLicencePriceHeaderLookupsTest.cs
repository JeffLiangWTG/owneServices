using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business.Test;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicencePriceHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLicenceEditions()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			AssertContainsExactElementsInAnyOrder(BillingConstants.GetLicenceEditionList(), priceHeader.Lookups.LicenceEditions);
		}

		public void TestSystemCodes()
		{
			UsageBillingSettingsTest.SetupValidTestRegistry();

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			CodeDescriptionPairList expected = BillingConstants.PriceHeaderType.GetPriceHeaderTypeList();
			AssertContainsExactElementsInAnyOrder(expected, priceHeader.Lookups.SystemCodes);
		}

		public void TestPricelistVersions()
		{
			LicenceHeader header = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			LicenceCompany company = header.Company;

			AddPriceHeader(company, "V100");
			AddPriceHeader(company, "V300");
			AddPriceHeader(company, "V200");

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(header);

			CodeDescriptionPairList expected = new CodeDescriptionPairList();
			expected.AddPair("V100");
			expected.AddPair("V200");
			expected.AddPair("V300");

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			var actual = priceHeader.Lookups.PricelistVersions;
			AssertEquals(expected[0].Code, actual[0].Code);
			AssertEquals(expected[1].Code, actual[1].Code);
			AssertEquals(expected[2].Code, actual[2].Code);
		}

		public void TestDiscountCodes()
		{
			LicenceHeader header = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			LicenceCompany company = header.Company;

			AddDiscount(company, "V100");
			AddDiscount(company, "V300");
			AddDiscount(company, "V200");

			var stlPrices1 = company.PriceHeaders.AddNew();
			stlPrices1.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			stlPrices1.L6_DiscountCode = "STL1";
			stlPrices1.L6_PricelistVersion = "STL v5.0";

			var stlPrices2 = company.PriceHeaders.AddNew();
			stlPrices2.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			stlPrices2.L6_DiscountCode = "STL1";
			stlPrices2.L6_PricelistVersion = "STL v6.0";

			var stlPrices3 = company.PriceHeaders.AddNew();
			stlPrices3.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			stlPrices3.L6_DiscountCode = "STL2";
			stlPrices3.L6_PricelistVersion = "STL v7.0";

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(header);

			CodeDescriptionPairList expected = new CodeDescriptionPairList();
			expected.AddPair("V100");
			expected.AddPair("V200");
			expected.AddPair("V300");

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			var actual = priceHeader.Lookups.DiscountCodes;
			AssertEquals(expected[0].Code, actual[0].Code);
			AssertEquals(expected[1].Code, actual[1].Code);
			AssertEquals(expected[2].Code, actual[2].Code);

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			var stlActual = priceHeader.Lookups.DiscountCodes;

			CodeDescriptionPairList stlExpected = new CodeDescriptionPairList();
			stlExpected.AddPair("STL1");
			stlExpected.AddPair("STL2");

			AssertEquals(stlExpected[0].Code, stlActual[0].Code);
			AssertEquals(stlExpected[1].Code, stlActual[1].Code);
		}

		public void TestPriceCodesForCategory()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);

			var stdPrices = stdCompany.PriceHeaders.AddNew();
			stdPrices.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			stdPrices.L6_PricelistVersion = "CW1 v5.0";
			stdPrices.L6_IsStandard = false;

			var item1 = stdPrices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ODM;
			item1.L7_Code = "ZZZ";
			item1.L7_Description = "Item 1";

			var item2 = stdPrices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.ODM;
			item2.L7_Code = "AAA";
			item2.L7_Description = "Item 2";

			Factory.Save();

			var priceHeader1 = Factory.New<ClientLicencePriceHeader>();
			priceHeader1.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			priceHeader1.L6_PricelistVersion = "CW1 v5.0";
			priceHeader1.L6_IsStandard = true;

			var priceCodes = priceHeader1.Lookups.PriceCodesForCategory(BillingConstants.BillingSystem.ODM);
			AssertEquals("ZZZ", priceCodes[0].Code);
			AssertEquals("AAA", priceCodes[1].Code);

			var priceHeader2 = Factory.New<ClientLicencePriceHeader>();
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			priceHeader2.L6_PricelistVersion = "CW1 v7.0";
			priceHeader2.L6_IsStandard = false;

			var item3 = priceHeader2.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.ODM;
			item3.L7_Code = "BBB";
			item3.L7_Description = "Item 3";

			priceCodes = priceHeader2.Lookups.PriceCodesForCategory(BillingConstants.BillingSystem.ODM);
			AssertEquals("BBB", priceCodes[0].Code);
		}

		internal static ClientLicencePriceHeader AddPriceHeader(LicenceCompany company, string version)
		{
			var prices = company.PriceHeaders.AddNew();
			prices.L6_PricelistVersion = version;
			prices.L6_LicenceEdition = BillingConstants.LicenceEdition.Country;
			prices.L6_RN_NKCountry = "AU";
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			prices.L6_ValidFrom = ZDateTime.Now;
			return prices;
		}

		internal static ClientLicenceBillingDiscount AddDiscount(LicenceCompany company, string version)
		{
			var discount = company.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_DiscountCode = version;
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Description = "Disco";
			discount.L5_Discount = 10m;
			discount.L5_StartDate = new ZDateTime(2010, 1, 1);
			return discount;
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}
	}
}
