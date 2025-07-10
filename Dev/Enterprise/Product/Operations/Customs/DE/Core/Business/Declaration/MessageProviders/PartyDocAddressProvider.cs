using CargoWise.Customs.DE.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class PartyDocAddressProvider : IPartyDocAddress
	{
		public static PartyDocAddressProvider NewOrNull(JobDocAddress jobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyDocAddressProvider(jobDocAddress) : null;

		PartyDocAddressProvider(JobDocAddress jobDocAddress)
		{
			this.jobDocAddress = jobDocAddress;
		}
		readonly JobDocAddress jobDocAddress;

		public string AdditionalAddressInformation => jobDocAddress.E2_AddressOverride ? jobDocAddress.E2_CompanyName : jobDocAddress.Organisation.OH_FullName;

		public string Address => jobDocAddress.E2_Address1;

		public string Address2 => jobDocAddress.E2_Address2;

		public string City => jobDocAddress.E2_City;

		public string Postcode => jobDocAddress.E2_Postcode;

		public string Country => jobDocAddress.E2_RN_NKCountryCode;
	}
}
