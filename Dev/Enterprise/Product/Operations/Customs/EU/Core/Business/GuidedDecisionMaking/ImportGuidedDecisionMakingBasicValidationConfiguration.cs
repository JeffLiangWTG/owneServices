namespace Enterprise.Customs.EU.Business
{
	public class ImportGuidedDecisionMakingBasicValidationConfiguration : IGuidedDecisionMakingBasicValidationConfiguration
	{
		public bool IsPreferenceRequired => true;

		public bool IsQuotaOrderNumberRequired => true;

		public bool IsCountryOfOriginRequired => true;

		public bool IsCountryOfDestinationRequired => false;
	}
}
