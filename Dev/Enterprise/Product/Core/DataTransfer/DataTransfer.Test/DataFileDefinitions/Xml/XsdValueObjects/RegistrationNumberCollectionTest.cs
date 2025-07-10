using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.RegistrationNumberCollection))]
	sealed class RegistrationNumberCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.RegistrationNumberCollection value = null;
			value = new Xsd.OrganisationDetail().RegistrationNumbers;
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestFindRegistrationNumber()
		{
			Xsd.RegistrationNumberCollection collection = new Xsd.RegistrationNumberCollection();
			AssertNull("Should return nothing", collection.FindRegistrationNumber(Xsd.RegistrationNumberTypes.GTN, GlbCompany.CurrentCompany.Country.Code));

			ZString country = GlbCompany.CurrentCompany.Country.Code;
			Xsd.RegistrationNumber regNo = collection.AddNew();
			regNo.CountryOfRegistration = country + "XXX";
			regNo.NumberType = Xsd.RegistrationNumberTypes.GTN;

			AssertNull("Should return nothing", collection.FindRegistrationNumber(Xsd.RegistrationNumberTypes.GTN, GlbCompany.CurrentCompany.Country.Code));

			regNo.CountryOfRegistration = country;
			AssertEquals("Should return RegNo", regNo, collection.FindRegistrationNumber(Xsd.RegistrationNumberTypes.GTN, GlbCompany.CurrentCompany.Country.Code));
		}

		public void TestFindOrCreateForCurrentCountry()
		{
			Xsd.RegistrationNumberCollection collection = new Xsd.RegistrationNumberCollection();
			Xsd.RegistrationNumber newRegNo = collection.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.UNC);
			AssertEquals("New reg no created with correct type", RegistrationNumberTypes.UNC, newRegNo.NumberType);
			AssertEquals("New reg no created with correct country", GlbCompany.CurrentCompany.Country.Code, newRegNo.CountryOfRegistration);
		}

		public void TestFindOrCreate()
		{
			Xsd.RegistrationNumberCollection collection = new Xsd.RegistrationNumberCollection();

			Xsd.RegistrationNumber regNo = collection.AddNew();
			regNo.Number = "Number";
			regNo.NumberType = RegistrationNumberTypes.BHR;
			regNo.CountryOfRegistration = "AU";

			Xsd.RegistrationNumber decoyRegNo = collection.AddNew();
			decoyRegNo.Number = "Number";
			decoyRegNo.NumberType = RegistrationNumberTypes.VAT;
			decoyRegNo.CountryOfRegistration = "AU";

			Xsd.RegistrationNumber decoyRegNo2 = collection.AddNew();
			decoyRegNo2.Number = "Number";
			decoyRegNo2.NumberType = RegistrationNumberTypes.BHR;
			decoyRegNo2.CountryOfRegistration = "NZ";

			Xsd.RegistrationNumber foundRegNo = collection.FindOrCreate(RegistrationNumberTypes.BHR, "AU");
			AssertEquals("Correct reg no should be found", true, foundRegNo == regNo);

			Xsd.RegistrationNumber newRegNo = collection.FindOrCreate(RegistrationNumberTypes.CBR, "AU");
			AssertEquals("New reg no created with correct type", RegistrationNumberTypes.CBR, newRegNo.NumberType);
			AssertEquals("New reg no created with correct country", "AU", newRegNo.CountryOfRegistration);
		}
	}
}
