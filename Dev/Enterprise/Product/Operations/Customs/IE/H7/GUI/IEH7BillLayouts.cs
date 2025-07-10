using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI
{
	public class IEH7BillLayouts : IPanelLayoutProvider
	{
		public IEH7BillLayouts()
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
			builder.Add(billControlBag.AdditionalProcedureDropEdit, ControlWidthClass.Long);
			builder.Add(billControlBag.MovementReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(billControlBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(common.LocationInformationTextBox, ControlWidthClass.Long);
			builder.Add(billControlBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(billControlBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
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

			return builder.Build();
		}
	}
}
