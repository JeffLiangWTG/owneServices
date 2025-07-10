using CargoWise.Types;

using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingAdditionalCodeFromCondition : GuidedDecisionMakingAdditionalCode
	{
		public GuidedDecisionMakingAdditionalCodeFromCondition(GuidedDecisionMakingBasic guidedDecisionMakingBasic) : base(guidedDecisionMakingBasic)
		{
		}

		protected override ZString GetApplicableToDescription()
		{
			var conditionType = ApplicableToType;
			var conditionTypeDescription = UniversalReferenceDataHelper.GetConditionTypeDescription(Factory, Parent.DataGrouping, conditionType);
			return Res.GetString("CB8C5944-B144-443C-93F3-85699B381C29", "Condition Type: {0} {1}", conditionType, conditionTypeDescription);
		}
	}
}
