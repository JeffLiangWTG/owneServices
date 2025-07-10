using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionAgreementApprovalFilterBusinessObject : FilterStripBusinessObject
	{
		public CommissionAgreementApprovalFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "CommissionAgreementApprovals";
		}

		#region Filter Categories

		public static FilterCategory CommissionsFilterCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("2F89ED62-C48C-47B8-982C-54942DEB3C5C", "Commissions"));

		public static FilterCategory AgreementsFilterCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("1906A116-CF12-4C75-AD6A-0E919E0A92DF", "Agreements"));

		public static FilterCategory OpportunitiesFilterCategory => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("552FA049-DD8F-42D4-B529-B1298F0CC218", "Opportunities"));

		#endregion

		#region Module Filters

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var opportunityIdFilter = new ModuleGuidFilter(FilterDescription.OpportunityId, ModuleIDs.Opportunity, OrgCommissionAgreementSchema.CA0_P8, Opportunities)
			{
				MultilingualDescription = ResString.GetMultilingualString("A9349091-5459-4B9E-A6FC-ECD305B3CC71", "Opportunity Id"),
				Category = OpportunitiesFilterCategory
			};
			filters.AddFilter(opportunityIdFilter);

			var agreementStatusFilter = new ModuleTextFilter(FilterDescription.AgreementStatus, GetStatusQuery, Statuses)
			{
				MultilingualDescription = ResString.GetMultilingualString("71FC1687-EC48-49A5-8708-24A5F625E81E", "Agreement Status"),
				Category = AgreementsFilterCategory
			};
			filters.AddFilter(agreementStatusFilter);

			var customerFilter = new ModuleGuidFilter(FilterDescription.Customer, ModuleIDs.Organisation, OrgCommissionAgreementSchema.CA0_OH_Customer, Organizations)
			{
				MultilingualDescription = ResString.GetMultilingualString("946FA909-269C-4B0B-AEAA-ACA1E93344E8", "Customer"),
				Category = AgreementsFilterCategory
			};
			filters.AddFilter(customerFilter);

			var effectiveDateFilter = new ModuleDateFilter(FilterDescription.EffectiveDate, OrgCommissionAgreementSchema.CA0_EffectiveDate, true)
			{
				MultilingualDescription = ResString.GetMultilingualString("26F82DF9-A4AF-490D-991A-96731BDBC6B5", "Effective Date"),
				Category = AgreementsFilterCategory
			};
			filters.AddFilter(effectiveDateFilter);

			var opportunityStatusFilter = new ModuleTextFilter(FilterDescription.OpportunityStatus, GetOpportunityStatusQuery, OpportunityStatuses)
			{
				MultilingualDescription = ResString.GetMultilingualString("71F44900-1153-4164-852D-697E0696313C", "Opportunity Status"),
				Category = OpportunitiesFilterCategory
			};
			filters.AddFilter(opportunityStatusFilter);

			var companyFilter = new ModuleGuidFilter(FilterDescription.Company, ModuleIDs.GlbCompany, GetCompanyQuery, Company)
			{
				MultilingualDescription = ResString.GetMultilingualString("B080FAE6-4E63-4C0E-B0B5-15954352761C", "Company"),
				Category = OpportunitiesFilterCategory
			};
			filters.AddFilter(companyFilter);

			var basisFilter = new ModuleTextFilter(FilterDescription.Basis, OrgCommissionAgreementSchema.CA0_CommissionBasis, Basis)
			{
				MultilingualDescription = ResString.GetMultilingualString("B9A2A5D2-705E-4A68-B445-3172DB30C636", "Basis"),
				Category = CommissionsFilterCategory
			};
			filters.AddFilter(basisFilter);

			var entityStaffFilter = new ModuleNkFilter(FilterDescription.EntityStaff, GetEntityStaffQuery, ModuleIDs.GlbStaff, Staff)
			{
				MultilingualDescription = ResString.GetMultilingualString("31C7CF45-EBA8-4B2D-BEC3-AF4A8D63A21B", "Entity Staff"),
				Category = FilterCategories.Organisations
			};
			filters.AddFilter(entityStaffFilter);

			var entityOrganizationFilter = new ModuleGuidFilter(FilterDescription.EntityOrganization, ModuleIDs.Organisation, GetEntityOrganizationQuery, Organizations)
			{
				MultilingualDescription = ResString.GetMultilingualString("E9651F16-0982-449C-8229-F6AC63E9AE9E", "Entity Organization"),
				Category = FilterCategories.Organisations
			};
			filters.AddFilter(entityOrganizationFilter);

			var salesPersonFilter = new ModuleNkFilter(FilterDescription.SalesPerson, GetSalesPersonQuery, ModuleIDs.GlbStaff, Staff)
			{
				MultilingualDescription = ResString.GetMultilingualString("7C8B86DC-96A3-4FBF-BF94-8C56CCBEA70E", "Sales Person"),
				Category = FilterCategories.Organisations
			};
			filters.AddFilter(salesPersonFilter);

			var salespersonBranchFilter = new ModuleGuidFilter(FilterDescription.SalespersonBranch, ModuleIDs.GlbBranch, GetSalesPersonBranchQuery, Branch)
			{
				MultilingualDescription = ResString.GetMultilingualString("4CC121DD-FF8F-4943-9E5B-19D9F10F74BA", "Sales Person Branch"),
				Category = FilterCategories.Organisations
			};
			filters.AddFilter(salespersonBranchFilter);

			var salespersonDepartmentFilter = new ModuleGuidFilter(FilterDescription.SalespersonDepartment, ModuleIDs.GlbDepartment, GetSalesPersonDepartmentQuery, Department)
			{
				MultilingualDescription = ResString.GetMultilingualString("710CE5B7-6946-4A35-B80F-3552BA08BB14", "Sales Person Department"),
				Category = FilterCategories.Organisations
			};
			filters.AddFilter(salespersonDepartmentFilter);

			return filters;
		}

		ZQuery GetOpportunityStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var opportunityStatusSubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			opportunityStatusSubQuery.AddToFilter(OrgOpportunitySchema.P8_Status, value);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddSubQuery(OrgCommissionAgreementSchema.CA0_P8, opportunityStatusSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetSalesPersonQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var salesPersonSubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			salesPersonSubQuery.AddToFilter(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, value);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddSubQuery(OrgCommissionAgreementSchema.CA0_P8, salesPersonSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));

			switch (value)
			{
				case OrgCommissionAgreementStatusList.Codes.Reversed:
					query.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					break;

				case OrgCommissionAgreementStatusList.Codes.Expired:
					query.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);
					query.AddToFilter(OrgCommissionAgreementSchema.CA0_ExpiredDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Today);
					break;

				default:
					query.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);
					query.AddToFilter(OrgCommissionAgreementSchema.CA0_ExpiredDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
					break;
			}

			return query;
		}

		ZQuery GetEntityStaffQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var entityStaffSubQuery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreementRecipient), OrgCommissionAgreementRecipientSchema.CAR_CA0);
			entityStaffSubQuery.AddToFilter(OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff, value);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddSubQuery(OrgCommissionAgreementSchema.PK, entityStaffSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetEntityOrganizationQuery(ZGuid value)
		{
			var glbCompanySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
			glbCompanySubQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, value);

			var glbBranchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			glbBranchSubQuery.AddSubQuery(GlbBranchSchema.GB_GC, glbCompanySubQuery, JoinCondition.And);

			var glbStaffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			glbStaffSubQuery.AddSubQuery(GlbStaffSchema.GS_GB_HomeBranch, glbBranchSubQuery, JoinCondition.And);

			var orgCommissionAgreementRecipientSubQuery = new ZDBOnlySubQuery(typeof(OrgCommissionAgreementRecipient), OrgCommissionAgreementRecipientSchema.CAR_CA0);
			orgCommissionAgreementRecipientSubQuery.AddSubQuery(OrgCommissionAgreementRecipientSchema.CAR_GS_NKStaff, glbStaffSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddSubQuery(OrgCommissionAgreementSchema.PK, orgCommissionAgreementRecipientSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetSalesPersonBranchQuery(ZGuid value)
		{
			var glbBranchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			glbBranchSubQuery.AddToFilter(GlbBranchSchema.PK, value);

			var glbStaffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			glbStaffSubQuery.AddSubQuery(GlbStaffSchema.GS_GB_HomeBranch, glbBranchSubQuery, JoinCondition.And);

			var salesPersonSubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			salesPersonSubQuery.AddSubQuery(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, glbStaffSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddSubQuery(OrgCommissionAgreementSchema.CA0_P8, salesPersonSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetSalesPersonDepartmentQuery(ZGuid value)
		{
			var glbDepartmentSubQuery = new ZDBOnlySubQuery(typeof(GlbDepartment), GlbDepartmentSchema.PK);
			glbDepartmentSubQuery.AddToFilter(GlbDepartmentSchema.PK, value);

			var glbStaffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			glbStaffSubQuery.AddSubQuery(GlbStaffSchema.GS_GE_HomeDepartment, glbDepartmentSubQuery, JoinCondition.And);

			var salesPersonSubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			salesPersonSubQuery.AddSubQuery(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, glbStaffSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddSubQuery(OrgCommissionAgreementSchema.CA0_P8, salesPersonSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetCompanyQuery(ZGuid value)
		{
			var glbCompanySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
			glbCompanySubQuery.AddToFilter(GlbCompanySchema.PK, value);

			var salesPersonSubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.PK);
			salesPersonSubQuery.AddSubQuery(OrgOpportunitySchema.P8_GC, glbCompanySubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));
			query.AddSubQuery(OrgCommissionAgreementSchema.CA0_P8, salesPersonSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Module Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string OpportunityId = "Opportunity Id";
			public const string OpportunityStatus = "Opportunity Status";
			public const string Customer = "Customer";
			public const string SalesPerson = "Sales Person";
			public const string AgreementStatus = "Agreement Status";
			public const string EffectiveDate = "Effective Date";
			public const string Basis = "Basis";
			public const string EntityStaff = "Entity Staff";
			public const string EntityOrganization = "Entity Organization";
			public const string SalespersonBranch = "Sales Person Branch";
			public const string SalespersonDepartment = "Sales Person Department";
			public const string Company = "Company";

			#endregion
		}

		#endregion

		#region Lookups

		#region Agreements

		OrgOpportunityCollection Opportunities => opportunities ?? (opportunities = new OrgOpportunityCollection(Factory));
		OrgOpportunityCollection opportunities;

		#endregion

		#region OpportunityStatuses

		ICodeDescriptionBoolList OpportunityStatuses => opportunityStatuses ?? (opportunityStatuses = OrganisationsDataRegistry.Instance.OpportunityStatus.Value);
		ICodeDescriptionBoolList opportunityStatuses;

		#endregion

		#region Customers

		OrgHeaderCollection Organizations => organizations ?? (organizations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organizations;

		#endregion

		#region Staff

		GlbStaffCollection Staff => staff ?? (staff = new GlbStaffCollection(Factory));
		GlbStaffCollection staff;

		#endregion

		#region Branch

		GlbBranchCollection Branch => branch ?? (branch = new GlbBranchCollection(Factory));
		GlbBranchCollection branch;

		#endregion

		#region Department

		GlbDepartmentCollection Department => department ?? (department = new GlbDepartmentCollection(Factory));
		GlbDepartmentCollection department;

		#endregion

		#region Company

		GlbCompanyCollection Company => company ?? (company = new GlbCompanyCollection(Factory));
		GlbCompanyCollection company;

		#endregion

		#region Statuses

		public ReadOnlyCodeDescriptionPairList Statuses => GetStatusCodeDescriptionPairList();

		CodeDescriptionPairList GetStatusCodeDescriptionPairList()
		{
			var statusCodeDescriptionPairList = new CodeDescriptionPairList();

			statusCodeDescriptionPairList.AddPair("A/I", ResString.GetMultilingualString("OrgCommissionAgreementStatusList|ActiveInactive", "Active/Inactive"));
			statusCodeDescriptionPairList.AddPair(OrgCommissionAgreementStatusList.Codes.Expired, OrgCommissionAgreementStatusList.Descriptions.Expired);
			statusCodeDescriptionPairList.AddPair(OrgCommissionAgreementStatusList.Codes.Reversed, OrgCommissionAgreementStatusList.Descriptions.Reversed);
			return statusCodeDescriptionPairList;
		}

		#endregion

		#region Basis

		ICodeDescriptionPairList Basis => basis ?? (basis = CommissionLookups.New(Factory).CommissionBasisType);
		ICodeDescriptionPairList basis;

		#endregion

		#endregion
	}
}
