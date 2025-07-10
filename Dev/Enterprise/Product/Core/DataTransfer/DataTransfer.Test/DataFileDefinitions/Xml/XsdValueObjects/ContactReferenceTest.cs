using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ContactReference))]
	sealed class ContactReferenceTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.ContactReference value = null;
			value = new Xsd.ContactReferenceCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.ContactReference valueObject = new Xsd.ContactReference();
			AssertEquals("IsSpecified=false by default", false, valueObject.IsSpecified);
			valueObject.Organisation.EDICode = "EDICODE";
			AssertEquals("IsSpecified=true when the org is specified", true, valueObject.IsSpecified);
			valueObject.IsSpecified = false;
			AssertEquals("IsSpecified=false when IsSpecified explicitly set", false, valueObject.IsSpecified);
		}
	}
}
