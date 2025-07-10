using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class PostcodeAddressProvider : IPostcodeAddress
	{
		public static PostcodeAddressProvider New(CusGoodsLocation goodsLocation) => new PostcodeAddressProvider(goodsLocation);

		PostcodeAddressProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}
		readonly CusGoodsLocation goodsLocation;

		public string HouseNumber => goodsLocation.CGL_AdditionalIdentifier;

		public string Postcode => goodsLocation.Address.E2_Postcode;

		public string Country => goodsLocation.Address.E2_RN_NKCountryCode;
	}
}
