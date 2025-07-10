using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientPremiumServiceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPremiumServiceTypes()
		{
			var registryValue = new CodeDescriptionPairList();
			registryValue.AddPairIfNotExist("#AA", "DESC_#AA");
			EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var service = lic.Database.PremiumServices.AddNew();
			var lookups = new ClientPremiumServiceLookups(service);
			AssertEquals("", lookups.PremiumServiceTypes[0].Code);
			AssertEquals("<Please define a price list for this database, valid at the service start date>", lookups.PremiumServiceTypes[0].Description);

			var prices = stdCompany.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_DiscountCode = "STL1";
			prices.L6_PricelistVersion = "STL v5.0";

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.Service;
			item1.L7_Code = "ZZZ";
			item1.L7_Description = "Item 1";

			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.Service;
			item2.L7_Code = "AAA";
			item2.L7_Description = "Item 2";

			var item3 = prices.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.Service;
			item3.L7_Code = "MMM";
			item3.L7_Description = "Item 3";

			var itemNonService = prices.Items.AddNew();
			itemNonService.L7_Category = BillingConstants.BillingSystem.STL;
			itemNonService.L7_Code = "NON";
			itemNonService.L7_Description = "Item Non-Service";

			var startDate = ZDateTime.Today;
			var priceLink = lic.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_L6 = prices.PK;
			priceLink.PHL_RX_NKCurrency = "USD";
			priceLink.PHL_ValidFrom = startDate;

			service.CPS_StartDate = startDate;
			lookups = new ClientPremiumServiceLookups(service);
			var types = lookups.PremiumServiceTypes;
			AssertEquals("AAA", types[0].Code);
			AssertEquals("MMM", types[1].Code);
			AssertEquals("ZZZ", types[2].Code);
			AssertEquals("#AA", types[3].Code);

			AssertEquals("Item 2", types[0].Description);
			AssertEquals("Item 3", types[1].Description);
			AssertEquals("Item 1", types[2].Description);
			AssertEquals("DESC_#AA", types[3].Description);

			AssertEquals(4, types.Count);
		}

		public void TestGetPremiumServiceTypesInUse()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var prices = BillingTestHelper.CreateStlPriceList(stdCompany, "AAA", "BBB", "CCC");
			foreach (var item in prices.Items)
			{
				item.L7_Category = BillingConstants.BillingSystem.Service;
			}

			var today = ZDateTime.Today;
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");

			var priceLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink1.PHL_L6 = prices.PK;
			priceLink1.PHL_RX_NKCurrency = "USD";
			priceLink1.PHL_ValidFrom = today;

			var priceLink2 = lic1.Database.PriceHeaderLinks.AddNew();
			priceLink2.PHL_L6 = prices.PK;
			priceLink2.PHL_RX_NKCurrency = "USD";
			priceLink2.PHL_ValidFrom = today;

			var service1a = lic1.Database.PremiumServices.AddNew();
			service1a.CPS_StartDate = today;
			service1a.CPS_Type = "AAA";
			service1a.CPS_Units = 1;

			var service1b = lic1.Database.PremiumServices.AddNew();
			service1b.CPS_StartDate = today;
			service1b.CPS_Type = "BBB";
			service1b.CPS_Units = 1;

			var service2a = lic1.Database.PremiumServices.AddNew();
			service2a.CPS_StartDate = today;
			service2a.CPS_Type = "BBB";
			service2a.CPS_Units = 1;

			var service2b = lic1.Database.PremiumServices.AddNew();
			service2b.CPS_StartDate = today;
			service2b.CPS_Type = "AAA";
			service2b.CPS_Units = 1;

			var actual = ClientPremiumServiceLookups.GetPremiumServiceTypesInUse(Factory);

			AssertEquals("AAA", actual[0].Code);
			AssertEquals("BBB", actual[1].Code);
			AssertEquals("Item AAA", actual[0].Description);
			AssertEquals("Item BBB", actual[1].Description);
			AssertEquals(2, actual.Count);
		}

		public void TestUsageOwners()
		{
			var parent = Factory.New<LicenceDatabase>();
			var premiumService1 = parent.PremiumServices.AddNew();
			var premiumService2 = parent.PremiumServices.AddNew();
			var usageOwner1 = Factory.New<ClientCompany>();
			var usageOwner2 = Factory.New<ClientCompany>();
			usageOwner1.LCC_LD = parent.PK;
			usageOwner2.LCC_LD = parent.PK;
			premiumService1.CPS_LCC = usageOwner1.PK;
			premiumService2.CPS_LCC = usageOwner2.PK;

			var lookups = new ClientPremiumServiceLookups(premiumService1);
			var count = lookups.UsageOwners.Count;
			AssertContainsExactElementsInAnyOrder(lookups.UsageOwners, new[] { usageOwner1, usageOwner2 });
		}

		public void TestUsageOwnersOrganisations()
		{
			var parent = Factory.New<LicenceDatabase>();
			var premiumService1 = parent.PremiumServices.AddNew();
			var premiumService2 = parent.PremiumServices.AddNew();
			var usageOwner1 = Factory.New<ClientCompany>();
			var usageOwner2 = Factory.New<ClientCompany>();
			usageOwner1.LCC_LD = parent.PK;
			usageOwner2.LCC_LD = parent.PK;
			var org1 = Factory.New<EDIOrgHeader>();
			var org2 = Factory.New<EDIOrgHeader>();
			premiumService1.CPS_LCC = usageOwner1.PK;
			premiumService2.CPS_LCC = usageOwner2.PK;
			usageOwner1.LCC_OH = org1.PK;
			usageOwner2.LCC_OH = org2.PK;

			var lookups = new ClientPremiumServiceLookups(premiumService1);
			AssertContainsExactElementsInAnyOrder(lookups.UsageOwnersOrganisations, new[] { org1, org2 });
		}
	}
}
