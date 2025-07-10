using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FTADetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new FTADetailsUserControl();

		[ThreadStatic]
		static FTADetailsControlBag instance;

		public static FTADetailsControlBag Instance => instance ??= new FTADetailsControlBag();

		FTADetailsControlBag()
		{
			LawCodeDropEdit = RegisterControl(nameof(FTADetailsUserControl.LawCodeDropEdit));
			CustomsDisbursementBillDropEdit = RegisterControl(nameof(FTADetailsUserControl.CustomsDisbursementBillDropEdit));
		}
		public ControlReference LawCodeDropEdit { get; }
		public ControlReference CustomsDisbursementBillDropEdit { get; }
	}
}
