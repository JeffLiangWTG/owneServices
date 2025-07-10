using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding
{
	public class TokenAuthenticationOnBoardingFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddIntFilters(filters);
			AddFlagFilters(filters);
			AddIncidentFilters(filters);
			AddLicenceEnterpriseFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Claim Mapping Name", EdiTokenAuthOnBoardingDataSchema.TOD_ClaimMappingName);
			filters.AddTextFilter("Configuration Identifier", EdiTokenAuthOnBoardingDataSchema.TOD_ConfigurationIdentifier);
			filters.AddTextFilter("System Unique Identifier", EdiTokenAuthOnBoardingDataSchema.TOD_SystemUniqueIdentifier);
			filters.AddTextFilter("Verification Username", EdiTokenAuthOnBoardingDataSchema.TOD_VerificationUsername);
			filters.AddTextFilter("Tenant ID", GetTenantQuery);
		}

		void AddIntFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Retry", EdiTokenAuthOnBoardingDataSchema.TOD_Retry);
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			AddFlagTextFilter(filters, EdiTokenAuthOnBoardingDataSchema.TOD_ClaimMappingIdentifier, new OIDCClaimMappingIdentifiers(),
				ResString.GetMultilingualString("68AC514B-867D-4F58-B9C0-AB15ABF22307", "Claim Mapping Identifier"));

			AddFlagTextFilter(filters, EdiTokenAuthOnBoardingDataSchema.TOD_Status, new OnBoardingStatuses(),
				ResString.GetMultilingualString("5B26E566-86F7-43E3-BAFA-2B3D9447E2CD", "Onboarding Status"));

			AddFlagTextFilter(filters, EdiTokenAuthOnBoardingDataSchema.TOD_OIDCServer, new OIDCServerTypesList(),
				ResString.GetMultilingualString("46159F6A-F059-49D2-8B08-2B9341DF0939", "OIDC Server"));

			AddWinzorOnlyFilters(filters);
		}

		void AddFlagTextFilter(ModuleFilterCollection filters, SchemaStringColumn column, CodeDescriptionPairList codeDescriptionPairList, ResourceString resourceString)
		{
			ZQuery GetQuery(ZString value) => StatusAll.EqualsUnresolvedOrLocalized(value, ignoreCase: false) ? new ZQuery() : new ZQuery(column, value);

			var codeDescriptionWithAllPairList = new CodeDescriptionPairList();
			codeDescriptionWithAllPairList.AddPair(StatusAll, Res.GetString("BCA6182E-F0AF-4B8A-81A0-153CE5FD6CA0", "Show all records"));
			codeDescriptionWithAllPairList.AddRange(codeDescriptionPairList);

			var filter = filters.AddTextFilter(resourceString.EnglishText, GetQuery, codeDescriptionWithAllPairList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = resourceString;
		}

		void AddWinzorOnlyFilters(ModuleFilterCollection filters)
		{
			var winzorOnlyFilter = filters.AddFlagsFilter("Winzor Only", new string[] { Res.GetString("1DA010EF-BB8D-49D4-9BA8-E144B61E911C", "Winzor Only") }, new GetFlagsQuery[] { GetWinzorOnlyFilter });
			winzorOnlyFilter.Category = FilterCategories.StatusAndFlags;
			winzorOnlyFilter.MultilingualDescription = ResString.GetMultilingualString("1DA010EF-BB8D-49D4-9BA8-E144B61E911C", "Winzor Only");
		}

		void AddIncidentFilters(ModuleFilterCollection filters)
		{
			var incidentFilter = new ModuleGuidForeignCollectionFilter("Incident", ClientModuleRegistration.SupportIncident, EdiTokenAuthOnBoardingDataSchema.PK, EdiTokenAuthOnBoardingDataSchema.TOD_IM, new SupportIncidentCollection(Factory), typeof(EdiTokenAuthOnBoardingData));
			incidentFilter.MultilingualDescription = ResString.GetMultilingualString("2A45B433-96C3-453F-9B75-CCA7FC00D41F", "Incident");
			filters.AddFilter(CustomiseModuleGuidForeignCollectionFilter(incidentFilter));
		}

		void AddLicenceEnterpriseFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
		}

		ZQuery GetTenantQuery(SQLComparisonOperator comparisonOperator, ZString key)
		{
			var ediIdentityTenantSubQuery = new ZDBOnlySubQuery(typeof(EdiIdentityTenant), EdiIdentityTenantSchema.PK);
			ediIdentityTenantSubQuery.AddToFilter(JoinCondition.And, EdiIdentityTenantSchema.IDT_TenantId, comparisonOperator, key);

			var ediTokenAuthOnBoardingDataQuery = new ZDBOnlyQuery(typeof(EdiTokenAuthOnBoardingData));
			ediTokenAuthOnBoardingDataQuery.AddSubQuery(EdiTokenAuthOnBoardingDataSchema.TOD_IDT, ediIdentityTenantSubQuery, JoinCondition.And);

			return ediTokenAuthOnBoardingDataQuery;
		}

		ZQuery GetLicenceEnterpriseQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var enterprisePK = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out enterprisePK);

			var result = new ZDBOnlyQuery(typeof(EdiTokenAuthOnBoardingData));

			var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceEnterpriseSchema.PK);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.Equal, ZString.Empty);
			}
			else if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.PK, comparisonOperator, pkValue);
			}
			result.AddSubQuery(EdiTokenAuthOnBoardingDataSchema.TOD_LE, enterpriseSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetWinzorOnlyFilter(ZBool value)
		{
			return new ZQuery().AddToFilter(EdiTokenAuthOnBoardingDataSchema.TOD_WinzorOnly, value);
		}

		ModuleGuidForeignCollectionFilter CustomiseModuleGuidForeignCollectionFilter(ModuleGuidForeignCollectionFilter filter)
		{
			filter.ComparisonOperator_List.Clear();
			filter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.AnyMatch);
			filter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.NoneMatch);
			return filter;
		}
	}
}
