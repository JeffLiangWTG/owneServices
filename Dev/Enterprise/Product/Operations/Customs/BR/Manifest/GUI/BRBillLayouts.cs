using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Manifest.GUI
{
	public sealed class BRBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public BRBillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var brBillControlBag = BRBillControlBag.Instance;
			builder.AddControlBag(brBillControlBag);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(brBillControlBag.DocumentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(brBillControlBag.SellerCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(brBillControlBag.CEMercanteTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);
			builder.Add(brBillControlBag.ToOrderCheckBox, ControlWidthClass.Long);
			builder.Add(brBillControlBag.BLServiceCheckBox, ControlWidthClass.Long);
			builder.Add(brBillControlBag.FRTModeDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

			builder.SetVisibility(brBillControlBag.ToOrderCheckBox, b => b.IsMercante, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(brBillControlBag.BLServiceCheckBox, b => b.IsMercante, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(brBillControlBag.FRTModeDropEdit, b => b.IsMercante, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(brBillControlBag.SellerCountryCodeFindBox, b => b.IsMercante, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.TransportValueConvertToLocalCurrencyControl, b => !b.IsMercante, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(brBillControlBag.CEMercanteTextBox, b => b.IsMercante, b => b.Header?.AMA_ManifestTypeInfo);

			builder.SetCaption(common.FreightValueConvertToLocalCurrencyControl, h => Res.GetData("047CA2EF-F235-4447-8240-17127A671643", "Freight Value"));
			builder.SetCaption(common.TransportValueConvertToLocalCurrencyControl, h => Res.GetData("F022CE4A-18F9-48DE-8311-CEB8AB12018C", "Goods Value"));

			return builder.Build();
		}
	}
}
