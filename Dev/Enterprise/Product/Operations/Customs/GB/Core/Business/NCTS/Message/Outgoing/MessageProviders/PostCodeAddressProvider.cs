using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class PostCodeAddressProvider : IPostCodeAddress
	{
		readonly CusGoodsLocation goodsLocation;
		public PostCodeAddressProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		}

		public string HouseNumber => goodsLocation.CGL_AdditionalIdentifier;

		public string Postcode => goodsLocation.Address.E2_Postcode;

		public string Country => goodsLocation.Address.E2_RN_NKCountryCode;
	}
}
