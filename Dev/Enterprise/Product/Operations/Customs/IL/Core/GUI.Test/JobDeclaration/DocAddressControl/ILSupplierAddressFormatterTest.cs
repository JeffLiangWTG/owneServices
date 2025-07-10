using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class ILSupplierAddressFormatterTest : TestCaseWithFactory
	{
		public void TestNoExceptionWhenAdressIsNull()
		{
			var addressFormatter = new ILSupplierAddressFormatter(Factory, null);
			AssertNoExceptionThrown("Null check should prevent formatter to crash when address parameter is null.", () => addressFormatter.PostalAddress());
			AssertContains("Address formatter should return empty Customs # when address parameter is null", "Customs #: <Empty>", addressFormatter.PostalAddress());
		}

		public void TestNoExceptionWhenAdressHeaderIsNull()
		{
			var address = Factory.New<OrgAddress>();
			AssertNull("Prequisite: Address header should be null.", address.Header);
			var addressFormatter = new ILSupplierAddressFormatter(Factory, address);
			AssertNoExceptionThrown("Null check should prevent formatter to crash when address OrgHeader is null.", () => addressFormatter.PostalAddress());
			AssertContains("Address formatter should return empty Customs # when address parameter is null", "Customs #: <Empty>", addressFormatter.PostalAddress());
		}

		public void TestCustomsNumber()
		{
			var orgHeader = CreateOrganisationWithAddressAndCSC("HEADER", "Header Name", "Header Address", "532687", Core.Constants.CountryGuids.Israel);
			var addressFormatter = new ILSupplierAddressFormatter(Factory, orgHeader.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert("Customs # should be included in address.", formattedAddress.Contains("Customs #: 532687"));
		}

		public void TestCustomsNumberWithNotILCountry()
		{
			var orgHeader = CreateOrganisationWithAddressAndCSC("HEADER", "Header Name", "Header Address", "111111", Core.Constants.CountryGuids.Latvia);
			var addressFormatter = new ILSupplierAddressFormatter(Factory, orgHeader.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert("Customs # should be empty in address.", formattedAddress.Contains("Customs #: <Empty>"));
		}

		OrgHeader CreateOrganisationWithAddressAndCSC(ZString orgCode, ZString fullName, ZString address, ZString csc, ZGuid country)
		{
			var countryCode = Factory.Load<RefCountry>(country);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = fullName;
			orgHeader.MainAddress.OA_Address1 = address;
			orgHeader.SetCustomsCode(OrgCusCode.CodeTypes.SupplierCode, countryCode, csc);
			return orgHeader;
		}
	}
}
