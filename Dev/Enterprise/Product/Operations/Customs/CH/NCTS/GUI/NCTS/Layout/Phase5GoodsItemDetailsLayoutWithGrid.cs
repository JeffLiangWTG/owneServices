using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class Phase5GoodsItemDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public Phase5GoodsItemDetailsLayoutWithGrid()
	{
		layout = CreateGoodsItemDetailLayout();
	}
	readonly PanelLayout layout;

	public PanelLayout Layout => layout;

	public Type GridUserControlType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemsGridUserControl);

	PanelLayout CreateGoodsItemDetailLayout()
	{
		var builder = new GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();
		var chBag = builder.CommonBag;
		var euBag = EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.ItemNumberTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
		builder.Add(euBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(chBag.HarmonisedTariffFindBox, ControlWidthClass.Long);
		builder.Add(euBag.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(euBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(euBag.UNDangerousGoodsUserControl, ControlWidthClass.Long);
		builder.Add(euBag.CusC4NumberCodeFindBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
