using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationBond))]
	sealed class USDeclarationBondTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationBond uSDeclarationBond = new Xsd.USDeclarationBond();
			AssertEquals(false, uSDeclarationBond.IsSpecified);

			uSDeclarationBond.AccountNumber = "33";
			AssertEquals(true, uSDeclarationBond.IsSpecified);

			uSDeclarationBond.AccountNumber = "";
			uSDeclarationBond.SuretyCode = "33";
			AssertEquals(true, uSDeclarationBond.IsSpecified);

			uSDeclarationBond.SuretyCode = "";
			uSDeclarationBond.ADDCVDSuretyCode = "33";
			AssertEquals(true, uSDeclarationBond.IsSpecified);

			uSDeclarationBond.ADDCVDSuretyCode = "";
			uSDeclarationBond.Amount = 10m;
			AssertEquals(true, uSDeclarationBond.IsSpecified);

			uSDeclarationBond.Amount = 0m;
			AssertEquals(false, uSDeclarationBond.IsSpecified);

			uSDeclarationBond.TypeSpecified = true;
			AssertEquals(true, uSDeclarationBond.IsSpecified);
		}
	}
}
