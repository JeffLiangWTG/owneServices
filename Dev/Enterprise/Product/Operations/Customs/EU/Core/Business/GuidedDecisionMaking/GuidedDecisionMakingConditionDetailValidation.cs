using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingConditionDetailValidation : AutoGuidedDecisionMakingConditionDetailValidation
	{
		public GuidedDecisionMakingConditionDetailValidation(AutoGuidedDecisionMakingConditionDetail parent) : base(parent) { }

		public new GuidedDecisionMakingConditionDetail Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (GuidedDecisionMakingConditionDetail)base.Parent; }
		}

		protected override void CheckReference()
		{
			if (Parent.Type.Equals(Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument) && Parent.IsTicked)
			{
				var info = Parent.ReferenceInfo;
				if (info.Value.IsEmpty)
				{
					info.AddWarning(Res.GetString("61E724D9-7E70-47D0-852D-66AD26E3F62E", "Please enter a Reference."));
				}
			}
		}

		protected override void CheckDateOfIssue()
		{
			if (Parent.Type.Equals(Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument) && Parent.IsTicked)
			{
				var info = Parent.DateOfIssueInfo;
				if (info.Value.IsEmpty)
				{
					info.AddWarning(Res.GetString("81DFA7AE-8645-40B2-89E9-FE1ADF8C2851", "Please enter a Date of Issue."));
				}
			}
		}
	}
}
