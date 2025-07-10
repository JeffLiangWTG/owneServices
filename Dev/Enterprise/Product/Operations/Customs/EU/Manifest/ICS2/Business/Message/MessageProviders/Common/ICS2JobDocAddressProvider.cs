using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2JobDocAddressProvider : IAddress
	{
		internal ICS2JobDocAddressProvider(ICS2JobDocAddress address)
		{
			jobDocAddress = address;
		}

		public static ICS2JobDocAddressProvider NewOrNull(ICS2JobDocAddress address) => address == null || address.IsEmpty ? null : new ICS2JobDocAddressProvider(address);

		readonly ICS2JobDocAddress jobDocAddress;

		public string City => jobDocAddress.City;

		public string Country => jobDocAddress.E2_RN_NKCountryCode;

		public string SubDivision => jobDocAddress.SubDivision.GetNullIfEmpty();

		public string Street => jobDocAddress.Address1.GetNullIfEmpty();

		public string PostCode => jobDocAddress.Postcode;

		public string StreetAdditionalLine => jobDocAddress.Address2.GetNullIfEmpty();

		public string Number => jobDocAddress.Number.GetNullIfEmpty() ?? "0";

		public string PoBox => jobDocAddress.POBox.GetNullIfEmpty();
	}
}
