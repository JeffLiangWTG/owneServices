using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class AdditionalDocumentGridColumnsBag
	{
		public static AdditionalDocumentGridColumnsBag Instance => instance ?? (instance = new AdditionalDocumentGridColumnsBag());

		[ThreadStatic]
		static AdditionalDocumentGridColumnsBag instance;

		AdditionalDocumentGridColumnsBag()
		{
			CSI_SubTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(AdditionalInfo.Schema.CSI_SubType, 80);
			CSI_SubTypeTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AdditionalInfo.Schema.CSI_SubType, 80);
			CSI_CodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(AdditionalInfo.Schema.CSI_Code, 80);
			ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(AdditionalInfo.Schema.CSI_ReferenceNumber, 120);
			ItemNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(AdditionalInfo.Schema.CSI_ItemNumber, 75);
			StatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(AdditionalInfo.Schema.CSI_Status, 75);
		}

		public IGridColumnReference CSI_SubTypeDropEditColumn { get; }

		public IGridColumnReference CSI_SubTypeTextBoxColumn { get; }

		public IGridColumnReference CSI_CodeFindBoxColumn { get; }

		public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

		public IGridColumnReference ItemNumberCalcEditColumn { get; }

		public IGridColumnReference StatusDropEditColumn { get; }
	}
}
