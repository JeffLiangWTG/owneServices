using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemPreviousDocumentsGridColumnsBagTest : TestCase
	{
		public void TestTypeCodeFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.TypeCodeFindBoxColumn);
			var columnInfo = ColumnsBag.TypeCodeFindBoxColumn.CreateGridColumnInfo() as ZCodeFindBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_Code", columnInfo.ColumnName);
		}

		public void TestReferenceNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.ReferenceNumberTextBoxColumn);
			var columnInfo = ColumnsBag.ReferenceNumberTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_ReferenceNumber", columnInfo.ColumnName);
		}

		public void TestItemNumberCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.ItemNumberCalcEditColumn);
			var columnInfo = ColumnsBag.ItemNumberCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_ItemNumber", columnInfo.ColumnName);
		}

		public void TestNumOfPackagesCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.NumOfPackagesCalcEditColumn);
			var columnInfo = ColumnsBag.NumOfPackagesCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_Quantity2", columnInfo.ColumnName);
		}

		public void TestPackageTypeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.PackageTypeDropEditColumn);
			var columnInfo = ColumnsBag.PackageTypeDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_UnitOfQuantity2", columnInfo.ColumnName);
		}

		public void TestQuantityCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.QuantityCalcEditColumn);
			var columnInfo = ColumnsBag.QuantityCalcEditColumn.CreateGridColumnInfo() as ZCalcEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_Quantity", columnInfo.ColumnName);
		}

		public void TestUnitOfQuantityDropEditColumn()
		{
			AssertNotNull(ColumnsBag.UnitOfQuantityDropEditColumn);
			var columnInfo = ColumnsBag.UnitOfQuantityDropEditColumn.CreateGridColumnInfo() as ZDropEditColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_UnitOfQuantity", columnInfo.ColumnName);
		}

		public void TestComplementTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.ComplementTextBoxColumn);
			var columnInfo = ColumnsBag.ComplementTextBoxColumn.CreateGridColumnInfo() as ZTextBoxColumnStyleInfo;
			AssertNotNull(columnInfo);
			AssertEquals("ColumnName", "CSI_ReferenceNumber2", columnInfo.ColumnName);
		}

		Phase5GoodsItemPreviousDocumentsGridColumnsBag ColumnsBag => Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;
	}
}
