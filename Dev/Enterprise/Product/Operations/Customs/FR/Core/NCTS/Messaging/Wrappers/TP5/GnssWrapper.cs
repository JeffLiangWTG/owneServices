using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class GnssWrapper : IGnss
	{
		GnssWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public static GnssWrapper New(CusGoodsLocationAddress locationofGoodsAddress) => locationofGoodsAddress == null ? null : new GnssWrapper(locationofGoodsAddress);

		public string Latitude => latitude ?? (latitude = goodsLocationAddress.E2_Latitude == 0m ? null : goodsLocationAddress.E2_Latitude.ToString());
		string latitude;

		public string Longitude => longitude ?? (longitude = goodsLocationAddress.E2_Longitude == 0m ? null : goodsLocationAddress.E2_Longitude.ToString());
		string longitude;
	}
}
