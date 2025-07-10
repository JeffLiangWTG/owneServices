using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class CountryOfRoutingConfiguration : EU.NCTS.Business.CountryOfRoutingConfiguration
{
	protected override ICountryOfRoutingPhase5ValidationDecider GetCountryOfRoutingDeparturePhase5ValidationDecider(EU.NCTS.Business.NctsHeader header) => new CountryOfRoutingDeparturePhase5ValidationDecider();
}
