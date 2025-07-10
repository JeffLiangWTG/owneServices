using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class GoodsItemDetailsControlBag : ControlBag
	{
		public GoodsItemDetailsControlBag()
		{
			ItemNumberTextBox = RegisterControl(nameof(GoodsItemDetailsUserControl.ItemNumberTextBox));
			DeclarationGoodsItemNumberTextBox = RegisterControl(nameof(GoodsItemDetailsUserControl.DeclarationGoodsItemNumberTextBox));
			DescriptionOfGoodsTextBox = RegisterControl(nameof(GoodsItemDetailsUserControl.DescriptionOfGoodsTextBox));
			GrossWeightCalcDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.NetWeightCalcDropEdit));
			CommodityCodeTariffFindBox = RegisterControl(nameof(GoodsItemDetailsUserControl.CommodityCodeTariffFindBox));
			DeclarationTypeDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.DeclarationTypeDropEdit));
			CountryOfDispatchDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.CountryOfDispatchDropEdit));
			CountryOfOriginDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.CountryOfOriginDropEdit));
			CountryOfDestinationDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.CountryOfDestinationDropEdit));
			ConsigneeDocAddressControl = RegisterControl(nameof(GoodsItemDetailsUserControl.ConsigneeDocAddressControl));
			CommercialReferenceNumberTextBox = RegisterControl(nameof(GoodsItemDetailsUserControl.CommercialReferenceNumberTextBox));
			TransportChargesMethodOfPaymentDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.TransportChargesMethodOfPaymentDropEdit));
			CusC4NumberCodeFindBox = RegisterControl(nameof(GoodsItemDetailsUserControl.CusC4NumberCodeFindBox));
			UNDangerousGoodsUserControl = RegisterControl(nameof(GoodsItemDetailsUserControl.UNDangerousGoodsUserControl));
			CustomsQuantityDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.CustomsQuantityDropEdit));
			CustomsThirdQuantityDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.CustomsThirdQuantityDropEdit));
			CustomsFourthQuantityDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.CustomsFourthQuantityDropEdit));
			SupplementaryUnitsCalcDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.SupplementaryUnitsCalcDropEdit));
			CustomsValueCalcDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.CustomsValueCalcDropEdit));
			TaxOrFeeDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.TaxOrFeeDropEdit));
			AdditionalSupplementaryCodesUserControl = RegisterControl(nameof(GoodsItemDetailsUserControl.AdditionalSupplementaryCodesUserControl));
			FeesUserControl = RegisterControl(nameof(GoodsItemDetailsUserControl.FeesUserControl));
			LinePriceCalcDropEdit = RegisterControl(nameof(GoodsItemDetailsUserControl.LinePriceCalcDropEdit));
		}

		public static GoodsItemDetailsControlBag Instance => instance ?? (instance = new GoodsItemDetailsControlBag());

		[ThreadStatic]
		static GoodsItemDetailsControlBag instance;

		public ControlReference ItemNumberTextBox { get; }

		public ControlReference DeclarationGoodsItemNumberTextBox { get; }

		public ControlReference DescriptionOfGoodsTextBox { get; }

		public ControlReference GrossWeightCalcDropEdit { get; }

		public ControlReference NetWeightCalcDropEdit { get; }

		public ControlReference CommodityCodeTariffFindBox { get; }

		public ControlReference DeclarationTypeDropEdit { get; }

		public ControlReference CountryOfDispatchDropEdit { get; }

		public ControlReference CountryOfOriginDropEdit { get; }

		public ControlReference CountryOfDestinationDropEdit { get; }

		public ControlReference ConsigneeDocAddressControl { get; }

		public ControlReference CommercialReferenceNumberTextBox { get; }

		public ControlReference TransportChargesMethodOfPaymentDropEdit { get; }

		public ControlReference CusC4NumberCodeFindBox { get; }

		public ControlReference UNDangerousGoodsUserControl { get; }

		public ControlReference SupplementaryUnitsCalcDropEdit { get; }

		public ControlReference CustomsQuantityDropEdit { get; }

		public ControlReference CustomsThirdQuantityDropEdit { get; }

		public ControlReference CustomsFourthQuantityDropEdit { get; }

		public ControlReference CustomsValueCalcDropEdit { get; }

		public ControlReference TaxOrFeeDropEdit { get; }

		public ControlReference AdditionalSupplementaryCodesUserControl { get; }

		public ControlReference FeesUserControl { get; }

		public ControlReference LinePriceCalcDropEdit { get; }

		protected override Control CreateTemplate() => new GoodsItemDetailsUserControl();
	}
}
