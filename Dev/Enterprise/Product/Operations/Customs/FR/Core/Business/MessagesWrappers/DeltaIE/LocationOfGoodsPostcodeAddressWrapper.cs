using CargoWise.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class LocationOfGoodsPostcodeAddressWrapper : CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.ILocationOfGoodsAddress
	{
		LocationOfGoodsPostcodeAddressWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public string Country => country ?? (country = goodsLocationAddress.E2_RN_NKCountryCode);
		string country;

		public string Postcode => postcode ?? (postcode = goodsLocationAddress.E2_Postcode);
		string postcode;

		public string HouseNumber => houseNumber ?? (houseNumber = goodsLocationAddress.E2_Address1);
		string houseNumber;

		public static LocationOfGoodsPostcodeAddressWrapper New(CusGoodsLocationAddress goodsLocationAddress) => goodsLocationAddress == null ? null : new LocationOfGoodsPostcodeAddressWrapper(goodsLocationAddress);
	}
}
