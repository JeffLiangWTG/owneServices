using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class GoodsItemDifferencesDetailsColumnLayout : IPanelLayoutProvider
{
	public GoodsItemDifferencesDetailsColumnLayout()
	{
		GoodsItemDifferencesDetailsColumn = CreateGoodsItemDifferencesDetailsColumn();
	}

	public PanelLayout Layout => GoodsItemDifferencesDetailsColumn;

	PanelLayout GoodsItemDifferencesDetailsColumn { get; }

	PanelLayout CreateGoodsItemDifferencesDetailsColumn()
	{
		var builder = new Phase5GoodsItemDifferencesDetailsColumnLayoutBuilder<Business.NctsArrivalCargoDesc>();
		var commonBag = builder.CommonBag;
		builder.AddControlBag(commonBag);
		var chBag = GoodsItemDifferencesDetailsColumnControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(chBag.UnloadingRemarkCodeDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.UnloadingRemarkTextTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclaredValueLabel, ControlWidthClass.Long);
		builder.Add(chBag.DeclaredCommodityCodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclaredCusCodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclaredDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclaredGrossWeightDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.DeclaredNetWeightDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.UnloadedValueLabel, ControlWidthClass.Long, commonBag.DeclaredValueLabel);
		builder.Add(chBag.UnloadedCommodityCodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.UnloadedCusCodeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.UnloadedDescriptionTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.UnloadedGrossWeightDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.UnloadedNetWeightDropEdit, ControlWidthClass.Long);

		builder.SetVisibility(chBag.UnloadedCommodityCodeCodeFindBox, x => x.UnloadedColumsVisible, x => x.BY_UnloadedStateInfo);

		return builder.Build();
	}
}
