namespace Enterprise.Customs.EU.Business.Testing
{
	public class GuidedDecisionMakingBasicValidationForTest : GuidedDecisionMakingBasicValidation
	{
		public GuidedDecisionMakingBasicValidationForTest(GuidedDecisionMakingBasic parent) : base(parent)
		{
		}

		public new IGuidedDecisionMakingBasicValidationConfiguration exportConfiguration => base.exportConfiguration;
		public new IGuidedDecisionMakingBasicValidationConfiguration importConfiguration => base.importConfiguration;
	}
}
