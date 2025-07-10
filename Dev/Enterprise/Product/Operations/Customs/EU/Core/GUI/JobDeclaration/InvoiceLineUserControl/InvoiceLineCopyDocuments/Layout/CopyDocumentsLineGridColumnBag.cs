using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class CopyDocumentsLineGridColumnBag
	{
		public static CopyDocumentsLineGridColumnBag Instance => instance ??= new CopyDocumentsLineGridColumnBag();

		[ThreadStatic]
		static CopyDocumentsLineGridColumnBag instance;

		CopyDocumentsLineGridColumnBag()
		{
			TypeColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoCopyDocumentsSelectionLine.Schema.CSI_Type, 80);
			SubTypeColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoCopyDocumentsSelectionLine.Schema.CSI_SubType, 80);
			CodeColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoCopyDocumentsSelectionLine.Schema.CSI_Code, 80);
			ReferenceNumberColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoCopyDocumentsSelectionLine.Schema.CSI_ReferenceNumber, 120);
			ReferenceNumber2Column = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoCopyDocumentsSelectionLine.Schema.CSI_ReferenceNumber2, 120);
			DescriptionColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AutoCopyDocumentsSelectionLine.Schema.CSI_Description, 120);
			IsSelectedColumn = new GridColumnReference<ZCheckBoxColumnStyleInfo>(AutoCopyDocumentsSelectionLine.Schema.IsSelected, 40);
		}

		public IGridColumnReference TypeColumn { get; }

		public IGridColumnReference CodeColumn { get; }

		public IGridColumnReference SubTypeColumn { get; }

		public IGridColumnReference ReferenceNumberColumn { get; }

		public IGridColumnReference ReferenceNumber2Column { get; }

		public IGridColumnReference DescriptionColumn { get; }

		public IGridColumnReference IsSelectedColumn { get; }
	}
}
