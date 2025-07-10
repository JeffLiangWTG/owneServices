using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public class SupportingDocumentDetailsControlBag : ControlBag
	{
		public SupportingDocumentDetailsControlBag()
		{
			TypeDropEdit = RegisterControl(nameof(SupportingDocumentDetailsControl.typeDropEdit));
			ReferenceNumberTextBox = RegisterControl(nameof(SupportingDocumentDetailsControl.referenceNumberTextBox));
			EDocGuidDropEditGuidDropEdit = RegisterControl(nameof(SupportingDocumentDetailsControl.eDocGuidDropEditGuidDropEdit));
			StatusTextBox = RegisterControl(nameof(SupportingDocumentDetailsControl.statusTextBox));
			AdditionalDescriptionTextBox = RegisterControl(nameof(SupportingDocumentDetailsControl.additionalDescriptionTextBox));
			CustomsDocIDTextBox = RegisterControl(nameof(SupportingDocumentDetailsControl.customsDocIDTextBox));
		}

		public ControlReference TypeDropEdit { get; }
		public ControlReference ReferenceNumberTextBox { get; }
		public ControlReference EDocGuidDropEditGuidDropEdit { get; }
		public ControlReference StatusTextBox { get; }
		public ControlReference AdditionalDescriptionTextBox { get; }
		public ControlReference CustomsDocIDTextBox { get; }

		public static SupportingDocumentDetailsControlBag Instance => lazySupportingDocumentDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<SupportingDocumentDetailsControlBag> lazySupportingDocumentDetailsControlBag = new Lazy<SupportingDocumentDetailsControlBag>(() => new SupportingDocumentDetailsControlBag());

		protected override Control CreateTemplate() => new SupportingDocumentDetailsControl();
	}
}
