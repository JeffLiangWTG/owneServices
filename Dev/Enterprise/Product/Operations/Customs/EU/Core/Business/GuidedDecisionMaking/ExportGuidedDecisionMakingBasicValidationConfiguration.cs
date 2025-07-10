namespace Enterprise.Customs.EU.Business
{
	public class ExportGuidedDecisionMakingBasicValidationConfiguration : IGuidedDecisionMakingBasicValidationConfiguration
	{
		public bool IsPreferenceRequired => false;

		public bool IsQuotaOrderNumberRequired => false;

		public bool IsCountryOfOriginRequired => false;

		public bool IsCountryOfDestinationRequired => true;
	}
}
