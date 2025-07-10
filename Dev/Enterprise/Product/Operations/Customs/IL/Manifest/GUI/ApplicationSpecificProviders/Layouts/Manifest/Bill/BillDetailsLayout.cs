using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class BillDetailsLayout : IPanelLayoutProvider
	{
		public BillDetailsLayout()
		{
			BillDetails = CreateBillDetailsLayout();
		}

		public PanelLayout Layout => BillDetails;

		PanelLayout BillDetails { get; }

		PanelLayout CreateBillDetailsLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var billControlBag = ILBillControlBag.Instance;
			builder.AddControlBag(billControlBag);

			builder.AddColumn();
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(billControlBag.DischargePortCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(billControlBag.ConditionDropEdit, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
