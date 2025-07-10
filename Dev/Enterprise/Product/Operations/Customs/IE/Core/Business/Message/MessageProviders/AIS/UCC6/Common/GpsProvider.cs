using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class GpsProvider : IGps
	{
		GpsProvider(string latitude, string longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public static GpsProvider New(string latitude, string longitude)
		{
			if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
			{
				return null;
			}
			return new GpsProvider(latitude, longitude);
		}

		public string Latitude { get; }

		public string Longitude { get; }
	}
}
