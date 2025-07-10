using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPBillLayouts : IPanelLayoutProvider
	{
		PanelLayout BillDetails { get; }

		public PanelLayout Layout => BillDetails;

		public JPBillLayouts()
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
			var jpBillBag = JPBillControlBag.Instance;
			Builder.AddControlBag(jpBillBag);

			Builder.AddColumn();
			Builder.Add(common.BillNumberTextBox, ControlWidthClass.Long);
			Builder.Add(jpBillBag.FinalDestinationUserControl, ControlWidthClass.Long);
			Builder.Add(common.DischargePortCodeFindBox, ControlWidthClass.Long);
			Builder.Add(jpBillBag.TariffFindBox, ControlWidthClass.Long);
			Builder.Add(jpBillBag.RepresentativeHSCodeFindBox, ControlWidthClass.Long);
			Builder.Add(common.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(jpBillBag.CustomsWeightCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(common.NetWeightCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(jpBillBag.CustomsNetWeightCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(common.VolumeCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(jpBillBag.CustomsVolumeCalcDropEdit, ControlWidthClass.Long);
			Builder.Add(jpBillBag.GoodsOriginCodeFindBox, ControlWidthClass.Long);
			Builder.Add(jpBillBag.SpecialCargoCodeFindBox, ControlWidthClass.Long);
			Builder.Add(jpBillBag.GoodsLocationCodeFindBox, ControlWidthClass.Long);
			Builder.Add(jpBillBag.CargoTypeDropEdit, ControlWidthClass.Long);

			Builder.AddColumn();
			Builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			Builder.Add(common.MarksAndNumbersTextBox, ControlWidthClass.Long);
			Builder.Add(common.RemarksTextBox, ControlWidthClass.Long);
			Builder.Add(common.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			Builder.Add(common.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		}

		void SetVisibilities()
		{
			var common = Builder.CommonBag;
			var jpBillBag = JPBillControlBag.Instance;
			bool IsNotHDFNorHCH(AsycudaBill bill) => !bill.IsHDF && !bill.IsHCH;

			Builder.SetVisibility(jpBillBag.TariffFindBox, IsNotHDFNorHCH);
			Builder.SetVisibility(jpBillBag.RepresentativeHSCodeFindBox, h => !h.IsHDF && h.ShouldShowRepresentativeHSCode);
			Builder.SetVisibility(common.DischargePortCodeFindBox, IsNotHDFNorHCH);
			Builder.SetVisibility(common.NetWeightCalcDropEdit, IsNotHDFNorHCH);
			Builder.SetVisibility(jpBillBag.CustomsNetWeightCalcDropEdit, IsNotHDFNorHCH);
			Builder.SetVisibility(common.VolumeCalcDropEdit, h => !h.IsAir);
			Builder.SetVisibility(jpBillBag.CustomsVolumeCalcDropEdit, h => !h.IsAir);
			Builder.SetVisibility(jpBillBag.GoodsLocationCodeFindBox, h => !h.IsHDF && !h.IsNVC);
			Builder.SetVisibility(jpBillBag.SpecialCargoCodeFindBox, h => h.IsHCH || h.IsNVC);
			Builder.SetVisibility(jpBillBag.GoodsOriginCodeFindBox, IsNotHDFNorHCH);
			Builder.SetVisibility(jpBillBag.CargoTypeDropEdit, h => h.IsHDF);
			Builder.SetVisibility(common.MarksAndNumbersTextBox, IsNotHDFNorHCH);
			Builder.SetVisibility(common.RemarksTextBox, IsNotHDFNorHCH);
			Builder.SetVisibility(common.FreightValueConvertToLocalCurrencyControl, h => !h.IsAir);
			Builder.SetVisibility(common.TransportValueConvertToLocalCurrencyControl, h => !h.IsAir);
		}

		BillLayoutBuilder<AsycudaBill> Builder => builder ??= new BillLayoutBuilder<AsycudaBill>();
		BillLayoutBuilder<AsycudaBill> builder;
	}
}
