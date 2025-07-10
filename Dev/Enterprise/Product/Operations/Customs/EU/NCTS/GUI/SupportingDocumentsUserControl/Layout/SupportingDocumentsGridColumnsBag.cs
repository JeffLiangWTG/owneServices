using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class SupportingDocumentsGridColumnsBag
	{
		public static SupportingDocumentsGridColumnsBag Instance => instance ?? (instance = new SupportingDocumentsGridColumnsBag());

		[ThreadStatic]
		static SupportingDocumentsGridColumnsBag instance;

		public SupportingDocumentsGridColumnsBag()
		{
			SequenceNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_LineNo, 80);
			CodeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_Code, 80);
			ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ReferenceNumber, 200);
			ItemNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ItemNumber, 80);
			ReferenceNumber2TextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsAdditionalInfo.Schema.CSI_ReferenceNumber2, 200);
		}

		public IGridColumnReference SequenceNumberCalcEditColumn { get; }

		public IGridColumnReference CodeCodeFindBoxColumn { get; }

		public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

		public IGridColumnReference ItemNumberCalcEditColumn { get; }

		public IGridColumnReference ReferenceNumber2TextBoxColumn { get; }
	}
}
