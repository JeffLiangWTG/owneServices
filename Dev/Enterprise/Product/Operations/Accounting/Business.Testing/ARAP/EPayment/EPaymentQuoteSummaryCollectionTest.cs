using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(EPaymentQuoteSummaryCollection))]
	public class EPaymentQuoteSummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EPaymentQuoteSummaryCollection>
	{
		public void TestReloadSummaries()
		{
			var batchPoster = Factory.New<APPaymentBatchPoster>();
			batchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			var quote = GetNewQuoteFromBatchPoster(batchPoster);
			Factory.Save();

			var summaryCollection = new EPaymentQuoteSummaryCollection(batchPoster);
			AcceptQuoteGivenProviderReference(quote, "ABCDEF");
			quote.QU_Status = QuoteStatusCodes.Discarded;
			Factory.Save();

			summaryCollection.ReloadSummaries();
			AssertEquals("PreRequisite", 0, summaryCollection.Count);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var quoteInNewFactory = newFactory.Load<EPaymentQuote>(quote.PK);
			quoteInNewFactory.QU_Status = QuoteStatusCodes.Accepted;
			newFactory.Save();

			summaryCollection.ReloadSummaries(false);
			AssertEquals("Quote should not be refreshed so summary won't be added.", 0, summaryCollection.Count);

			summaryCollection.ReloadSummaries();
			AssertEquals("Quote should be refreshed to ACP status so summary can be added.", 1, summaryCollection.Count);
		}

		public void TestSummaryDoesNotSplitQuotesByProviderReference()
		{
			var batchPoster = Factory.New<APPaymentBatchPoster>();
			batchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			var quote1 = GetNewQuoteFromBatchPoster(batchPoster);
			var quote2 = GetNewQuoteFromBatchPoster(batchPoster);
			Factory.Save();

			AcceptQuoteGivenProviderReference(quote1, "ABCDEF");
			AcceptQuoteGivenProviderReference(quote2, "123450");
			Factory.Save();

			var summaryCollection = new EPaymentQuoteSummaryCollection(batchPoster);
			summaryCollection.ReloadSummaries();
			var summaryList = summaryCollection.Cast<EPaymentQuoteSummary>();
			AssertEquals("EPayment Quotes should not be split by Provider Reference", 1, summaryList.Count());
		}

		void AcceptQuoteGivenProviderReference(EPaymentQuote quote, ZString providerReference)
		{
			quote.QU_Status = QuoteStatusCodes.Accepted;
			quote.QU_ExchangeRate = 1.0M;
			quote.QU_ExchangeRateInverted = 1.0M;
			quote.QU_FromAmount = 100M;
			quote.QU_FeeAmount = 0M;
			quote.QU_ProviderReference = providerReference;
			quote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
		}

		public void TestSplitsQuotesByProviderCurrencyStatusError()
		{
			var batchPoster = Factory.New<APPaymentBatchPoster>();
			batchPoster.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			var quote1 = GetNewQuoteFromBatchPoster(batchPoster);
			var quote2 = GetNewQuoteFromBatchPoster(batchPoster);
			bool isGroup1(EPaymentQuoteSummary x) => x.ProviderCode == EPaymentProviderCodes.Codes.OFX && x.PaymentCurrencyCode == CurrencyCodes.Australia && x.Status == QuoteStatusCodes.Queued && x.ErrorMessage.IsEmpty;

			var quote3 = GetNewQuoteFromBatchPoster(batchPoster);
			var quote4 = GetNewQuoteFromBatchPoster(batchPoster);
			quote3.QU_RX_NKToCurrency = CurrencyCodes.UnitedStates;
			quote4.QU_RX_NKToCurrency = CurrencyCodes.UnitedStates;
			bool isGroup2(EPaymentQuoteSummary x) => x.ProviderCode == EPaymentProviderCodes.Codes.OFX && x.PaymentCurrencyCode == CurrencyCodes.UnitedStates && x.Status == QuoteStatusCodes.Queued && x.ErrorMessage.IsEmpty;

			var quote5 = GetNewQuoteFromBatchPoster(batchPoster);
			var quote6 = GetNewQuoteFromBatchPoster(batchPoster);
			quote5.QU_Status = QuoteStatusCodes.Failed;
			quote6.QU_Status = QuoteStatusCodes.Failed;
			bool isGroup3(EPaymentQuoteSummary x) => x.ProviderCode == EPaymentProviderCodes.Codes.OFX && x.PaymentCurrencyCode == CurrencyCodes.Australia && x.Status == QuoteStatusCodes.Failed && x.ErrorMessage.IsEmpty;

			var quote7 = GetNewQuoteFromBatchPoster(batchPoster);
			var quote8 = GetNewQuoteFromBatchPoster(batchPoster);
			quote7.QU_Status = QuoteStatusCodes.Failed;
			quote7.QU_ErrorDescription = "Some error message";
			quote8.QU_Status = QuoteStatusCodes.Failed;
			quote8.QU_ErrorDescription = "Some error message";
			bool isGroup4(EPaymentQuoteSummary x) => x.ProviderCode == EPaymentProviderCodes.Codes.OFX && x.PaymentCurrencyCode == CurrencyCodes.Australia && x.Status == QuoteStatusCodes.Failed && x.ErrorMessage == "Some error message";

			var discardedQuote = GetNewQuoteFromBatchPoster(batchPoster);
			discardedQuote.QU_Status = QuoteStatusCodes.Discarded;
			Factory.Save();

			var summaryCollection = new EPaymentQuoteSummaryCollection(batchPoster);
			var summaryList = summaryCollection.Cast<EPaymentQuoteSummary>();
			AssertEquals(4, summaryList.Count());
			AssertEquals(1, summaryList.Count(x => isGroup1(x)));
			AssertEquals(1, summaryList.Count(x => isGroup2(x)));
			AssertEquals(1, summaryList.Count(x => isGroup3(x)));
			AssertEquals(1, summaryList.Count(x => isGroup4(x)));
			AssertEquals(0, summaryList.Count(x => x.Status == QuoteStatusCodes.Discarded));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var quote1InNewFactory = newFactory.Load<EPaymentQuote>(quote1.PK);
			var quote2InNewFactory = newFactory.Load<EPaymentQuote>(quote2.PK);
			quote1InNewFactory.QU_Status = QuoteStatusCodes.Failed;
			quote2InNewFactory.QU_Status = QuoteStatusCodes.Failed;
			newFactory.Save();

			summaryList = summaryCollection.Cast<EPaymentQuoteSummary>();
			AssertEquals(4, summaryList.Count());
			AssertEquals(1, summaryList.Count(x => isGroup1(x)));
			AssertEquals(1, summaryList.Count(x => isGroup2(x)));
			AssertEquals(1, summaryList.Count(x => isGroup3(x)));
			AssertEquals(1, summaryList.Count(x => isGroup4(x)));
			AssertEquals(0, summaryList.Count(x => x.Status == QuoteStatusCodes.Discarded));

			summaryCollection.ReloadSummaries();
			summaryList = summaryCollection.Cast<EPaymentQuoteSummary>();
			AssertEquals(3, summaryList.Count());
			AssertEquals(0, summaryList.Count(x => isGroup1(x)));
			AssertEquals(1, summaryList.Count(x => isGroup2(x)));
			AssertEquals(1, summaryList.Count(x => isGroup3(x)));
			AssertEquals(1, summaryList.Count(x => isGroup4(x)));
			AssertEquals(0, summaryList.Count(x => x.Status == QuoteStatusCodes.Discarded));
		}

		EPaymentQuote GetNewQuoteFromBatchPoster(APPaymentBatchPoster batchPoster)
		{
			var payment = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			batchPoster.PaymentApprovalCollection.Add(payment);
			payment.AV_Amount = 100;
			Factory.Save();
			var quote = TestObjectCreator.CreateEPaymentQuote(payment);
			return quote;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EPaymentQuoteSummary(new[] { Factory.NewWithValidTestData<EPaymentQuote>() }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Australia, QuoteStatusCodes.Queued,  ZString.Empty);
		}

		protected override EPaymentQuoteSummaryCollection GetCollectionToTest()
		{
			return new EPaymentQuoteSummaryCollection(Factory.New<APPaymentBatchPoster>());
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
