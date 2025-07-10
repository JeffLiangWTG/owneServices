using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.AntiDumping))]
	sealed class AntiDumpingTest : ValueObjectTestCase
	{
		public void TestIsSpecifiedTrueByDefault()
		{
			Xsd.AntiDumping value = new Xsd.AntiDumping();
			AssertEquals(false, value.IsSpecified);

			value.CaseNo = "XYZ";
			AssertEquals(true, value.IsSpecified);
		}
	}
}
