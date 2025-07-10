using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class ItineraryWrapper : IItinerary
	{
		public ItineraryWrapper(ZString countryOfRoutingCode)
		{
			this.countryOfRoutingCode = countryOfRoutingCode;
		}
		readonly ZString countryOfRoutingCode;

		public string CountryOfRoutingCode => countryOfRoutingCode;
	}
}
