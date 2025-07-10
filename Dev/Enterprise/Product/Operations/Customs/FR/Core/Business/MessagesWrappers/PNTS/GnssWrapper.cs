using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class GnssWrapper : IGnss
	{
		GnssWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public string Latitude => latitude ?? (latitude = goodsLocationAddress.E2_Latitude.ToString());
		string latitude;

		public string Longitude => longitude ?? (longitude = goodsLocationAddress.E2_Longitude.ToString());
		string longitude;

		public static GnssWrapper New(CusGoodsLocationAddress locationofGoodsAddress) => locationofGoodsAddress == null ? null : new GnssWrapper(locationofGoodsAddress);
	}
}
