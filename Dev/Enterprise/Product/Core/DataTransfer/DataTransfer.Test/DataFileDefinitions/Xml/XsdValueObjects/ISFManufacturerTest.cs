using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ISFManufacturer))]
	sealed class ISFManufacturerTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.ISFManufacturer manufacturer = new Xsd.ISFManufacturer();
			AssertEquals("Should not be specified by default", false, manufacturer.IsSpecified);

			manufacturer.AddressReference.Organisation = new Organisation();
			manufacturer.AddressReference.Organisation.EDICode = "EDICODE";
			manufacturer.ManufacturerID = "ID123";
			AssertEquals("Precondition", true, manufacturer.AddressReference.IsSpecified);
			AssertEquals("manufacturer.IsSpecified", true, manufacturer.IsSpecified);

			manufacturer.AddressReference.Organisation.EDICode = ZString.Empty;
			AssertEquals("manufacturer.IsSpecified", false, manufacturer.IsSpecified);

			manufacturer.AddressLine1 = "AddressLine1";
			AssertEquals("Should be specified", true, manufacturer.IsSpecified);

			manufacturer.AddressLine1 = "";
			AssertEquals("Should not be specified", false, manufacturer.IsSpecified);

			manufacturer.IsSpecified = false;
			AssertEquals("Should not be specified", false, manufacturer.IsSpecified);

			manufacturer.AddressLine1 = "AddressLine1";
			AssertEquals("Should be specified", true, manufacturer.IsSpecified);

			manufacturer.ManufacturerID = "";
			AssertEquals("Should not be specified", false, manufacturer.IsSpecified);

			manufacturer.AddressReference.Organisation = new Organisation();
			manufacturer.AddressReference.Organisation.EDICode = "EDICODE";
			AssertEquals("Should not be specified", false, manufacturer.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
				"If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
				59, TypeDescriptor.GetProperties(typeof(Xsd.ISFManufacturer)).Count);
		}
	}
}
