using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CH.Business;

public class AddressDataProvider : IAddress
{
	public static AddressDataProvider New(IDocAddress docAddress) => docAddress == null ? null : new AddressDataProvider(docAddress);

	AddressDataProvider(IDocAddress jobDocAddress)
	{
		this.jobDocAddress = jobDocAddress;
	}
	readonly IDocAddress jobDocAddress;

	public string CareOf => jobDocAddress.E2_AdditionalAddressInformation.ReturnNullIfEmpty();

	public string City => jobDocAddress.E2_City;

	public string Country => jobDocAddress.E2_RN_NKCountryCode;

	public string Postcode => jobDocAddress.E2_Postcode;

	public string StreetAndNumber => (jobDocAddress.E2_Address1 + " " + jobDocAddress.E2_Address2).Trim();
}
