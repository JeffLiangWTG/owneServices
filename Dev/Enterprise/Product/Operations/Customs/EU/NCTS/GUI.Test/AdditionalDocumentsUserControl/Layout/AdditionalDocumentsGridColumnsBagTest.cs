using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class AdditionalDocumentsGridColumnsBagTest : TestCase
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

		public void TestSubTypeDropEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.SubTypeDropEditColumn);
				var columnInfo = ColumnsBag.SubTypeDropEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_SubType", columnInfo.ColumnName);
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
				AssertEquals("Width", 60, columnInfo.Width);
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
				AssertEquals("Width", 150, columnInfo.Width);
			});
		}

		public void TestDescriptionTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.DescriptionTextBoxColumn);
				var columnInfo = ColumnsBag.DescriptionTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CSI_Description", columnInfo.ColumnName);
				AssertEquals("Width", 300, columnInfo.Width);
			});
		}

		AdditionalDocumentsGridColumnsBag ColumnsBag => AdditionalDocumentsGridColumnsBag.Instance;
	}
}
