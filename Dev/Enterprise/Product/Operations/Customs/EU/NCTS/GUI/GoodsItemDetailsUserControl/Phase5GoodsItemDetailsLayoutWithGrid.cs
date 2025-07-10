using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
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
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ItemNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.DeclarationGoodsItemNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CommodityCodeTariffFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.UNDangerousGoodsUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.CusC4NumberCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsFourthQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TaxOrFeeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.FeesUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.Long);

			builder.AddControlBehaviour(commonBag.CommodityCodeTariffFindBox, new TariffBoxNomenclatureSelectionModeBehaviour());

			return builder.Build();
		}
	}
}
