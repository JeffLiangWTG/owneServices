using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business.Test;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceBillingDiscountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSystemCodes()
		{
			UsageBillingSettingsTest.SetupValidTestRegistry();
			ClientLicenceBillingDiscount billingDiscount = Factory.New<ClientLicenceBillingDiscount>();
			CodeDescriptionPairList expected = new CodeDescriptionPairList(BillingConstants.GetBillingSystemList());
			expected.RemoveCode(BillingConstants.BillingSystem.Maintenance);
			expected.RemoveCode(BillingConstants.BillingSystem.Fee);
			expected.RemoveCode(BillingConstants.BillingSystem.STL);
			expected.AddPair("PL0", "ABC - Price List #0");
			expected.AddPair("PL1", "ABC - Price List #1");
			AssertContainsExactElementsInAnyOrder(expected, billingDiscount.Lookups.SystemCodes);
		}

		public void TestDiscountTypes()
		{
			ClientLicenceBillingDiscount billingDiscount = Factory.New<ClientLicenceBillingDiscount>();
			AssertContainsExactElementsInAnyOrder(BillingConstants.GetDiscountTypeList(), billingDiscount.Lookups.DiscountTypes);
		}

		public void TestModuleCodeList()
		{
			ClientLicenceBillingDiscount billingDiscount = Factory.New<ClientLicenceBillingDiscount>();
			AssertContainsExactElementsInAnyOrder(LicenceModuleList.Instance.Names, billingDiscount.Lookups.ModuleCodeList);
		}

		public void TestDiscountBreakUnits()
		{
			ClientLicenceBillingDiscount billingDiscount = Factory.New<ClientLicenceBillingDiscount>();
			AssertContainsExactElementsInAnyOrder(BillingConstants.GetDiscountBreakUnitList(), billingDiscount.Lookups.DiscountBreakUnits);
		}

		public void TestSubCodes()
		{
			ClientLicenceBillingDiscount billingDiscount = Factory.New<ClientLicenceBillingDiscount>();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(), billingDiscount.Lookups.SubCodes);

			var mappings = new CodeDescriptionPairList();
			mappings.AddPair("Interface1", "PSQ 1000");
			mappings.AddPair("Interface2", "PSQ 2000");
			EDIDataRegistry.Instance.ClientMappingBillingNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);
			var interfaceGroupPrice = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			interfaceGroupPrice.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			interfaceGroupPrice.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			interfaceGroupPrice.L7_Description = "Interfaces Group";
			interfaceGroupPrice.L7_Ref4 = "G01";
			var interface1Price = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			interface1Price.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			interface1Price.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			interface1Price.L7_Description = "Sub Interface 1";
			interface1Price.L7_ParentCode = "G01";
			var interface2Price = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			interface2Price.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			interface2Price.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			interface2Price.L7_Description = "Sub Interface 2";
			interface2Price.L7_ParentCode = "G01";
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddRange(mappings);
			expectedList.AddPair("G01", "Interfaces Group");
			billingDiscount.L5_SystemCode = BillingConstants.BillingSystem.ClientMapping;
			AssertContainsExactElementsInAnyOrder(expectedList, billingDiscount.Lookups.SubCodes);

			billingDiscount.L5_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			AssertContainsExactElementsInAnyOrder(new ABMCustomsTransactionTypes(), billingDiscount.Lookups.SubCodes);
		}
	}
}
