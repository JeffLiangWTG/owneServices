using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class DepartureDetailsControlBag : ControlBag
	{
		public DepartureDetailsControlBag()
		{
			DepartureGoodsLocationZCodeFindBox = RegisterControl(nameof(DepartureDetailsUserControl.DepartureGoodsLocationCodeFindBox));
			TNNDocumentTypeDropEdit = RegisterControl(nameof(DepartureDetailsUserControl.TNNDocumentTypeDropEdit));
		}

		protected override Control CreateTemplate() => new DepartureDetailsUserControl();

		public static DepartureDetailsControlBag Instance => instance ?? (instance = new DepartureDetailsControlBag());

		[ThreadStatic]
		static DepartureDetailsControlBag instance;

		public ControlReference DepartureGoodsLocationZCodeFindBox { get; }

		public ControlReference TNNDocumentTypeDropEdit { get; }
	}
}
