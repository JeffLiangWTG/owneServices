using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.PackingDetails))]
	sealed class PackingDetailsTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			AssertNotNull("The line above was probably commented out", new Xsd.PackingDetails());
		}

		public void TestIsSpecified()
		{
			Xsd.PackingDetails packingDetails = new Xsd.PackingDetails();
			AssertEquals("Default", false, packingDetails.IsSpecified);

			packingDetails.Housebill = "TEST";
			AssertEquals(true, packingDetails.IsSpecified);

			packingDetails.Housebill = "";
			packingDetails.Masterbill = "TEST";
			AssertEquals(true, packingDetails.IsSpecified);

			packingDetails.IsSpecified = false;
			AssertEquals(false, packingDetails.IsSpecified);
		}
	}
}
