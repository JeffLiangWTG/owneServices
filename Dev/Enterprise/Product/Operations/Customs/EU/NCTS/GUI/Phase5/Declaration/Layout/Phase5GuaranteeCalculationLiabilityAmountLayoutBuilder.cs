using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5GuaranteeCalculationLiabilityAmountLayoutBuilder : ColumnLayoutBuilder<CalculateLiabilityBizObj, Phase5GuaranteeCalculationLiabilityAmountControlBag>
	{
		public override Phase5GuaranteeCalculationLiabilityAmountControlBag CommonBag => Phase5GuaranteeCalculationLiabilityAmountControlBag.Instance;

		public override ColumnLayoutBuilderCaptionWidthSize CaptionWidth => base.CaptionWidth + 20; // make room for larger caption (%)

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.LiabilityAmountTotalValueCalculationMethodUserControl, (x) => x.Guarantee.NctsHeader.Configuration.GuaranteeConfiguration.UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods);
		}
	}
}
