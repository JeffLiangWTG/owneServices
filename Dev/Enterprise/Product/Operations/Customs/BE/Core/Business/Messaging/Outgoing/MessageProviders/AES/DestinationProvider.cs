using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class DestinationProvider : IDestination
{
	public DestinationProvider(string countryOfDestination, string regionOfDestination)
	{
		CountryOfDestination = countryOfDestination;
		RegionOfDestination = regionOfDestination;
	}

	public string CountryOfDestination { get; }

	public string RegionOfDestination { get; }
}
