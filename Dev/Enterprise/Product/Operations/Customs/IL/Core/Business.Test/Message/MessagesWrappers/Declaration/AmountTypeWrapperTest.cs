using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class AmountTypeWrapperTest : Customs.Business.Testing.DataProviderTestCase<AmountTypeWrapper>
	{
		public void TestNewOrNull()
		{
			AssertNull("When zero amount and no currency", AmountTypeWrapper.NewOrNull(0, ""));
			AssertNull("When zero amount and currency is not empty", AmountTypeWrapper.NewOrNull(0, "USD"));
			AssertNull("When not zero amount and currency is empty", AmountTypeWrapper.NewOrNull(0.2m, ""));
		}

		public void TestCurrencyID()
		{
			var wrapper = CreateWrapper(1, "");
			AssertEquals("When Currency is empty", null, wrapper);
			wrapper = CreateWrapper(1, "AAA");
			AssertEquals("When Currency is not exist", null, wrapper.CurrencyID);

			wrapper = CreateWrapper(1, "USD");
			AssertEquals("When Currency is valid", Iso3AlphaCurrencyCodeContentType.Usd, wrapper.CurrencyID);

			wrapper = CreateWrapper(1, "ILS");
			AssertEquals("When Currency is valid", Iso3AlphaCurrencyCodeContentType.Ils, wrapper.CurrencyID);
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
