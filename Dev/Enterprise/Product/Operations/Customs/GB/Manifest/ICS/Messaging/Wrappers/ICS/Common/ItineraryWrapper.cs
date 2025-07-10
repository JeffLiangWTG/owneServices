using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	internal class ItineraryWrapper : IItinerary
	{
		readonly ZString countryOfRoutingCode;

		public ItineraryWrapper(ZString countryOfRoutingCode)
		{
			this.countryOfRoutingCode = countryOfRoutingCode;
		}

		ZString IItinerary.CountryOfRoutingCode => countryOfRoutingCode;
	}
}
