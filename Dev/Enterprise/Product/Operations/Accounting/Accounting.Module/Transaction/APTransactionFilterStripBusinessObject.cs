using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class APTransactionFilterStripBusinessObject : TransactionFilterStripBusinessObject
	{
		public override ZString CreditorDebtorText => AccountingUtils.OrganisationFilterTypes.Creditor;

		public override MultilingualString CreditorDebtorCaption
		{
			get { return ResString.GetMultilingualString("Accounting|APTransactionFilter|Creditor", "Creditor"); }
		}

		protected override MultilingualString CreditorDebtorGroupCaption
		{
			get { return ResString.GetMultilingualString("Accounting|APTransactionFilter|CreditorGroup", "Creditor Group"); }
		}

		public override ZBool IsPayableModule
		{
			get { return ZBool.True; }
		}

		protected override ModuleIdentifier CreditorDebtorGroupModuleID
		{
			get { return ModuleIDs.OrgCreditorGroup; }
		}

		protected override ZString[] LedgersToUse
		{
			get { return new ZString[] { LedgerTypes.AccountsPayable }; }
		}

		protected virtual ZString[] TransactionTypesForSupplierCostReferenceFilter
		{
			get { return new ZString[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote }; }
		}

		public override ZBool ShouldShowRelatedClaim
		{
			get { return true; }
		}

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string RelatedTransactionDebtorCreditOnHold = "ON HOLD";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string RelatedTransactionDebtorCreditNotOnHold = "NOT ON HOLD";
		internal const string RelatedTransactionDebtorCreditAll = "ALL";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string HasRelatedTransactions = "HAS AR";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		internal const string HasNoRelatedTransactions = "NO AR";

		#endregion

		#region AddOrganisationFilters

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationFilters(filters);

			ModuleTextFilter defaultPaymentMethodFilter = filters.AddTextFilter("Default Payment Method", GetDefaultPaymentMethodQuery, new CodeDescriptionPairList(OLookUpEditType.APPaymentMethod));
			defaultPaymentMethodFilter.Category = FilterCategories.Organisations;
			defaultPaymentMethodFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|DefaultPaymentMethod", "Default Payment Method");
		}

		ZQuery GetDefaultPaymentMethodQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (!value.IsEmpty)
			{
				ZDBOnlySubQuery apAccountDetailsQuery = new ZDBOnlySubQuery(typeof(AccAPAccountDetails), AccAPAccountDetailsSchema.A1_OB);
				apAccountDetailsQuery.AddToFilter(AccAPAccountDetailsSchema.A1_PaymentMethod, value);
				if (value == OrganisationsDataRegistry.Instance.APPaymentMethod.Value)
				{
					apAccountDetailsQuery.AddToFilter(JoinCondition.Or, AccAPAccountDetailsSchema.A1_PaymentMethod, "DEF");
				}
				apAccountDetailsQuery.AddToFilter(AccAPAccountDetailsSchema.A1_IsDefaultAccount, true);

				ZDBOnlySubQuery orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
				orgCompanyDataQuery.AddSubQuery(apAccountDetailsQuery, JoinCondition.And);

				result.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgCompanyDataQuery, JoinCondition.And);
			}

			return result;
		}

		protected override void AddInvoiceAddressFilter(ModuleFilterCollection filters)
		{
			OrgWithAddressFilter orgWithAddressFilter = new OrgWithAddressFilter("Creditor and Address", GetAPOrganizationAndAddressFilter, false);
			orgWithAddressFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|CreditorAndAddress", "Creditor and Address");
			filters.AddCustomFilter(orgWithAddressFilter);
		}

		#endregion

		#region AddNumberFilters

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);
			AddTransactionNumberFilter(filters);
			AddInternalReferenceNumberFilter(filters);
			AddFlagsFilters(filters);
			AddSupplierCostReferenceFilter(filters);
			AddGovernmentAllocatedIDFilter(filters);
		}

		void AddGovernmentAllocatedIDFilter(ModuleFilterCollection filters)
		{
			var isColumnEnabled = AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.Value;

			if (isColumnEnabled)
			{
				filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.GovernmentAllocatedID, AccTransactionHeaderSchema.AH_GovernmentAllocatedID)
					.WithMaxLengthOf(AccTransactionHeaderSchema.AH_GovernmentAllocatedID)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|GovernmentAllocatedID", "Government Allocated Number");
			}
		}

		protected void AddTransactionNumberFilter(ModuleFilterCollection filters)
		{
			ModuleNumberFilter transactionNumberFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.TransactionNumber, AccTransactionHeaderSchema.AH_TransactionNum);
			transactionNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|TransactionNumber", "Transaction #");
			transactionNumberFilter.Prefix = "T";
		}

		protected virtual void AddInternalReferenceNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.InternalReferenceNumber, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|InternalReferenceNumber", "Internal Reference #");
		}

		protected void AddSupplierCostReferenceFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.SupplierCostReference, GetSupplierCostReferenceQuery).WithMaxLengthOf(AccTransactionHeaderSchema.AH_ChequeOrReference).
				MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|SupplierCostReference", "Supplier Cost Reference");
		}

		ZQuery GetSupplierCostReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, comparisonOperator, value);
			if (TransactionTypesForSupplierCostReferenceFilter != null)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypesForSupplierCostReferenceFilter);
			}
			return query;
		}

		#endregion

		#region AddDateFilters

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);

			filters.AddDateFilter(AccountingUtils.DateFilterTypes.RequisitionDate, AccTransactionHeaderSchema.AH_RequisitionDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|RequisitionDate", "Payment Requested Date");
			filters.AddDateFilter(AccountingUtils.DateFilterTypes.DocumentReceivedDate, AccTransactionHeaderSchema.AH_DocumentReceivedDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|DocumentReceivedDate", "Document Received Date");
		}

		#endregion

		#region AddStatusesFilters

		protected override void AddStatusesFilters(ModuleFilterCollection filters)
		{
			base.AddStatusesFilters(filters);

			ModuleTextFilter relatedTransactionDebtorCreditOnHoldStatusFilter = filters.AddTextFilter("Related Transaction Debtor on Hold", GetRelatedTransactionDebtorCreditOnHoldStatusQuery, RelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList);
			relatedTransactionDebtorCreditOnHoldStatusFilter.Category = FilterCategories.StatusAndFlags;
			relatedTransactionDebtorCreditOnHoldStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|RelatedTransactionDebtorCreditOnHoldStatus", "Related Transaction Debtor on Hold");

			ModuleTextFilter hasRelatedTransactionFilter = filters.AddTextFilter("Has Related Transactions", GetHasRelatedTransactionsQuery, HasRelatedTransactionsFilterOptionsList);
			hasRelatedTransactionFilter.Category = FilterCategories.StatusAndFlags;
			hasRelatedTransactionFilter.MultilingualDescription = ResString.GetMultilingualString("6FEE352C-E0D9-4AEF-A2B1-838FC031B8C1", "Has Related Transactions");

			ModuleTextFilter hasRelatedAPClaimFilter = filters.AddTextFilter("Has Related Claim", GetHasRelatedClaimQuery, HasRelatedClaimFilterOptionsList);
			hasRelatedAPClaimFilter.Category = FilterCategories.StatusAndFlags;
			hasRelatedAPClaimFilter.MultilingualDescription = ResString.GetMultilingualString("071ce275-7d9f-49fa-bef1-71b020a9f75d", "Has Related Claim");
		}

		ZQuery GetRelatedTransactionDebtorCreditOnHoldStatusQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (!value.IsEmpty)
			{
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@AR", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@AP", LedgerTypes.AccountsPayable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@INV", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
				parameters.Add("@CRD", TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);
				parameters.Add("@CreditOnHoldStatus", value == RelatedTransactionDebtorCreditOnHold ? "Y" : "N", OrgCompanyDataSchema.OB_AROnCreditHold);

				if (value == RelatedTransactionDebtorCreditNotOnHold)
				{
					result.AddFilterAndZSQLParameterCollection(GetRelatedTransactionCommandTextCommon((NoResString)@"SELECT 
	" + AccTransactionHeaderSchema.Constants.PK + (NoResString)@"
FROM
(",
@",
	MAX(CAST(" + OrgCompanyDataSchema.Constants.OB_AROnCreditHold + " AS TINYINT)) OVER (PARTITION BY PrimaryHeader." + AccTransactionHeaderSchema.Constants.PK + ") AS MaxCreditOnHold",
@"
	INNER JOIN " + OrgCompanyDataSchema.Constants.SqlSchemaName + "." + OrgCompanyDataSchema.Constants.TableName + " ON " + OrgCompanyDataSchema.Constants.OB_OH + " = SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_OH + " AND " + OrgCompanyDataSchema.Constants.OB_GC + " = SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_GC + @") InnerSelect
WHERE
	MaxCreditOnHold = @CreditOnHoldStatus)", false), parameters);
				}
				else
				{
					result.AddFilterAndZSQLParameterCollection(GetRelatedTransactionCommandTextCommon(string.Empty, string.Empty, @"
	INNER JOIN " + OrgCompanyDataSchema.Constants.SqlSchemaName + "." + OrgCompanyDataSchema.Constants.TableName + " ON " + OrgCompanyDataSchema.Constants.OB_OH + " = SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_OH + " AND " + (value != RelatedTransactionDebtorCreditAll ? OrgCompanyDataSchema.Constants.OB_AROnCreditHold + (NoResString)" = @CreditOnHoldStatus AND " : string.Empty) + OrgCompanyDataSchema.Constants.OB_GC + " = SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_GC + ")", false), parameters);
				}
			}

			return result;
		}

		ZQuery GetHasRelatedTransactionsQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (!value.IsEmpty)
			{
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@AR", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@AP", LedgerTypes.AccountsPayable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@INV", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
				parameters.Add("@CRD", TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);

				result.AddFilterAndZSQLParameterCollection(GetRelatedTransactionCommandTextCommon(string.Empty, string.Empty, ")", value == HasNoRelatedTransactions), parameters);
			}

			return result;
		}

		#endregion

		#region AddFinancialFilters

		protected override void AddFinancialFilters(ModuleFilterCollection filters)
		{
			base.AddFinancialFilters(filters);

			ModuleTextFilter paymentRequisitionStatus = filters.AddTextFilter("Payment Requisition Status", GetPaymentRequisitionStatusQuery, AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value.GetCodeDescriptionPairList());
			paymentRequisitionStatus.Category = FilterCategories.StatusAndFlags;
			paymentRequisitionStatus.MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|PaymentRequisitionStatus", "Payment Requisition Status");

			AddServiceCodeFilter(filters);
			AddNotionalWHTFilter(filters);
		}

		ZQuery GetPaymentRequisitionStatusQuery(ZString value)
		{
			return new ZQuery(AccTransactionHeaderSchema.AH_RequisitionStatus, value);
		}

		#endregion

		#region Flags

		protected virtual void AddFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleFlagsFilter selfBillingInvoiceFilter = filters.AddFlagsFilter(JobInvoicingEDocsProviderSupporter.SelfBillingInvoice, new string[] { Res.GetString("Accounting|APTransactionFilter|SelfBillingInvoiceOnly", "Self Billing Invoice Only") }, new GetFlagsQuery[] { IsSelfBillingInvoiceQuery });
			selfBillingInvoiceFilter.DefaultProperties[Res.GetString("Accounting|APTransactionFilter|SelfBillingInvoiceOnly", "Self Billing Invoice Only")] = true;
			selfBillingInvoiceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APTransactionFilter|SelfBillingInvoice", "Self Billing Invoice");
		}

		#endregion

		#region IsSelfBillingInvoiceQuery

		ZQuery IsSelfBillingInvoiceQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value)
			{
				query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCategory, Core.Constants.TransactionCategory.Codes.SelfBilling);
			}

			return query;
		}

		#endregion

		#region Lookups

		#region RelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList

		CodeDescriptionPairList fRelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList;
		public CodeDescriptionPairList RelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList
		{
			get
			{
				if (fRelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList == null)
				{
					fRelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList = new CodeDescriptionPairList();

					fRelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList.AddPair(RelatedTransactionDebtorCreditOnHold, Res.GetString("Accounting|APTransactionFilter|RelatedTransactionDebtorCreditOnHold", "Credit On Hold Debtor/s"));
					fRelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList.AddPair(RelatedTransactionDebtorCreditNotOnHold, Res.GetString("Accounting|APTransactionFilter|RelatedTransactionDebtorCreditNotOnHold", "No Credit On Hold Debtor/s"));
					fRelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList.AddPair(RelatedTransactionDebtorCreditAll, Res.GetString("Accounting|APTransactionFilter|RelatedTransactionDebtorCreditAll", "Both On Hold and Not On Hold Debtor/s"));
				}

				return fRelatedTransactionDebtorCreditOnHoldStatusFilterOptionsList;
			}
		}

		#endregion

		#region HasRelatedTransactionsFilterOptionsList

		CodeDescriptionPairList fHasRelatedTransactionsFilterOptionsList;
		public CodeDescriptionPairList HasRelatedTransactionsFilterOptionsList
		{
			get
			{
				if (fHasRelatedTransactionsFilterOptionsList == null)
				{
					fHasRelatedTransactionsFilterOptionsList = new CodeDescriptionPairList();

					fHasRelatedTransactionsFilterOptionsList.AddPair(HasRelatedTransactions, Res.GetString("Accounting|APTransactionFilter|HasRelatedTransactions", "Only transactions with related AR transaction/s"));
					fHasRelatedTransactionsFilterOptionsList.AddPair(HasNoRelatedTransactions, Res.GetString("Accounting|APTransactionFilter|HasNoRelatedTransactions", "Only transactions with no Related AR transaction"));
				}

				return fHasRelatedTransactionsFilterOptionsList;
			}
		}

		#endregion

		#endregion

		protected override SecurityCheckpoint ViewingNonLoginBranchTransactions
		{
			get { return Env.Security.PayablesViewingNonLoginBranchTransactions; }
		}
	}
}
