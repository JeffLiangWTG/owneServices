namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingConditionValidation : AutoGuidedDecisionMakingConditionValidation
	{
		public GuidedDecisionMakingConditionValidation(AutoGuidedDecisionMakingCondition parent) : base(parent) { }

		#region Implementation

		public new GuidedDecisionMakingCondition Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (GuidedDecisionMakingCondition)base.Parent; }
		}

		#endregion
	}
}
