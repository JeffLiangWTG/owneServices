using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.DimensionValue))]
	sealed class DimensionValueTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.DimensionValue value = null;
			value = new Xsd.DimensionValueCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.DimensionValue dimensionValue = new Xsd.DimensionValue();
			AssertEquals("DimensionValue should NOT be specified by default", false, dimensionValue.IsSpecified);

			dimensionValue.Value = 1;
			dimensionValue.DimensionType = null;
			AssertEquals("DimensionValue should be specified unless explicitly not specified", true, dimensionValue.IsSpecified);

			dimensionValue.Value = 0;
			AssertEquals("DimensionValue should be specified even if there is a zero value", true, dimensionValue.IsSpecified);

			dimensionValue.IsSpecified = false;
			AssertEquals("IsSpecified=false when explicitly set to false", false, dimensionValue.IsSpecified);
		}

		public void TestFromAmountAndUnit()
		{
			Xsd.DimensionValue @int = Xsd.DimensionValue.FromAmountAndUnit(new ZInt(12), "KG");
			AssertEquals("value", 12m, @int.Value);
			AssertEquals("unit", "KG", @int.DimensionType);

			Xsd.DimensionValue dec = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(12.34), "M3");
			AssertEquals("value", 12.34m, dec.Value);
			AssertEquals("unit", "M3", dec.DimensionType);

			dec = Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(12.34), null);
			AssertEquals("value", 12.34m, dec.Value);
			AssertEquals("unit", true, dec.DimensionType.IsEmpty);

			AssertNotNull("Always return a value", Xsd.DimensionValue.FromAmountAndUnit(new ZInt(0), "KG"));
			AssertNotNull("Always return a value", Xsd.DimensionValue.FromAmountAndUnit(new ZDecimal(0), "KG"));
		}
	}
}
