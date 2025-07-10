using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI;

public sealed class BillDetailsLayout : IPanelLayoutProvider
{
	PanelLayout BillDetails { get; }

	PanelLayout IPanelLayoutProvider.Layout => BillDetails;

	public BillDetailsLayout()
	{
		BillDetails = CreateBillDetailsLayout();
	}

	PanelLayout CreateBillDetailsLayout()
	{
		var builder = new BillDetailsLayoutBuilder();
		var common = builder.CommonBag;
		var aeBag = BillDetailsControlBag.Instance;

		builder.AddControlBag(aeBag);
		builder.AddColumn();

		builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
		builder.Add(aeBag.SplitBillCheckBox, ControlWidthClass.Long);
		builder.Add(aeBag.SplitBillNumberCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
		builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
		builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.CargoTypeDropEdit, ControlWidthClass.Long);
		builder.Add(common.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
		builder.Add(common.MessageStatusTextBox, ControlWidthClass.Long);
		builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);
		builder.Add(common.SenderReferenceTextBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
		builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
		builder.Add(common.DeliveryAgentAddressControl, ControlWidthClass.Long);
		builder.Add(common.AgentAddressControl, ControlWidthClass.Long);
		builder.Add(common.ForwarderAddressControl, ControlWidthClass.Long);
		builder.Add(aeBag.ForwarderMPCITextBox, ControlWidthClass.Medium, common.ForwarderAddressControl);

		builder.AddColumn();
		builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
		builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

		builder.SetCaption(common.AgentAddressControl, b => Res.GetData("FBDC89AF-726F-44CE-874E-C8E5D6DE46F1", "Origin Agent"));

		return builder.Build();
	}
}
