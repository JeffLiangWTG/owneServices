using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class UnloadingDifferencesSupportingDocumentsGridColumnsBag
	{
		public static UnloadingDifferencesSupportingDocumentsGridColumnsBag Instance => instance ?? (instance = new UnloadingDifferencesSupportingDocumentsGridColumnsBag());

		[ThreadStatic]
		static UnloadingDifferencesSupportingDocumentsGridColumnsBag instance;

		public UnloadingDifferencesSupportingDocumentsGridColumnsBag()
		{
			SequenceNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_LineNo, 80);
			ItemNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ItemNumber, 80);
			StatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Status, 95);
			CodeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Code, 80);
			ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ReferenceNumber, 150);
			ReferenceNumber2TextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ReferenceNumber2, 200);
		}

		public IGridColumnReference SequenceNumberCalcEditColumn { get; }

		public IGridColumnReference ItemNumberCalcEditColumn { get; }

		public IGridColumnReference StatusDropEditColumn { get; }

		public IGridColumnReference CodeCodeFindBoxColumn { get; }

		public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

		public IGridColumnReference ReferenceNumber2TextBoxColumn { get; }
	}
}
