using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.OrganisationDetailCountrySpecificDetails))]
	sealed class OrganisationDetailCountrySpecificDetailsTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.OrganisationDetailCountrySpecificDetails details = new Xsd.OrganisationDetailCountrySpecificDetails();
			AssertEquals("Should not be specified by default", false, details.IsSpecified);

			details.USOrganisationSpecificDetails.ImporterOfRecordDetails.PayersUnitNo = "df4";
			AssertEquals(true, details.IsSpecified);
		}
	}
}
