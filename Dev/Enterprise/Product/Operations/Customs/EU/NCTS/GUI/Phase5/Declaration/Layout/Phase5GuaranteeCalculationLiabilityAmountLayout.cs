using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GuaranteeCalculationLiabilityAmountLayout : IPanelLayoutProvider
	{
		public Phase5GuaranteeCalculationLiabilityAmountLayout()
		{
			Layout = CreatePhase5GuaranteeCalculationLiabilityAmountLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreatePhase5GuaranteeCalculationLiabilityAmountLayout()
		{
			var builder = new Phase5GuaranteeCalculationLiabilityAmountLayoutBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.LiabilityAmountTotalValueCalculationMethodUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.TotalValueCalcDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.LiabilityPercentageIntEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.LiabilityAmountCalcDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
