using CargoWise.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GpsWrapper : CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.IGps
	{
		GpsWrapper(CusGoodsLocationAddress goodsLocationAddress)
		{
			this.goodsLocationAddress = Argument.NotNull(goodsLocationAddress, nameof(goodsLocationAddress));
		}
		readonly CusGoodsLocationAddress goodsLocationAddress;

		public string Latitude => latitude ?? (latitude = goodsLocationAddress.E2_Latitude.ToString());
		string latitude;

		public string Longitude => longitude ?? (longitude = goodsLocationAddress.E2_Longitude.ToString());
		string longitude;

		public static GpsWrapper New(CusGoodsLocationAddress locationofGoodsAddress) => locationofGoodsAddress == null ? null : new GpsWrapper(locationofGoodsAddress);
	}
}
