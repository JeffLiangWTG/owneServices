using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment
{
	public class OpeningPaymentLookups : AccTransactionHeaderLookups
	{
		public OpeningPaymentLookups(OpeningPayment parent)
			: base(parent)
		{
		}

		#region BankAccounts

		public override AccBankAccountCollection BankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					ZQuery branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, GlbBranch.CurrentBranch.PK);
					branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

					ZQuery bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
					bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
					bankFilter.AddToFilter(branchFilter, JoinCondition.And);

					fBankAccounts = new AccBankAccountCollection(Factory, bankFilter);
				}
				return fBankAccounts;
			}
		}
		AccBankAccountCollection fBankAccounts;

		#endregion
	}
}
