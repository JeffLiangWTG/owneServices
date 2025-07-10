using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ZADeclaration))]
	sealed class ZADeclarationTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.ZADeclaration zADeclaration = new Xsd.ZADeclaration();
			AssertEquals("Should not be specified by default", false, zADeclaration.IsSpecified);

			zADeclaration.DistrictOffice = "x";
			AssertEquals("Should be specified", true, zADeclaration.IsSpecified);

			zADeclaration.DistrictOffice = "";
			AssertEquals("Should be specified", false, zADeclaration.IsSpecified);

			zADeclaration.PortOfExitOrDestination = "blh";
			AssertEquals("Should be specified", true, zADeclaration.IsSpecified);

			zADeclaration.PortOfExitOrDestination = "";
			AssertEquals("Should be specified", false, zADeclaration.IsSpecified);
		}
	}
}
