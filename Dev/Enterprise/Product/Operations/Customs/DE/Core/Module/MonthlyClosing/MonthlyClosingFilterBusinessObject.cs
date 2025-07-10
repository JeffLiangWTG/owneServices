using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module
{
	public class MonthlyClosingFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string JobNumber = "Job Number";
			public const string AccountingPeriodFrom = "Period From";
			public const string AccountingPeriodTo = "Period To";
			public const string AuthorizationNumber = "Authorization Number";
			public const string DeclarationType = "Declaration Type";
			public const string CustomsStatus = "Customs Status";
			public const string RegistrationNumber = "Registration Number";
			public const string DeclarationBranch = "Declaration Branch";
			public const string Declarant = "Declarant";
			public const string Representative = "Representative";
			public const string RepresentedParty = "Represented Party";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobNumberFilter = result.AddTextFilter(Schema.JobNumber, CusReconDeclarationSchema.CRD_JobReferenceNumber);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("40322081-2950-4171-A4DE-39873B4D2B7C", Schema.JobNumber);
			jobNumberFilter.MaxLength = CusReconDeclarationSchema.CRD_JobReferenceNumber.MaxLength;
			jobNumberFilter.SupportsBlankComparisonOperators = false;

			var accountingPeriodFromFilter = result.AddDateFilter(Schema.AccountingPeriodFrom, CusReconDeclarationSchema.CRD_PeriodFrom);
			accountingPeriodFromFilter.Category = FilterCategories.Dates;
			accountingPeriodFromFilter.MultilingualDescription = ResString.GetMultilingualString("8DB4B861-AE71-4AB3-AE40-B3D8C25CBFC7", Schema.AccountingPeriodFrom);

			var accountingPeriodToFilter = result.AddDateFilter(Schema.AccountingPeriodTo, CusReconDeclarationSchema.CRD_PeriodTo);
			accountingPeriodToFilter.Category = FilterCategories.Dates;
			accountingPeriodToFilter.MultilingualDescription = ResString.GetMultilingualString("D1036E0C-9A71-4A13-AD2F-2644549983A9", Schema.AccountingPeriodTo);

			var authorizationNumberFilter = result.AddTextFilter(Schema.AuthorizationNumber, GetAuthorizationNumberQuery);
			authorizationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			authorizationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("4AF43B5C-4B22-4E2E-8869-4640AFA2263A", Schema.AuthorizationNumber);
			authorizationNumberFilter.MaxLength = CusPermitHeaderSchema.CPH_Number.MaxLength;
			authorizationNumberFilter.SupportsBlankComparisonOperators = false;

			var declarationTypeFilter = result.AddTextFilter(Schema.DeclarationType, CusReconDeclarationSchema.CRD_DeclarationType, ReconDeclarationLookups.DeclarationTypeList);
			declarationTypeFilter.Category = FilterCategories.ModesAndTypes;
			declarationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("DBCB9BF9-7E2B-42E6-A9E4-34F95BF9D3BB", Schema.DeclarationType);
			declarationTypeFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

			var customsStatusFilter = result.AddTextFilter(Schema.CustomsStatus, CusReconDeclarationSchema.CRD_CustomsStatus, ReconDeclarationLookups.CustomsStatusList);
			customsStatusFilter.Category = FilterCategories.StatusAndFlags;
			customsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("47EE1FB7-871C-4422-BFA9-50E5993EADE3", Schema.CustomsStatus);
			customsStatusFilter.MaxLength = CusReconDeclarationSchema.CRD_CustomsStatus.MaxLength;
			customsStatusFilter.SupportsBlankComparisonOperators = false;

			var registrationNumberFilter = result.AddTextFilter(Schema.RegistrationNumber, GetRegistrationNumberQuery);
			registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("76CF32CA-2C93-4967-B6BF-FF7E46ACB08E", Schema.RegistrationNumber);
			registrationNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			registrationNumberFilter.SupportsBlankComparisonOperators = false;
			registrationNumberFilter.SubGroup = new EntryNumberSubGroup();

			var branchFilter = result.AddGuidFilter(Schema.DeclarationBranch, ModuleIDs.GlbBranch, CusReconDeclarationSchema.CRD_GB_Branch, new GlbBranchCollection(Factory));
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("EC40EA34-4E79-4A55-89B0-45A3ADA5FCD0", "Declaration Branch");

			var declarantFilter = new ModuleGuidFilterForOrg(Schema.Declarant, ModuleIDs.Organisation, GetDeclarantQuery, new OrgHeaderCollection(Factory));
			declarantFilter.Category = FilterCategories.Organisations;
			declarantFilter.MultilingualDescription = ResString.GetMultilingualString("6D3FB626-3B44-4BE4-932E-519A60E0335F", Schema.Declarant);
			result.AddFilter(declarantFilter);

			var representativeFilter = new ModuleGuidFilterForOrg(Schema.Representative, ModuleIDs.Organisation, GetRepresentativeQuery, new OrgHeaderCollection(Factory));
			representativeFilter.Category = FilterCategories.Organisations;
			representativeFilter.MultilingualDescription = ResString.GetMultilingualString("8EB1D918-30E0-41D2-9F37-C34C0FDC99EB", Schema.Representative);
			result.AddFilter(representativeFilter);

			var representedPartyFilter = new ModuleGuidFilterForOrg(Schema.RepresentedParty, ModuleIDs.Organisation, GetBuyingAgentQuery, new OrgHeaderCollection(Factory));
			representedPartyFilter.Category = FilterCategories.Organisations;
			representedPartyFilter.MultilingualDescription = ResString.GetMultilingualString("1DFDF57A-6583-4A78-AE1B-A345FD63F2E9", Schema.RepresentedParty);
			result.AddFilter(representedPartyFilter);

			return result;
		}

		static ZQuery GetDeclarantQuery(ZGuid value) => GetAddressOrganizationQuery(value, CusReconDeclarationSchema.CRD_OA_DeclarantAddress);

		static ZQuery GetRepresentativeQuery(ZGuid value) => GetAddressOrganizationQuery(value, CusReconDeclarationSchema.CRD_OA_RepresentativeAddress);

		static ZQuery GetBuyingAgentQuery(ZGuid value) => GetAddressOrganizationQuery(value, CusReconDeclarationSchema.CRD_OA_BuyingAgentAddress);

		static ZQuery GetAddressOrganizationQuery(ZGuid value, SchemaColumn addressColumn)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, value);

			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
			result.AddSubQuery(addressColumn, orgAddressSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetAuthorizationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(CusAuthorisationHeader), CusPermitHeaderSchema.PK);
			subQuery.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			subQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
			subQuery.AddToFilter(CusPermitHeaderSchema.CPH_Number, comparisonOperator, value);

			var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
			result.AddSubQuery(CusReconDeclarationSchema.CRD_CPH_ReconClearanceAuthorisation, subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
		}

		Business.CusReconDeclarationLookups ReconDeclarationLookups => reconDeclarationLookups ?? (reconDeclarationLookups = Factory.GetNull<Business.CusReconDeclaration>().Lookups);
		Business.CusReconDeclarationLookups reconDeclarationLookups;

		class EntryNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, Customs.Business.CusReconBase.AutoCusReconDeclaration.Schema.TableName);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Germany);
				subQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(CusReconDeclaration));
				result.AddSubQuery(CusReconDeclarationSchema.PK, subQuery, JoinCondition.And);
				return result;
			}
		}
	}
}
