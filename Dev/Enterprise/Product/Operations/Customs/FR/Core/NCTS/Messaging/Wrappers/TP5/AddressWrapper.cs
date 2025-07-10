using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class AddressWrapper : IAddress
	{
		AddressWrapper(OrgAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}

		readonly OrgAddress address;

		public static AddressWrapper New(OrgAddress address) => address == null ? null : new AddressWrapper(address);

		public string StreetAndNumber => streetAndNumber ?? (streetAndNumber = address.OA_Address1);
		string streetAndNumber;

		public string PostCode => postCode ?? (postCode = address.OA_PostCode);
		string postCode;

		public string City => city ?? (city = address.OA_City);
		string city;

		public string Country => country ?? (country = address.OA_RN_NKCountryCode);
		string country;
	}
}
