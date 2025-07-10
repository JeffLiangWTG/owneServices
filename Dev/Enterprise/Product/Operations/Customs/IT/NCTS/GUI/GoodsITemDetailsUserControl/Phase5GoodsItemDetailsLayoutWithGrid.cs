using System;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class Phase5GoodsItemDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public Phase5GoodsItemDetailsLayoutWithGrid()
	{
		Layout = CreateGoodsItemDetailLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(Phase5DepartureGoodsItemsGridUserControl);

	PanelLayout CreateGoodsItemDetailLayout()
	{
		var builder = new GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();
		var itBag = builder.CommonBag;
		var euBag = EU.NCTS.GUI.GoodsItemDetailsControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.ItemNumberTextBox, ControlWidthClass.Auto);
		builder.Add(euBag.DeclarationGoodsItemNumberTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.CustomsStatusUserControl, ControlWidthClass.Long);
		builder.Add(euBag.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
		builder.Add(euBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.CommodityCodeTariffFindBox, ControlWidthClass.Long);
		builder.Add(euBag.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(euBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(euBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.UNDangerousGoodsUserControl, ControlWidthClass.Long);
		builder.Add(euBag.CusC4NumberCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.CustomsQuantityDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.CustomsThirdQuantityDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.CustomsFourthQuantityDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.LinePriceCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.TaxOrFeeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(euBag.FeesUserControl, ControlWidthClass.Auto);
		builder.Add(euBag.ConsigneeDocAddressControl, ControlWidthClass.Long);
		builder.AddControlBehaviour(euBag.CommodityCodeTariffFindBox, new TariffBoxNomenclatureSelectionModeBehaviour());

		builder.SetVisibility(euBag.TransportChargesMethodOfPaymentDropEdit, x => x.IsInPhase5TransitionPeriod);

		return builder.Build();
	}
}
