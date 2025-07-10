using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class GPSProvider : IGps
	{
		public static GPSProvider New(ZString latitude, ZString longitude) => new GPSProvider(latitude, longitude);

		GPSProvider(ZString latitude, ZString longitude)
		{
			this.latitude = latitude;
			this.longitude = longitude;
		}
		readonly ZString latitude;
		readonly ZString longitude;

		public string Latitude => latitude;

		public string Longitude => longitude;
	}
}
