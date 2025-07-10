using System;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public sealed class PreviousDocumentsGridColumnBag
	{
		public static PreviousDocumentsGridColumnBag Instance => instance ??= new PreviousDocumentsGridColumnBag();

		[ThreadStatic]
		static PreviousDocumentsGridColumnBag instance;
		PreviousDocumentsGridColumnBag()
		{
			CodeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_Code, 80, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
			ReferenceNumberColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_ReferenceNumber, 120, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
		}

		public IGridColumnReference CodeCodeFindBoxColumn { get; }

		public IGridColumnReference ReferenceNumberColumn { get; }
	}
}
