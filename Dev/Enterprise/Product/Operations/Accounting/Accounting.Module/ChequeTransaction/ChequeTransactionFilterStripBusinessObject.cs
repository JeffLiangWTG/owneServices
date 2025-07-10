using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class ChequeTransactionFilterStripBusinessObject : AccountingFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			ModuleFilter filter = filters.AddNumberFilter("Transaction Number", AccPaymentBatchSchema.APB_BatchNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ChequeTransactionFilter|TransactionNumber", "Transaction Number");
			filter.MaxLength = AccPaymentBatchSchema.APB_BatchNumber.MaxLength;

			filter = filters.AddTextFilter("Transaction Type", AccPaymentBatchSchema.APB_PaymentType, ChequeTransactionTypesList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ChequeTransactionFilter|TransactionType", "Transaction Type");

			filter = filters.AddDateFilter("Transaction Date", AccPaymentBatchSchema.APB_PaymentDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ChequeTransactionFilter|TransactionDate", "Transaction Date");

			filter = filters.AddGuidFilter("Debtor/Creditor", ModuleIDs.Organisation, AccPaymentBatchSchema.APB_OH_DebtorOrCreditor, OrganizationList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ChequeTransactionFilter|DebtorOrCreditor", "Debtor/Creditor");

			filter = filters.AddGuidFilter("Bank Account", ModuleIDs.AccBankAccount, AccPaymentBatchSchema.APB_AB, BankList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ChequeTransactionFilter|BankAccount", "Bank Account");

			filter = filters.AddGuidFilter("Creating Branch", ModuleIDs.GlbBranch, AccPaymentBatchSchema.APB_GB, BranchList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ChequeTransactionFilter|CreatingBranch", "Creating Branch");

			return filters;
		}

		#region OrganizationList

		public OrgHeaderCollection OrganizationList => organizationList ?? (organizationList = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organizationList;

		#endregion

		#region BankList

		public AccBankAccountCollection BankList => bankList ?? (bankList = new AccBankAccountCollection(Factory, new ZQuery(AccBankAccountSchema.AB_AccountType, SQLComparisonOperator.NotEqual, AccountTypeCodeDescriptionPairList.Codes.CSH)));
		AccBankAccountCollection bankList;

		#endregion

		#region BranchList

		public GlbBranchCollection BranchList => branchList ?? (branchList = new GlbBranchCollection(Factory));
		GlbBranchCollection branchList;

		#endregion

		#region ChequeTransactionTypesList

		public CodeDescriptionPairList ChequeTransactionTypesList => chequeTransactionTypesList ?? (chequeTransactionTypesList = new CodeDescriptionPairList(OLookUpEditType.ChequeTransactionHeader));
		CodeDescriptionPairList chequeTransactionTypesList;

		#endregion

		#region BatchStatusList

		public CodeDescriptionPairList BatchStatusList => batchStatusList ?? (batchStatusList = new CodeDescriptionPairList(OLookUpEditType.PaymentBatchStatus));
		CodeDescriptionPairList batchStatusList;

		#endregion
	}
}
