using CargoWise.Types;

using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingAdditionalCodeFromStatisticalAdditionalCode : GuidedDecisionMakingAdditionalCode
	{
		public GuidedDecisionMakingAdditionalCodeFromStatisticalAdditionalCode(GuidedDecisionMakingBasic guidedDecisionMakingBasic) : base(guidedDecisionMakingBasic)
		{
		}

		protected override ZString GetApplicableToDescription()
		{
			var statisticalAdditionalCodeType = ApplicableToType;
			var statisticalAdditionalCodeTypeDescription = UniversalReferenceDataHelper.GetTariffAdditionalCodeCategoryDescription(Factory, Parent.DataGrouping, statisticalAdditionalCodeType);

			return Res.GetString("5BCC56B8-7429-4957-864B-13A8CB8AC823", "Statistical Add. Codes: {0} {1}", statisticalAdditionalCodeType, statisticalAdditionalCodeTypeDescription);
		}
	}
}
