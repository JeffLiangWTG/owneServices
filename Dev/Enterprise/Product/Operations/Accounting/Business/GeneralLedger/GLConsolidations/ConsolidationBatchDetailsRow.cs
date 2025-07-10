using CargoWise.Types;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class ConsolidationBatchDetailsRow
	{
		public ConsolidationBatchDetailsRow()
		{
		}

		public ConsolidationBatchDetailsRow(ZInt period, ZString gLAccount, ZString branchCode, ZString departmentCode,
			ZString group, ZString company, ZString org, ZString localCurrency, ZDecimal localCurrencyAmt, ZString oSCurrency, ZDecimal oSCurrencyAmt)
		{
			this.Period = period;
			this.GLAccount = gLAccount;
			this.BranchCode = branchCode;
			this.DepartmentCode = departmentCode;
			this.ConsolidationGroupCode = group;
			this.PostingCompanyCode = company;
			this.TransactionOrganisationCode = org;
			this.PostingCompanyCurrency = localCurrency;
			this.AmountInPostingCompanyCurrency = localCurrencyAmt;
			this.TransactionCurrency = oSCurrency;
			this.AmountInTransactionCurrency = oSCurrencyAmt;
		}

		public ZInt Period;
		public ZString GLAccount;
		public ZString BranchCode;
		public ZString DepartmentCode;
		public ZString ConsolidationGroupCode;
		public ZString PostingCompanyCode;
		public ZString TransactionOrganisationCode;
		public ZString PostingCompanyCurrency;
		public ZDecimal AmountInPostingCompanyCurrency;
		public ZString TransactionCurrency;
		public ZDecimal AmountInTransactionCurrency;

		public override string ToString()
		{
			return string.Join(" ", Period, GLAccount, BranchCode, DepartmentCode, ConsolidationGroupCode.PadRight(20), PostingCompanyCode, TransactionOrganisationCode.PadRight(12), PostingCompanyCurrency, AmountInPostingCompanyCurrency.ToString(2).PadLeft(8), TransactionCurrency, AmountInTransactionCurrency.ToString(2).PadLeft(8));
		}
	}
}