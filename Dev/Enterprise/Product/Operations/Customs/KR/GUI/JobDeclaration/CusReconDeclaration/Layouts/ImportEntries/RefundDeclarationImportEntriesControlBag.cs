using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class RefundDeclarationImportEntriesControlBag : ControlBag
	{
		RefundDeclarationImportEntriesControlBag()
		{
			SequenceNumberCalcEdit = RegisterControl(nameof(ImportEntriesDetailsUserControl.SequenceNumberCalcEdit));
			ImportEntryNumberCodeFindBox = RegisterControl(nameof(ImportEntriesDetailsUserControl.ImportEntryNumberCodeFindBox));
			ImportEntryLineNumCodeFindBox = RegisterControl(nameof(ImportEntriesDetailsUserControl.ImportEntryLineNumCodeFindBox));
			CustomsDisbursementBillCodeFindBox = RegisterControl(nameof(ImportEntriesDetailsUserControl.CustomsDisbursementBillCodeFindBox));
			AmendSequenceNumber5WNDropEdit = RegisterControl(nameof(ImportEntriesDetailsUserControl.AmendSequenceNumber5WNDropEdit));
		}

		public static RefundDeclarationImportEntriesControlBag Instance => instance ?? (instance = new RefundDeclarationImportEntriesControlBag());
		[ThreadStatic]
		static RefundDeclarationImportEntriesControlBag instance;

		protected override Control CreateTemplate() => new ImportEntriesDetailsUserControl();

		public ControlReference SequenceNumberCalcEdit { get; }
		public ControlReference ImportEntryNumberCodeFindBox { get; }
		public ControlReference ImportEntryLineNumCodeFindBox { get; }
		public ControlReference CustomsDisbursementBillCodeFindBox { get; }
		public ControlReference AmendSequenceNumber5WNDropEdit { get; }
	}
}
