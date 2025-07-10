using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class AdditionalDocumentsGridColumnsBag
	{
		public static AdditionalDocumentsGridColumnsBag Instance => instance ?? (instance = new AdditionalDocumentsGridColumnsBag());

		[ThreadStatic]
		static AdditionalDocumentsGridColumnsBag instance;

		public AdditionalDocumentsGridColumnsBag()
		{
			SequenceNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_LineNo, 80);
			SubTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_SubType, 80);
			CodeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Code, 60);
			ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ReferenceNumber, 150);
			DescriptionTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Description, 300);
		}

		public IGridColumnReference SequenceNumberCalcEditColumn { get; }

		public IGridColumnReference SubTypeDropEditColumn { get; }

		public IGridColumnReference CodeCodeFindBoxColumn { get; }

		public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

		public IGridColumnReference DescriptionTextBoxColumn { get; }
	}
}
