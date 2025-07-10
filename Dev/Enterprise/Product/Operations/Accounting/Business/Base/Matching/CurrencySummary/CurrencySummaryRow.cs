using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class CurrencySummaryRow : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CurrencySummaryRow(IMatchingCollection transactions)
			: base(transactions.Factory)
		{
			fTransactions = transactions;
		}

		CurrencySummary Parent
		{
			get
			{
				if (parent == null && ParentCollections != null && ParentCollections.Count > 0)
				{
					var parentCollection = ParentCollections.First() as CurrencySummaryRowCollection;
					parent = parentCollection?.Parent;
				}
				return parent;
			}
		}
		CurrencySummary parent;

		#region AddAmountToTotals

		public void AddAmountToTotals(ZDecimal oSMatchedTotal, ZDecimal localMatchedTotal, string transactionType)
		{
			Amount += oSMatchedTotal;
			LocalAmount += localMatchedTotal;
			switch (transactionType)
			{
				case TransactionTypes.AdjustmentNote:
					AdjustmentNoteTotal += oSMatchedTotal;
					break;
				case TransactionTypes.Contra:
					ContraTotal += oSMatchedTotal;
					break;
				case TransactionTypes.CreditNote:
					CreditNoteTotal += oSMatchedTotal;
					break;
				case TransactionTypes.Discount:
					DiscountAmount += oSMatchedTotal;
					break;
				case TransactionTypes.ExchangeDifference:
					ExchangeDifferenceAmount += oSMatchedTotal;
					break;
				case TransactionTypes.Invoice:
					InvoiceTotal += oSMatchedTotal;
					break;
				case TransactionTypes.Journal:
					JournalTotal += oSMatchedTotal;
					break;
				case TransactionTypes.Overpayment:
					OverpaymentAmount += oSMatchedTotal;
					break;
				case TransactionTypes.Payment:
					PaymentTotal += oSMatchedTotal;
					break;
				case TransactionTypes.Receipt:
					ReceiptTotal += oSMatchedTotal;
					break;
				case TransactionTypes.Transfer:
					TransferTotal += oSMatchedTotal;
					break;
			}
		}

		#endregion

		public void RefreshAmount(object obj, System.EventArgs e)
		{
			IMatching matchingObj = obj as IMatching;
			if (matchingObj != null)
			{
				ReTotalAmounts(matchingObj.TransactionType);
			}
		}

		void UpdateTransactionsExchangeRate()
		{
			foreach (var transaction in fTransactions)
			{
				var provider = transaction as ICurrencySummaryDataProvider;
				if (provider != null && provider.CurrencyCode == Currency)
				{
					provider.SetExchangeRate(ExchangeRate);
				}
			}
		}

		#region Properties

		#region Currency

		internal FunctionalitySuspender ShowErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDealsSuspender
		{
			get { return showErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDealsSuspender ?? (showErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDealsSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender showErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDealsSuspender;

		FunctionalitySuspender SetAskUserToUpdateAllExchangeRateSuspender
		{
			get { return setAskUserToUpdateAllExchangeRateSuspender ?? (setAskUserToUpdateAllExchangeRateSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender setAskUserToUpdateAllExchangeRateSuspender;

		public FunctionalitySuspender UpdateTransactionsExchangeRateSuspender
		{
			get { return updateTransactionsExchangeRateSuspender ?? (updateTransactionsExchangeRateSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender updateTransactionsExchangeRateSuspender;

		[MaxLength(3)]
		[List("Currencies")]
		public ZString Currency
		{
			get { return currency; }
			set
			{
				if (value != Currency)
				{
					SetNonPersistentPropertyValue(CurrencyInfo, ref currency, value);
					using (SetAskUserToUpdateAllExchangeRateSuspender.GetSuspender())
					{
						ExchangeRate = AccountingUtils.GetExchangeRate(Currency, ExchangeRateType.Buy, ZDateTime.Now);
					}
				}
			}
		}
		ZString currency;

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal ExchangeRate
		{
			get { return exchangeRate; }
			set
			{
				if (!ShowErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDealsSuspender.IsSuspended && value != ExchangeRate && CheckIfActiveDealExistForPaymentApprovals() )
				{
					Parent.SummaryRows.ShowErrorWhenTryingToUpdateExchangeRateOfPaymentsWithActiveDeals();
				}
				else
				{
					if (value != ExchangeRate)
					{
						SetNonPersistentPropertyValue(ExchangeRateInfo, ref exchangeRate, value);
						ExchangeRateInfo.RefreshBinding();
					}

					var updateAllExchangeRate = true;
					if (!SetAskUserToUpdateAllExchangeRateSuspender.IsSuspended && Parent != null)
					{
						updateAllExchangeRate = Parent.SummaryRows.AskUpdatePaymentsExchangeRate(Currency, ExchangeRate);
					}
					if (!UpdateTransactionsExchangeRateSuspender.IsSuspended && updateAllExchangeRate)
					{
						UpdateTransactionsExchangeRate();
					}
				}
			}
		}
		ZDecimal exchangeRate;

		public ZPropertyInfo ExchangeRateInfo => GetZPropertyInfo(nameof(ExchangeRate));

		protected bool ExchangeRate_ReadOnly => Currency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		bool CheckIfActiveDealExistForPaymentApprovals()
		{
			var activeDealExist = false;
			foreach (var transaction in fTransactions)
			{
				if (transaction is ICurrencySummaryDataProvider provider && provider.CurrencyCode == Currency && provider.HasActiveDeal)
				{
					activeDealExist = true;
					break;
				}
			}
			return activeDealExist;
		}

		#endregion

		#region CurrencyDecimals

		public ZInt CurrencyDecimals
		{
			get
			{
				var accountCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency);
				return accountCurrency != null ? accountCurrency.Decimals : 2;
			}
		}

		public ZPropertyInfo CurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyDecimals)); }
		}

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();
		public int OSDecimals => CurrencyDecimals;
		public int ExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#endregion

		#region Amount

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal Amount
		{
			get { return fAmount; }
			set
			{
				fAmount = value;
				AmountInfo.RefreshBinding();
				AverageExRateInfo.RefreshBinding();
			}
		}

		ZDecimal fAmount;

		public ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(nameof(Amount)); }
		}

		#endregion

		#region AverageExRate

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public ZDecimal AverageExRate
		{
			get { return Env.CurrentCompany.ExchangeRate.GetRate(LocalAmount, Amount); }
		}

		public ZPropertyInfo AverageExRateInfo
		{
			get { return GetZPropertyInfo(nameof(AverageExRate)); }
		}

		#endregion

		#region LocalAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalAmount
		{
			get { return fLocalAmount; }
			set
			{
				fLocalAmount = value;
				LocalAmountInfo.RefreshBinding();
				AverageExRateInfo.RefreshBinding();
				Parent?.TransactionsLocalAmountTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fLocalAmount;

		public ZPropertyInfo LocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmount)); }
		}

		#endregion

		#region InvoiceTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal InvoiceTotal
		{
			get { return fInvoiceTotal; }
			set
			{
				fInvoiceTotal = value;
				InvoiceTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fInvoiceTotal;

		public ZPropertyInfo InvoiceTotalInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceTotal)); }
		}

		#endregion

		#region CreditNoteTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal CreditNoteTotal
		{
			get { return fCreditNoteTotal; }
			set
			{
				fCreditNoteTotal = value;
				CreditNoteTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fCreditNoteTotal;

		public ZPropertyInfo CreditNoteTotalInfo
		{
			get { return GetZPropertyInfo(nameof(CreditNoteTotal)); }
		}

		#endregion

		#region ReceiptTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal ReceiptTotal
		{
			get { return fReceiptTotal; }
			set
			{
				fReceiptTotal = value;
				ReceiptTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fReceiptTotal;

		public ZPropertyInfo ReceiptTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptTotal)); }
		}

		#endregion

		#region PaymentTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal PaymentTotal
		{
			get { return fPaymentTotal; }
			set
			{
				fPaymentTotal = value;
				PaymentTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fPaymentTotal;

		public ZPropertyInfo PaymentTotalInfo
		{
			get { return GetZPropertyInfo(nameof(PaymentTotal)); }
		}

		#endregion

		#region AdjustmentNoteTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal AdjustmentNoteTotal
		{
			get { return fAdjustmentNoteTotal; }
			set
			{
				fAdjustmentNoteTotal = value;
				AdjustmentNoteTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fAdjustmentNoteTotal;

		public ZPropertyInfo AdjustmentNoteTotalInfo
		{
			get { return GetZPropertyInfo(nameof(AdjustmentNoteTotal)); }
		}

		#endregion

		#region JournalTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal JournalTotal
		{
			get { return fJournalTotal; }
			set
			{
				fJournalTotal = value;
				JournalTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fJournalTotal;

		public ZPropertyInfo JournalTotalInfo
		{
			get { return GetZPropertyInfo(nameof(JournalTotal)); }
		}

		#endregion

		#region ContraTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal ContraTotal
		{
			get { return fContraTotal; }
			set
			{
				fContraTotal = value;
				ContraTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fContraTotal;

		public ZPropertyInfo ContraTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ContraTotal)); }
		}

		#endregion

		#region TransferTotal

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal TransferTotal
		{
			get { return fTransferTotal; }
			set
			{
				fTransferTotal = value;
				TransferTotalInfo.RefreshBinding();
			}
		}

		ZDecimal fTransferTotal;

		public ZPropertyInfo TransferTotalInfo
		{
			get { return GetZPropertyInfo(nameof(TransferTotal)); }
		}

		#endregion

		#region OverpaymentAmount

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OverpaymentAmount
		{
			get { return fOverpaymentAmount; }
			set
			{
				fOverpaymentAmount = value;
				OverpaymentAmountInfo.RefreshBinding();
			}
		}

		ZDecimal fOverpaymentAmount;

		public ZPropertyInfo OverpaymentAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OverpaymentAmount)); }
		}

		#endregion

		#region DiscountAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal DiscountAmount
		{
			get { return fDiscountAmount; }
			set
			{
				fDiscountAmount = value;
				DiscountAmountInfo.RefreshBinding();
			}
		}

		ZDecimal fDiscountAmount;

		public ZPropertyInfo DiscountAmountInfo
		{
			get { return GetZPropertyInfo(nameof(DiscountAmount)); }
		}

		#endregion

		#region ExchangeDifferenceAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ExchangeDifferenceAmount
		{
			get { return fExchangeDifferenceAmount; }
			set
			{
				fExchangeDifferenceAmount = value;
				ExchangeDifferenceAmountInfo.RefreshBinding();
			}
		}

		ZDecimal fExchangeDifferenceAmount;

		public ZPropertyInfo ExchangeDifferenceAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeDifferenceAmount)); }
		}

		#endregion

		#endregion

		#region Lookups

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		#endregion

		#region Implementation

		void ReTotalAmounts(string transactionType)
		{
			Amount = fTransactions.TotalOSAmountForSpecifiedCurrency(Currency);
			LocalAmount = fTransactions.TotalLocalAmountForSpecifiedCurrency(Currency);
			ZDecimal totalForType = fTransactions.TotalOSAmountForSpecifiedCurrencyAndTransactionType(Currency, transactionType);
			switch (transactionType)
			{
				case TransactionTypes.AdjustmentNote:
					AdjustmentNoteTotal = totalForType;
					break;
				case TransactionTypes.Contra:
					ContraTotal = totalForType;
					break;
				case TransactionTypes.CreditNote:
					CreditNoteTotal = totalForType;
					break;
				case TransactionTypes.Invoice:
					InvoiceTotal = totalForType;
					break;
				case TransactionTypes.Journal:
					JournalTotal = totalForType;
					break;
				case TransactionTypes.Payment:
					PaymentTotal = totalForType;
					break;
				case TransactionTypes.Receipt:
					ReceiptTotal = totalForType;
					break;
				case TransactionTypes.Transfer:
					TransferTotal = totalForType;
					break;
			}
		}

		IMatchingCollection fTransactions;

		#endregion
	}
}
