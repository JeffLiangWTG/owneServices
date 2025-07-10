using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentDetailsGridColumnsBag))]
sealed class HouseConsignmentDetailsGridColumnsBagTest : TestCase
{
	public void TestStatusDropEditColumn()
	{
		AssertNotNull(ColumnsBag.StatusDropEditColumn);

		var columnInfo = ColumnsBag.StatusDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "B0_BillStatus", columnInfo.ColumnName);
	}

	public void TestCountryOfDestinationDropEditColumn()
	{
		AssertNotNull(ColumnsBag.CountryOfDestinationDropEditColumn);
		var columnInfo = ColumnsBag.CountryOfDestinationDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", "B0_RN_NKCountryOfDestination", columnInfo.ColumnName);
	}

	HouseConsignmentDetailsGridColumnsBag ColumnsBag => HouseConsignmentDetailsGridColumnsBag.Instance;
}
