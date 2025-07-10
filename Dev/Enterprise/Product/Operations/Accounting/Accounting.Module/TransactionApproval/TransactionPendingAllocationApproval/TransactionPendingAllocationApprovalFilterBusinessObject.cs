using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class TransactionPendingAllocationApprovalFilterBusinessObject : TransactionApprovalFilterBusinessObject
	{
		readonly TransactionHeaderSubGroup TransactionSubGroup = new TransactionHeaderSubGroup();

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);

			var invoiceDateFilter = filters.AddDateFilter("Invoice Date", AccTransactionHeaderSchema.AH_InvoiceDate);
			invoiceDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|InvoiceDate", "Invoice Date");
			invoiceDateFilter.SubGroup = TransactionSubGroup;
		}

		protected override void AddReferenceFilters(ModuleFilterCollection filters)
		{
			base.AddReferenceFilters(filters);

			var transactionsFilterCategory = new FilterCategory(ResString.GetMultilingualString("31A796F0-70EE-46FB-BC9E-EE210FC8860D", "Transactions"));

			var transactionNumberfilter = filters.AddTextFilter("Transaction Number", AccTransactionHeaderSchema.AH_TransactionNum);
			transactionNumberfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|ReferenceNumber", "Reference Number");
			transactionNumberfilter.Category = transactionsFilterCategory;
			transactionNumberfilter.SubGroup = TransactionSubGroup;

			var creditorFilter = filters.AddGuidFilter("Creditor", ModuleIDs.Organisation, AccTransactionHeaderSchema.AH_OH, Creditors);
			creditorFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|Creditor", "Creditor");
			creditorFilter.Category = transactionsFilterCategory;
			creditorFilter.SubGroup = TransactionSubGroup;

			var complianceSubTypes = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (complianceSubTypes.Count > 0)
			{
				var complianceSubTypeFilter = filters.AddTextFilter("Compliance Sub Type", AccTransactionHeaderSchema.AH_ComplianceSubType, complianceSubTypes);
				complianceSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TransactionsPendingAllocationFilterControl|14F8766D-39B2-4427-96D4-4EEB77899849", "Compliance Sub Type");
				complianceSubTypeFilter.Category = transactionsFilterCategory;
				complianceSubTypeFilter.SubGroup = TransactionSubGroup;

				complianceSubTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				complianceSubTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
				complianceSubTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				complianceSubTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			}
		}

		protected override CodeDescriptionPairList GetApprovalStatusList()
		{
			return TransactionPendingAllocationApprovalRequestLookups.TransactionPendingAllocationApprovalStatusCodeDescriptionList(Factory, Env.CurrentCompany.Country.Code);
		}

		OrgHeaderCollection creditors;

		OrgHeaderCollection Creditors
		{
			get { return creditors ?? (creditors = new OrgHeaderCollection(Factory, GetAH_OHListFilter())); }
		}

		protected virtual ZQuery GetAH_OHListFilter()
		{
			var result = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		class TransactionHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(GenApprovalRequest));
				var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, subQuery, JoinCondition.And);
				return result;
			}
		}
	}
}
