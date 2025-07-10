using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AmountTypeWrapperTest : DataProviderTestCase<AmountTypeWrapper>
	{
		public void TestNewOrNull()
		{
			AssertNull("When zero amount and no currency", AmountTypeWrapper.NewOrNull(0, ""));
			AssertNull("When zero amount and currency is not empty", AmountTypeWrapper.NewOrNull(0, "USD"));
			AssertNull("When not zero amount and currency is empty", AmountTypeWrapper.NewOrNull(0.2m, ""));
		}

		public void TestCurrencyId()
		{
			var wrapper = CreateWrapper(1, "");
			AssertEquals("When Currency is empty", null, wrapper);
			wrapper = CreateWrapper(1, "AAA");
			AssertEquals("When Currency is not exist", null, wrapper.CurrencyId);

			wrapper = CreateWrapper(1, "USD");
			AssertEquals("When Currency is valid", Iso3AlphaCurrencyCodeContentType.Usd, wrapper.CurrencyId);
		}

		public void TestValue()
		{
			var wrapper = CreateWrapper(1, "USD");
			AssertEquals(1M, wrapper.Value);
		}

		protected override AmountTypeWrapper GetProvider() => AmountTypeWrapper.NewOrNull(1, "USD");

		IAmountType CreateWrapper(ZDecimal invoiceAmount, ZString invoiceCurrency) => AmountTypeWrapper.NewOrNull(invoiceAmount, invoiceCurrency);
	}
}
