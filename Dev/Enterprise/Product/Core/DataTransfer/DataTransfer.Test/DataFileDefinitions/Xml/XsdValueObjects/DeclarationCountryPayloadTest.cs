using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.DeclarationCountryPayload))]
	sealed class DeclarationCountryPayloadTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.DeclarationCountryPayload declarationCountryPayload = new Xsd.DeclarationCountryPayload();
			AssertEquals(false, declarationCountryPayload.IsSpecified);

			declarationCountryPayload.USDeclaration.IsSpecified = true;
			AssertEquals(true, declarationCountryPayload.IsSpecified);
		}
	}
}
