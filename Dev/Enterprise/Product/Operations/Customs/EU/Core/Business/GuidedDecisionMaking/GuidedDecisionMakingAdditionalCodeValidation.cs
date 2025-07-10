namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingAdditionalCodeValidation : AutoGuidedDecisionMakingAdditionalCodeValidation
	{
		public GuidedDecisionMakingAdditionalCodeValidation(AutoGuidedDecisionMakingAdditionalCode parent) : base(parent) { }

		#region Implementation

		public new GuidedDecisionMakingAdditionalCode Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (GuidedDecisionMakingAdditionalCode)base.Parent; }
		}

		#endregion
	}
}
