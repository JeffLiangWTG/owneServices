using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.GUI.Testing
{
	public class ILImporterAddressFormatterTest : TestCaseWithFactory
	{
		public void TestNoExceptionWhenAdressIsNull()
		{
			var addressFormatter = new ILImporterAddressFormatter(Factory, null);
			AssertNoExceptionThrown("Null check should prevent formatter to crash when address parameter is null.", () => addressFormatter.PostalAddress());
			AssertContains("Address formatter should return empty VAT when address parameter is null", "VAT #: <Empty>", addressFormatter.PostalAddress());
		}

		public void TestNoExceptionWhenAdressHeaderIsNull()
		{
			var address = Factory.New<OrgAddress>();
			AssertNull("Prequisite: Address header should be null.", address.Header);
			var addressFormatter = new ILImporterAddressFormatter(Factory, address);
			AssertNoExceptionThrown("Null check should prevent formatter to crash when address OrgHeader is null.", () => addressFormatter.PostalAddress());
			AssertContains("Address formatter should return empty VAT when address parameter is null", "VAT #: <Empty>", addressFormatter.PostalAddress());
		}

		public void TestVatNumber()
		{
			var orgHeader = CreateOrganisationWithAddressAndVAT("HEADER", "Header Name", "Header Address", "532687", Core.Constants.CountryGuids.Israel);
			var addressFormatter = new ILImporterAddressFormatter(Factory, orgHeader.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert("VAT should be included in address.", formattedAddress.Contains("VAT #: 532687"));
		}

		public void TestVatNumberWithNotILCountry()
		{
			var orgHeader = CreateOrganisationWithAddressAndVAT("HEADER", "Header Name", "Header Address", "111111", Core.Constants.CountryGuids.Latvia);
			var addressFormatter = new ILImporterAddressFormatter(Factory, orgHeader.MainAddress);
			var formattedAddress = addressFormatter.PostalAddress();
			Assert("VAT should be empty in address.", formattedAddress.Contains("VAT #: <Empty>"));
		}

		OrgHeader CreateOrganisationWithAddressAndVAT(ZString orgCode, ZString fullName, ZString address, ZString vat, ZGuid country)
		{
			var countryCode = Factory.Load<RefCountry>(country);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = fullName;
			orgHeader.MainAddress.OA_Address1 = address;
			orgHeader.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, countryCode, vat);
			return orgHeader;
		}
	}
}
