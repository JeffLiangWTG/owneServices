using CargoWise.Types;

using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingAdditionalCodeFromRate : GuidedDecisionMakingAdditionalCode
	{
		public GuidedDecisionMakingAdditionalCodeFromRate(GuidedDecisionMakingBasic guidedDecisionMakingBasic) : base(guidedDecisionMakingBasic)
		{
		}

		protected override ZString GetApplicableToDescription()
		{
			var rateType = ApplicableToType;
			var rateTypeDescription = UniversalReferenceDataHelper.GetRateTypeDescription(Factory, Parent.DataGrouping, rateType);
			return Res.GetString("327B0777-E522-4218-92DF-8B0FE2F4198C", "Rate Type: {0} {1}", rateType, rateTypeDescription);
		}
	}
}
