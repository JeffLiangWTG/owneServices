using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionCurrencySummaryRowCollection : NonPersistentBusinessObjectCollection<TransactionCurrencySummaryRow>
	{
		public TransactionCurrencySummaryRowCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransactionCurrencySummaryRow AddNewByCurrency(TransactionHeader transaction)
		{
			TransactionCurrencySummaryRow row = GetSummaryRow(transaction.AH_RX_NKTransactionCurrency);
			if (row == null)
			{
				row = base.AddNew();
				row.Currency = transaction.AH_RX_NKTransactionCurrency;
			}
			row.AddAmountToTotals(transaction);

			return row;
		}

		public TransactionCurrencySummaryRow GetSummaryRow(ZString currencyCode)
		{
			TransactionCurrencySummaryRow result = null;
			foreach (TransactionCurrencySummaryRow row in Elements)
			{
				if (row.Currency == currencyCode)
				{
					result = row;
					break;
				}
			}

			return result;
		}

		protected override bool AllowNewCore => false;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransactionCurrencySummaryRow(Factory);
		}

		#endregion
	}
}
