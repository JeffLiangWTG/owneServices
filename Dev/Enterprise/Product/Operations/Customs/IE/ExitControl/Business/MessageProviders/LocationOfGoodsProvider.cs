using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class LocationOfGoodsProvider : ILocationOfGoods
	{
		public LocationOfGoodsProvider(string locationCodeType, string unlocode)
		{
			this.locationCodeType = locationCodeType;
			this.unlocode = unlocode;
		}

		public static ILocationOfGoods New(CusGoodsLocation goodsLocation)
		{
			return goodsLocation == null ? null : new LocationOfGoodsProvider(goodsLocation.CGL_Type, goodsLocation.CGL_AdditionalIdentifier);
		}

		string ILocationOfGoods.LocationCodeType => locationCodeType;

		readonly string locationCodeType;
		string ILocationOfGoods.UNLocode => unlocode;
		readonly string unlocode;
	}
}
