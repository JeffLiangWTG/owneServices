using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CarnetDetailsControlBag : ControlBag
	{
		CarnetDetailsControlBag()
		{
			HouseBillSplitDropEdit = RegisterControl(nameof(CarnetDetailsControlBag.HouseBillSplitDropEdit));
			CarnetUseDropEdit = RegisterControl(nameof(CarnetDetailsControlBag.CarnetUseDropEdit));
			CarnetCertificateNoTextBox = RegisterControl(nameof(CarnetDetailsControlBag.CarnetCertificateNoTextBox));
			EffectiveToDateDateEdit = RegisterControl(nameof(CarnetDetailsControlBag.EffectiveToDateDateEdit));
			RepresentativeProductNameLongTextControl = RegisterControl(nameof(CarnetDetailsControlBag.RepresentativeProductNameLongTextControl));
			CargoManagementNoTextBox = RegisterControl(nameof(CarnetDetailsControlBag.CargoManagementNoTextBox));
			HouseBillTextBox = RegisterControl(nameof(CarnetDetailsControlBag.HouseBillTextBox));
			TotalWeightCalcDropEdit = RegisterControl(nameof(CarnetDetailsControlBag.TotalWeightCalcDropEdit));
			TotalQtyCalcEdit = RegisterControl(nameof(CarnetDetailsControlBag.TotalQtyCalcEdit));
			NoPackagesCalcDropEdit = RegisterControl(nameof(CarnetDetailsControlBag.NoPackagesCalcDropEdit));
			TotalAmountConvertToLocalCurrencyControl = RegisterControl(nameof(CarnetDetailsControlBag.TotalAmountConvertToLocalCurrencyControl));
		}

		public static CarnetDetailsControlBag Instance => instance ?? (instance = new CarnetDetailsControlBag());
		[ThreadStatic]
		static CarnetDetailsControlBag instance;

		public ControlReference HouseBillSplitDropEdit { get; }
		public ControlReference CarnetUseDropEdit { get; }
		public ControlReference CarnetCertificateNoTextBox { get; }
		public ControlReference EffectiveToDateDateEdit { get; }
		public ControlReference RepresentativeProductNameLongTextControl { get; }
		public ControlReference CargoManagementNoTextBox { get; }
		public ControlReference HouseBillTextBox { get; }
		public ControlReference TotalWeightCalcDropEdit { get; }
		public ControlReference TotalQtyCalcEdit { get; }
		public ControlReference NoPackagesCalcDropEdit { get; }
		public ControlReference TotalAmountConvertToLocalCurrencyControl { get; }

		protected override Control CreateTemplate() => new CarnetDetailsUserControl();
	}
}
