using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiPriceUsageMappingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPriceCategories()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var priceHeader = BillingTestHelper.CreateStlPriceList(licCompany, "USR", "SHP");
			var dpsPrice = BillingTestHelper.AddPriceItem(priceHeader, "DPS", "", BillingConstants.FeeType.Transactional, 5m);
			dpsPrice.L7_Category = BillingConstants.BillingSystem.DeniedPartyScreening;

			var map = priceHeader.UsageMaps.AddNew();
			var descriptions = new CodeDescriptionPairList();
			descriptions.AddPair("STL", "STL Desc");
			descriptions.AddPair(BillingConstants.BillingSystem.DeniedPartyScreening, "DPS Desc");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, descriptions);
			AssertEquals("STL - STL Desc\r\nDPS - DPS Desc", map.Lookups.PriceCategories.ElementsAsString);
		}

		public void TestPriceCodes()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var priceHeader = BillingTestHelper.CreateStlPriceList(licCompany, "USR", "SHP");
			var dpsPrice = BillingTestHelper.AddPriceItem(priceHeader, "DPS", "", BillingConstants.FeeType.Transactional, 5m);
			dpsPrice.L7_Category = BillingConstants.BillingSystem.DeniedPartyScreening;

			var map = priceHeader.UsageMaps.AddNew();
			AssertEquals("USR, SHP, DPS", map.Lookups.PriceCodesForCategory.CodesAsString);

			map.PUM_PriceCategory = BillingConstants.BillingSystem.STL;
			AssertEquals("USR, SHP", map.Lookups.PriceCodesForCategory.CodesAsString);

			map.PUM_PriceCategory = BillingConstants.BillingSystem.DeniedPartyScreening;
			AssertEquals("DPS", map.Lookups.PriceCodesForCategory.CodesAsString);
		}
	}
}