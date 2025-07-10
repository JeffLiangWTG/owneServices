using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GuaranteeCalculationLiabilityAmountControlBag : ControlBag
	{
		public Phase5GuaranteeCalculationLiabilityAmountControlBag()
		{
			LiabilityPercentageIntEdit = RegisterControl(nameof(Phase5GuaranteeCalculationLiabilityAmountUserControl.LiabilityPercentageIntEdit));
			LiabilityAmountCalcDropEdit = RegisterControl(nameof(Phase5GuaranteeCalculationLiabilityAmountUserControl.LiabilityAmountCalcDropEdit));
			TotalValueCalcDropEdit = RegisterControl(nameof(Phase5GuaranteeCalculationLiabilityAmountUserControl.TotalValueCalcDropEdit));
			LiabilityAmountTotalValueCalculationMethodUserControl = RegisterControl(nameof(Phase5GuaranteeCalculationLiabilityAmountUserControl.LiabilityAmountTotalValueCalculationMethodUserControl));
		}

		public static Phase5GuaranteeCalculationLiabilityAmountControlBag Instance => instance ?? (instance = new Phase5GuaranteeCalculationLiabilityAmountControlBag());

		[ThreadStatic]
		static Phase5GuaranteeCalculationLiabilityAmountControlBag instance;

		public ControlReference LiabilityPercentageIntEdit { get; }

		public ControlReference LiabilityPercentageLabel { get; }

		public ControlReference LiabilityAmountCalcDropEdit { get; }

		public ControlReference TotalValueCalcDropEdit { get; }

		public ControlReference LiabilityAmountTotalValueCalculationMethodUserControl { get; }

		protected override Control CreateTemplate() => new Phase5GuaranteeCalculationLiabilityAmountUserControl();
	}
}
