using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5GNSSWrapper : IArrivalNCTSGNSS
	{
		public ArrivalNCTS5GNSSWrapper(ZDecimal latitude, ZDecimal longitude)
		{
			Latitude = latitude.IsEmpty ? string.Empty : latitude.ToString(7);
			Longitude = longitude.IsEmpty ? string.Empty : longitude.ToString(7);
		}
		protected readonly CusGoodsLocation location;

		public ZString Latitude { get; }

		public ZString Longitude { get; }
	}
}
