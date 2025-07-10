using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Transaction
{
	public class TransactionsPendingAllocationFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			SetActiveStatusFilter(isCancelledColumn: null, reverseBools: false);

			var filters = new ModuleFilterCollection();

			filters.AddDateFilter("Invoice Date", AccTransactionHeaderSchema.AH_InvoiceDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|InvoiceDate", "Invoice Date");
			filters.AddDateFilter("Due Date", AccTransactionHeaderSchema.AH_DueDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|DueDate", "Due Date");
			filters.AddDateFilter("Post Date", AccTransactionHeaderSchema.AH_PostDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|PostDate", "Post Date");
			filters.AddDateFilter("Document Received Date", AccTransactionHeaderSchema.AH_DocumentReceivedDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|DocumentReceivedDate", "Document Received Date");

			var transactionsFilterCategory = new FilterCategory(ResString.GetMultilingualString("31A796F0-70EE-46FB-BC9E-EE210FC8860D", "Transactions"));

			ModuleFilter filter = filters.AddNumberFilter("Transaction Number", AccTransactionHeaderSchema.AH_TransactionNum);
			filter.Category = transactionsFilterCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|TransactionNumber", "Transaction Number");

			filter = filters.AddGuidFilter("Creditor", ModuleIDs.Organisation, AccTransactionHeaderSchema.AH_OH, Organisations);
			filter.Category = transactionsFilterCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|Creditor", "Creditor");

			var orgWithAddressFilter = new OrgWithAddressFilter("Creditor and Address", GetAPOrganizationAndAddressFilter, false);
			orgWithAddressFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|CreditorAndAddress", "Creditor and Address");
			orgWithAddressFilter.Category = transactionsFilterCategory;
			filters.AddCustomFilter(orgWithAddressFilter);

			filter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, Departments);
			filter.Category = transactionsFilterCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|Department", "Department");

			filter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, Branches);
			filter.Category = transactionsFilterCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|Branch", "Branch");

			filter = filters.AddNkFilter("Currency", AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, Currencies);
			filter.Category = transactionsFilterCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionsPendingAllocationFilter|Currency", "Currency");

			AddComplianceSubTypeFilter(filters, transactionsFilterCategory);

			AddApprovalStatusFilter(filters);

			AddBranchManagementCodeFilter(filters);

			return filters;
		}

		void AddApprovalStatusFilter(ModuleFilterCollection filters)
		{
			var approvalStatusFilter = filters.AddTextFilter("Approval Status", ViewMostRecentGenApprovalRequestSchema.XP_ApprovalStatus, TransactionPendingAllocationApprovalRequestLookups.TransactionPendingAllocationApprovalStatusCodeDescriptionList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			approvalStatusFilter.Category = FilterCategories.StatusAndFlags;
			approvalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|ApprovalStatus", "Approval Status");
			approvalStatusFilter.SubGroup = new ApprovalStatusSubGroup();

			RemoveNonLookupOperators(approvalStatusFilter);
		}

		void AddComplianceSubTypeFilter(ModuleFilterCollection filters, FilterCategory category)
		{
			var complianceSubTypes = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			if (complianceSubTypes.Count > 0)
			{
				var complianceSubTypeFilter = filters.AddTextFilter("Compliance Sub Type", AccTransactionHeaderSchema.AH_ComplianceSubType, complianceSubTypes);
				complianceSubTypeFilter.Category = category;
				complianceSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("TransactionsPendingAllocationFilterControl|14F8766D-39B2-4427-96D4-4EEB77899849", "Compliance Sub Type");

				RemoveNonLookupOperators(complianceSubTypeFilter);
			}
		}

		static void RemoveNonLookupOperators(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
		}

		public GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		public GlbDepartmentCollection Departments
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrganisationsFindBoxCollection(Factory)); }
		}

		OrganisationsFindBoxCollection organisations;

		class ApprovalStatusSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				var subQuery = new ZDBOnlySubQuery(typeof(ViewMostRecentGenApprovalRequest), ViewMostRecentGenApprovalRequestSchema.XP_ParentID);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
		}
	}
}
