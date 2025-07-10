using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDifferencesDetailsColumnControlBag : ControlBag
	{
		Phase5GoodsItemDifferencesDetailsColumnControlBag()
		{
			DeclaredValueLabel = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.DeclaredValueLabel));
			DeclaredCommodityCodeCodeFindBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.DeclaredCommodityCodeCodeFindBox));
			DeclaredCusCodeCodeFindBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.DeclaredCusCodeCodeFindBox));
			DeclaredDescriptionTextBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.DeclaredDescriptionTextBox));
			DeclaredGrossWeightDropEdit = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.DeclaredGrossWeightDropEdit));
			DeclaredNetWeightDropEdit = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.DeclaredNetWeightDropEdit));
			UnloadedValueLabel = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.UnloadedValueLabel));
			UnloadedCommodityCodeCodeFindBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.UnloadedCommodityCodeCodeFindBox));
			UnloadedCusCodeCodeFindBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.UnloadedCusCodeCodeFindBox));
			UnloadedDescriptionTextBox = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.UnloadedDescriptionTextBox));
			UnloadedGrossWeightDropEdit = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.UnloadedGrossWeightDropEdit));
			UnloadedNetWeightDropEdit = RegisterControl(nameof(Phase5GoodsItemDifferencesDetailsColumnUserControl.UnloadedNetWeightDropEdit));
		}

		public static Phase5GoodsItemDifferencesDetailsColumnControlBag Instance => instance ?? (instance = new Phase5GoodsItemDifferencesDetailsColumnControlBag());

		[ThreadStatic]
		static Phase5GoodsItemDifferencesDetailsColumnControlBag instance;

		public ControlReference DeclaredValueLabel { get; }

		public ControlReference DeclaredCommodityCodeCodeFindBox { get; }

		public ControlReference DeclaredCusCodeCodeFindBox { get; }

		public ControlReference DeclaredDescriptionTextBox { get; }

		public ControlReference DeclaredGrossWeightDropEdit { get; }

		public ControlReference DeclaredNetWeightDropEdit { get; }

		public ControlReference UnloadedValueLabel { get; }

		public ControlReference UnloadedCommodityCodeCodeFindBox { get; }

		public ControlReference UnloadedCusCodeCodeFindBox { get; }

		public ControlReference UnloadedDescriptionTextBox { get; }

		public ControlReference UnloadedGrossWeightDropEdit { get; }

		public ControlReference UnloadedNetWeightDropEdit { get; }

		protected override Control CreateTemplate() => new Phase5GoodsItemDifferencesDetailsColumnUserControl();
	}
}
