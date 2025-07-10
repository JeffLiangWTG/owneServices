using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrganisationDetailEDITransmissionDetails))]
	sealed class OrganisationDetailEDITransmissionAddressTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.OrganisationDetailEDITransmissionDetails value = null;
			value = new Xsd.OrganisationDetailEDITransmissionDetailsCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.OrganisationDetailEDITransmissionDetails organisationDetailEDITransmissionDetails = new Xsd.OrganisationDetailEDITransmissionDetails();
			AssertEquals("Should not be specified by default", false, organisationDetailEDITransmissionDetails.IsSpecified);

			organisationDetailEDITransmissionDetails.Address = "Test";
			AssertEquals("Should be specified when CommunicationAddress populated", true, organisationDetailEDITransmissionDetails.IsSpecified);

			organisationDetailEDITransmissionDetails.IsSpecified = false;
			AssertEquals("IsSpecified=false when explicitly set to false", false, organisationDetailEDITransmissionDetails.IsSpecified);
		}
	}
}
