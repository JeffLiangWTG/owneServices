using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USOrganisationSpecificDetailsFDA))]
	sealed class USOrganisationSpecificDetailsFDATest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USOrganisationSpecificDetailsFDA fda = new Xsd.USOrganisationSpecificDetailsFDA();
			AssertEquals(false, fda.IsSpecified);

			fda.Email = "TEST";
			AssertEquals(true, fda.IsSpecified);

			fda.Email = "";
			fda.FirstName = "fdsdfs";
			AssertEquals(true, fda.IsSpecified);

			fda.FirstName = "";
			fda.FoodFacilityRegistrationExemption = USManufacturerProducerDataTypeFoodFacilityRegistrationExemption.C;
			AssertEquals(true, fda.IsSpecified);
		}
	}
}
