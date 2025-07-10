using System;
using System.Collections.Generic;
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
using static Enterprise.Accounting.Business.Base.Matching.CurrencySummaryRowCollection;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(CurrencySummaryRow))]
	public class CurrencySummaryRow_InnerTest : NonPersistentBusinessObjectTestCase
	{
		#region DecimalPlaces

		public void TestZDecimalsHaveCorrectDecimalPlacesCurrencySummaryRow()
		{
			var localList = new List<string>
				{
					nameof(SummaryRow.LocalAmount),
					nameof(SummaryRow.DiscountAmount),
					nameof(SummaryRow.ExchangeDifferenceAmount)
				};

			var osList = new List<string>
				{
					nameof(SummaryRow.Amount),
					nameof(SummaryRow.InvoiceTotal),
					nameof(SummaryRow.CreditNoteTotal),
					nameof(SummaryRow.ReceiptTotal),
					nameof(SummaryRow.PaymentTotal),
					nameof(SummaryRow.AdjustmentNoteTotal),
					nameof(SummaryRow.JournalTotal),
					nameof(SummaryRow.ContraTotal),
					nameof(SummaryRow.TransferTotal),
					nameof(SummaryRow.OverpaymentAmount)
				};

			var exList = new List<string>
				{
					nameof(SummaryRow.AverageExRate)
				};

			var tester = new DecimalPlacesAttributeTester(SummaryRow);
			tester.CheckLocalCurrency(localList, nameof(SummaryRow.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(SummaryRow.OSDecimals), nameof(SummaryRow.Currency), SummaryRow);
			tester.CheckExchangeRate(exList, nameof(SummaryRow.ExchangeRateDecimals));
		}

		#endregion

		#region ReTotalAmounts Test

		public void TestReTotalAmounts()
		{
			IMatchingCollection transactions = new IMatchingCollection(Factory);

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aRInv.AH_ExchangeRate = 1m;
			aRInv.AH_OSExTaxAmount = 90m;
			((IMatching)aRInv).OSPartialPaymentAmount = 90m;

			APCreditNote aPCrd = Factory.NewWithValidTestData<APCreditNote>();
			aPCrd.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aPCrd.AH_ExchangeRate = 1m;
			aPCrd.AH_OSExTaxAmount = 40m;
			((IMatching)aPCrd).OSPartialPaymentAmount = 40m;

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aRRec.AH_ExchangeRate = 1m;
			aRRec.AH_OSExTaxAmount = 500m;
			((IMatching)aRRec).OSPartialPaymentAmount = -500m;

			SummaryRow.FTransactions_ForTestOnly.Add(aRInv);
			SummaryRow.FTransactions_ForTestOnly.Add(aPCrd);
			SummaryRow.FTransactions_ForTestOnly.Add(aRRec);

			SummaryRow.InvoiceTotal = 0m;
			SummaryRow.ReceiptTotal = 0m;
			SummaryRow.CreditNoteTotal = 0m;
			SummaryRow.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			SummaryRow.ReTotalAmounts_ForTestOnly(TransactionTypes.CreditNote);
			AssertEquals("Amount should be -370", -370m, SummaryRow.Amount);
			AssertEquals("LocalAmount should be -370", -370m, SummaryRow.LocalAmount);
			AssertEquals("CreditNoteTotal should be 40", 40m, SummaryRow.CreditNoteTotal);

			SummaryRow.ReTotalAmounts_ForTestOnly(TransactionTypes.Invoice);
			AssertEquals("InvoiceTotal should be 90", 90m, SummaryRow.InvoiceTotal);

			SummaryRow.ReTotalAmounts_ForTestOnly(TransactionTypes.Receipt);
			AssertEquals("ReceiptTotal should be -500", -500m, SummaryRow.ReceiptTotal);
		}

		#endregion

		#region AddAmountToTotals Test

		public void TestAddAmountToTotals()
		{
			SummaryRow.InvoiceTotal = 0m;
			SummaryRow.Amount = 0m;
			SummaryRow.LocalAmount = 0m;
			SummaryRow.AddAmountToTotals(40m, 30m, TransactionTypes.Invoice);
			AssertEquals("OSAmount should be 40", 40m, SummaryRow.Amount);
			AssertEquals("LocalAmount should be 30", 30m, SummaryRow.LocalAmount);
			AssertEquals("InvoiceTotal should be 40", 40m, SummaryRow.InvoiceTotal);

			SummaryRow.AddAmountToTotals(-50m, -25m, TransactionTypes.Receipt);
			AssertEquals("OSAmount should be -10", -10m, SummaryRow.Amount);
			AssertEquals("LocalAmount should be 5", 5m, SummaryRow.LocalAmount);
			AssertEquals("ReceiptTotal should be -50", -50m, SummaryRow.ReceiptTotal);
		}

		public void TestAV_RX_NKPaymentCurrency()
		{
			var creator = new TestObjectCreator(Factory);

			SummaryRow.Currency = creator.AUD.RX_Code;
			AssertEquals("exRate updated to 1", 1m, SummaryRow.ExchangeRate);

			creator.CreateUSDBuyRate(0.88m, ZDateTime.Now);
			Factory.Save();
			SummaryRow.Currency = creator.USD.RX_Code;
			AssertEquals("exRate updated to Today's rate", 0.88m, SummaryRow.ExchangeRate);

			SummaryRow.Currency = creator.EUR.RX_Code;
			AssertEquals("exRate default is zero if there is no today's rate", 0m, SummaryRow.ExchangeRate);
		}

		public void TestUpdatingCurrencySummaryRowExchangeRateUpdateRelatedTransactionsExchangeRate()
		{
			var creator = new TestObjectCreator(Factory);
			Parent.SummaryRows.OnUpdatePaymentsExchangeRate += new EventHandler<UpdatePaymentsExchangeRateEventArgs>(collection_OnUpdatePaymentsExchangeRate);

			var payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment1.AV_RX_NKPaymentCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			payment1.AV_Amount = 90m;

			var payment2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment2.AV_RX_NKPaymentCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			payment2.AV_Amount = 100m;

			var payment3 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment3.AV_RX_NKPaymentCurrency = creator.USD.RX_Code;
			payment3.AV_Amount = 110m;

			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			inv1.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			inv1.AH_OSExTaxAmount = 100m;
			((IMatching)inv1).OSPartialPaymentAmount = 100m;

			MatchingCollection.Add(payment1);
			MatchingCollection.Add(payment2);
			MatchingCollection.Add(payment3);
			MatchingCollection.Add(inv1);

			payment1.AV_PayExRate = 1m;
			payment2.AV_PayExRate = 1m;
			payment3.AV_PayExRate = 1.5m;
			inv1.AH_ExchangeRate = 2m;

			var usdSummaryRow = Parent.SummaryRows.GetSummaryRow(creator.USD.RX_Code);
			AssertEquals("AUD payment", 1m, payment1.AV_PayExRate);
			AssertEquals("AUD payment", 1m, payment2.AV_PayExRate);
			AssertEquals("USD payment", 1.5m, payment3.AV_PayExRate);
			AssertEquals("USD invoice", 2m, inv1.AH_ExchangeRate);
			usdSummaryRow.ExchangeRate = 1.3m;
			AssertEquals("No update because this payment is not USD", 1m, payment1.AV_PayExRate);
			AssertEquals("No update because this payment is not USD", 1m, payment2.AV_PayExRate);
			AssertEquals("Should be updated", 1.3m, payment3.AV_PayExRate);
			AssertEquals("No update because it is not a payment", 2m, inv1.AH_ExchangeRate);

			Parent.SummaryRows.OnUpdatePaymentsExchangeRate -= new EventHandler<UpdatePaymentsExchangeRateEventArgs>(collection_OnUpdatePaymentsExchangeRate);
		}

		void collection_OnUpdatePaymentsExchangeRate(object sender, UpdatePaymentsExchangeRateEventArgs e)
		{
			e.Answer = true;
		}

		public void TestUpdatingCurrencySummaryRowExchangeRateIsNotAllowedWhenPaymentApprovalHasActiveDeal()
		{
			var errorMessageCount = 0;
			var creator = new TestObjectCreator(Factory);
			Parent.SummaryRows.OnUpdatePaymentsExchangeRate += new EventHandler<UpdatePaymentsExchangeRateEventArgs>(collection_OnUpdatePaymentsExchangeRate);
			Parent.SummaryRows.OnUpdateExchangeRateOfPaymentsWithActiveDeals += new EventHandler<EventArgs>(collection_OnUpdateExchangeRateOfPaymentsWithActiveDeals);

			var payment = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment.AV_RX_NKPaymentCurrency = creator.USD.RX_Code;
			payment.AV_Amount = 100m;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			invoice.AH_OSExTaxAmount = 100m;

			MatchingCollection.Add(payment);
			MatchingCollection.Add(invoice);

			payment.AV_PayExRate = 2m;
			invoice.AH_ExchangeRate = 2m;

			var usdSummaryRow = Parent.SummaryRows.GetSummaryRow(creator.USD.RX_Code);
			AssertEquals("USD payment", 2m, payment.AV_PayExRate);
			AssertEquals("USD invoice", 2m, invoice.AH_ExchangeRate);

			creator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Cancelled, payment);
			Factory.Save();
			usdSummaryRow.ExchangeRate = 1.3m;

			AssertEquals("Should be updated", 1.3m, payment.AV_PayExRate);
			AssertEquals("No update because it is not a payment", 2m, invoice.AH_ExchangeRate);
			AssertEquals(0, errorMessageCount);

			var activeDeal = creator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued, payment);
			activeDeal.AED_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(10); // to ensure this is newer than previous deal.
			Factory.Save();
			usdSummaryRow.ExchangeRate = 2.7m;

			AssertEquals("Should not updated", 1.3m, payment.AV_PayExRate);
			AssertEquals("No update because it is not a payment", 2m, invoice.AH_ExchangeRate);
			AssertEquals(1, errorMessageCount);

			Parent.SummaryRows.OnUpdatePaymentsExchangeRate -= new EventHandler<UpdatePaymentsExchangeRateEventArgs>(collection_OnUpdatePaymentsExchangeRate);
			Parent.SummaryRows.OnUpdateExchangeRateOfPaymentsWithActiveDeals += new EventHandler<EventArgs>(collection_OnUpdateExchangeRateOfPaymentsWithActiveDeals);

			void collection_OnUpdateExchangeRateOfPaymentsWithActiveDeals(object sender, EventArgs e) => errorMessageCount++;
		}

		public void TestExchangeRateReadOnly()
		{
			var creator = new TestObjectCreator(Factory);

			var inv1 = Factory.NewWithValidTestData<ARInvoice>();
			inv1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			inv1.AH_ExchangeRate = 1m;
			inv1.AH_OSExTaxAmount = 90m;
			((IMatching)inv1).OSPartialPaymentAmount = 90m;

			var inv2 = Factory.NewWithValidTestData<ARInvoice>();
			inv2.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			inv2.AH_ExchangeRate = 1.5m;
			inv2.AH_OSExTaxAmount = 180m;
			((IMatching)inv2).OSPartialPaymentAmount = 180m;

			MatchingCollection.Add(inv1);
			MatchingCollection.Add(inv2);

			var usdSummaryRow = Parent.SummaryRows.GetSummaryRow(creator.USD.RX_Code);
			Assert("Foreign summary row exRate should be editable", !usdSummaryRow.ExchangeRate_ReadOnly_ForTestOnly);
			var audSummaryRow = Parent.SummaryRows.GetSummaryRow(creator.AUD.RX_Code);
			Assert("Local summary row exRate should be read only", audSummaryRow.ExchangeRate_ReadOnly_ForTestOnly);
		}

		#endregion

		#region Implementation

		protected CurrencySummaryRow SummaryRow
		{
			get { return (CurrencySummaryRow)CachedBusinessObject; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CurrencySummaryRow(MatchingCollection);
		}

		IMatchingCollection MatchingCollection
		{
			get
			{
				if (fMatchingCollection == null)
				{
					fMatchingCollection = new IMatchingCollection(Factory);
				}
				return fMatchingCollection;
			}
		}
		IMatchingCollection fMatchingCollection;

		CurrencySummary Parent
		{
			get
			{
				if (parent == null)
				{
					parent = new CurrencySummary(MatchingCollection);
				}
				return parent;
			}
		}
		CurrencySummary parent;

		#endregion
	}
}
