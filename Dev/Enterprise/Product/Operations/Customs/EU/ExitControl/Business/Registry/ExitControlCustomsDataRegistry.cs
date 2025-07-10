using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.EUExitControl;

namespace Enterprise.Customs.EU.ExitControl.Registry
{
	public sealed class ExitControlCustomsDataRegistry : RegistryItemSet, IExitControlCustomsDataRegistry
	{
		public static ExitControlCustomsDataRegistry Instance => instance ?? (instance = new ExitControlCustomsDataRegistry());

		[ThreadStatic]
		static ExitControlCustomsDataRegistry instance;

		ExitControlCustomsDataRegistry()
		{
		}

		public BooleanRegistryItem EnableExitControlPlugin
		{
			get
			{
				return GetItem("EnableExitControlPlugin", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableExitControlPlugin",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
						ResString.GetMultilingualString("E66EC592-4BE6-4A1A-A5CC-D5CDD9DEBDA8", "Enable Exit Control Plug-in"),
						ResString.GetMultilingualString("BCBC69B2-A834-4B84-A878-58BB94CD0031", "Set to YES to display Exit Control tab in Export Brokerage, Shipments and Consols."),
						RegistryStorageFlags.Company,
						false);

					result.CountryFilterPKs = CountryGuids.CountriesUnderEUCustomsJurisdiction.Except(new Guid[3] {
						CountryGuids.Instance.Ireland,
						CountryGuids.Instance.Netherlands,
						CountryGuids.Instance.Poland })
					.ToArray();
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableExitControlModule
		{
			get
			{
				return GetItem("EnableExitControlModule", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableExitControlModule",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
						ResString.GetMultilingualString("22BAFAEB-4D19-41C1-BA14-D45B9169C599", "Enable Exit Control Module"),
						ResString.GetMultilingualString("B2DE96F0-9B84-42A6-A2B5-9FE8C62CA9F3", "Set to YES to enable Exit Control module."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);

					result.CountryFilterPKs = CountryGuids.CountriesUnderEUCustomsJurisdiction.Except(new Guid[5] {
						CountryGuids.Instance.Germany,
						CountryGuids.Instance.Ireland,
						CountryGuids.Instance.Netherlands,
						CountryGuids.Instance.Poland,
						CountryGuids.Instance.Spain })
					.ToArray();
					return result;
				});
			}
		}

		#region Exit Control Notifications

		public GroupNotificationRegistryItem<ExitControlGroupNotification> SendExitControlErrors =>
			GetItem("SendExitControlErrorsTo", () =>
			{
				var result = new GroupNotificationRegistryItem<ExitControlGroupNotification>(
					"SendExitControlErrorsTo",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
					ResString.GetMultilingualString("88efadce-6cde-4a87-9077-f6bc9d138d9e", "Send Exit Control Errors To"),
					ResString.GetMultilingualString("9dd7e673-5f83-4584-983d-f5b67c5d55f8", "Send Exit Control errors to staff member, nominated group or both"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new ExitControlGroupNotification(Constants.EmailTo.StaffMember, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
				return result;
			});

		public GroupNotificationRegistryItem<ExitControlGroupNotification> SendExitControlAcknowledgements =>
			GetItem("SendExitControlAcknowledgementsTo", () =>
			{
				var result = new GroupNotificationRegistryItem<ExitControlGroupNotification>(
					"SendExitControlAcknowledgementsTo",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
					ResString.GetMultilingualString("43d6edfa-c79c-4fc2-8f4e-79354cead002", "Send Exit Control Acknowledgements To"),
					ResString.GetMultilingualString("61e57f59-0506-4a36-bad3-bb4743cd7c1e", "Send Exit Control acknowledgements to staff member, nominated group or both"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new ExitControlGroupNotification(Constants.EmailTo.StaffMember, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
				return result;
			});

		#endregion

		#region Exit Control Job Number Customization
		public BillCustomisationRegistryItem ExitControlCustomization
		{
			get
			{
				return GetItem("ExitControlJobNumberCustomization", delegate
				{
					var dataType = new BillCustomisationRegistryDataType();
					dataType.Categories = NumberCustomisationElementCategories.Standard;
					dataType.GeneratedNumberName = ResString.GetMultilingualString("177bfd9c-6caf-459a-ad9d-25565b282d8a", "Exit Control Job Number");
					dataType.MaxLength = CusExitHeaderSchema.CXH_JobReference.MaxLength;

					return new BillCustomisationRegistryItem(
						"ExitControlJobNumberCustomization",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ExitControl,
						ResString.GetMultilingualString("8de1426f-b7f4-413a-9eeb-2458674f3290", "Exit Control Job Number Customization"),
						ResString.GetMultilingualString("681cba6c-ddee-4a8a-bdc4-54030440eaf8", "Override this value to customize how exit control job numbers are formatted"),
						RegistryStorageFlags.All,
						dataType);
				});
			}
		}
		#endregion

		public override bool IsForProductivityWise => false;

		IRegistryItem IExitControlCustomsDataRegistry.EnableExitControlPlugin => EnableExitControlPlugin;

		IRegistryItem IExitControlCustomsDataRegistry.EnableExitControlModule => EnableExitControlModule;

		public bool IsExitControlPluginEnabledForCurrentCompany
		{
			get
			{
				return CurrentCountryHasExitControlEnabledByDefault || (CurrentCountryIsInEU && EnableExitControlPlugin.Value);
			}
		}

		public bool IsExitControlModuleEnabledForCurrentCompany
		{
			get
			{
				return CurrentCountryHasExitControlEnabledByDefault || (CurrentCountryIsInEU && EnableExitControlModule.Value) || CurrentCountryHasExitControlModuleEnabledByDefault;
			}
		}

		static bool CurrentCountryHasExitControlEnabledByDefault
		{
			get
			{
				var countryCode = GlbCompany.CurrentCompany?.GC_RN_NKCountryCode.ToString();
				var countriesWithExitControlEnabled = new ZString[] { Constants.CountryCodes.Ireland, Constants.CountryCodes.Netherlands, Constants.CountryCodes.Poland };
				return countriesWithExitControlEnabled.Contains(countryCode);
			}
		}

		static bool CurrentCountryIsInEU
		{
			get { return GlbCompany.CurrentCompany?.Country?.RN_EconomicGrouping.ToString() == EconomicGroupList.Codes.EuropeanUnion; }
		}

		static bool CurrentCountryHasExitControlModuleEnabledByDefault
		{
			get
			{
				var countryCode = GlbCompany.CurrentCompany?.GC_RN_NKCountryCode.ToString();
				var countriesWithExitControlModuleEnabled = new ZString[] { Constants.CountryCodes.Germany, Constants.CountryCodes.Spain };
				return countriesWithExitControlModuleEnabled.Contains(countryCode);
			}
		}
	}
}
