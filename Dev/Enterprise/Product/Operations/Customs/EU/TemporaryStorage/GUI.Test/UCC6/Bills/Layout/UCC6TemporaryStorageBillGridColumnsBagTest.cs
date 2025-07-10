using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStorageBillGridColumnsBagTest : TestCase
{
	public void TestIsMasterCheckBoxColumn()
	{
		AssertNotNull(ColumnsBag.IsMasterCheckBoxColumn);
		var columnInfo = ColumnsBag.IsMasterCheckBoxColumn.CreateGridColumnInfo() as ZCheckBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ABL_Calc_IsMaster", columnInfo.ColumnName);
		AssertEquals("IsReadOnly", true, columnInfo.IsReadOnly);
	}

	public void TestBillDocumentTypeDropEditColumn()
	{
		AssertNotNull(ColumnsBag.BillDocumentTypeDropEditColumn);
		var columnInfo = ColumnsBag.BillDocumentTypeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "TypeOfBillDocument", columnInfo.ColumnName);
	}

	public void TestBillNumberTextBoxColumn()
	{
		AssertNotNull(ColumnsBag.BillNumberTextBoxColumn);
		var columnInfo = ColumnsBag.BillNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ABL_BillNumber", columnInfo.ColumnName);
	}

	public void TestUCRNumberTextBoxColumn()
	{
		AssertNotNull(ColumnsBag.UCRNumberTextBoxColumn);
		var columnInfo = ColumnsBag.UCRNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ABL_UCRNumber", columnInfo.ColumnName);
	}

	public void TestGrossWeightCalcEditColumn()
	{
		AssertNotNull(ColumnsBag.GrossWeightCalcEditColumn);
		var columnInfo = ColumnsBag.GrossWeightCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ABL_GrossWeight", columnInfo.ColumnName);
	}

	public void TestGrossWeightUQDropEditColumn()
	{
		AssertNotNull(ColumnsBag.GrossWeightUQDropEditColumn);
		var columnInfo = ColumnsBag.GrossWeightUQDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ABL_GrossWeightUQ", columnInfo.ColumnName);
	}

	public void TestConsignorOrgFindBoxColumn()
	{
		AssertNotNull(ColumnsBag.ConsignorOrgFindBoxColumn);
		var columnInfo = ColumnsBag.ConsignorOrgFindBoxColumn.CreateGridColumnInfo() as ZOrganisationFindBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ConsignorOrgPK", columnInfo.ColumnName);
	}

	public void TestShipperAddressDropEditColumn()
	{
		AssertNotNull(ColumnsBag.ShipperAddressDropEditColumn);
		var columnInfo = ColumnsBag.ShipperAddressDropEditColumn.CreateGridColumnInfo() as ZAddressDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ABL_OA_Shipper", columnInfo.ColumnName);
	}

	public void TestConsigneeOrgFindBoxColumn()
	{
		AssertNotNull(ColumnsBag.ConsigneeOrgFindBoxColumn);
		var columnInfo = ColumnsBag.ConsigneeOrgFindBoxColumn.CreateGridColumnInfo() as ZOrganisationFindBoxColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ConsigneeOrgPK", columnInfo.ColumnName);
	}

	public void TestConsigneeAddressDropEditColumn()
	{
		AssertNotNull(ColumnsBag.ConsigneeAddressDropEditColumn);
		var columnInfo = ColumnsBag.ConsigneeAddressDropEditColumn.CreateGridColumnInfo() as ZAddressDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "ABL_OA_Consignee", columnInfo.ColumnName);
	}

	UCC6TemporaryStorageBillGridColumnsBag ColumnsBag => UCC6TemporaryStorageBillGridColumnsBag.Instance;
}
