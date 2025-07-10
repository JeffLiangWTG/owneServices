using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.H7.GUI
{
	public class FRH7BillLayouts : IPanelLayoutProvider
	{
		public FRH7BillLayouts()
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
			builder.Add(common.ShipmentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(billControlBag.LocationOfGoodsUserControl, ControlWidthClass.Long);
			builder.Add(billControlBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(billControlBag.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.CarrierReferenceTextBox, ControlWidthClass.Long);
			builder.Add(common.UCRNumberTextBox, ControlWidthClass.Long);
			builder.Add(billControlBag.StandAloneDeclarationUserControl, ControlWidthClass.Long);
			builder.Add(billControlBag.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

			builder.SetCaption(billControlBag.AdditionalProcedureDropEdit, _ => Res.GetData("8ab550bb-08d3-4622-bde7-1d776b6bc018", "Add. Procedure(s)"));
			builder.SetCaption(common.ShipmentTypeDropEdit, _ => Res.GetData("55f8e5f7-abcc-4bfa-9a43-24217f2bd0ce", "Add. Declaration Type"));
			builder.SetCaption(billControlBag.LocationOfGoodsUserControl, _ => Res.GetData("9033ed45-3aa9-4b16-8b6f-9be5b879d54f", "Location of Goods"));
			builder.SetCaption(billControlBag.StandAloneDeclarationUserControl, _ => Res.GetData("f5f2f844-b37e-4d50-aa1a-a313c8334e3d", "Stand Alone Declaration"));

			return builder.Build();
		}
	}
}
