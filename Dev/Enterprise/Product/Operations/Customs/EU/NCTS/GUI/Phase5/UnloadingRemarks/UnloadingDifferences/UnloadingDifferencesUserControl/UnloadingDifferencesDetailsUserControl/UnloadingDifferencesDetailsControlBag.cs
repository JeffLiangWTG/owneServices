using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class UnloadingDifferencesDetailsControlBag : ControlBag
	{
		UnloadingDifferencesDetailsControlBag()
		{
			DeclaredValueLabel = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.DeclaredValueLabel));
			TotalGrossMassDeclaredValueCalcEdit = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.TotalGrossMassDeclaredValueCalcEdit));
			TotalPackagesDeclaredValueCalcEdit = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.TotalPackagesDeclaredValueCalcEdit));
			InlandTransportModeDropEdit = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.InlandTransportModeDropEdit));

			UnloadedValueLabel = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.UnloadedValueLabel));
			RecalculateTotalsButton = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.RecalculateTotalsButton));
			EffectiveGrossWeightUnloadedCalcEdit = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.EffectiveGrossWeightUnloadedCalcEdit));
			TotalPackagesUnloadedValueCalcEdit = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.TotalPackagesUnloadedValueCalcEdit));
			SeparatorLabel = RegisterControl(nameof(UnloadingDifferencesDetailsUserControl.SeparatorLabel));
		}

		public ControlReference DeclaredValueLabel { get; }

		public ControlReference UnloadedValueLabel { get; }

		public ControlReference TotalGrossMassDeclaredValueCalcEdit { get; }

		public ControlReference EffectiveGrossWeightUnloadedCalcEdit { get; }

		public ControlReference TotalPackagesDeclaredValueCalcEdit { get; }

		public ControlReference TotalPackagesUnloadedValueCalcEdit { get; }

		public ControlReference RecalculateTotalsButton { get; }

		public ControlReference InlandTransportModeDropEdit { get; }

		public ControlReference SeparatorLabel { get; }

		public static UnloadingDifferencesDetailsControlBag Instance => instance ?? (instance = new UnloadingDifferencesDetailsControlBag());

		[ThreadStatic]
		static UnloadingDifferencesDetailsControlBag instance;

		protected override Control CreateTemplate() => new UnloadingDifferencesDetailsUserControl();
	}
}
