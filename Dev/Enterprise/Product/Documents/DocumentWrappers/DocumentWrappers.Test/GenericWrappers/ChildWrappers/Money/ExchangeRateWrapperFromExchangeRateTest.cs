using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ExchangeRateWrapperFromExchangeRate))]
	sealed class ExchangeRateWrapperFromExchangeRateTest : ExchangeRateWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			ExchangeRate exchangeRate = Factory.New<ExchangeRate>();
			ExchangeRateWrapperFromExchangeRate emptyWrapper = new ExchangeRateWrapperFromExchangeRate(exchangeRate, Factory);

			AssertEquals(0.00m, emptyWrapper.BuyRate);
			AssertEquals(0.00m, emptyWrapper.SellRate);
			AssertEquals(0.00m, emptyWrapper.SellRateAgent);
			AssertEquals(ZString.Empty, emptyWrapper.Currency.Code);
		}

		public void TestWrapperMappingsFull()
		{
			ExchangeRate exchangeRate = Factory.New<ExchangeRate>();
			exchangeRate.JF_RX_NKRateCurrency = "USD";
			exchangeRate.JF_BaseRate = 2.00m;
			exchangeRate.JF_CFXPercent = 50.00m;

			ExchangeRateWrapperFromExchangeRate fullWrapper = new ExchangeRateWrapperFromExchangeRate(exchangeRate, Factory);

			AssertEquals(2.00m, fullWrapper.BuyRate);
			AssertEquals(1.00m, fullWrapper.SellRate);
			AssertEquals(1.00m, fullWrapper.SellRateAgent);
			AssertEquals("USD", fullWrapper.Currency.Code);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Currency : USD
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "USD";

			ExchangeRate exchangeRate = Factory.New<ExchangeRate>();
			exchangeRate.JF_BaseRate = 2.00m;
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;

			return new ExchangeRateWrapperFromExchangeRate(exchangeRate, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ExchangeRateWrapperFromExchangeRate(null, Factory);
		}
	}
}
