using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.RegistrationNumber))]
	sealed class RegistrationNumberTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.RegistrationNumber registrationNumber = new Xsd.RegistrationNumber();
			AssertEquals("Should not be specified by default", false, registrationNumber.IsSpecified);

			registrationNumber.NumberType = RegistrationNumberTypes.GST;
			AssertEquals("RegistrationNumber.IsSpecified", false, registrationNumber.IsSpecified);

			registrationNumber.Number = "EDID";
			AssertEquals("RegistrationNumber.IsSpecified", true, registrationNumber.IsSpecified);

			registrationNumber.Number = ZString.Empty;
			AssertEquals("RegistrationNumber.IsSpecified", false, registrationNumber.IsSpecified);

			registrationNumber.CountryOfRegistration = "AU";
			AssertEquals("RegistrationNumber.IsSpecified", true, registrationNumber.IsSpecified);

			registrationNumber.CountryOfRegistration = ZString.Empty;
			AssertEquals("RegistrationNumber.IsSpecified", false, registrationNumber.IsSpecified);
		}

		public void TestCompileTimeCheck()
		{
			Xsd.RegistrationNumber value = null;
			value = new Xsd.RegistrationNumberCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}
	}
}
