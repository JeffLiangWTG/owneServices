using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class PostCodeAddressWrapper : IPostCodeAddress
	{
		PostCodeAddressWrapper(CusGoodsLocationAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly CusGoodsLocationAddress address;

		public static PostCodeAddressWrapper New(CusGoodsLocationAddress address) => address == null ? null : new PostCodeAddressWrapper(address);

		public string HouseNumber => houseNumber ?? (houseNumber = address.E2_AdditionalAddressInformation);
		string houseNumber;

		public string PostCode => postCode ?? (postCode = address.E2_Postcode);
		string postCode;

		public string Country => country ?? (country = address.E2_RN_NKCountryCode);
		string country;
	}
}
