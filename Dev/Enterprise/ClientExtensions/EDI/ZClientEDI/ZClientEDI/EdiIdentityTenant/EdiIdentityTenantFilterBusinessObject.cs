using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IdentityTenant
{
	public class EdiIdentityTenantFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Graph Client Id", EdiIdentityTenantSchema.IDT_GraphClientId);
			filters.AddTextFilter("Name", EdiIdentityTenantSchema.IDT_Name);
			filters.AddTextFilter("Tenant Id", EdiIdentityTenantSchema.IDT_TenantId);
			filters.AddTextFilter("OIDC Client Id", EdiIdentityTenantSchema.IDT_OidcClientId);
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var onboardingStatusFilter = filters.AddTextFilter("Onboarding Status", GetOnboardingStatusQuery, OnboardingStatusList);
			onboardingStatusFilter.Category = FilterCategories.StatusAndFlags;
			onboardingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("9ADA891A-8B18-461E-9013-5A77192D0904", "Onboarding Status");
		}

		CodeDescriptionPairList OnboardingStatusList
		{
			get
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();
				codeDescriptionPairList.AddPair(StatusAll, ShowAllDescription);
				codeDescriptionPairList.AddPair(StatusOnboarding, Res.GetString("5E170362-2A16-41B2-A298-9FA51BAE2359", "Onboarding Only"));
				codeDescriptionPairList.AddPair(StatusNotOnboarding, Res.GetString("DB95BAC4-12A8-45ED-9C69-0FFA75CE7C06", "Not Onboarding Only"));
				return codeDescriptionPairList;
			}
		}

		internal static string StatusOnboarding => Res.GetString("5F1E1751-7EF4-48B4-A068-A86D6807BB4A", "Onboarding");
		internal static string StatusNotOnboarding => Res.GetString("B88198AB-9F96-4AA3-AF4A-A844567223E1", "Not Onboarding");
		internal static string ShowAllDescription => Res.GetString("CCCA396D-48AE-4DAB-AE68-BF0EB28B0D00", "Show All Records");

		ZQuery GetOnboardingStatusQuery(ZString value)
		{
			if (StatusAll.EqualsUnresolvedOrLocalized(value, ignoreCase: false))
			{
				return new ZQuery();
			}

			return new ZQuery(EdiIdentityTenantSchema.IDT_Onboarding, value == StatusOnboarding);
		}
	}
}
