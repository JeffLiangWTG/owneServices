#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class TransactionHeaderWithLines
	{
		public bool IsLocalCurrencyTransaction_ForTestOnly => IsLocalCurrencyTransaction;

		public void SetTransactionLinesCurrency_ForTestOnly(ZString invoiceCurrencyNK)
		{
			SetTransactionLinesCurrency(invoiceCurrencyNK);
		}

		public void SetTransactionLinesExchangeRate_ForTestOnly(ZDecimal exchangeRate)
		{
			SetTransactionLinesExchangeRate(exchangeRate);
		}

		public bool IsOverrideTaxBranchSecurityAllowed_ForTestOnly => IsOverrideTaxBranchSecurityAllowed;
	}
}

#endif
