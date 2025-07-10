using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public sealed class GoodsItemDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public GoodsItemDetailsLayoutWithGrid()
		{
			Layout = CreateGoodsItemDetailLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(EU.NCTS.GUI.Phase5DepartureGoodsItemsGridUserControl);

		PanelLayout CreateGoodsItemDetailLayout()
		{
			var builder = new EU.NCTS.GUI.GoodsItemDetailsLayoutBuilder<Business.NctsDepartureCargoDesc>();
			var commonBag = builder.CommonBag;

			var deBag = GoodsItemDetailsControlBag.Instance;
			builder.AddControlBag(deBag);

			builder.AddColumn();

			builder.Add(deBag.GoodsItemDetailsItemNoPlusMainPackUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CommodityCodeTariffFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDispatchDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfDestinationDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.UNDangerousGoodsUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.CusC4NumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TaxOrFeeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.FeesUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
