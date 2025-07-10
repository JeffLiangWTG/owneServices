using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CommonGNSSWrapper : ICommonGNSS
	{
		public CommonGNSSWrapper(ZDecimal latitude, ZDecimal longitude)
		{
			Latitude = latitude.IsEmpty ? string.Empty : latitude.ToString(7);
			Longitude = longitude.IsEmpty ? string.Empty : longitude.ToString(7);
		}

		public ZString Latitude { get; }

		public ZString Longitude { get; }
	}
}
