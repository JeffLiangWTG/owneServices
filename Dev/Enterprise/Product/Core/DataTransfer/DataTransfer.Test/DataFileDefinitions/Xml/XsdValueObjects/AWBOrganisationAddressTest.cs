using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(AWBOrganisationAddress))]
	sealed class AWBOrganisationAddressTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				3, typeof(AWBOrganisationAddress).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			Xsd.AWBOrganisationAddress orgAddressValueObject = new Xsd.AWBOrganisationAddress();
			AssertEquals("Should not be specified by default", false, orgAddressValueObject.IsSpecified);

			//address override
			Xsd.AWBOrgAddressOverride addrOverride = new Xsd.AWBOrgAddressOverride();
			addrOverride.AddressLine1 = "something";
			orgAddressValueObject.Item = addrOverride;
			AssertEquals("IsSpecified is true as address override 's addressline1 is not empty", true, orgAddressValueObject.IsSpecified);

			addrOverride.AddressLine1 = "";
			AssertEquals("IsSpecified is false as address override 's addressline1 is  empty", false, orgAddressValueObject.IsSpecified);

			//doc address
			Xsd.AWBOrgDocAddress docAddress = new Xsd.AWBOrgDocAddress();
			orgAddressValueObject.Item = docAddress;

			AssertEquals("IsSpecified is false as company name element  is  empty", false, orgAddressValueObject.IsSpecified);

			docAddress.CompanyName = "blah";
			AssertEquals("IsSpecified is true as doc address 's company name is not empty", true, orgAddressValueObject.IsSpecified);
		}
	}
}
