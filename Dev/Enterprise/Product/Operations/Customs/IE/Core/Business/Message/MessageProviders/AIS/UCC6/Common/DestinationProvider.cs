using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class DestinationProvider : IDestination
	{
		DestinationProvider(string countryOfDestination, string regionOfDestination)
		{
			CountryOfDestination = countryOfDestination;
			RegionOfDestination = regionOfDestination;
		}

		public static DestinationProvider New(string countryOfDestination, string regionOfDestination)
		{
			DestinationProvider result = null;
			if (!string.IsNullOrEmpty(countryOfDestination) || !string.IsNullOrEmpty(regionOfDestination))
			{
				result = new DestinationProvider(countryOfDestination, regionOfDestination);
			}
			return result;
		}

		public string CountryOfDestination { get; }

		public string RegionOfDestination { get; }

		public string CcQualifier => null;
	}
}
