using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Organisations))]
	sealed class OrganisationsTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.OrganisationDetail value = null;
			value = new Xsd.OrganisationDetailCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.Organisation organisation = new Xsd.Organisation();
			AssertEquals("Should not be specified by default", false, organisation.OrganisationDetails.IsSpecified);

			organisation.OrganisationDetails.Name = null;
			organisation.OrganisationDetails.Addresses.AddNew();
			AssertEquals("Should be specified if there is a address", true, organisation.OrganisationDetails.IsSpecified);

			organisation.OrganisationDetails.Name = "splaty";
			organisation.OrganisationDetails.Addresses.Clear();
			AssertEquals("Should be specified if there is a name", true, organisation.OrganisationDetails.IsSpecified);

			organisation.OrganisationDetails.IsSpecified = false;
			AssertEquals("Should not be specified if explicitly told not to be", false, organisation.OrganisationDetails.IsSpecified);
		}
	}
}
