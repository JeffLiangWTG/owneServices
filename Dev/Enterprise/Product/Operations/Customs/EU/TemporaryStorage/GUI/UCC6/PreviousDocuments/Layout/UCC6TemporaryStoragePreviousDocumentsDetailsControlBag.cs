using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public sealed class UCC6TemporaryStoragePreviousDocumentsDetailsControlBag : ControlBag
	{
		UCC6TemporaryStoragePreviousDocumentsDetailsControlBag()
		{
			GoodItemIdentifierCalcEdit = RegisterControl(nameof(UCC6TemporaryStoragePreviousDocumentsDetailsUserControl.GoodItemIdentifierCalcEdit));
			PackageCalcDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePreviousDocumentsDetailsUserControl.PackageCalcDropEdit));
			QuantityCalcDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePreviousDocumentsDetailsUserControl.QuantityCalcDropEdit));
			ReferenceNumberTextBox = RegisterControl(nameof(UCC6TemporaryStoragePreviousDocumentsDetailsUserControl.ReferenceNumberTextBox));
			TypeCodeFindBox = RegisterControl(nameof(UCC6TemporaryStoragePreviousDocumentsDetailsUserControl.TypeCodeFindBox));
		}

		protected override Control CreateTemplate() => new UCC6TemporaryStoragePreviousDocumentsDetailsUserControl();

		public static UCC6TemporaryStoragePreviousDocumentsDetailsControlBag Instance => uCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UCC6TemporaryStoragePreviousDocumentsDetailsControlBag> uCC6TemporaryStoragePreviousDocumentsDetailsControlBag = new Lazy<UCC6TemporaryStoragePreviousDocumentsDetailsControlBag>(() => new UCC6TemporaryStoragePreviousDocumentsDetailsControlBag());

		public ControlReference GoodItemIdentifierCalcEdit { get; }

		public ControlReference PackageCalcDropEdit { get; }

		public ControlReference QuantityCalcDropEdit { get; }

		public ControlReference ReferenceNumberTextBox { get; }

		public ControlReference TypeCodeFindBox { get; }
	}
}
