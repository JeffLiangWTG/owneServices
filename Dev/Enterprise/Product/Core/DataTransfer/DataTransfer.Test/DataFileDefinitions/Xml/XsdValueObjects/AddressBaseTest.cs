using System.ComponentModel;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.AddressBase))]
	sealed class AddressBaseTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.AddressBase address = new Xsd.AddressBase();
			AssertEquals("Should not be specified by default", false, address.IsSpecified);

			address.AddressLine1 = "Address 1";
			AssertEquals("Should be specified", true, address.IsSpecified);

			address.IsSpecified = false;
			AssertEquals("Should Not be specified", false, address.IsSpecified);
		}

		public void TestIfNewPropertiesAddedIsSpecifiedNeedsToChange()
		{
			AssertEquals(
				"If the number of properties has changed, make sure IsSpecified is still implemented correctly and doesn't need to take into account the new properties",
				35, TypeDescriptor.GetProperties(typeof(Xsd.AddressBase)).Count);
		}

		public void TestCompileTimeCheck()
		{
			Xsd.AddressBase value = null;
			value = new Xsd.AddressBaseCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestGetPhoneNumber()
		{
			Xsd.AddressBase address = new Xsd.AddressBase();
			Xsd.TelephoneNumber fax = address.TelephoneNumbers.AddNew();
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			fax.Value = "12345678";

			Xsd.TelephoneNumber phone = address.TelephoneNumbers.AddNew();
			phone.NumberType = Xsd.TelephoneNumberNumberType.Business;
			phone.Value = "87654321";

			AssertEquals("Should return the fax number", fax.Value, address.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax));
			AssertEquals("Should return the Phone number", phone.Value, address.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business));
		}
	}
}
