using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RF415GoodsLocationProvider : IRF415GoodsLocation
	{
		readonly CusGoodsLocation goodsLocation;
		readonly IDocAddress address;

		public RF415GoodsLocationProvider(CusGoodsLocation goodsLocation)
		{
			this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
			this.address = goodsLocation.Address;
		}

		public string AdditionalIdentifier => string.Empty;

		public string LocationTypeCode => goodsLocation.CGL_Type;

		public string City => address?.E2_City;

		public string Country => goodsLocation.Parent is CusEntryInstruction instruction && instruction.IsUCC5 ? Core.Constants.CountryCodes.Ireland : address?.E2_RN_NKCountryCode;

		public string StreetAndNumber => MessageProviderHelper.GetDocAddressLine(address);

		public string Postcode => address?.E2_Postcode;

		public string Identification => goodsLocation.CGL_AdditionalIdentifier;

		public string QualifierIdentification => goodsLocation.CGL_Qualifier;
	}
}
