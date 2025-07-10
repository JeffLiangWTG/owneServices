using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USOrganisationSpecificDetails))]
	sealed class USOrganisationSpecificDetailsTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USOrganisationSpecificDetails org = new Xsd.USOrganisationSpecificDetails();
			AssertEquals(false, org.IsSpecified);

			org.FDA.Email = "dfDSF";
			AssertEquals(true, org.IsSpecified);
		}
	}
}
