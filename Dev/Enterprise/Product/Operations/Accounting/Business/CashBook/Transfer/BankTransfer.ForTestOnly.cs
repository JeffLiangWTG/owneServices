#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public partial class BankTransfer
	{
		public BankTransferCharge FinanceCharge_ForTestOnly
		{
			get { return FinanceCharge; }
			set { FinanceCharge = value; }
		}

		public BankTransferFundingInfoCalculator BankTransferFundingInfoCalculator_ForTestOnly
		{
			get { return bankTransferFundingInfoCalculator; }
			set { bankTransferFundingInfoCalculator = value; }
		}

		public bool ShouldForceImbalancedTransferRowTo_ForTestOnly { get; set; }
	}
}

#endif
