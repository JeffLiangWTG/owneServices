using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Registry.Testing
{
	[TestedType(typeof(ExitControlCustomsDataRegistry))]
	sealed class ExitControlCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<ExitControlCustomsDataRegistry>
	{
		public void TestSendExitControlErrors() => CombineAssertions(() =>
		{
			var registryItem = ExitControlCustomsDataRegistry.Instance.SendExitControlErrors;
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<ExitControlGroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"SendExitControlErrorsTo",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
				"Send Exit Control Errors To",
				"Send Exit Control errors to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
			AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, registryItem.CountryFilterPKs);
		});

		public void TestSendExitControlAcknowledgements() => CombineAssertions(() =>
		{
			var registryItem = ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements;
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<ExitControlGroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"SendExitControlAcknowledgementsTo",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
				"Send Exit Control Acknowledgements To",
				"Send Exit Control acknowledgements to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
			AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, registryItem.CountryFilterPKs);
		});

		public void TestEnableExitControlPlugin() 
		{
			TestRegistryItem(ItemSet.EnableExitControlPlugin,
						"EnableExitControlPlugin",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
						"Enable Exit Control Plug-in",
						"Set to YES to display Exit Control tab in Export Brokerage, Shipments and Consols.",
						RegistryStorageFlags.Company,
						false);
			CombineAssertions(() =>
			{
				var euCountriesExceptionGuids = CountryGuids.CountriesUnderEUCustomsJurisdiction.Except(new Guid[] { CountryGuids.Instance.Ireland , CountryGuids.Instance.Netherlands, CountryGuids.Instance.Poland });
				foreach (var countryGuid in euCountriesExceptionGuids)
				{
					Assert(ItemSet.EnableExitControlPlugin.CountryFilterPKs.Contains(countryGuid));
				}
				Assert(!ItemSet.EnableExitControlPlugin.CountryFilterPKs.Contains(Constants.CountryGuids.Ireland));
			});
		}

		public void TestEnableExitControlModule()
		{
			TestRegistryItem(ItemSet.EnableExitControlModule,
						"EnableExitControlModule",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
						"Enable Exit Control Module",
						"Set to YES to enable Exit Control module.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
			CombineAssertions(() =>
			{
				var euCountriesExceptionGuids = CountryGuids.CountriesUnderEUCustomsJurisdiction.Except(new Guid[] { CountryGuids.Instance.Germany, CountryGuids.Instance.Ireland, CountryGuids.Instance.Netherlands, CountryGuids.Instance.Poland, CountryGuids.Instance.Spain });
				AssertContainsExactElementsInAnyOrder(euCountriesExceptionGuids, ItemSet.EnableExitControlModule.CountryFilterPKs);
			});
		}

		public void TestIsExitControlPluginEnableForCurrentCompany() => CombineAssertions(() =>
		{
			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Ireland, false, true);
			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Ireland, true, true);

			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Poland, false, true);
			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Poland, true, true);

			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Netherlands, false, true);
			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Netherlands, true, true);

			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Australia, false, false);
			AssertIsExitControlPluginEnableForCurrentCompany(Constants.CountryCodes.Australia, true, false);

			var euCountriesExceptionGuids = CountryGuids.CountriesUnderEUCustomsJurisdiction.Except(new Guid[] { CountryGuids.Instance.Ireland, CountryGuids.Instance.Netherlands, CountryGuids.Instance.Poland });
			foreach (var countryGuid in euCountriesExceptionGuids)
			{
				var country = Factory.Load<RefCountry>(countryGuid);
				AssertIsExitControlPluginEnableForCurrentCompany(country.Code, false, false);
				AssertIsExitControlPluginEnableForCurrentCompany(country.Code, true, true);
			}
		});

		void AssertIsExitControlPluginEnableForCurrentCompany(string country, bool enableExitControlPluginTemporaryValue, bool expected)
		{
			using (ExitControlCustomsDataRegistry.Instance.EnableExitControlPlugin.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableExitControlPluginTemporaryValue))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					AssertEquals($"Check for [country:{country}] and [registry value:{enableExitControlPluginTemporaryValue}]", expected, ExitControlCustomsDataRegistry.Instance.IsExitControlPluginEnabledForCurrentCompany);
				}
			}
		}

		public void TestIsExitControlModuleEnableForCurrentCompany() => CombineAssertions(() =>
		{
			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Germany, false, true);
			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Germany, true, true);

			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Spain, false, true);
			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Spain, true, true);

			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Poland, false, true);
			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Poland, true, true);

			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Netherlands, false, true);
			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Netherlands, true, true);

			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Ireland, false, true);
			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Ireland, true, true);

			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Australia, false, false);
			AssertIsExitControlModuleEnableForCurrentCompany(Constants.CountryCodes.Australia, true, false);

			var euCountriesExceptionGuids = CountryGuids.CountriesUnderEUCustomsJurisdiction.Except(new Guid[] { CountryGuids.Instance.Germany, CountryGuids.Instance.Spain, CountryGuids.Instance.Ireland, CountryGuids.Instance.Netherlands, CountryGuids.Instance.Poland });
			foreach (var countryGuid in euCountriesExceptionGuids)
			{
				var country = Factory.Load<RefCountry>(countryGuid);
				AssertIsExitControlModuleEnableForCurrentCompany(country.Code, false, false);
				AssertIsExitControlModuleEnableForCurrentCompany(country.Code, true, true);
			}
		});

		void AssertIsExitControlModuleEnableForCurrentCompany(string country, bool enableExitControlPluginTemporaryValue, bool expected)
		{
			using (ExitControlCustomsDataRegistry.Instance.EnableExitControlModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableExitControlPluginTemporaryValue))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					AssertEquals($"Check for [country:{country}] and [registry value:{enableExitControlPluginTemporaryValue}]", expected, ExitControlCustomsDataRegistry.Instance.IsExitControlModuleEnabledForCurrentCompany);
				}
			}
		}

		public void TestExitControlCustomization()
		{
			TestGenericRegistryItem(ItemSet.ExitControlCustomization, "ExitControlJobNumberCustomization", RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl, "Exit Control Job Number Customization", "Override this value to customize how exit control job numbers are formatted", RegistryStorageFlags.All);
			var dataType = (BillCustomisationRegistryDataType)ItemSet.ExitControlCustomization.DataType;
			AssertEquals("GeneratedNumberName", "Exit Control Job Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
		}
	}
}
