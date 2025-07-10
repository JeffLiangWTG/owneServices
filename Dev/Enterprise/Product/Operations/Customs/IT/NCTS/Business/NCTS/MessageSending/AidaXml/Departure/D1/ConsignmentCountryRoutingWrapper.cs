using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class ConsignmentCountryRoutingWrapper : IConsignmentCountryRouting
{
	public ConsignmentCountryRoutingWrapper(CountryOfRouting countryOfRouting)
	{
		this.countryOfRouting = Argument.NotNull(countryOfRouting, nameof(countryOfRouting));
	}

	int IConsignmentCountryRouting.SequenceNumber => countryOfRouting.CY_Order;

	string IConsignmentCountryRouting.CountryCode => countryOfRouting.CY_Data;

	readonly CountryOfRouting countryOfRouting;
}
