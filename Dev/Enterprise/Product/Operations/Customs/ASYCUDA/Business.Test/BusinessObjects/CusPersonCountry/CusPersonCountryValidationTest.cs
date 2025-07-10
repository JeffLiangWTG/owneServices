using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class CusPersonCountryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBoilerPlateListValidationWhyOhWhyCantThisBeAutomated()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var person = header.Persons.AddNew();
			var country = person.Countries.AddNew();
			country.CPC_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			foreach (var purpose in new[] { "OCC", "RSN" })
			{
				country.CPC_Type = purpose;
				country.CPC_Value = "xxx";
				AssertHasMessageErrorContaining(country.CPC_ValueInfo, "list");
				country.CPC_Value = "";
				AssertHasErrorContaining(country.CPC_ValueInfo, MandatoryValidation.MustBeEntered);
				country.CPC_Value = country.Lookups.DataValues[0].Code;
				AssertNoMessageErrorContaining(country.CPC_ValueInfo, "list");
				AssertNoErrorContaining(country.CPC_ValueInfo, MandatoryValidation.MustBeEntered);
			}

			country.CPC_Type = "XXX";
			AssertHasMessageErrorContaining(country.CPC_TypeInfo, "list");
			country.CPC_Type = "";
			AssertHasMessageErrorContaining(country.CPC_TypeInfo, "not entered");
			country.CPC_Type = country.Lookups.DataTypes[0].Code;
			AssertNoMessageErrorContaining(country.CPC_TypeInfo, "list");
			AssertNoMessageErrorContaining(country.CPC_TypeInfo, "not entered");

			country.CPC_RN_NKCountry = "XX";
			AssertHasMessageErrorContaining(country.CPC_RN_NKCountryInfo, "list");
			country.CPC_RN_NKCountry = "";
			AssertHasMessageErrorContaining(country.CPC_RN_NKCountryInfo, "not entered");
			country.CPC_RN_NKCountry = country.Lookups.Countries[0].Code;
			AssertNoMessageErrorContaining(country.CPC_RN_NKCountryInfo, "list");
			AssertNoMessageErrorContaining(country.CPC_RN_NKCountryInfo, "not entered");
		}
	}
}
