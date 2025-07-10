using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class OtherDetailsControlBag : ControlBag
	{
		OtherDetailsControlBag()
		{
			ProductDropEdit = RegisterControl(nameof(OtherDetailsLayoutUserControl.ProductDropEdit));
			LineNoCalcEdit = RegisterControl(nameof(OtherDetailsLayoutUserControl.LineNoCalcEdit));
			Agency1CodeFindBox = RegisterControl(nameof(OtherDetailsLayoutUserControl.Agency1CodeFindBox));
			Agency2CodeFindBox = RegisterControl(nameof(OtherDetailsLayoutUserControl.Agency2CodeFindBox));
			Agency3CodeFindBox = RegisterControl(nameof(OtherDetailsLayoutUserControl.Agency3CodeFindBox));
			InspectionDropEdit = RegisterControl(nameof(OtherDetailsLayoutUserControl.InspectionDropEdit));
			DeliveryCompanyDropEdit = RegisterControl(nameof(OtherDetailsLayoutUserControl.DeliveryCompanyDropEdit));
		}

		public static OtherDetailsControlBag Instance => instance ?? (instance = new OtherDetailsControlBag());

		[ThreadStatic]
		static OtherDetailsControlBag instance;
		public ControlReference ProductDropEdit { get; }
		public ControlReference LineNoCalcEdit { get; }
		public ControlReference Agency1CodeFindBox { get; }
		public ControlReference Agency2CodeFindBox { get; }
		public ControlReference Agency3CodeFindBox { get; }
		public ControlReference InspectionDropEdit { get; }
		public ControlReference DeliveryCompanyDropEdit { get; }

		protected override Control CreateTemplate() => new OtherDetailsLayoutUserControl();
	}
}
