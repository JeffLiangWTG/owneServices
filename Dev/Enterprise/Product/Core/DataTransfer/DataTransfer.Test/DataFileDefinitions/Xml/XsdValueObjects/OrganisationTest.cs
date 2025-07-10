using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Organisation))]
	sealed class OrganisationTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Organisation value = null;
			value = new Xsd.OrganisationCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.Organisation organisation = new Xsd.Organisation();
			AssertEquals("Should not be specified by default", false, organisation.IsSpecified);

			organisation.EDICode = "";
			organisation.OwnerCode = "";
			organisation.OrganisationDetails.Name = "splaty";
			AssertEquals("Should be specified if there are org details", true, organisation.IsSpecified);

			organisation.EDICode = "xxx";
			organisation.OwnerCode = "";
			organisation.OrganisationDetails.IsSpecified = false;
			AssertEquals("Should be specified if there is an edi code", true, organisation.IsSpecified);

			organisation.EDICode = "";
			organisation.OwnerCode = "xxx";
			organisation.OrganisationDetails.IsSpecified = false;
			AssertEquals("Should be specified if there is an owner code", true, organisation.IsSpecified);

			organisation.IsSpecified = false;
			AssertEquals("Should not be specified if explicitly told not to be", false, organisation.IsSpecified);
		}
	}
}
