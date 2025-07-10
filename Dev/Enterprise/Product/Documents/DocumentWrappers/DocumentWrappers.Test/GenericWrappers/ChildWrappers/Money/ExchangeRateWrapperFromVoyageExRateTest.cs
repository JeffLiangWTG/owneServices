using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ExchangeRateWrapperFromVoyageExRate))]
	sealed class ExchangeRateWrapperFromVoyageExRateTest : ExchangeRateWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			VoyageExRate exchangeRate = Factory.New<VoyageExRate>();
			ExchangeRateWrapperFromVoyageExRate emptyWrapper = new ExchangeRateWrapperFromVoyageExRate(exchangeRate, Factory);

			AssertEquals(0.00m, emptyWrapper.BuyRate);
			AssertEquals(0.00m, emptyWrapper.SellRate);
			AssertEquals(0.00m, emptyWrapper.SellRateAgent);
			AssertEquals(ZString.Empty, emptyWrapper.Currency.Code);
		}

		public void TestWrapperMappingsFull()
		{
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, "UAH");

			VoyageExRate exchangeRate = Factory.New<VoyageExRate>();
			exchangeRate.E8_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.E8_VoyageExchangeRate = 2.00m;

			ExchangeRateWrapperFromVoyageExRate fullWrapper = new ExchangeRateWrapperFromVoyageExRate(exchangeRate, Factory);

			AssertEquals(0.00m, fullWrapper.BuyRate);
			AssertEquals(2.00m, fullWrapper.SellRate);
			AssertEquals(0.00m, fullWrapper.SellRateAgent);
			AssertEquals("UAH", fullWrapper.Currency.Code);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Currency : UAH
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "UAH";

			VoyageExRate exchangeRate = Factory.New<VoyageExRate>();
			exchangeRate.E8_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.E8_VoyageExchangeRate = 2.00m;

			return new ExchangeRateWrapperFromVoyageExRate(exchangeRate, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ExchangeRateWrapperFromVoyageExRate(null, Factory);
		}
	}
}
