using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.MonetaryAmount))]
	sealed class MonetaryAmountTest : ValueObjectTestCase
	{
		public void TestIsSpecifiedTrueByDefault()
		{
			Xsd.MonetaryAmount value = new Xsd.MonetaryAmount();
			AssertEquals("IsSpecified should be true by default", true, value.IsSpecified);

			value.IsSpecified = false;
			AssertEquals("IsSpecified should allowed false when explicitly set to be", false, value.IsSpecified);
		}

		public void TestValueIsRoundedAndScaleTruncated()
		{
			Xsd.FinancialValue value = new Xsd.FinancialValue();
			value.Value = 1.12345678m;
			AssertEquals("Should round to 4 decimal places, scale must be truncated (no trailing zeros when ToString()'d)", "1.1235", value.Value.ToString());
		}
	}
}
