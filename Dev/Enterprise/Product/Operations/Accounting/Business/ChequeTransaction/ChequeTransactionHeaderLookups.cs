using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ChequeTransaction
{
	public class ChequeTransactionHeaderLookups : AccPaymentBatchLookups
	{
		public ChequeTransactionHeaderLookups(AutoAccPaymentBatch parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList PaymentTypeList => paymentMethods ??= new CodeDescriptionPairList(OLookUpEditType.ChequeTransactionHeader);
		CodeDescriptionPairList paymentMethods;

		#region Bank Accounts

		public override AccBankAccountCollection BankAccounts =>
			fBankAccounts ??= new AccBankAccountCollection(Factory,
				GetBankAccountsQuery(
					new ZQuery(AccBankAccountSchema.AB_AccountType, SQLComparisonOperator.NotEqual, AccountTypeCodeDescriptionPairList.Codes.CSH)));
		AccBankAccountCollection fBankAccounts;

		public override AccBankAccountCollection FundingBankAccounts =>
			fFundingBankAccounts ??= new AccBankAccountCollection(Factory,
				GetBankAccountsQuery(
					new ZQuery(AccBankAccountSchema.AB_AccountType, SQLComparisonOperator.Equal, AccountTypeCodeDescriptionPairList.Codes.CSH)));
		AccBankAccountCollection fFundingBankAccounts;

		ZQuery GetBankAccountsQuery(ZQuery additionalFilter = null)
		{
			var parent = (ChequeTransactionHeader)Parent;
			var branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, parent.Branch.PK);
			branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

			var bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, parent.Company.PK);
			bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
			bankFilter.AddToFilter(branchFilter, JoinCondition.And);
			if (additionalFilter != null)
			{
				bankFilter.AddToFilter(additionalFilter, JoinCondition.And);
			}

			return bankFilter;
		}

		#endregion
	}
}
