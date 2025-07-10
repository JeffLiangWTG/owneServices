using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class AdditionalDocumentControlBag : ControlBag
	{
		public AdditionalDocumentControlBag()
		{
			KindDropEdit = RegisterControl(nameof(AdditionalDocumentUserControl.KindDropEdit));
			TypeCodeFindBox = RegisterControl(nameof(AdditionalDocumentUserControl.TypeCodeFindBox));
			ReferenceNumberTextBox = RegisterControl(nameof(AdditionalDocumentUserControl.ReferenceNumberTextBox));
			DescriptionTextBox = RegisterControl(nameof(AdditionalDocumentUserControl.DescriptionTextBox));
			DescriptionMultilineTextBox = RegisterControl(nameof(AdditionalDocumentUserControl.DescriptionMultilineTextBox));
			LineNoCalcEdit = RegisterControl(nameof(AdditionalDocumentUserControl.LineNoCalcEdit));
			StatusLabel = RegisterControl(nameof(AdditionalDocumentUserControl.StatusLabel));
		}

		public static AdditionalDocumentControlBag Instance => instance ?? (instance = new AdditionalDocumentControlBag());

		[ThreadStatic]
		static AdditionalDocumentControlBag instance;

		public ControlReference LineNoCalcEdit { get; }

		public ControlReference StatusLabel { get; }

		public ControlReference KindDropEdit { get; }

		public ControlReference TypeCodeFindBox { get; }

		public ControlReference ReferenceNumberTextBox { get; }

		public ControlReference DescriptionTextBox { get; }

		public ControlReference DescriptionMultilineTextBox { get; }

		protected override Control CreateTemplate() => new AdditionalDocumentUserControl();
	}
}
