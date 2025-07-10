using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AR.Manifest.GUI
{
	public class ARBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public ARBillLayouts()
		{
			BillDetails = CreateBillLayout();
		}

		PanelLayout CreateBillLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var ar = ARBillControlBag.Instance;
			builder.AddControlBag(ar);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.DepartureDateEdit, ControlWidthClass.Auto);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Long);
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

			builder.AddColumn();
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(ar.IsMonitoredTransitCheckBox, ControlWidthClass.Auto);
			builder.Add(ar.IsInformedToRenarCheckBox, ControlWidthClass.Auto);

			builder.SetVisibility(ar.IsMonitoredTransitCheckBox, h => h.IsSea, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(ar.IsInformedToRenarCheckBox, h => h.IsSea, h => h.Header?.AMA_TransportModeInfo);

			builder.SetVisibility(common.FreightValueConvertToLocalCurrencyControl, h => !h.IsSea, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.TransportValueConvertToLocalCurrencyControl, h => !h.IsSea, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.InsuranceValueConvertToLocalCurrencyControl, h => !h.IsSea, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.DiscountValueConvertToLocalCurrencyControl, h => !h.IsSea, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.OtherChargesValueConvertToLocalCurrencyControl, h => !h.IsSea, h => h.Header?.AMA_TransportModeInfo);
			builder.SetVisibility(common.CustomsValueConvertToLocalCurrencyControl, h => !h.IsSea, h => h.Header?.AMA_TransportModeInfo);

			builder.SetCaption(common.FreightValueConvertToLocalCurrencyControl, h => Enterprise.Customs.AR.Manifest.GUI.Res.GetData("6EE468DC-0905-4437-9246-6AE040C1FDBD", "Freight Value"));
			builder.SetCaption(common.TransportValueConvertToLocalCurrencyControl, h => Enterprise.Customs.AR.Manifest.GUI.Res.GetData("791BBDCC-2BE3-4257-8161-D6ACF864BA12", "Goods Value"));

			return builder.Build();
		}
	}
}
