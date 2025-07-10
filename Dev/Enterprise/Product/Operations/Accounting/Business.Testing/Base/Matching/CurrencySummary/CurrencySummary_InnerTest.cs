using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(CurrencySummary))]
	public class CurrencySummary_InnerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitCurrencySummaryRowCollection()
		{
			ObjectCreator.CreateExchangeRate(ObjectCreator.USD, "BUY", 1.5m);
			ObjectCreator.CreateExchangeRate(ObjectCreator.CNY, "BUY", 2.5m);

			var paymentApproval1 = ObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, ObjectCreator.AUDBankAccount, ObjectCreator.AUDChequeBook);
			paymentApproval1.AV_Amount = 1000m;
			paymentApproval1.AV_RX_NKPaymentCurrency = ObjectCreator.USD.Code;
			paymentApproval1.AV_PayExRate = 1m;

			var paymentApproval2 = ObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, ObjectCreator.AUDBankAccount, ObjectCreator.AUDChequeBook);
			paymentApproval2.AV_Amount = 2000m;
			paymentApproval2.AV_RX_NKPaymentCurrency = ObjectCreator.CNY.Code;
			paymentApproval2.AV_PayExRate = 2m;

			var paymentApproval3 = ObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, ObjectCreator.AUDBankAccount, ObjectCreator.AUDChequeBook);
			paymentApproval3.AV_Amount = 3000m;
			paymentApproval3.AV_RX_NKPaymentCurrency = ObjectCreator.CNY.Code;
			paymentApproval3.AV_PayExRate = 3m;

			var paymentApproval4 = ObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, ObjectCreator.AUDBankAccount, ObjectCreator.AUDChequeBook);
			paymentApproval4.AV_Amount = 4000m;
			paymentApproval4.AV_RX_NKPaymentCurrency = ObjectCreator.CNY.Code;
			paymentApproval4.AV_PayExRate = 3m;
			Factory.Save();

			var collection = new APPaymentApprovalWithoutAuthorisationCollection(Factory);
			collection.Add(paymentApproval1);
			collection.Add(paymentApproval2);
			collection.Add(paymentApproval3);
			collection.Add(paymentApproval4);

			var summary = GetNewBusinessObject(collection);

			var summaryRowUSD = summary.SummaryRows.OfType<CurrencySummaryRow>().Single(x => x.Currency == ObjectCreator.USD.Code);
			var summaryRowCNY = summary.SummaryRows.OfType<CurrencySummaryRow>().Single(x => x.Currency == ObjectCreator.CNY.Code);
			AssertEquals(2, summary.SummaryRows.Count);

			AssertEquals(1.5m, summaryRowUSD.ExchangeRate);
			AssertEquals(1000m, summaryRowUSD.Amount);

			AssertEquals(2.5m, summaryRowCNY.ExchangeRate);
			AssertEquals(2000m + 3000m + 4000m, summaryRowCNY.Amount);

			AssertEquals(1000m, paymentApproval1.AV_Amount);
			AssertEquals(2000m, paymentApproval2.AV_Amount);
			AssertEquals(3000m, paymentApproval3.AV_Amount);
			AssertEquals(4000m, paymentApproval4.AV_Amount);

			AssertInitCurrencySummaryRowCollection(paymentApproval1, paymentApproval2, paymentApproval3, paymentApproval4, summaryRowUSD, summaryRowCNY);
		}

		protected virtual void AssertInitCurrencySummaryRowCollection(PaymentApprovalBase paymentApproval1, PaymentApprovalBase paymentApproval2, PaymentApprovalBase paymentApproval3, PaymentApprovalBase paymentApproval4, CurrencySummaryRow summaryRowUSD, CurrencySummaryRow summaryRowCNY)
		{
			AssertEquals("paymentApproval1's Exchange Rate changed", 1.5m, paymentApproval1.AV_PayExRate);
			AssertEquals("paymentApproval2's Exchange Rate changed", 2.5m, paymentApproval2.AV_PayExRate);
			AssertEquals("paymentApproval3's Exchange Rate changed", 2.5m, paymentApproval3.AV_PayExRate);
			AssertEquals("paymentApproval4's Exchange Rate changed", 2.5m, paymentApproval4.AV_PayExRate);

			AssertEquals(666.67m, paymentApproval1.AV_Calc_LocalAmount);

			AssertEquals(666.67m, summaryRowUSD.LocalAmount);
			AssertEquals(1.499993m, summaryRowUSD.AverageExRate);

			AssertEquals(800m, paymentApproval2.AV_Calc_LocalAmount);
			AssertEquals(1200m, paymentApproval3.AV_Calc_LocalAmount);
			AssertEquals(1600m, paymentApproval4.AV_Calc_LocalAmount);

			AssertEquals(800m + 1200m + 1600m, summaryRowCNY.LocalAmount);
			AssertEquals(2.5m, summaryRowCNY.ExchangeRate);
		}

		public void TestCurrencySummaryWhenARReceiptBankAccountAndAmountChange()
		{
			TestCurrencySummaryWhenReceiptOrPaymentBankAccountAndAmountChange(false, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
		}

		public void TestCurrencySummaryWhenAPReceiptBankAccountAndAmountChange()
		{
			TestCurrencySummaryWhenReceiptOrPaymentBankAccountAndAmountChange(false, ZArchitecture.Core.LedgerTypes.AccountsPayable);
		}

		public void TestCurrencySummaryWhenARPaymentBankAccountAndAmountChange_SetExchangeRateBeforeAmountIsSet()
		{
			TestCurrencySummaryWhenReceiptOrPaymentBankAccountAndAmountChange(true, ZArchitecture.Core.LedgerTypes.AccountsReceivable, false);
		}

		public void TestCurrencySummaryWhenAPPaymentBankAccountAndAmountChange_SetExchangeRateBeforeAmountIsSet()
		{
			TestCurrencySummaryWhenReceiptOrPaymentBankAccountAndAmountChange(true, ZArchitecture.Core.LedgerTypes.AccountsPayable, false);
		}

		public void TestCurrencySummaryWhenARPaymentBankAccountAndAmountChange_SetAmountBeforeExchangeRateIsSet()
		{
			TestCurrencySummaryWhenReceiptOrPaymentBankAccountAndAmountChange(true, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
		}

		public void TestCurrencySummaryWhenAPPaymentBankAccountAndAmountChange_SetAmountBeforeExchangeRateIsSet()
		{
			TestCurrencySummaryWhenReceiptOrPaymentBankAccountAndAmountChange(true, ZArchitecture.Core.LedgerTypes.AccountsPayable);
		}

		void TestCurrencySummaryWhenReceiptOrPaymentBankAccountAndAmountChange(bool isTestingPayment, ZString ledger, bool setAmountBeforeExchangeRate = true)
		{
			PaymentApprovalBase payment = null;
			ReceiptPaymentBase receipt = null;

			if (isTestingPayment)
			{
				payment = ObjectCreator.CreatePaymentApproval(ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable ? typeof(ARPaymentApprovalWithoutAuthorisation) : typeof(APPaymentApprovalWithoutAuthorisation), ZArchitecture.Core.ReceiptTypes.Cheque, ObjectCreator.AUDBankAccount, ObjectCreator.AUDChequeBook);
				payment.AV_Amount = 100m;
			}
			else
			{
				receipt = ObjectCreator.CreateReceiptOrPayment(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ledger, 100m, ObjectCreator.AUDBankAccount.PK);
				((IMatching)receipt).OSPartialPaymentAmount = receipt.AH_OSExTaxAmount;
			}

			AssertEquals("Precondition", 0, SummaryObj.SummaryRows.Count);

			if (isTestingPayment)
			{
				SummaryObj.FMatchingTransactions_ForTestOnly.Add(payment);
			}
			else
			{
				SummaryObj.FMatchingTransactions_ForTestOnly.Add(receipt);
			}

			AssertEquals("one summary row created for transaction", 1, SummaryObj.SummaryRows.Count);

			var summaryRow = SummaryObj.SummaryRows[0];
			AssertEquals(Core.Constants.CurrencyCodes.Australia, summaryRow.Currency);
			AssertEquals(100m, summaryRow.Amount);
			AssertEquals(100m, summaryRow.LocalAmount);
			AssertEquals(1m, summaryRow.AverageExRate);

			if (isTestingPayment)
			{
				payment.AV_AB = ObjectCreator.USDBankAccount.PK;
				if (setAmountBeforeExchangeRate)
				{
					payment.AV_Amount = ((IMatching)payment).OSPartialPaymentAmount = 50m;
				}
				payment.AV_PayExRate = 2m;
				if (!setAmountBeforeExchangeRate)
				{
					payment.AV_Amount = ((IMatching)payment).OSPartialPaymentAmount = 50m;
				}
				payment.AV_AK = ObjectCreator.USDChequeBook.PK;
			}
			else
			{
				receipt.AH_AB = ObjectCreator.USDBankAccount.PK;
				receipt.AH_ExchangeRate = 2m;
				receipt.AH_OSExTaxAmount = ((IMatching)receipt).OSPartialPaymentAmount = 50m;
			}

			AssertEquals(1, SummaryObj.SummaryRows.Count);
			var newSummaryRow = SummaryObj.SummaryRows[0];
			AssertNotEquals("new summary row created when currency changed.", summaryRow.PK, newSummaryRow.PK);

			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, newSummaryRow.Currency);
			AssertEquals(50m, newSummaryRow.Amount);
			AssertEquals(25m, newSummaryRow.LocalAmount);
			AssertEquals(2m, newSummaryRow.AverageExRate);
		}

		#region RowAddedWhenNewCurrencyAdded Test

		public void TestRowAddedWhenNewCurrencyAdded()
		{
			ARInvoice aRInv = GetARInvoice(GlbCompany.CurrentCompany.LocalCurrency, 2m, 100m, 90m);

			AssertEquals("There should be no currency summary rows", 0, SummaryObj.SummaryRows.Count);
			SummaryObj.FMatchingTransactions_ForTestOnly.Add(aRInv);
			AssertEquals("There should be 1 currency summary row", 1, SummaryObj.SummaryRows.Count);
			CurrencySummaryRow row = SummaryObj.SummaryRows[0];
			AssertEquals("The row should have the local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, row.Currency);
			AssertEquals("Row should have amount = 90", 90m, row.Amount);
			AssertEquals("InvoiceTotal should be 90", 90m, row.InvoiceTotal);
			AssertEquals("LocalTotal should be 45", 45m, row.LocalAmount);
		}

		public void TestTransactionsLocalAmountTotal()
		{
			var creator = new TestObjectCreator(Factory);

			var invoice1 = GetARInvoice(GlbCompany.CurrentCompany.LocalCurrency, 1m, 60m, 45m);
			SummaryObj.FMatchingTransactions_ForTestOnly.Add(invoice1);
			AssertEquals("There should be 1 currency summary row", 1, SummaryObj.SummaryRows.Count);

			var audCurrencySummaryRow = SummaryObj.SummaryRows.GetSummaryRow(creator.AUD.RX_Code);
			AssertEquals("AUD Local Total", 45m, audCurrencySummaryRow.LocalAmount);
			AssertEquals("All local Total amount", 45m, SummaryObj.TransactionsLocalAmountTotal);

			var invoice2 = GetARInvoice(GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, 75m);
			SummaryObj.FMatchingTransactions_ForTestOnly.Add(invoice2);
			AssertEquals("There should be 1 currency summary row", 1, SummaryObj.SummaryRows.Count);
			AssertEquals("AUD Local Total", 120m, audCurrencySummaryRow.LocalAmount);
			AssertEquals("All local Total amount", 120m, SummaryObj.TransactionsLocalAmountTotal);

			var invoice3 = GetARInvoice(creator.USD, 2m, 200m, 180m);
			SummaryObj.FMatchingTransactions_ForTestOnly.Add(invoice3);
			AssertEquals("There should be 2 currency summary row", 2, SummaryObj.SummaryRows.Count);

			var usdCurrencySummaryRow = SummaryObj.SummaryRows.GetSummaryRow(creator.USD.RX_Code);
			AssertEquals("AUD Local Total", 120m, audCurrencySummaryRow.LocalAmount);
			AssertEquals("USD Local Total (180/2)", 90m, usdCurrencySummaryRow.LocalAmount);
			AssertEquals("All local Total amount (120+90)", 210m, SummaryObj.TransactionsLocalAmountTotal);

			SummaryObj.FMatchingTransactions_ForTestOnly.Remove(invoice2);
			AssertEquals("AUD Local Total", 45m, audCurrencySummaryRow.LocalAmount);
			AssertEquals("USD Local Total", 90m, usdCurrencySummaryRow.LocalAmount);
			AssertEquals("All local Total amount", 135m, SummaryObj.TransactionsLocalAmountTotal);

			audCurrencySummaryRow.LocalAmount = 100m;
			usdCurrencySummaryRow.LocalAmount = 50m;
			AssertEquals("All local Total amount (100+50)", 150m, SummaryObj.TransactionsLocalAmountTotal);
		}

		#endregion

		#region RowRemovedWhenCurrencyRemoved Test

		public void TestRowRemovedWhenCurrencyRemoved()
		{
			IMatchingCollection transactions = new IMatchingCollection(Factory);

			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			APInvoice aPInv = GetAPInvoice(GlbCompany.CurrentCompany.LocalCurrency, 1m, 90m, -60m);
			transactions.Add(aPInv);

			ARInvoice aRInv = GetARInvoice(currency, 1m, 40m, 4m);
			transactions.Add(aRInv);

			CurrencySummary summary = new CurrencySummary(transactions);
			AssertEquals("There should be 2 currency summary rows", 2, summary.SummaryRows.Count);
			transactions.Remove(aPInv);
			AssertEquals("There should be one row", 1, summary.SummaryRows.Count);
		}

		#endregion

		#region AmountChangesWhenTransactionAdded Test

		public void TestAmountChangesWhenTransactionAdded()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			ARInvoice aRInv = GetARInvoice(currency, 1m, 100m, 50m);
			SummaryObj.FMatchingTransactions_ForTestOnly.Add(aRInv);

			CurrencySummaryRow summaryRow = SummaryObj.SummaryRows[0];
			AssertNotNull("Summary row should be added", summaryRow);
			AssertEquals("Amount should be 50", 50m, summaryRow.Amount);
			AssertEquals("InvoiceTotal should be 50", 50m, summaryRow.InvoiceTotal);

			APInvoice aPInv = GetAPInvoice(currency, 1m, 50m, -30m);

			SummaryObj.FMatchingTransactions_ForTestOnly.Add(aPInv);
			AssertEquals("Amount should be 20", 20m, summaryRow.Amount);
			AssertEquals("InvoiceTotal should be 20", 20m, summaryRow.InvoiceTotal);
		}

		#endregion

		#region AmountChangesWhenTransactionRemoved Test

		public void TestAmountChangesWhenTransactionRemoved()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();

			ARReceipt aRRec_Dummy = Factory.NewWithValidTestData<ARReceipt>();
			aRRec_Dummy.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aRRec_Dummy.AH_ExchangeRate = 1m;
			aRRec_Dummy.AH_OSExTaxAmount = 99999m;
			IMatching aRRec_Dummy_Match = aRRec_Dummy;
			aRRec_Dummy_Match.OSPartialPaymentAmount = -99999m;

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aRRec.AH_ExchangeRate = 1m;
			aRRec.AH_OSExTaxAmount = 100m;
			IMatching aRRec_Match = aRRec;
			aRRec_Match.OSPartialPaymentAmount = -40m;

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aPPay.AH_ExchangeRate = 1m;
			aPPay.AH_OSExTaxAmount = 50m;
			IMatching aPPay_Match = aPPay;
			aPPay_Match.OSPartialPaymentAmount = 10m;

			SummaryObj.FMatchingTransactions_ForTestOnly.Add(aRRec_Match);
			SummaryObj.FMatchingTransactions_ForTestOnly.Add(aPPay_Match);
			SummaryObj.FMatchingTransactions_ForTestOnly.Add(aRRec_Dummy_Match);

			CurrencySummaryRow row_LocalCurr = SummaryObj.SummaryRows.GetSummaryRow(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("Amount should be -30", -30m, row_LocalCurr.Amount);
			AssertEquals("PaymentTotal should be 10", 10m, row_LocalCurr.PaymentTotal);
			AssertEquals("ReceiptTotal should be -40", -40m, row_LocalCurr.ReceiptTotal);
			SummaryObj.FMatchingTransactions_ForTestOnly.Remove(aPPay);
			AssertEquals("Amount should be -40", -40m, row_LocalCurr.Amount);
			AssertEquals("PaymentTotal should be 0", 0m, row_LocalCurr.PaymentTotal);
			AssertEquals("ReceiptTotal should be -40", -40m, row_LocalCurr.ReceiptTotal);
			SummaryObj.FMatchingTransactions_ForTestOnly.Remove(aRRec);
			AssertEquals("There should only be 1 Currency Summary row", 1, SummaryObj.SummaryRows.Count);
		}

		#endregion

		#region AmountChangesWhenPartPaidAmountChanges Test

		public void TestAmountChangesWhenPartPaidAmountChanges()
		{
			ARInvoice aRInv = GetARInvoice(GlbCompany.CurrentCompany.LocalCurrency, 1m, 30m, 15m);

			SummaryObj.FMatchingTransactions_ForTestOnly.Add(aRInv);
			CurrencySummaryRow summaryRow = SummaryObj.SummaryRows.GetSummaryRow(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals("Amount should be 15", 15m, summaryRow.Amount);
			AssertEquals("InvoiceTotal should be 15", 15m, summaryRow.InvoiceTotal);
			((IMatching)aRInv).OSPartialPaymentAmount = 19m;
			AssertEquals("Amount should be 19", 19m, summaryRow.Amount);
			AssertEquals("InvoiceTotal should be 19", 19m, summaryRow.InvoiceTotal);
		}

		#endregion

		#region Implementation

		protected CurrencySummary SummaryObj
		{
			get { return CachedBusinessObject as CurrencySummary; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(new IMatchingCollection(Factory));
		}

		protected virtual CurrencySummary GetNewBusinessObject(IMatchingCollection collection)
		{
			return new CurrencySummary(collection);
		}

		ARInvoice GetARInvoice(RefCurrency currency, decimal exRate, decimal oSExTaxAmt, decimal oSPartialPayment)
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aRInv.AH_ExchangeRate = exRate;
			aRInv.AH_OSExTaxAmount = oSExTaxAmt;
			((IMatching)aRInv).OSPartialPaymentAmount = oSPartialPayment;
			return aRInv;
		}

		APInvoice GetAPInvoice(RefCurrency currency, decimal exRate, decimal oSExTaxAmt, decimal oSPartialPayment)
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aPInv.AH_ExchangeRate = exRate;
			aPInv.AH_OSExTaxAmount = oSExTaxAmt;
			((IMatching)aPInv).OSPartialPaymentAmount = oSPartialPayment;
			return aPInv;
		}

		protected TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		#endregion
	}
}
