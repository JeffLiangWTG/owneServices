using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiLicenceSettingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPriceCodesForCategory()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("INV", "E-Invoicing");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;

			var eInvoicingPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "IT1", "TP1", "TP1");
			eInvoicingPrices.L6_SystemCode = "INV";
			var eInvoicePriceItem = eInvoicingPrices.Items[0];
			eInvoicePriceItem.L7_Category = "ACC";
			var tierPrice1 = eInvoicingPrices.Items[1];
			var tierPrice2 = eInvoicingPrices.Items[1];
			tierPrice1.L7_Category = tierPrice2.L7_Category = "ACC";
			tierPrice1.L7_UnitBreak = 0;
			tierPrice2.L7_UnitBreak = 5;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "SHP");

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var monthToday = BillingTestHelper.MonthToday;
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", false);
			var db = lic.Database;
			var priceLink = BillingTestHelper.CreatePriceLink(db, stlPrices, monthToday);

			var priceSetting = Factory.New<PriceLicenceSetting>();
			priceSetting.LS9_ValidFrom = monthToday;
			priceSetting.LS9_LD = db.PK;
			db.LicenceSettings.Add(priceSetting);

			priceSetting.PriceCategory = "ACC";
			priceSetting.PriceCode = "IT1";

			var lookups = priceSetting.Lookups;
			Assert("global price list category found", lookups.PriceCategories.ContainsCode("ACC"));
			var priceCodes = lookups.PriceCodesForCategory;
			Assert("global price list price code found", priceCodes.ContainsCode("IT1"));

			AssertEquals("TP1", lookups.PriceTierCodes.Single());
			AssertEquals("TP1", lookups.PriceTierCodesForCategory.OfType<ICodeDescription>().Single().Code);
		}

		public void TestDiscountSuspensionPolicyCodes()
		{
			var setting = Factory.New<PriceLicenceSetting>();
			AssertContainsExactElementsInAnyOrder(new DiscountSuspensionPolicyList(), setting.Lookups.DiscountSuspensionPolicyCodes);
		}
	}
}
