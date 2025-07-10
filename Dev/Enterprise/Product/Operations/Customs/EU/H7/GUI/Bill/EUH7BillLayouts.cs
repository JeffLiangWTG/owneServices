using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7BillLayouts : IPanelLayoutProvider
	{
		public EUH7BillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		public PanelLayout Layout => BillDetails;

		PanelLayout BillDetails { get; }

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var billControlBag = EUH7BillControlBag.Instance;
			builder.AddControlBag(billControlBag);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(billControlBag.MovementReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(billControlBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(billControlBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(billControlBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(billControlBag.ContainerUserControl, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);
			builder.Add(billControlBag.StandAloneDeclarationUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			builder.Add(billControlBag.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

			builder.SetCaption(common.ShipperAddressControl, x => Res.GetData("0268be83-a058-4642-9c5f-afc337ca6d0b", "Shipper", "Shipper", "Shipper", "The party shipping the goods."));
			builder.SetCaption(common.ConsigneeAddressControl, x => Res.GetData("22d780f1-6a9d-492f-9726-c8d336a80eda", "CNE", "Consignee", "Consignee", "The party to whom the goods are consigned."));
			builder.SetCaption(common.NotifyPartyAddressControl, x => Res.GetData("355602ff-a2bb-4084-b453-fa4223ee4a1e", "NP", "Notify Party", "Notify Party", "The party to be notified at entry of the arrival of the goods, as stipulated in the master bill of lading or master air waybill."));

			return builder.Build();
		}
	}
}
