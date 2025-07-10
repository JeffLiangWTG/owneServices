using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class CurrencySummary : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CurrencySummary(IMatchingCollection transactions)
			: base(transactions.Factory)
		{
			fMatchingTransactions = transactions;
			var collection = SummaryRows;
			fMatchingTransactions.CountChanged += new CollectionCountChangedEventHandler(fMatchingTransactions_CountChanged);
			fMatchingTransactions.Cast<IMatching>().ForEach(x => x.PaymentCurrencyCodeInfo.ValueChanged += new EventHandler(Reset));
		}

		#region SummaryRows

		public CurrencySummaryRowCollection SummaryRows
		{
			get
			{
				if (fSummaryRows == null)
				{
					fSummaryRows = new CurrencySummaryRowCollection(fMatchingTransactions, this);
				}
				return fSummaryRows;
			}
		}

		CurrencySummaryRowCollection fSummaryRows;

		#endregion

		#region Implementation

		IMatchingCollection fMatchingTransactions;

		void fMatchingTransactions_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			IMatching matchingObj = e.BizObject as IMatching;
			if (matchingObj != null && !e.BizObject.IsDeleted)
			{
				if (e.ItemAdded)
				{
					SummaryRows.AddNewUsingIMatching(matchingObj);
					matchingObj.PaymentCurrencyCodeInfo.ValueChanged += new EventHandler(Reset);
				}
				else if (e.ItemRemoved)
				{
					SummaryRows.RemoveCurrencyUsingIMatching(matchingObj);
					matchingObj.PaymentCurrencyCodeInfo.ValueChanged -= new EventHandler(Reset);
				}
			}
		}

		public void Reset(object obj, EventArgs e)
		{
			SummaryRows.RemoveAll();
			SummaryRows.InitialiseElements();
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TransactionsLocalAmountTotal => SummaryRows.Cast<CurrencySummaryRow>().Sum(x => x.LocalAmount);

		public ZPropertyInfo TransactionsLocalAmountTotalInfo => GetZPropertyInfo(nameof(TransactionsLocalAmountTotal));

		int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion
	}
}
