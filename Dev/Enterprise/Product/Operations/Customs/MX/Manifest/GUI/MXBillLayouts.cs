using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public sealed class MXBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public MXBillLayouts()
		{
			BillDetails = CreateBillLayout();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		PanelLayout CreateBillLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

			builder.SetVisibility(common.FreightValueConvertToLocalCurrencyControl, h => h.IsAir, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.TransportValueConvertToLocalCurrencyControl, h => h.IsAir, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.InsuranceValueConvertToLocalCurrencyControl, h => h.IsAir, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.DiscountValueConvertToLocalCurrencyControl, h => h.IsAir, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.OtherChargesValueConvertToLocalCurrencyControl, h => h.IsAir, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.CustomsValueConvertToLocalCurrencyControl, h => h.IsAir, h => h.Header?.AMA_TransportModeInfo);

			builder.SetCaption(common.FreightValueConvertToLocalCurrencyControl, h => Enterprise.Customs.MX.Manifest.GUI.Res.GetData("5E047451-6388-4CF9-9285-3EF6165EB0E1", "Freight Value"));
			builder.SetCaption(common.TransportValueConvertToLocalCurrencyControl, h => Enterprise.Customs.MX.Manifest.GUI.Res.GetData("68A0C893-3788-4576-B8FA-7BCE284889C6", "Goods Value"));

			return builder.Build();
		}
	}
}
