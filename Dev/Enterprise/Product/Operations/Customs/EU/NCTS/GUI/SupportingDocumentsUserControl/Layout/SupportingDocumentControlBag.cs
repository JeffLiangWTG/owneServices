using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class SupportingDocumentControlBag : ControlBag
	{
		public SupportingDocumentControlBag()
		{
			TypeCodeFindBox = RegisterControl(nameof(SupportingDocumentUserControl.TypeCodeFindBox));
			ReferenceNumberTextBox = RegisterControl(nameof(SupportingDocumentUserControl.ReferenceNumberTextBox));
			ComplementTextBox = RegisterControl(nameof(SupportingDocumentUserControl.ComplementTextBox));
			ItemNumberCalcEdit = RegisterControl(nameof(SupportingDocumentUserControl.ItemNumberCalcEdit));
			LineNoCalcEdit = RegisterControl(nameof(SupportingDocumentUserControl.LineNoCalcEdit));
			StatusLabel = RegisterControl(nameof(SupportingDocumentUserControl.StatusLabel));
			CountryCodeFindBox = RegisterControl(nameof(SupportingDocumentUserControl.CountryCodeFindBox));
		}

		public static SupportingDocumentControlBag Instance => instance ?? (instance = new SupportingDocumentControlBag());

		[ThreadStatic]
		static SupportingDocumentControlBag instance;

		public ControlReference LineNoCalcEdit { get; }

		public ControlReference StatusLabel { get; }

		public ControlReference TypeCodeFindBox { get; }

		public ControlReference ReferenceNumberTextBox { get; }

		public ControlReference ItemNumberCalcEdit { get; }

		public ControlReference ComplementTextBox { get; }

		public ControlReference CountryCodeFindBox { get; }

		protected override Control CreateTemplate() => new SupportingDocumentUserControl();
	}
}
