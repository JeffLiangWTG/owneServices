using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class UnloadingDifferencesAdditionalDocumentsGridColumnsBag
	{
		public static UnloadingDifferencesAdditionalDocumentsGridColumnsBag Instance => instance ?? (instance = new UnloadingDifferencesAdditionalDocumentsGridColumnsBag());

		[ThreadStatic]
		static UnloadingDifferencesAdditionalDocumentsGridColumnsBag instance;

		public UnloadingDifferencesAdditionalDocumentsGridColumnsBag()
		{
			SequenceNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_LineNo, 80);
			ItemNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ItemNumber, 80);
			StatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Status, 95);
			SubTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_SubType, 80);
			CodeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Code, 80);
			ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ReferenceNumber, 150);
			DescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Description, 200);
		}

		public IGridColumnReference SequenceNumberCalcEditColumn { get; }

		public IGridColumnReference ItemNumberCalcEditColumn { get; }

		public IGridColumnReference StatusDropEditColumn { get; }

		public IGridColumnReference SubTypeDropEditColumn { get; }

		public IGridColumnReference CodeCodeFindBoxColumn { get; }

		public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

		public IGridColumnReference DescriptionTextBoxColumn { get; }
	}
}
