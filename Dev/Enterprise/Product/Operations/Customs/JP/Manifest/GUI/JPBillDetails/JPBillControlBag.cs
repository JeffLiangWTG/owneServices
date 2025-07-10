using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPBillControlBag : ControlBag
	{
		public static JPBillControlBag Instance => instance ??= new JPBillControlBag();

		[ThreadStatic]
		static JPBillControlBag instance;

		JPBillControlBag()
		{
			FinalDestinationUserControl = RegisterControl(nameof(FinalDestinationUserControl));
			GoodsLocationCodeFindBox = RegisterControl(nameof(GoodsLocationCodeFindBox));
			SpecialCargoCodeFindBox = RegisterControl(nameof(SpecialCargoCodeFindBox));
			CargoTypeDropEdit = RegisterControl(nameof(CargoTypeDropEdit));
			TariffFindBox = RegisterControl(nameof(TariffFindBox));
			RepresentativeHSCodeFindBox = RegisterControl(nameof(RepresentativeHSCodeFindBox));
			GoodsOriginCodeFindBox = RegisterControl(nameof(GoodsOriginCodeFindBox));
			CustomsWeightCalcDropEdit = RegisterControl(nameof(CustomsWeightCalcDropEdit));
			CustomsNetWeightCalcDropEdit = RegisterControl(nameof(CustomsNetWeightCalcDropEdit));
			CustomsVolumeCalcDropEdit = RegisterControl(nameof(CustomsVolumeCalcDropEdit));
		}

		protected override Control CreateTemplate()
		{
			return new JPManifestBillSpecificUserControl();
		}

		public ControlReference FinalDestinationUserControl { get; }
		public ControlReference GoodsOriginCodeFindBox { get; }
		public ControlReference GoodsLocationCodeFindBox { get; }
		public ControlReference SpecialCargoCodeFindBox { get; }
		public ControlReference CargoTypeDropEdit { get; }
		public ControlReference TariffFindBox { get; }
		public ControlReference RepresentativeHSCodeFindBox { get; }
		public ControlReference CustomsWeightCalcDropEdit { get; }
		public ControlReference CustomsNetWeightCalcDropEdit { get; }
		public ControlReference CustomsVolumeCalcDropEdit { get; }
	}
}
