using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Registry.Testing
{
	[TestedType(typeof(EmcsCustomsDataRegistry))]
	sealed class EmcsCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<EmcsCustomsDataRegistry>
	{
		public void TestIsForProductivityWise()
		{
			AssertEquals(false, EmcsCustomsDataRegistry.Instance.IsForProductivityWise);
		}

		public void TestEnableEmcsFunctions()
		{
			var registryItem = EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions;
			CombineAssertions(() =>
			{
				AssertType<BooleanRegistryItem>(registryItem);
				TestGenericRegistryItem(registryItem,
					"EnableEmcsFunctions",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
					"Enable EMCS Functions",
					"Set to YES to enable EMCS module",
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false);
			});
		}

		public void TestEnableEmcsFunctions_GB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var registryItem = EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions;
				CombineAssertions(() =>
				{
					AssertType<BooleanRegistryItem>(registryItem);
					TestGenericRegistryItem(registryItem,
						"EnableEmcsFunctions",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
						"Enable EMCS Functions",
						"Set to YES to enable EMCS module",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public void TestEnableEmcsFunctions_CountryFilter()
		{
			AssertContainsExactElementsInAnyOrder(Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, EmcsCustomsDataRegistry.Instance.EnableEmcsFunctions.CountryFilterPKs);
		}

		public void TestEmcsSendMessageErrors()
		{
			var registryItem = EmcsCustomsDataRegistry.Instance.EmcsSendMessageErrors;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<EmcsGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"EmcsSendMessageErrorsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
					"Send EMCS Errors To",
					"Send EMCS message errors to staff member, nominated group or combination of both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertDefaultValues(registryItem.DefaultValue);
			});
		}

		public void TestEmcsSendMessageErrors_CountryFilter()
		{
			AssertContainsExactElementsInAnyOrder(Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, EmcsCustomsDataRegistry.Instance.EmcsSendMessageErrors.CountryFilterPKs);
		}

		public void TestEmcsSendConsigneeAcknowledgements()
		{
			var registryItem = EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<EmcsGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"EmcsSendConsigneeAcknowledgementsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
					"Send EMCS Consignee Acknowledgements To",
					"Send EMCS consignee acknowledgements to staff member, nominated group or combination of both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertDefaultValues(registryItem.DefaultValue);
			});
		}

		public void TestEmcsSendConsigneeAcknowledgements_CountryFilter()
		{
			AssertContainsExactElementsInAnyOrder(Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements.CountryFilterPKs);
		}

		public void TestEmcsSendConsignorAcknowledgements()
		{
			var registryItem = EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<EmcsGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"EmcsSendConsignorAcknowledgementsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_EMCS,
					"Send EMCS Consignor Acknowledgements To",
					"Send EMCS consignor acknowledgements to staff member, nominated group or combination of both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertDefaultValues(registryItem.DefaultValue);
			});
		}

		public void TestEmcsSendConsignorAcknowledgements_CountryFilter()
		{
			AssertContainsExactElementsInAnyOrder(Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements.CountryFilterPKs);
		}

		void AssertDefaultValues(EmcsGroupNotification defaultValue)
		{
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, defaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, defaultValue.SendGroupPK);
		}
	}
}
