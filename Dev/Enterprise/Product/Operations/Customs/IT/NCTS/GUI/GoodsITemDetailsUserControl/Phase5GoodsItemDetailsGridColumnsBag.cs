using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5GoodsItemDetailsGridColumnsBag
{
	public Phase5GoodsItemDetailsGridColumnsBag()
	{
		StatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_Status, 50);
	}

	public static Phase5GoodsItemDetailsGridColumnsBag Instance => instance ?? (instance = new Phase5GoodsItemDetailsGridColumnsBag());

	public IGridColumnReference StatusDropEditColumn { get; }

	[ThreadStatic]
	static Phase5GoodsItemDetailsGridColumnsBag instance;
}
