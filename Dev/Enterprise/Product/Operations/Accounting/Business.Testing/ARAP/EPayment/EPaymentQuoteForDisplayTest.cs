using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.EPayment
{
	[TestedType(typeof(EPaymentQuoteForDisplay))]
	class EPaymentQuoteForDisplayTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EPaymentQuoteForDisplay(Factory.New<EPaymentQuote>());
		}

		public void TestDisplayBizObjectTiedToRealObject()
		{
			var quote = Factory.NewWithValidTestData<EPaymentQuote>();
			var displayQuote = new EPaymentQuoteForDisplay(quote);

			var propertyPairs = new (ZPropertyInfo quoteInfo, ZPropertyInfo displayInfo)[]
			{
				(quote.QU_ProviderCodeInfo, displayQuote.ProviderCodeInfo),
				(quote.QU_StatusInfo, displayQuote.RealQuote.QU_StatusInfo),
				(quote.QU_LastResponseReceivedUtcInfo, displayQuote.LastResponseReceivedUtcInfo),
				(quote.QU_RX_NKToCurrencyInfo, displayQuote.ToCurrencyInfo),
				(quote.QU_ToAmountInfo, displayQuote.ToAmountInfo),
				(quote.QU_FromAmountInfo, displayQuote.FromAmountInfo),
				(quote.QU_FeeAmountInfo, displayQuote.FeeAmountInfo),
				(quote.QU_InternalReferenceInfo, displayQuote.InternalReferenceInfo),
				(quote.QU_ErrorDescriptionInfo, displayQuote.ErrorDescriptionInfo),
				(quote.QU_ExchangeRateInfo, displayQuote.ExchangeRateInfo),
				(quote.QU_ExchangeRateInvertedInfo, displayQuote.ExchangeRateInvertedInfo),
				(quote.QU_ProviderReferenceInfo, displayQuote.ProviderReferenceInfo),
				(quote.QU_RX_NKFromCurrencyInfo, displayQuote.FromCurrencyInfo),
			};

			propertyPairs.ForEach(x => AssertEquals(x.quoteInfo.Value, x.displayInfo.Value));

			propertyPairs.Where(x => x.quoteInfo.PropertyType == typeof(ZString)).ForEach(x => x.quoteInfo.Value = new ZString("ABC"));
			propertyPairs.Where(x => x.quoteInfo.PropertyType == typeof(ZDateTime)).ForEach(x => x.quoteInfo.Value = ZDateTime.BrettsBirthday);
			propertyPairs.Where(x => x.quoteInfo.PropertyType == typeof(ZDecimal)).ForEach(x => x.quoteInfo.Value = (ZDecimal)((ZDecimal)x.quoteInfo.Value + new ZDecimal(50m)));

			propertyPairs.ForEach(x => AssertEquals(x.quoteInfo.Value, x.displayInfo.Value));
		}

		public void TestRealQuoteCantBeNull()
		{
			AssertExceptionThrown(typeof(NullReferenceException), () => new EPaymentQuoteForDisplay(null));
		}

		public void TestFactoryNotNull()
		{
			var quote = Factory.NewWithValidTestData<EPaymentQuote>();
			var displayQuote = new EPaymentQuoteForDisplay(quote);

			AssertNotNull(displayQuote.Factory);
		}

		public void TestCurrenciesPropertyIsAccessible()
		{
			var properties = EPaymentQuoteForDisplay.GetProperties(typeof(EPaymentQuoteForDisplay));
			AssertNotNull(properties.Find("Currencies", false));
		}
	}
}
