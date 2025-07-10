using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CusGoodsLocationAddressWrapper : IAddress
	{
		CusGoodsLocationAddressWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public static CusGoodsLocationAddressWrapper New(CusGoodsLocationAddress goodsLocationAddress) => goodsLocationAddress == null ? null : new CusGoodsLocationAddressWrapper(goodsLocationAddress);

		public string City => city ?? (city = goodsLocationAddress.E2_City);
		string city;

		public string Country => country ?? (country = goodsLocationAddress.E2_RN_NKCountryCode);
		string country;

		public string PostCode => postCode ?? (postCode = goodsLocationAddress.E2_Postcode);
		string postCode;

		public string StreetAndNumber => streetAndNumber ?? (streetAndNumber = goodsLocationAddress.E2_Address1);
		string streetAndNumber;
	}
}
