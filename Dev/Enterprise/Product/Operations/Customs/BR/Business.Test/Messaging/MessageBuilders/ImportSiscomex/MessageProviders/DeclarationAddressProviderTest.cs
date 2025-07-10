using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationAddressProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationAddressProvider_Address()
		{
			var codesCountry = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("BR", "105"),
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codesCountry);

			AssertEquals("DeclarationAddressProvider should be", null, DeclarationAddressProvider.New(null));

			var orgAdress = Factory.NewWithValidTestData<OrgAddress>();
			orgAdress.CompanyName = "MANUFACTURER";
			orgAdress.OA_Address1 = "VIA ANTONIO CAVALIERI DUCATI 3";
			orgAdress.OA_AdditionalAddressInformation = "SAN VITALE, BO";
			orgAdress.OA_RN_NKCountryCode = "BR";
			orgAdress.OA_City = "MANUFACTURER CITY";
			orgAdress.OA_State = "MS";

			var addressProvider = DeclarationAddressProvider.New(orgAdress);
			CombineAssertions(() =>
			{
				AssertEquals("CompanyName should be", "MANUFACTURER", addressProvider.Name);
				AssertEquals("Address should be", "VIA ANTONIO CAVALIERI DUCATI 3", addressProvider.Address);
				AssertEquals("Address should be", "0", addressProvider.AddressNumber);
				AssertEquals("Address Complementary should be", "SAN VITALE, BO", addressProvider.AddressComplementary);
				AssertEquals("City Name should be", "MANUFACTURER CITY", addressProvider.CityName);
				AssertEquals("Address State Code should be", "MS", addressProvider.AddressStateCode);
				AssertEquals("Address State Description should be", "Mato Grosso do Sul", addressProvider.AddressState);
				AssertEquals("Country Code Description should be", "105", addressProvider.CountryCode);
			});

			orgAdress.OA_AddressMap = "SNA1[29-29]SA1[0-27]";
			addressProvider = DeclarationAddressProvider.New(orgAdress);
			CombineAssertions(() =>
			{
				AssertEquals("Address should be", "VIA ANTONIO CAVALIERI DUCATI", addressProvider.Address);
				AssertEquals("Address should be", "3", addressProvider.AddressNumber);
			});
		}
	}
}
