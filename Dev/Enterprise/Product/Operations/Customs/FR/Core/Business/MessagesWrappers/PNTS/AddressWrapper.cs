using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class AddressWrapper : IAddress
	{
		AddressWrapper(CusGoodsLocationAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly CusGoodsLocationAddress address;

		public string City => city ?? (city = address.E2_City);
		string city;

		public string Country => country ?? (country = address.E2_RN_NKCountryCode);
		string country;

		public string PostCode => postCode ?? (postCode = address.E2_Postcode);
		string postCode;

		public string StreetAndNumber => streetAndNumber ?? (streetAndNumber = address.E2_Address1);
		string streetAndNumber;

		public static AddressWrapper New(CusGoodsLocationAddress address) => address == null ? null : new AddressWrapper(address);
	}
}
