using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStorageSupportingDocumentsDetailsControlBag : ControlBag
	{
		public UCC6TemporaryStorageSupportingDocumentsDetailsControlBag()
		{
			CodeCodeFindBox = RegisterControl(nameof(UCC6TemporaryStorageSupportingDocumentsDetailsUserControl.CodeCodeFindBox));
			ReferenceNumberTextBox = RegisterControl(nameof(UCC6TemporaryStorageSupportingDocumentsDetailsUserControl.ReferenceNumberTextBox));
		}

		protected override Control CreateTemplate() => new UCC6TemporaryStorageSupportingDocumentsDetailsUserControl();

		public static UCC6TemporaryStorageSupportingDocumentsDetailsControlBag Instance => uCC6TemporaryStorageSupportingDocumentsDetailControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UCC6TemporaryStorageSupportingDocumentsDetailsControlBag> uCC6TemporaryStorageSupportingDocumentsDetailControlBag = new Lazy<UCC6TemporaryStorageSupportingDocumentsDetailsControlBag>(() => new UCC6TemporaryStorageSupportingDocumentsDetailsControlBag());

		public ControlReference CodeCodeFindBox { get; }
		public ControlReference ReferenceNumberTextBox { get; }
	}
}
