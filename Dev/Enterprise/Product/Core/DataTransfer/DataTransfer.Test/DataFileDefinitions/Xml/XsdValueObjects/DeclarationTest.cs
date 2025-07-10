using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Declaration))]
	sealed class DeclarationTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Declaration value = null;
			value = new Xsd.DeclarationCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.Declaration declaration = new Xsd.Declaration();
			AssertEquals(false, declaration.IsSpecified);

			declaration.ManifestID = "x";
			AssertEquals(true, declaration.IsSpecified);

			declaration.ManifestID = null;
			declaration.AddCustomsDetails.AddNew();
			AssertEquals(true, declaration.IsSpecified);

			declaration.AddCustomsDetails.Clear();
			declaration.LandedCostingHeader.LandedCostingDate = ZDateTime.Now;
			declaration.LandedCostingHeader.LandedCostingGroupHeaders.AddNew();
			AssertEquals(true, declaration.IsSpecified);

			declaration.LandedCostingHeader.LandedCostingDate = ZDateTime.Empty;
			declaration.LandedCostingHeader.LandedCostingGroupHeaders.Clear();

			declaration.CountryPayload.USDeclaration.IsSpecified = true;
			AssertEquals(true, declaration.IsSpecified);

			declaration.CountryPayload.USDeclaration.IsSpecified = false;
			AssertEquals(false, declaration.IsSpecified);

			declaration.CountryPayload.ZADeclaration.DistrictOffice = "DO";
			AssertEquals(true, declaration.IsSpecified);

			declaration.CountryPayload.ZADeclaration.DistrictOffice = "";
			AssertEquals(false, declaration.IsSpecified);
		}
	}
}
