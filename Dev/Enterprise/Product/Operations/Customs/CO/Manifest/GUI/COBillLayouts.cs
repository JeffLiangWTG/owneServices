using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CO.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CO.Manifest.GUI
{
	public sealed class COBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => BillDetails;

		public COBillLayouts()
		{
			BillDetails = CreateBillLayout();
		}

		PanelLayout CreateBillLayout()
		{
			AddControls();
			SetVisibilities();

			return Builder.Build();
		}

		void AddControls()
		{
			var common = Builder.CommonBag;
			var co = COBillControlBag.Instance;
			Builder.AddControlBag(co);

			Builder.AddColumn();
			Builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			Builder.Add(common.OriginCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(common.GoodsLocationAddressControl, ControlWidthClass.Long);
			Builder.Add(common.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
			builder.Add(co.BillIssueDateEdit, ControlWidthClass.Auto);

			Builder.AddColumn();
			Builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			Builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			Builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			Builder.Add(common.ShipperAddressControl, ControlWidthClass.Long);
			Builder.Add(common.ConsigneeAddressControl, ControlWidthClass.Long);
			Builder.Add(common.AgentAddressControl, ControlWidthClass.Long);

			Builder.AddColumn();
			Builder.Add(common.PrepaidCollectDropEdit, ControlWidthClass.Long);
			Builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(co.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(co.TravelDocumentTypeDropEdit, ControlWidthClass.Long);
			Builder.Add(co.CargoDispositionDropEdit, ControlWidthClass.Long);
			Builder.Add(common.ContainerModeDropEdit, ControlWidthClass.Long);
			Builder.Add(co.MultimodalCheckBox, ControlWidthClass.Long);
			Builder.Add(co.CarriersLiabilityCheckBox, ControlWidthClass.Long);
		}

		void SetVisibilities()
		{
			var common = Builder.CommonBag;
			var co = COBillControlBag.Instance;

			Builder.SetVisibility(co.CargoDispositionDropEdit, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.TravelDocumentTypeDropEdit, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.MultimodalCheckBox, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
			Builder.SetVisibility(co.CarriersLiabilityCheckBox, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
			Builder.SetVisibility(common.GoodsLocationAddressControl, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
			Builder.SetVisibility(common.GoodsLocationDropEditWithFixedWidth, b => !b.IsSea, b => b.Header?.AMA_TransportModeInfo);
			Builder.SetVisibility(common.ContainerModeDropEdit, b => b.IsSea, b => b.Header?.AMA_TransportModeInfo);
		}

		BillLayoutBuilder<AsycudaBill> Builder => builder ?? (builder = new BillLayoutBuilder<AsycudaBill>());
		BillLayoutBuilder<AsycudaBill> builder;
	}
}
