using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class SupportingDocumentsGridColumnsBagTest : TestCase
	{
		public void TestSequenceNumberCalcEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.SequenceNumberCalcEditColumn);
				var columnInfo = ColumnsBag.SequenceNumberCalcEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_LineNo", columnInfo.ColumnName);
				AssertEquals("Width", 80, columnInfo.Width);
			});
		}

		public void TestCodeCodeFindBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.CodeCodeFindBoxColumn);
				var columnInfo = ColumnsBag.CodeCodeFindBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_Code", columnInfo.ColumnName);
				AssertEquals("Width", 80, columnInfo.Width);
			});
		}

		public void TestReferenceNumberTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.ReferenceNumberTextBoxColumn);
				var columnInfo = ColumnsBag.ReferenceNumberTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_ReferenceNumber", columnInfo.ColumnName);
				AssertEquals("Width", 200, columnInfo.Width);
			});
		}

		public void ItemNumberCalcEditColumn()
		{
			CombineAssertions(() =>
				{
				AssertNotNull(ColumnsBag.ItemNumberCalcEditColumn);
				var columnInfo = ColumnsBag.ItemNumberCalcEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_ItemNumber", columnInfo.ColumnName);
				AssertEquals("Width", 80, columnInfo.Width);
			});
		}

		public void TestReferenceNumber2TextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.ReferenceNumber2TextBoxColumn);
				var columnInfo = ColumnsBag.ReferenceNumber2TextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_ReferenceNumber2", columnInfo.ColumnName);
				AssertEquals("Width", 200, columnInfo.Width);
			});
		}

		SupportingDocumentsGridColumnsBag ColumnsBag => SupportingDocumentsGridColumnsBag.Instance;
	}
}
