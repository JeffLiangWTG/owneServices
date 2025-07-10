using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class Phase5GoodsItemDetailsGridColumnsBagTest : TestCase
{
	public void TestGrossWeightUnitDropEditColumn()
	{
		AssertNotNull(ColumnsBag.StatusDropEditColumn);
		var columnInfo = ColumnsBag.StatusDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "BY_Status", columnInfo.ColumnName);
	}

	Phase5GoodsItemDetailsGridColumnsBag ColumnsBag => Phase5GoodsItemDetailsGridColumnsBag.Instance;
}
