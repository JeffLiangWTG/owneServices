using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class GoodsItemPackageControlBag : ControlBag
	{
		public GoodsItemPackageControlBag()
		{
			PackageTypeDropEdit = RegisterControl(nameof(GoodsItemPackageUserControl.PackageTypeDropEdit));
			NumberOfPackagesCalcEdit = RegisterControl(nameof(GoodsItemPackageUserControl.NumberOfPackagesCalcEdit));
			MarksAndNumbersTextBox = RegisterControl(nameof(GoodsItemPackageUserControl.MarksAndNumbersTextBox));
			PackageIDTextBox = RegisterControl(nameof(GoodsItemPackageUserControl.PackageIDTextBox));
			BrandTextBox = RegisterControl(nameof(GoodsItemPackageUserControl.BrandTextBox));
			ModelTextBox = RegisterControl(nameof(GoodsItemPackageUserControl.ModelTextBox));
		}

		public static GoodsItemPackageControlBag Instance => instance ?? (instance = new GoodsItemPackageControlBag());

		[ThreadStatic]
		static GoodsItemPackageControlBag instance;

		public ControlReference PackageTypeDropEdit { get; }

		public ControlReference NumberOfPackagesCalcEdit { get; }

		public ControlReference MarksAndNumbersTextBox { get; }

		public ControlReference PackageIDTextBox { get; }

		public ControlReference BrandTextBox { get; }

		public ControlReference ModelTextBox { get; }

		protected override Control CreateTemplate() => new GoodsItemPackageUserControl();
	}
}
