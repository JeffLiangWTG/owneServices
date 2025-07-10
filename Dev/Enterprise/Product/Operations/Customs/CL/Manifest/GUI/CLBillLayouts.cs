using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public sealed class CLBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public CLBillLayouts()
		{
			BillDetails = CreateBillLayout();
		}

		PanelLayout CreateBillLayout()
		{
			var builder = new BillLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			var cl = CLBillControlBag.Instance;
			builder.AddControlBag(cl);

			builder.AddColumn();
			builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.GoodsLocationAddressControl, ControlWidthClass.Long);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Long);
			builder.Add(common.CustomsEntryNumberTextBox, ControlWidthClass.Long);
			builder.Add(common.BillStatusDropEdit, ControlWidthClass.Long);
			builder.Add(cl.RoRoCheckBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			builder.Add(common.NotifyPartyAddressControl, ControlWidthClass.Long);
			builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);

			builder.SetVisibility(cl.RoRoCheckBox, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetVisibility(common.FreightValueConvertToLocalCurrencyControl, b => b.IsAir, b => b.Header?.AMA_TransportModeInfo, b => b.Header?.AMA_ManifestTypeInfo);
			builder.SetCaption(common.FreightValueConvertToLocalCurrencyControl, b => Res.GetData("2359F2D2-7718-4FD9-8E6F-AE1D9D4A7F94", "Freight Value"));

			return builder.Build();
		}
	}
}
