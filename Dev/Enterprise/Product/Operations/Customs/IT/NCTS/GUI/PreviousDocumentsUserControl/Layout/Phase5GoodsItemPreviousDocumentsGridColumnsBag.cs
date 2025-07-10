using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5GoodsItemPreviousDocumentsGridColumnsBag
{
	public Phase5GoodsItemPreviousDocumentsGridColumnsBag()
	{
		TypeCodeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_Code, 80);
		NumOfPackagesCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_PackQty, 80);
		PackageTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_PackType, 80);
	}

	public static Phase5GoodsItemPreviousDocumentsGridColumnsBag Instance => instance ??= new Phase5GoodsItemPreviousDocumentsGridColumnsBag();

	public IGridColumnReference TypeCodeDropEditColumn { get; }

	public IGridColumnReference NumOfPackagesCalcEditColumn { get; }

	public IGridColumnReference PackageTypeDropEditColumn { get; }

	[ThreadStatic]
	static Phase5GoodsItemPreviousDocumentsGridColumnsBag instance;
}
