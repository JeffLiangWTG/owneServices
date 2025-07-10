using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class AdditionalDocumentGridColumnsBagTest : TestCase
	{
		public void TestCSI_SubTypeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.CSI_SubTypeDropEditColumn);

			var columnInfo = ColumnsBag.CSI_SubTypeDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_SubType", columnInfo.ColumnName);
			});
		}

		public void TestCSI_SubTypeTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.CSI_SubTypeTextBoxColumn);

			var columnInfo = ColumnsBag.CSI_SubTypeTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_SubType", columnInfo.ColumnName);
			});
		}

		public void TestCSI_CodeFindBoxColumn()
		{
			AssertNotNull(ColumnsBag.CSI_CodeFindBoxColumn);

			var columnInfo = ColumnsBag.CSI_CodeFindBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZCodeFindBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_Code", columnInfo.ColumnName);
			});
		}

		public void TestReferenceNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.ReferenceNumberTextBoxColumn);

			var columnInfo = ColumnsBag.ReferenceNumberTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_ReferenceNumber", columnInfo.ColumnName);
			});
		}

		public void TestItemNumberCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.ItemNumberCalcEditColumn);

			var columnInfo = ColumnsBag.ItemNumberCalcEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_ItemNumber", columnInfo.ColumnName);
			});
		}

		public void TestStatusDropEditColumn()
		{
			AssertNotNull(ColumnsBag.StatusDropEditColumn);

			var columnInfo = ColumnsBag.StatusDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_Status", columnInfo.ColumnName);
			});
		}

		AdditionalDocumentGridColumnsBag ColumnsBag => AdditionalDocumentGridColumnsBag.Instance;
	}
}
