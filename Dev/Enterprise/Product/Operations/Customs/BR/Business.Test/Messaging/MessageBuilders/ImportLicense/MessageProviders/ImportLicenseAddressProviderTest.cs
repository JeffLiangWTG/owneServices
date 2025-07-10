using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportLicense.Testing
{
	class ImportLicenseAddressProviderTest : TestCaseWithFactory
	{
		public void TestImportLicenseAddressProvider_Address()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "BR", "074", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var addressProvider = ImportLicenseAddressProvider.New(null);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName should be", string.Empty, addressProvider.Name);
				AssertEquals("Email should be", string.Empty, addressProvider.Email);
				AssertEquals("Address should be", string.Empty, addressProvider.Address);
				AssertEquals("Address Number should be", string.Empty, addressProvider.AddressNumber);
				AssertEquals("Address Complementary should be", string.Empty, addressProvider.AddressComplementary);
				AssertEquals("City Name should be", string.Empty, addressProvider.CityName);
				AssertEquals("Address State Code should be", string.Empty, addressProvider.AddressStateCode);
				AssertEquals("Address State Description should be", string.Empty, addressProvider.AddressState);
				AssertEquals("Contact should be", string.Empty, addressProvider.Contact);
				AssertEquals("AddressCountryCode should be", string.Empty, addressProvider.AddressCountryCode);
			});

			var orgAdress = Factory.NewWithValidTestData<OrgAddress>();
			orgAdress.CompanyName = "MANUFACTURER";
			orgAdress.OA_RN_NKCountryCode = "BR";
			orgAdress.OA_Email = "MANUFACTURER@TEST.COM";
			orgAdress.OA_Address1 = "VIA ANTONIO CAVALIERI DUCATI 3";
			orgAdress.PrimaryOrgAddressAdditionalInfoDetail = "MANUFACTURER ADDITIONAL ADDRESS";
			orgAdress.OA_City = "MANUFACTURER CITY";
			orgAdress.OA_State = "MS";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_Contact = "MANUFACTURER Contact";
			docAddress.E2_OA_Address = orgAdress.PK;

			addressProvider = ImportLicenseAddressProvider.New(docAddress);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName should be", "MANUFACTURER", addressProvider.Name);
				AssertEquals("Email should be", "MANUFACTURER@TEST.COM", addressProvider.Email);
				AssertEquals("Address should be", "VIA ANTONIO CAVALIERI DUCATI 3", addressProvider.Address);
				AssertEquals("Address Number should be", "0", addressProvider.AddressNumber);
				AssertEquals("Address Complementary should be", "MANUFACTURER ADDITIONAL ADDRESS", addressProvider.AddressComplementary);
				AssertEquals("City Name should be", "MANUFACTURER CITY", addressProvider.CityName);
				AssertEquals("Address State Code should be", "MS", addressProvider.AddressStateCode);
				AssertEquals("Address State Description should be", "Mato Grosso do Sul", addressProvider.AddressState);
				AssertEquals("Contact should be", "MANUFACTURER Contact", addressProvider.Contact);
				AssertEquals("AddressCountryCode should be", "074", addressProvider.AddressCountryCode);
			});

			orgAdress.OA_AddressMap = "SNA1[29-29]SA1[0-27]";

			addressProvider = ImportLicenseAddressProvider.New(docAddress);
			CombineAssertions(() =>
			{
				AssertEquals("Address should be", "VIA ANTONIO CAVALIERI DUCATI", addressProvider.Address);
				AssertEquals("Address Number should be", "3", addressProvider.AddressNumber);
			});
		}

		public void TestAddressCountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Country, "BTH", "Country Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Country, "BR", "074", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), Core.Constants.CountryCodes.Brazil);

			Factory.Save();

			var orgAdress = Factory.NewWithValidTestData<OrgAddress>();

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = orgAdress.PK;

			var addressProvider = ImportLicenseAddressProvider.New(docAddress);

			AssertNullOrEmpty("AddressCountryCode should be empty", addressProvider.AddressCountryCode);

			docAddress.E2_RN_NKCountryCode = "BR";
			orgAdress.OA_RN_NKCountryCode = "BR";
			addressProvider = ImportLicenseAddressProvider.New(docAddress);
			AssertEquals("AddressCountryCode should be", "074", addressProvider.AddressCountryCode);
		}
	}
}
