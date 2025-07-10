namespace Enterprise.Customs.EU.Business
{
	public interface IGuidedDecisionMakingBasicValidationConfiguration
	{
		bool IsPreferenceRequired { get; }

		bool IsQuotaOrderNumberRequired { get; }

		bool IsCountryOfOriginRequired { get; }

		bool IsCountryOfDestinationRequired { get; }
	}
}
