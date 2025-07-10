using CargoWise.Customs.EU.MessageContracts.ICS2;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AddressProvider : IAddress
	{
		AddressProvider(OrgAddress address)
		{
			orgAddress = address;
		}

		public static AddressProvider NewOrNull(OrgAddress address) => address == null ? null : new AddressProvider(address);

		readonly OrgAddress orgAddress;

		public string City => orgAddress.City.GetNullIfEmpty();

		public string Country => orgAddress.OA_RN_NKCountryCode.GetNullIfEmpty();

		public string SubDivision => null;

		public string Street => orgAddress.Address1.GetNullIfEmpty();

		public string PostCode => orgAddress.Postcode.GetNullIfEmpty();

		public string StreetAdditionalLine => orgAddress.Address2.GetNullIfEmpty();

		public string Number => "0";

		public string PoBox => null;
	}
}
