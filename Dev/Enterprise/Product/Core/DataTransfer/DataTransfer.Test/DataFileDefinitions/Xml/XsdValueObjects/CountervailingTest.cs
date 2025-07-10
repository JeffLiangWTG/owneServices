using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Countervailing))]
	sealed class CountervailingTest : ValueObjectTestCase
	{
		public void TestIsSpecifiedTrueByDefault()
		{
			Xsd.Countervailing value = new Xsd.Countervailing();
			AssertEquals(false, value.IsSpecified);

			value.CaseNo = "XYZ";
			AssertEquals(true, value.IsSpecified);
		}
	}
}
