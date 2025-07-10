#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class CurrencySummaryRow
	{
		public bool ExchangeRate_ReadOnly_ForTestOnly => ExchangeRate_ReadOnly;

		[BusinessObjectTestExclude]
		public Transaction.IMatchingCollection FTransactions_ForTestOnly
		{
			get { return fTransactions; }
			set { fTransactions = value; }
		}

		public void ReTotalAmounts_ForTestOnly(string transactionType)
		{
			ReTotalAmounts(transactionType);
		}
	}
}

#endif
