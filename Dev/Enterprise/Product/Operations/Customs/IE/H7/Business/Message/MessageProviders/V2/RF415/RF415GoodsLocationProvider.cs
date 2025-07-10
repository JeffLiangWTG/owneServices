using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415GoodsLocationProvider : IRF415GoodsLocation
	{
		public RF415GoodsLocationProvider(CusGoodsLocation cusGoodsLocation)
		{
			this.goodsLocation = Argument.NotNull(cusGoodsLocation, nameof(cusGoodsLocation));
		}

		readonly CusGoodsLocation goodsLocation;

		public string Identification => null;

		public string AdditionalIdentifier => goodsLocation.CGL_AdditionalIdentifier;

		public string QualifierIdentification => goodsLocation.CGL_Qualifier;

		public string LocationTypeCode => goodsLocation.CGL_Type;

		public string City => null;

		public string Country => null;

		public string StreetAndNumber => null;

		public string Postcode => null;
	}
}
