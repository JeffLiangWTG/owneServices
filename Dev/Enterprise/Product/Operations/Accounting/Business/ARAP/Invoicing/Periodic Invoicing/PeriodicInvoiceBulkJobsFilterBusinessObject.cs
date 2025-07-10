using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceBulkJobsFilterBusinessObject : PeriodicInvoiceBaseJobFilterBusinessObject
	{
		protected override ModuleNkFilter AddCurrencyFilter(ModuleFilterCollection filterCollection)
		{
			var filter = base.AddCurrencyFilter(filterCollection);
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;

			return filter;
		}

		protected override ModuleGuidFilter AddDebtorFilter(ModuleFilterCollection filterCollection)
		{
			var filter = base.AddDebtorFilter(filterCollection);
			filter.Visibility = FilterVisibility.Visible;
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;

			return filter;
		}

		protected override ModuleTextFilter AddInvoiceTypeFilter(ModuleFilterCollection filterCollection)
		{
			var filter = base.AddInvoiceTypeFilter(filterCollection);
			filter.Visibility = FilterVisibility.Visible;
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;

			return filter;
		}

		protected override void AddARSettlementGroupFilter(ModuleFilterCollection filterCollection)
		{
			ModuleFilter filter = new ModuleGuidFilter("AR Settlement Group", ModuleIDs.Organisation, GetSettlementGroupQuery, BindingLists.OrgHeader_List);
			filter.Category = ChargeOrganizationCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ARSettlementGroup", "AR Settlement Group");
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;

			filterCollection.AddFilter(filter);
		}

		protected override void AddOrganizationBranchFilter(ModuleFilterCollection filterCollection)
		{
			ModuleFilter filter = new ModuleGuidFilter("Organization Branch", ModuleIDs.GlbBranch, GetOrgBranchFilter, BindingLists.GlbBranch_List);
			filter.Category = ChargeOrganizationCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|OrganizationBranch", "Organization Branch");
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;

			filterCollection.AddFilter(filter);
		}

		protected override ModuleTextFilter AddCurrentCompanyFilter(ModuleFilterCollection filterCollection)
		{
			var filter = base.AddCurrentCompanyFilter(filterCollection);
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;

			return filter;
		}

		protected override ModuleTextFilter AddTransactionsEligibleForPostingFilter(ModuleFilterCollection filterCollection)
		{
			var filter = base.AddTransactionsEligibleForPostingFilter(filterCollection);
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;

			return filter;
		}

		protected override void AddDeferredTransactions(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Deferred Transactions", GetDeferredTransactions);
			filter.SubGroup = FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup;
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		ZQuery GetSettlementGroupQuery(ZGuid settlementGroup)
		{
			ZDBOnlyQuery chargeSettlementFilter = new ZDBOnlyQuery(typeof(Charge));

			ZDBOnlySubQuery orgRelatedPartyQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);

			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ARSettlementGroup);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, settlementGroup);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

			chargeSettlementFilter.AddSubQuery(JobChargeSchema.JR_OH_SellAccount, orgRelatedPartyQuery, JoinCondition.And);

			return chargeSettlementFilter;
		}

		ZQuery GetOrgBranchFilter(ZGuid value)
		{
			ZDBOnlyQuery chargedebtorFilter = new ZDBOnlyQuery(typeof(Charge));
			if (value.IsValid)
			{
				chargedebtorFilter.AddSubQuery(GetOrgBranchSubQuery(value), JoinCondition.And);
			}
			return chargedebtorFilter;
		}

		ZDBOnlySubQuery GetOrgBranchSubQuery(ZGuid branchPK)
		{
			ZDBOnlySubQuery orgFilter = new ZDBOnlySubQuery(typeof(OrgHeader), JobChargeSchema.JR_OH_SellAccount);
			ZDBOnlySubQuery orgCompanyDataFilter = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			orgCompanyDataFilter.AddToFilter(OrgCompanyDataSchema.OB_GB_ControllingBranch, branchPK);
			orgFilter.AddSubQuery(orgCompanyDataFilter, JoinCondition.And);
			return orgFilter;
		}

		ZQuery GetDeferredTransactions(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Charge));

			result.AddToFilter(JobChargeSchema.JR_InvoiceType, InvoiceTypeCalculationProvider.DeferredInvoiceTypes);

			return result;
		}
	}
}
