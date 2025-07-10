using System.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.DocAddress))]
	sealed class DocAddressTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.DocAddress address = new Xsd.DocAddress();
			AssertEquals("Should not be specified by default", false, address.IsSpecified);

			address.AddressReference.Organisation = new Organisation();
			address.AddressReference.Organisation.EDICode = "EDICODE";
			AssertEquals("Precondition", true, address.AddressReference.IsSpecified);
			AssertEquals("Address.IsSpecified", true, address.IsSpecified);

			address.AddressReference.Organisation.EDICode = ZString.Empty;
			AssertEquals("Address.IsSpecified", false, address.IsSpecified);

			address.AddressLine1 = "AddressLine1";
			AssertEquals("Should be specified", true, address.IsSpecified);

			address.AddressLine1 = "";
			AssertEquals("Should not be specified", false, address.IsSpecified);

			address.IsSpecified = false;
			AssertEquals("Should not be specified", false, address.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
				"If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
				55, TypeDescriptor.GetProperties(typeof(Xsd.DocAddress)).Count);
		}

		public void TestCompileTimeCheck()
		{
			Xsd.DocAddress value = null;
			value = new Xsd.DocAddressCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestGetPhoneNumber()
		{
			Xsd.DocAddress docAddress = new Xsd.DocAddress();
			Xsd.TelephoneNumber fax = docAddress.TelephoneNumbers.AddNew();
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			fax.Value = "12345678";

			Xsd.TelephoneNumber phone = docAddress.TelephoneNumbers.AddNew();
			phone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			phone.Value = "87654321";

			AssertEquals("Should return the fax number", fax.Value, docAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax));
			AssertEquals("Should return the Phone number", phone.Value, docAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business));
		}
	}
}
