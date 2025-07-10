using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class Phase5GoodsItemDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public Phase5GoodsItemDetailsLayoutWithGrid()
		{
			Layout = CreateGoodsItemDetailLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemsGridUserControl);

		PanelLayout CreateGoodsItemDetailLayout()
		{
			var builder = new EU.NCTS.GUI.GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();
			var commonBag = builder.CommonBag;

			var esBag = GoodsItemDetailsControlBag.Instance;
			builder.AddControlBag(esBag);

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
			builder.Add(esBag.IsVehiclesCheckBox, ControlWidthClass.Auto);
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
			builder.Add(esBag.ExciseCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(esBag.PVPValueCalcDropEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.FeesUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.Long);

			builder.SetVisibility(esBag.PVPValueCalcDropEdit, i => i.IsPVPApplicable, i => i.ExciseCodeInfo);

			return builder.Build();
		}
	}
}
