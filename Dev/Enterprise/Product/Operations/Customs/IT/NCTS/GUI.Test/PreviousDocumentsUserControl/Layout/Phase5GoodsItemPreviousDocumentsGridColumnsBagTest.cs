using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class Phase5GoodsItemPreviousDocumentsGridColumnsBagTest : TestCase
{
	public void TestTypeCodeFindBoxColumn()
	{
		AssertNotNull(ColumnsBag.TypeCodeDropEditColumn);
		var columnInfo = ColumnsBag.TypeCodeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_Code", columnInfo.ColumnName);
	}

	public void TestNumOfPackagesCalcEditColumn()
	{
		AssertNotNull(ColumnsBag.NumOfPackagesCalcEditColumn);
		var columnInfo = ColumnsBag.NumOfPackagesCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_PackQty", columnInfo.ColumnName);
	}

	public void TestPackageTypeDropEditColumn()
	{
		AssertNotNull(ColumnsBag.PackageTypeDropEditColumn);
		var columnInfo = ColumnsBag.PackageTypeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "CSI_PackType", columnInfo.ColumnName);
	}

	Phase5GoodsItemPreviousDocumentsGridColumnsBag ColumnsBag => Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;
}
