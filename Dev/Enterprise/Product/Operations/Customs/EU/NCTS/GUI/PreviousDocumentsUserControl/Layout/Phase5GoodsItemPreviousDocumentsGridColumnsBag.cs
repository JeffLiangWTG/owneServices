using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemPreviousDocumentsGridColumnsBag
	{
		public Phase5GoodsItemPreviousDocumentsGridColumnsBag()
		{
			TypeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_Code, 80);
			ReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_ReferenceNumber, 200);
			ItemNumberCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_ItemNumber, 80);
			NumOfPackagesCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_Quantity2, 80);
			PackageTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_UnitOfQuantity2, 80);
			QuantityCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_Quantity, 80);
			UnitOfQuantityDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_UnitOfQuantity, 80);
			ComplementTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_ReferenceNumber2, 200);
		}

		public static Phase5GoodsItemPreviousDocumentsGridColumnsBag Instance => instance ??= new Phase5GoodsItemPreviousDocumentsGridColumnsBag();

		public IGridColumnReference TypeCodeFindBoxColumn { get; }

		public IGridColumnReference ReferenceNumberTextBoxColumn { get; }

		public IGridColumnReference ItemNumberCalcEditColumn { get; }

		public IGridColumnReference NumOfPackagesCalcEditColumn { get; }

		public IGridColumnReference PackageTypeDropEditColumn { get; }

		public IGridColumnReference QuantityCalcEditColumn { get; }

		public IGridColumnReference UnitOfQuantityDropEditColumn { get; }

		public IGridColumnReference ComplementTextBoxColumn { get; }

		[ThreadStatic]
		static Phase5GoodsItemPreviousDocumentsGridColumnsBag instance;
	}
}
