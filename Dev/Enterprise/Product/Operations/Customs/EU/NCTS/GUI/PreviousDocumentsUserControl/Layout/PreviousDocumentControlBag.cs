using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class PreviousDocumentControlBag : ControlBag
	{
		public PreviousDocumentControlBag()
		{
			TypeCodeFindBox = RegisterControl(nameof(PreviousDocumentUserControl.TypeCodeFindBox));
			ReferenceNumberTextBox = RegisterControl(nameof(PreviousDocumentUserControl.ReferenceNumberTextBox));
			ItemNumberCalcEdit = RegisterControl(nameof(PreviousDocumentUserControl.ItemNumberCalcEdit));
			NumOfPackagesDropEdit = RegisterControl(nameof(PreviousDocumentUserControl.NumOfPackagesDropEdit));
			QuantityDropEdit = RegisterControl(nameof(PreviousDocumentUserControl.QuantityDropEdit));
			ComplementTextBox = RegisterControl(nameof(PreviousDocumentUserControl.ComplementTextBox));
			LineNoCalcEdit = RegisterControl(nameof(PreviousDocumentUserControl.LineNoCalcEdit));
			StatusTextBox = RegisterControl(nameof(PreviousDocumentUserControl.StatusTextBox));
		}

		public static PreviousDocumentControlBag Instance => instance ?? (instance = new PreviousDocumentControlBag());

		[ThreadStatic]
		static PreviousDocumentControlBag instance;

		public ControlReference TypeCodeFindBox { get; }

		public ControlReference ReferenceNumberTextBox { get; }

		public ControlReference ItemNumberCalcEdit { get; }

		public ControlReference NumOfPackagesDropEdit { get; }

		public ControlReference QuantityDropEdit { get; }

		public ControlReference ComplementTextBox { get; }

		public ControlReference LineNoCalcEdit { get; }

		public ControlReference StatusTextBox { get; }

		protected override Control CreateTemplate() => new PreviousDocumentUserControl();
	}
}
