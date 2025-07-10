using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.PIRPRulingType))]
	sealed class PIRPRulingTest : ValueObjectTestCase
	{
		public void TestIsSpecifiedTrueByDefault()
		{
			Xsd.PIRPRulingType value = new Xsd.PIRPRulingType();
			AssertEquals(false, value.IsSpecified);

			value.Number = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value.Number = "";
			value.Type = "XYZ";
			AssertEquals(true, value.IsSpecified);

			value.Number = "XQY";
			value.Type = "XYZ";
			AssertEquals(true, value.IsSpecified);
		}
	}
}
