using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportLicense.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportLicense
{
	public class ImportLicenseAddressProvider : IOrganizationAddress
	{
		public ImportLicenseAddressProvider(JobDocAddress address)
		{
			docAddress = address;
		}

		public static ImportLicenseAddressProvider New(JobDocAddress address) => new ImportLicenseAddressProvider(address);

		readonly JobDocAddress docAddress;

		public string Name => docAddress?.E2_CompanyName ?? string.Empty;

		public string Contact => docAddress?.E2_Contact ?? string.Empty;

		public string Email => docAddress?.E2_Email ?? string.Empty;

		public string Address => docAddress != null ? (docAddress.StreetNumber.IsNullOrEmpty() ? docAddress.E2_Address1.ToString() : docAddress.Street) : string.Empty;

		public string AddressNumber => docAddress != null ? (docAddress.StreetNumber.IsNullOrEmpty() ? "0" : docAddress.StreetNumber) : string.Empty;

		public string AddressComplementary => docAddress?.E2_AdditionalAddressInformation ?? string.Empty;

		public string CityName => docAddress?.E2_City ?? string.Empty;

		public string AddressStateCode => docAddress?.E2_State ?? string.Empty;

		public string AddressState => docAddress?.State ?? string.Empty;

		public string AddressCountryCode => docAddress != null ? BRRefCusMapper.MapCW1CountryCodeToCustomsCode(docAddress.Factory, docAddress.E2_RN_NKCountryCode).ToString() : string.Empty;
	}
}
