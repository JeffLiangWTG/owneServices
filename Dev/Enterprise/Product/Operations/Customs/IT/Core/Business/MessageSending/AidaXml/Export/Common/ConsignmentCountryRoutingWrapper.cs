using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class ConsignmentCountryRoutingWrapper : IConsignmentCountryRouting
{
	public ConsignmentCountryRoutingWrapper(ItineraryCountry countryData)
	{
		this.countryData = Argument.NotNull(countryData, nameof(countryData));
	}

	int IConsignmentCountryRouting.SequenceNumber => countryData.CY_Order;

	string IConsignmentCountryRouting.CountryCode => countryData.CY_Code;

	readonly ItineraryCountry countryData;
}
