using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.FinancialValue))]
	sealed class FinancialValueTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.FinancialValue value = null;
			value = new Xsd.FinancialValueCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.FinancialValue value = new Xsd.FinancialValue();
			AssertEquals("Should not be specified by default", false, value.IsSpecified);

			value.Value = 1;
			value.CurrencyCode = null;
			AssertEquals("Should be specified when a financial value is set", true, value.IsSpecified);

			value.Value = 0;
			value.CurrencyCode = "AUD";
			AssertEquals("Should be specified when a currency is set", true, value.IsSpecified);

			value.IsSpecified = false;
			AssertEquals("IsSpecified should be false when set explicitly to false", false, value.IsSpecified);
		}

		public void TestFromAmountAndCurrency()
		{
			RefCurrency aUD = Factory.LoadFromNaturalKey<RefCurrency>(ZArchitecture.Schema.RefCurrencySchema.RX_Code, "AUD");
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(ZArchitecture.Schema.RefCurrencySchema.RX_Code, "USD");

			Xsd.FinancialValue @int = Xsd.FinancialValue.FromAmountAndCurrency(new ZInt(12), aUD);
			AssertEquals("value", @int.Value, 12m);
			AssertEquals("Currency", @int.CurrencyCode, "AUD");

			Xsd.FinancialValue dec = Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(12.34), uSD);
			AssertEquals("value", dec.Value, 12.34m);
			AssertEquals("Currency", dec.CurrencyCode, "USD");

			AssertEquals("no value or currency should return null", null, Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(0), null));
			AssertEquals("no currency should return null", null, Xsd.FinancialValue.FromAmountAndCurrency(new ZDecimal(10), null));
		}

		public void TestFromAmountAndCurrencyCode()
		{
			Xsd.FinancialValue @int = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZInt(12), "AUD");
			AssertEquals("value", @int.Value, 12m);
			AssertEquals("Currency", @int.CurrencyCode, "AUD");

			Xsd.FinancialValue dec = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(12.34), "USD");
			AssertEquals("value", dec.Value, 12.34m);
			AssertEquals("Currency", dec.CurrencyCode, "USD");

			AssertEquals("no value or currency should return null", null, Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZInt(0), null));
			AssertEquals("no value currency should return null", null, Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(0), null));

			Xsd.FinancialValue nullCurrencyValue = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(12.34), null);
			AssertEquals("value", dec.Value, 12.34m);
			AssertEquals("Currency", null, null);
		}

		public void TestFromAmountAndCurrencyCode_PopulateCurrencyEvenThoughAmountIsZero()
		{
			Xsd.FinancialValue @int = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZInt(0), "AUD");
			AssertEquals("value", @int.Value, 0m);
			AssertEquals("Currency", @int.CurrencyCode, "AUD");
		}
	}
}
