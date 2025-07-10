using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDifferencesDetailsColumnLayout : IPanelLayoutProvider
	{
		public Phase5GoodsItemDifferencesDetailsColumnLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<Business.NctsArrivalCargoDesc>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.DeclaredValueLabel, ControlWidthClass.Long);
			builder.Add(commonBag.DeclaredCommodityCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclaredCusCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclaredDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclaredGrossWeightDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.DeclaredNetWeightDropEdit, ControlWidthClass.Long);
			builder.AddColumn();
			builder.Add(commonBag.UnloadedValueLabel, ControlWidthClass.Long);
			builder.Add(commonBag.UnloadedCommodityCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnloadedCusCodeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnloadedDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnloadedGrossWeightDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.UnloadedNetWeightDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
