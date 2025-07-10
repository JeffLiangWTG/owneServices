using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(EPaymentQuoteSummary))]
	public class EPaymentQuoteSummaryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFailsOnInvalidArguments()
		{
			AssertExceptionThrown<ArgumentException>(() => new EPaymentQuoteSummary(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));
			AssertExceptionThrown<ArgumentException>(() => new EPaymentQuoteSummary(new EPaymentQuote[] { null }, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));
			AssertExceptionThrown<ArgumentException>(() => new EPaymentQuoteSummary(Array.Empty<EPaymentQuote>(), ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));

			var quote = Factory.NewWithValidTestData<EPaymentQuote>();
			quote.QU_ErrorDescription = "Some Error message";

			AssertExceptionThrown<ArgumentException>(() => new EPaymentQuoteSummary(new EPaymentQuote[] { quote }, "invalid value", CurrencyCodes.Australia, QuoteStatusCodes.Queued, "Some Error message"));
			AssertExceptionThrown<ArgumentException>(() => new EPaymentQuoteSummary(new EPaymentQuote[] { quote }, EPaymentProviderCodes.Codes.OFX, "invalid value", QuoteStatusCodes.Queued, "Some Error message"));
			AssertExceptionThrown<ArgumentException>(() => new EPaymentQuoteSummary(new EPaymentQuote[] { quote }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Australia, "invalid value", "Some Error message"));
			AssertExceptionThrown<ArgumentException>(() => new EPaymentQuoteSummary(new EPaymentQuote[] { quote }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Australia, QuoteStatusCodes.Queued, "invalid value"));

			EPaymentQuoteSummary summary = null;
			AssertNoExceptionThrown(() => summary = new EPaymentQuoteSummary(new EPaymentQuote[] { quote }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Australia, QuoteStatusCodes.Queued, "Some Error message"));
			AssertEquals(EPaymentProviderCodes.Codes.OFX, summary.ProviderCode);
			AssertEquals(CurrencyCodes.Australia, summary.PaymentCurrencyCode);
			AssertEquals(QuoteStatusCodes.Queued, summary.Status);
			AssertEquals("Some Error message", summary.ErrorMessage);
		}

		[TestDate(2021, 2, 1)]
		public void TestSummariseRecentUpdateTime()
		{
			var summary = GetQueuedTestQuotesSummary();
			AssertEquals(ZDateTime.Empty, summary.MostRecentUpdateTime);

			summary = GetReceivedTestQuotesSummary();
			AssertEquals(ZDateTime.Today.AddDays(3), summary.MostRecentUpdateTime);
		}

		public void TestSummarisOsAmount()
		{
			var summary = GetQueuedTestQuotesSummary();
			AssertEquals(600m, summary.TotalPaymentAmount);

			summary = GetReceivedTestQuotesSummary();
			AssertEquals(600m, summary.TotalPaymentAmount);
		}

		public void TestSummariseLocalAmount()
		{
			var summary = GetQueuedTestQuotesSummary();
			AssertEquals(0m, summary.TotalFundingAmount);

			summary = GetReceivedTestQuotesSummary();
			AssertEquals(1200m, summary.TotalFundingAmount);
		}

		public void TestSummariseFeeAmount()
		{
			var summary = GetQueuedTestQuotesSummary();
			AssertEquals(0m, summary.TotalFeeAmount);

			summary = GetReceivedTestQuotesSummary();
			AssertEquals(60m, summary.TotalFeeAmount);
		}

		public void TestSummariseExchangeRates()
		{
			var summary = GetQueuedTestQuotesSummary();
			AssertEquals(0m, summary.AverageExRate);
			AssertEquals(0m, summary.AverageInverseExRate);

			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			var quotes = new List<EPaymentQuote>();
			for (int i = 1; i <= 3; i++)
			{
				var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, approval);
				quote.QU_RX_NKToCurrency = CurrencyCodes.UnitedStates;
				quote.QU_RX_NKFromCurrency = CurrencyCodes.Australia;
				quote.QU_LastResponseReceivedUtc = ZDateTime.Today.AddDays(i);
				quotes.Add(quote);
			}

			quotes[0].QU_ToAmount = 1;
			quotes[0].QU_FromAmount = 2;
			quotes[0].QU_ExchangeRate = 2;
			quotes[0].QU_ExchangeRateInverted = 0.5;

			quotes[1].QU_ToAmount = 10;
			quotes[1].QU_FromAmount = 40;
			quotes[1].QU_ExchangeRate = 4;
			quotes[1].QU_ExchangeRateInverted = 0.25;

			quotes[2].QU_ToAmount = 100;
			quotes[2].QU_FromAmount = 800;
			quotes[2].QU_ExchangeRate = 8;
			quotes[2].QU_ExchangeRateInverted = 0.125;
			summary = new EPaymentQuoteSummary(quotes, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.UnitedStates, QuoteStatusCodes.Received, ZString.Empty);
			AssertEquals(842m / 111m, summary.AverageExRate);
			AssertEquals(111m / 842m, summary.AverageInverseExRate);
		}

		public void TestDecimalPlaces()
		{
			var tnBranch = TestObjectCreator.CreateBranchWithCompany("TNDJE");
			tnBranch.Company.GC_RX_NKLocalCurrency = CurrencyCodes.Tunisia;
			Factory.Save();

			var auBranch = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			auBranch.Company.GC_RX_NKLocalCurrency = CurrencyCodes.Australia;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), tnBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var jpyQuote = Factory.NewWithValidTestData<EPaymentQuote>();
				jpyQuote.QU_RX_NKToCurrency = CurrencyCodes.Japan;
				var audQuote = Factory.NewWithValidTestData<EPaymentQuote>();
				audQuote.QU_RX_NKToCurrency = CurrencyCodes.Australia;
				var jpySummary = new EPaymentQuoteSummary(new[] { jpyQuote }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Japan, QuoteStatusCodes.Queued, ZString.Empty);
				var audSummary = new EPaymentQuoteSummary(new[] { audQuote }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Australia, QuoteStatusCodes.Queued, ZString.Empty);

				AssertEquals(3, jpySummary.FundingCurrencyDecimalPlaces);
				AssertEquals(3, audSummary.FundingCurrencyDecimalPlaces);
				AssertEquals(0, jpySummary.PaymentCurrencyDecimalPlaces);
				AssertEquals(2, audSummary.PaymentCurrencyDecimalPlaces);
				AssertEquals(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces, jpySummary.ExchangeRateDecimalPlaces);
				AssertEquals(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces, audSummary.ExchangeRateDecimalPlaces);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), auBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var jpyQuote = Factory.NewWithValidTestData<EPaymentQuote>();
				jpyQuote.QU_RX_NKToCurrency = CurrencyCodes.Japan;
				var audQuote = Factory.NewWithValidTestData<EPaymentQuote>();
				audQuote.QU_RX_NKToCurrency = CurrencyCodes.Australia;
				var jpySummary = new EPaymentQuoteSummary(new[] { jpyQuote }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Japan, QuoteStatusCodes.Queued, ZString.Empty);
				var audSummary = new EPaymentQuoteSummary(new[] { audQuote }, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Australia, QuoteStatusCodes.Queued, ZString.Empty);

				AssertEquals(2, jpySummary.FundingCurrencyDecimalPlaces);
				AssertEquals(2, audSummary.FundingCurrencyDecimalPlaces);
				AssertEquals(0, jpySummary.PaymentCurrencyDecimalPlaces);
				AssertEquals(2, audSummary.PaymentCurrencyDecimalPlaces);
				AssertEquals(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces, jpySummary.ExchangeRateDecimalPlaces);
				AssertEquals(GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces, audSummary.ExchangeRateDecimalPlaces);
			}
		}

		public void TestFundingCurrencyCode()
		{
			var auBranch = TestObjectCreator.CreateBranchWithCompany("AUSYD");
			auBranch.Company.GC_RX_NKLocalCurrency = CurrencyCodes.Australia;
			Factory.Save();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), auBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var quoteSummary = GetQueuedTestQuotesSummary();
				AssertEquals(CurrencyCodes.Australia, quoteSummary.FundingCurrencyCode);
			}

			var usBranch = TestObjectCreator.CreateBranchWithCompany("USDEMO");
			usBranch.Company.GC_RX_NKLocalCurrency = CurrencyCodes.UnitedStates;
			Factory.Save();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), usBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var quoteSummary = GetQueuedTestQuotesSummary();
				AssertEquals(CurrencyCodes.UnitedStates, quoteSummary.FundingCurrencyCode);
			}
		}

		EPaymentQuoteSummary GetQueuedTestQuotesSummary()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			var quotes = new List<EPaymentQuote>();
			for (int i = 1; i <= 3; i++)
			{
				var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Queued, approval);
				quote.QU_ToAmount = i * 100;
				quotes.Add(quote);
			}

			return new EPaymentQuoteSummary(quotes, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.Australia, QuoteStatusCodes.Queued, ZString.Empty);
		}

		EPaymentQuoteSummary GetReceivedTestQuotesSummary()
		{
			var approval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EFT, TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			var quotes = new List<EPaymentQuote>();
			for (int i = 1; i <= 3; i++)
			{
				var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, approval);
				quote.QU_RX_NKToCurrency = CurrencyCodes.UnitedStates;
				quote.QU_RX_NKFromCurrency = CurrencyCodes.Australia;
				quote.QU_ToAmount = i * 100;
				quote.QU_FromAmount = i * 200;
				quote.QU_FeeAmount = i * 10;
				quote.QU_ExchangeRate = 2;
				quote.QU_ExchangeRateInverted = 0.5;
				quote.QU_LastResponseReceivedUtc = ZDateTime.Today.AddDays(i);
				quotes.Add(quote);
			}

			return new EPaymentQuoteSummary(quotes, EPaymentProviderCodes.Codes.OFX, CurrencyCodes.UnitedStates, QuoteStatusCodes.Received, ZString.Empty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetQueuedTestQuotesSummary();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
