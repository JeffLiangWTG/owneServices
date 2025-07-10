using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2BillLayouts : IPanelLayoutProvider
	{
		public EUICS2BillLayouts()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		public PanelLayout Layout => BillDetails;

		PanelLayout BillDetails { get; }

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var billControlBag = EUICS2BillControlBag.Instance;
			builder.AddControlBag(billControlBag);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			builder.Add(billControlBag.FreightValueAndCurrencyCalcFindBox, ControlWidthClass.Long);
			builder.Add(billControlBag.ReceptacleIdTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(billControlBag.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.AgentAddressControl, ControlWidthClass.Long);

			builder.SetVisibility(billControlBag.FreightValueAndCurrencyCalcFindBox, bill => bill.Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F43);
			builder.SetVisibility(billControlBag.ReceptacleIdTextBox, bill => bill.IsReceptacleEnabled);

			return builder.Build();
		}
	}
}
