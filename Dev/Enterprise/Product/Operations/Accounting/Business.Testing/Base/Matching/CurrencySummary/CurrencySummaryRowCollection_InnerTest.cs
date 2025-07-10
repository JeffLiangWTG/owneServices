using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Accounting.Business.Base.Matching.CurrencySummaryRowCollection;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(CurrencySummaryRowCollection))]
	public class CurrencySummaryRowCollection_InnerTest : NonPersistentBusinessObjectCollectionTestCase<CurrencySummaryRowCollection>
	{
		#region AddNewUsingIMatching Test

		public void TestAddNewUsingIMatching()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();
			CurrencySummaryRow row_Curr1 = SummaryRowCollection.AddNew();
			row_Curr1.Currency = currency1.RX_Code;

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = currency2.RX_Code;
			aRInv.AH_ExchangeRate = 1m;
			aRInv.AH_OSExTaxAmount = 300m;
			((IMatching)aRInv).OSPartialPaymentAmount = 300m;

			AssertEquals("Precondition: should be 1 element in the collection", 1, SummaryRowCollection.Count);
			SummaryRowCollection.AddNewUsingIMatching(aRInv);
			AssertEquals("There should be 2 elements in the collection", 2, SummaryRowCollection.Count);
			CurrencySummaryRow row_Curr2 = SummaryRowCollection.GetSummaryRow(currency2.RX_Code);
			AssertEquals("InvoiceTotal should be 300", 300m, row_Curr2.InvoiceTotal);

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_RX_NKTransactionCurrency = currency1.RX_Code;
			aPInv.AH_ExchangeRate = 1m;
			aPInv.AH_OSExTaxAmount = 400m;
			((IMatching)aPInv).OSPartialPaymentAmount = -200m;

			SummaryRowCollection.AddNewUsingIMatching(aPInv);
			AssertEquals("There should still be 2 elements in the collection", 2, SummaryRowCollection.Count);
			AssertEquals("InvoiceTotal should be -200", -200m, row_Curr1.InvoiceTotal);
		}

		public void TestUpdatePaymentsExchangeRateEvent()
		{
			SummaryRowCollection.OnUpdatePaymentsExchangeRate += new EventHandler<UpdatePaymentsExchangeRateEventArgs>(collection_OnUpdatePaymentsExchangeRate);
			IsOnUpdatePaymentsExchangeRateRaised = false;
			SummaryRowCollection.AskUpdatePaymentsExchangeRate("USD", 1.55m);
			Assert("OnUpdatePaymentsExchangeRate event should be raised.", IsOnUpdatePaymentsExchangeRateRaised);
			SummaryRowCollection.OnUpdatePaymentsExchangeRate -= new EventHandler<UpdatePaymentsExchangeRateEventArgs>(collection_OnUpdatePaymentsExchangeRate);
		}

		void collection_OnUpdatePaymentsExchangeRate(object sender, UpdatePaymentsExchangeRateEventArgs e)
		{
			IsOnUpdatePaymentsExchangeRateRaised = true;
		}

		bool IsOnUpdatePaymentsExchangeRateRaised;

		public void TestNotAllowToAddNew()
		{
			Assert(!SummaryRowCollection.AllowNew);
		}

		#endregion

		#region GetSummaryRow Test

		public void TestGetSummaryRow()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();
			CurrencySummaryRow row_Curr1 = SummaryRowCollection.AddNew();
			row_Curr1.Currency = currency1.RX_Code;

			AssertNull("No group for Currency2", SummaryRowCollection.GetSummaryRow(currency2.RX_Code));
			CurrencySummaryRow rowFound = SummaryRowCollection.GetSummaryRow(currency1.RX_Code);
			AssertNotNull("Currency1 has a group", rowFound);
			AssertEquals("Should be the group for Currency1", row_Curr1, rowFound);
		}

		#endregion

		#region RemoveCurrencyUsingIMatching Test

		public void TestRemoveCurrencyUsingIMatching()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = currency1.RX_Code;
			aRInv.AH_ExchangeRate = 1m;
			aRInv.AH_OSExTaxAmount = 90m;
			((IMatching)aRInv).OSPartialPaymentAmount = 90m;

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_RX_NKTransactionCurrency = currency1.RX_Code;
			aPInv.AH_ExchangeRate = 1m;
			aPInv.AH_OSExTaxAmount = 70m;
			((IMatching)aPInv).OSPartialPaymentAmount = -70m;

			APCreditNote aPCrd = Factory.NewWithValidTestData<APCreditNote>();
			aPCrd.AH_RX_NKTransactionCurrency = currency2.RX_Code;
			aPCrd.AH_ExchangeRate = 1m;
			aPCrd.AH_OSExTaxAmount = 50m;
			((IMatching)aPCrd).OSPartialPaymentAmount = 50m;

			SummaryRowCollection.AddNewUsingIMatching(aRInv);
			SummaryRowCollection.AddNewUsingIMatching(aPInv);
			SummaryRowCollection.AddNewUsingIMatching(aPCrd);

			MatchingCollection.Add(aRInv);
			MatchingCollection.Add(aPInv);
			MatchingCollection.Add(aPCrd);

			AssertEquals("There should be 2 summary rows", 2, SummaryRowCollection.Count);
			MatchingCollection.Remove(aPCrd);
			SummaryRowCollection.RemoveCurrencyUsingIMatching(aPCrd);
			AssertEquals("There should be 1 summary row", 1, SummaryRowCollection.Count);
			AssertEquals("The summary row should be for Currency1", currency1.RX_Code, SummaryRowCollection[0].Currency);

			MatchingCollection.Remove(aRInv);
			SummaryRowCollection.RemoveCurrencyUsingIMatching(aRInv);
			AssertEquals("There should still be 1 summary row", 1, SummaryRowCollection.Count);
			AssertEquals("InvoiceTotal for Curr1 should be -70", -70m, SummaryRowCollection[0].InvoiceTotal);

			MatchingCollection.Remove(aPInv);
			SummaryRowCollection.RemoveCurrencyUsingIMatching(aPInv);
			AssertEquals("There should be no summary rows", 0, SummaryRowCollection.Count);
		}

		#endregion

		#region InitialiseElements Test

		public void TestInitialiseElements()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			MatchingCollection.Add(aRInv);
			MatchingCollection.Add(aPInv);

			AssertEquals("There should be 1 summary row", 1, SummaryRowCollection.Count);
		}

		#endregion

		protected CurrencySummaryRowCollection SummaryRowCollection
		{
			get { return Collection; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CurrencySummaryRow(MatchingCollection);
		}

		protected override CurrencySummaryRowCollection GetCollectionToTest()
		{
			return new CurrencySummaryRowCollection(MatchingCollection, Parent);
		}

		#region MatchingCollection

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
