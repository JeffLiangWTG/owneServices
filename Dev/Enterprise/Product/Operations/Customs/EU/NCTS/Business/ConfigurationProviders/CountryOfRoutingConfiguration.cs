namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CountryOfRoutingConfiguration
	{
		public CountryOfRoutingConfiguration()
		{
		}

		public ICountryOfRoutingValidationDecider GetValidationDecider(NctsHeader header) => GetValidationDeciderCore(header);

		protected virtual ICountryOfRoutingValidationDecider GetValidationDeciderCore(NctsHeader header)
		{
			if (header?.IsPhase5Departure ?? false)
			{
				return GetCountryOfRoutingDeparturePhase5ValidationDecider(header);
			}

			return null;
		}

		protected virtual ICountryOfRoutingPhase5ValidationDecider GetCountryOfRoutingDeparturePhase5ValidationDecider(NctsHeader header) => new CountryOfRoutingDeparturePhase5ValidationDecider();
	}
}
