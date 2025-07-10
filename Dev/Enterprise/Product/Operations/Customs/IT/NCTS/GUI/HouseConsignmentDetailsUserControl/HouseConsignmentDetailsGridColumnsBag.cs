using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class HouseConsignmentDetailsGridColumnsBag
{
	public HouseConsignmentDetailsGridColumnsBag()
	{
		StatusDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsBill.Schema.B0_BillStatus, 50);
		CountryOfDestinationDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(NctsBill.Schema.B0_RN_NKCountryOfDestination, 115);
	}

	public static HouseConsignmentDetailsGridColumnsBag Instance => instance ?? (instance = new HouseConsignmentDetailsGridColumnsBag());

	[ThreadStatic]
	static HouseConsignmentDetailsGridColumnsBag instance;

	public IGridColumnReference StatusDropEditColumn { get; }

	public IGridColumnReference CountryOfDestinationDropEditColumn { get; }
}
