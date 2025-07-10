namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingVATValidation : AutoGuidedDecisionMakingVATValidation
	{
		public GuidedDecisionMakingVATValidation(AutoGuidedDecisionMakingVAT parent) : base(parent)
		{
		}

		public new GuidedDecisionMakingVAT Parent => (GuidedDecisionMakingVAT)base.Parent;
	}
}
