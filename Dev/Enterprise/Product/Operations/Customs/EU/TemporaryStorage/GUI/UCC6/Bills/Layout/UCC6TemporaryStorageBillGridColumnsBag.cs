using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStorageBillGridColumnsBag
{
	public static UCC6TemporaryStorageBillGridColumnsBag Instance => instance ??= new UCC6TemporaryStorageBillGridColumnsBag();

	[ThreadStatic]
	static UCC6TemporaryStorageBillGridColumnsBag instance;

	public UCC6TemporaryStorageBillGridColumnsBag()
	{
		IsMasterCheckBoxColumn = new GridColumnReference<ZCheckBoxColumnStyleInfo>("ABL_Calc_IsMaster", 80, x => x.IsReadOnly = true);
		BillDocumentTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>("TypeOfBillDocument", 80);
		BillNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>("ABL_BillNumber", 80);
		UCRNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>("ABL_UCRNumber", 80);
		GrossWeightCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>("ABL_GrossWeight", 80);
		GrossWeightUQDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>("ABL_GrossWeightUQ", 80);
		ConsignorOrgFindBoxColumn = new GridColumnReference<ZOrganisationFindBoxColumnStyleInfo>("ConsignorOrgPK", 96);
		ShipperAddressDropEditColumn = new GridColumnReference<ZAddressDropEditColumnStyleInfo>("ABL_OA_Shipper", 109);
		ConsigneeOrgFindBoxColumn = new GridColumnReference<ZOrganisationFindBoxColumnStyleInfo>("ConsigneeOrgPK", 97);
		ConsigneeAddressDropEditColumn = new GridColumnReference<ZAddressDropEditColumnStyleInfo>("ABL_OA_Consignee", 111);
	}

	public IGridColumnReference IsMasterCheckBoxColumn { get; }

	public IGridColumnReference BillDocumentTypeDropEditColumn { get; }

	public IGridColumnReference BillNumberTextBoxColumn { get; }

	public IGridColumnReference UCRNumberTextBoxColumn { get; }

	public IGridColumnReference GrossWeightCalcEditColumn { get; }

	public IGridColumnReference GrossWeightUQDropEditColumn { get; }

	public IGridColumnReference ConsignorOrgFindBoxColumn { get; }

	public IGridColumnReference ShipperAddressDropEditColumn { get; }

	public IGridColumnReference ConsigneeOrgFindBoxColumn { get; }

	public IGridColumnReference ConsigneeAddressDropEditColumn { get; }
}
