using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class CurrencySummaryRowCollection : NonPersistentBusinessObjectCollection<CurrencySummaryRow>	{
		public CurrencySummaryRowCollection(IMatchingCollection transactions, CurrencySummary parent)
			: base(transactions.Factory)
		{
			fTransactions = transactions;
			Parent = parent;
			InitialiseElements();
		}

		protected override bool AllowNewCore => false;

		public CurrencySummaryRow AddNewUsingIMatching(IMatching matchingObj)
		{
			CurrencySummaryRow row = null;
			row = GetSummaryRow(matchingObj.CurrencyCode);

			if (row == null)
			{
				row = base.AddNew();

				var isFromPaymentBatch = Parent is PaymentBatchCurrencySummary;

				using (isFromPaymentBatch ? row.UpdateTransactionsExchangeRateSuspender.GetSuspender() : null)
				using (isFromPaymentBatch ? row.ShowErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDealsSuspender.GetSuspender() : null)
				{
					row.Currency = matchingObj.CurrencyCode;
				}
			}

			row.AddAmountToTotals(matchingObj.OSPartialPaymentAmount, matchingObj.LocalPartialPaymentAmount, matchingObj.TransactionType);
			matchingObj.LocalPartialPaymentAmountInfo.ValueChanged += new EventHandler(row.RefreshAmount);
			return row;
		}

		public CurrencySummaryRow GetSummaryRow(ZString currencyCode)
		{
			CurrencySummaryRow result = null;
			foreach (CurrencySummaryRow row in Elements)
			{
				if (row.Currency == currencyCode)
				{
					result = row;
					break;
				}
			}
			return result;
		}

		public void RemoveCurrencyUsingIMatching(IMatching matchingObj)
		{
			var row = GetSummaryRow(matchingObj.CurrencyCode);
			if (!fTransactions.ContainsTransactionWithSpecifiedCurrency(matchingObj.CurrencyCode) && row != null)
			{
				Remove(row);
			}
			else if (row != null)
			{
				matchingObj.LocalPartialPaymentAmountInfo.ValueChanged -= new EventHandler(row.RefreshAmount);
				row.AddAmountToTotals(-matchingObj.OSPartialPaymentAmount, -matchingObj.LocalPartialPaymentAmount, matchingObj.TransactionType);
			}
		}

		public void InitialiseElements()
		{
			if (fTransactions != null)
			{
				foreach (IMatching matchingObj in fTransactions)
				{
					var row = AddNewUsingIMatching(matchingObj);
				}
			}
		}

		public void ShowErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDeals()
		{
			OnUpdateExchangeRateOfPaymentsWithActiveDeals?.Invoke(this, new EventArgs());
		}

		public event EventHandler<EventArgs> OnUpdateExchangeRateOfPaymentsWithActiveDeals;

		public ZBool AskUpdatePaymentsExchangeRate(ZString currencyCode, ZDecimal exchangeRate)
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			var eventArgs = new UpdatePaymentsExchangeRateEventArgs(currency, exchangeRate);
			OnUpdatePaymentsExchangeRate?.Invoke(this, eventArgs);
			return eventArgs.Answer;
		}

		public event EventHandler<UpdatePaymentsExchangeRateEventArgs> OnUpdatePaymentsExchangeRate;

		public class UpdatePaymentsExchangeRateEventArgs : EventArgs
		{
			public UpdatePaymentsExchangeRateEventArgs(RefCurrency currency, ZDecimal exchangeRate)
			{
				Currency = currency;
				ExchangeRate = exchangeRate;
			}

			public ZBool Answer { get; set; }

			public RefCurrency Currency { get; }

			[DecimalPlaces(nameof(ExchangeRateDecimals))]
			public ZDecimal ExchangeRate { get; }

			int ExchangeRateDecimals => Currency?.Decimals ?? GlbCompany.CurrentCompany.GetLocalDecimals();
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CurrencySummaryRow(fTransactions);
		}

		readonly IMatchingCollection fTransactions;
		public CurrencySummary Parent;

		#endregion
	}
}
