using CargoWise.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CusGoodsLocationAddressWrapper : CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.IAddress
	{
		CusGoodsLocationAddressWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public string City => city ?? (city = goodsLocationAddress.E2_City);
		string city;

		public string Country => country ?? (country = goodsLocationAddress.E2_RN_NKCountryCode);
		string country;

		public string Postcode => postcode ?? (postcode = goodsLocationAddress.E2_Postcode);
		string postcode;

		public string StreetAndNumber => streetAndNumber ?? (streetAndNumber = goodsLocationAddress.E2_Address1);
		string streetAndNumber;

		public static CusGoodsLocationAddressWrapper New(CusGoodsLocationAddress goodsLocationAddress) => goodsLocationAddress == null ? null : new CusGoodsLocationAddressWrapper(goodsLocationAddress);
	}
}
