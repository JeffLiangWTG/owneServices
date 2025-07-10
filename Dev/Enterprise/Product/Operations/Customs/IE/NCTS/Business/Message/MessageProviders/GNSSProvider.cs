using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GNSSProvider : IGNSS
	{
		public string Latitude => geography.Latitude.ToString();

		public string Longitude => geography.Longitude.ToString();

		public GNSSProvider(ZGeography geography)
		{
			this.geography = geography;
		}

		readonly ZGeography geography;
	}
}
