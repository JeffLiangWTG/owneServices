using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class FRAddressFormatterTest : TestCaseWithFactory
	{
		public void TestNoExceptionWhenAdressIsNull()
		{
			var addressFormatter = new FRAddressFormatter(Factory, null);
			AssertNoExceptionThrown("Null check should prevent formatter to crash when address parameter is null.", () => addressFormatter.PostalAddress());
			AssertContains("Adress formatter should return empty EORI when address parameter is null", "EORI: <Empty>", addressFormatter.PostalAddress());
		}

		public void TestNoExceptionWhenAdressHeaderIsNull()
		{
			var address = Factory.New<OrgAddress>();
			AssertNull("Prequisite: Address header should be null.", address.Header);
			var addressFormatter = new FRAddressFormatter(Factory, address);
			AssertNoExceptionThrown("Null check should prevent formatter to crash when address OrgHeader is null.", () => addressFormatter.PostalAddress());
			AssertContains("Adress formatter should return empty EORI when address parameter is null", "EORI: <Empty>", addressFormatter.PostalAddress());
		}

		public void TestEORI()
		{
			OrgHeader orgHeader = CreateOrganisationWithAddressAndEori("HEADER", "Header Name", "Header Address", "123456789", Core.Constants.CountryGuids.France);
			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			var addressFormatter = new FRAddressFormatter(Factory, orgHeader.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert("EORI should be included in address.", formattedAddress.Contains("EORI: FR12345678900001"));
		}

		public void TestEORIWithNotFRCountry()
		{
			OrgHeader orgHeader = CreateOrganisationWithAddressAndEori("HEADER", "Header Name", "Header Address", "11111111111", Core.Constants.CountryGuids.Netherlands);
			var addressFormatter = new FRAddressFormatter(Factory, orgHeader.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert("EORI should be included in address.", formattedAddress.Contains("EORI: NL11111111111"));
		}

		OrgHeader CreateOrganisationWithAddressAndEori(ZString orgCode, ZString fullName, ZString address, ZString eori, ZGuid country)
		{
			var countryCode = Factory.Load<RefCountry>(country);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = fullName;
			orgHeader.MainAddress.OA_Address1 = address;
			orgHeader.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, countryCode, eori);
			return orgHeader;
		}
	}
}
