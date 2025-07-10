using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(MoneyWrapper))]
	sealed class MoneyWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			RefCurrency jPY = RefCurrency.LoadFromCurrencyCode(Factory, "JPY");
			AssertEquals("USD.Decimals", 2, uSD.Decimals);
			AssertEquals("JPY.Decimals", 0, jPY.Decimals);

			MoneyWrapper wrapper1 = new MoneyWrapper(new Money(120.45m, uSD), Factory);
			AssertEquals("wrapper1.Amount", 120.45m, wrapper1.Amount);
			AssertEquals("wrapper1.AmountAndCurrencyCode", "120.45 USD", wrapper1.AmountAndCurrencyCode);
			AssertEquals("wrapper1.Currency.Code", "USD", wrapper1.Currency.Code);

			MoneyWrapper wrapper2 = new MoneyWrapper(new Money(12047.00m, jPY), Factory);
			AssertEquals("wrapper2.Amount", 12047.00m, wrapper2.Amount);
			AssertEquals("wrapper2.AmountAndCurrencyCode", "12,047 JPY", wrapper2.AmountAndCurrencyCode);
			AssertEquals("wrapper2.Currency.Code", "JPY", wrapper2.Currency.Code);

			MoneyWrapper wrapper3 = new MoneyWrapper(Money.Empty, Factory);
			AssertEquals("wrapper3.Amount", 0m, wrapper3.Amount);
			AssertEquals("wrapper3.AmountAndCurrencyCode", "", wrapper3.AmountAndCurrencyCode);
			AssertEquals("wrapper3.Currency.Code", "", wrapper3.Currency.Code);

			MoneyWrapper wrapper4 = new MoneyWrapper(new Money(120.00m, uSD), Factory);
			AssertEquals("wrapper4.Amount", 120.00m, wrapper4.Amount);
			AssertEquals("wrapper4.AmountAndCurrencyCode", "120.00 USD", wrapper4.AmountAndCurrencyCode);
			AssertEquals("wrapper4.Currency.Code", "USD", wrapper4.Currency.Code);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Money                           (Default Field: AmountAndCurrencyCode)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Currency                                Currency
Amount                                  Decimal
AmountAndCurrencyCode                   String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Currency : USD - United States Dollar
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			AssertEquals("USD.Decimals", 2, uSD.Decimals);

			return new MoneyWrapper(new Money(120.45m, uSD), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new MoneyWrapper(null, Factory);
		}
	}
}
