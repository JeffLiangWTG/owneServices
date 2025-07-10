using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

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
