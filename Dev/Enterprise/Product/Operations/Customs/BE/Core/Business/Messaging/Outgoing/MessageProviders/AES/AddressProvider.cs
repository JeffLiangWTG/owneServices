using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business;

public class AddressProvider : IAddress
{
	public AddressProvider(OrgAddress orgAddress, bool isTransitionPeriodAES30 = false)
	{
		this.orgAddress = orgAddress;
		isTransitionPeriod = isTransitionPeriodAES30;
	}
	readonly OrgAddress orgAddress;
	protected readonly bool isTransitionPeriod;

	public string StreetAndNumber => (orgAddress?.OA_Address1 + " " + orgAddress?.OA_Address2).Trim();

	public string Postcode => orgAddress?.OA_PostCode;

	public string City => orgAddress?.OA_City;

	public string Country => orgAddress?.OA_RN_NKCountryCode;

	public int StreetAndNumberMaxLength => isTransitionPeriod ? MessageSchema.AddressStreetAndNumberMaxLengthInTransitionPeriod : MessageSchema.AddressStreetAndNumberMaxLength;
}
