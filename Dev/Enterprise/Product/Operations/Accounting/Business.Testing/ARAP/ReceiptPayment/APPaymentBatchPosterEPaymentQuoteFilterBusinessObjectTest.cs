
using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.ZArchitecture.Business.ModuleFilterWithListAndComparisonOperators<CargoWise.Types.ZString>;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(APPaymentBatchPosterEPaymentQuoteFilterBusinessObject))]
	public class APPaymentBatchPosterEPaymentQuoteFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDiscardedQuotesFilter()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval.AV_Amount = 100;
			foreach (var status in QuoteStatusCodes.CodesList.GetAllCodes())
			{
				var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(status, approval);
			}
			Factory.Save();

			var filterObject = new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject();
			var filter = filterObject["Include Discarded Quotes"] as ModuleFlagsFilter;
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
			filter.IsActive = true;

			var quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(approval.PaymentQuotes.Where(x => x.QU_Status != QuoteStatusCodes.Discarded), quotesInFilter);

			filter.Property0 = true;
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(approval.PaymentQuotes, quotesInFilter);
		}

		public void TestProviderFilter()
		{
			AssertEquals("This test cannot be written until more providers are introduced", 1, EPaymentProviderCodes.CodesList.Count);
		}

		public void TestCurrencyFilter()
		{
			var quote1 = Factory.NewWithValidTestData<EPaymentQuote>();
			quote1.QU_RX_NKToCurrency = CurrencyCodes.UnitedStates;

			var quote2 = Factory.NewWithValidTestData<EPaymentQuote>();
			quote2.QU_RX_NKToCurrency = CurrencyCodes.Canada;
			Factory.Save();

			var filterObject = new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject();
			var filter = filterObject["Currency"] as ModuleNkFilter;
			AssertEquals(FilterVisibility.Visible, filter.Visibility);
			filter.IsActive = true;

			var quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2 }, quotesInFilter);

			filter.Property = CurrencyCodes.UnitedStates;
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new [] { quote1 }, quotesInFilter);

			filter.Property = CurrencyCodes.Chad;
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			Assert(!quotesInFilter.Any());
		}

		public void TestReceivedDateFilter()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval.AV_Amount = 100;
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, approval);
			quote1.QU_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			quote1.QU_LastResponseReceivedUtc = ZDateTime.Today;

			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, approval);
			quote2.QU_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			quote2.QU_LastResponseReceivedUtc = ZDateTime.Today.AddDays(-5);
			Factory.Save();

			var filterObject = new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject();
			var filter = filterObject["Received(Date / Time range)"] as ModuleDateFilter;
			AssertEquals(FilterVisibility.Visible, filter.Visibility);
			filter.IsActive = true;

			var quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2 }, quotesInFilter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = DateTime.Today.AddDays(-1);
			filter.Property2 = DateTime.Today.AddDays(1);
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1 }, quotesInFilter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = DateTime.Today.AddDays(2);
			filter.Property2 = DateTime.Today.AddDays(3);
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			Assert(!quotesInFilter.Any());
		}

		public void TestStatusFilter()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval.AV_Amount = 100;
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, approval);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, approval);
			Factory.Save();

			var filterObject = new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject();
			var filter = filterObject["Status"] as ModuleTextFilter;
			AssertEquals(FilterVisibility.Visible, filter.Visibility);
			filter.IsActive = true;

			var quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2 }, quotesInFilter);

			filter.Property = QuoteStatusCodes.Queued;
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1 }, quotesInFilter);

			filter.Property = QuoteStatusCodes.Error;
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			Assert(!quotesInFilter.Any());
		}

		public void TestQuoteNumberFilter()
		{
			var quote1 = Factory.NewWithValidTestData<EPaymentQuote>();
			var quote2 = Factory.NewWithValidTestData<EPaymentQuote>();
			Factory.Save();
			AssertTextFilter("Quote Number", quote1, quote2, (quote, value) => quote.QU_InternalReference = value);
		}

		public void TestProviderReferenceFilter()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval.AV_Amount = 100;
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, approval);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, approval);
			Factory.Save();
			AssertTextFilter("Provider Reference", quote1, quote2, (quote, value) => quote.QU_ProviderReference = value);
		}

		public void TestErrorMessageFilter()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval.AV_Amount = 100;
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, approval);
			var quote2 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, approval);
			Factory.Save();
			AssertTextFilter("Error Message", quote1, quote2, (quote, value) => quote.QU_ErrorDescription = value);
		}

		void AssertTextFilter(string filterName, EPaymentQuote quote1, EPaymentQuote quote2, Action<EPaymentQuote, ZString> setStringProperty)
		{
			setStringProperty(quote1, "1001");
			setStringProperty(quote2, "1002");
			Factory.Save();

			var filterObject = new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject();
			var filter = filterObject[filterName] as ModuleTextFilter;
			AssertEquals(FilterVisibility.Visible, filter.Visibility);
			filter.IsActive = true;

			var quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2 }, quotesInFilter);

			filter.ComparisonOperator = ComparisonConstants.StartsWith;
			filter.Property = "100";
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2 }, quotesInFilter);

			filter.Property = "1001";
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1 }, quotesInFilter);

			filter.Property = "10010";
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			Assert(!quotesInFilter.Any());
		}

		public void TestCreditorFilter()
		{
			var approval1 = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval1.AV_OH = TestObjectCreator.AALSHI.PK;
			approval1.AV_Amount = 100;
			var approval2 = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			approval2.AV_OH = TestObjectCreator.ABIGAS.PK;
			approval2.AV_Amount = 100;

			var quote1 = TestObjectCreator.CreateEPaymentQuote(approval1);
			var quote2 = TestObjectCreator.CreateEPaymentQuote(approval2);
			Factory.Save();

			var filterObject = new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject();
			var filter = filterObject["Creditor"] as ModuleGuidFilter;
			AssertEquals(FilterVisibility.Visible, filter.Visibility);
			filter.IsActive = true;

			var quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2 }, quotesInFilter);

			filter.Property = TestObjectCreator.AALSHI.PK;
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { quote1 }, quotesInFilter);

			filter.Property = TestObjectCreator.ActiveOrg.PK;
			quotesInFilter = Factory.Load<EPaymentQuote>(filterObject.Filter);
			Assert(!quotesInFilter.Any());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APPaymentBatchPosterEPaymentQuoteFilterBusinessObject();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
