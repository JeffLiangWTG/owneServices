using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	sealed class Phase5GoodsItemPreviousDocumentsGridColumnsBagTest : TestCase
	{
		public void TestReferenceNumberMultiControlColumn()
		{
			AssertNotNull(ColumnsBag.ReferenceNumberMultiControlColumn);
			var columnInfo = ColumnsBag.ReferenceNumberMultiControlColumn.CreateGridColumnInfo() as ZMultiControlColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_ReferenceNumber", columnInfo.ColumnName);
		}

		Phase5GoodsItemPreviousDocumentsGridColumnsBag ColumnsBag => Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;
	}
}
