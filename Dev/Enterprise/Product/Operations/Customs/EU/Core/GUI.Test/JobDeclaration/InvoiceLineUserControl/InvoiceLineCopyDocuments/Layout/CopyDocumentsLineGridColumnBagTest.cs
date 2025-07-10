using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class CopyDocumentsLineGridColumnBagTest : TestCase
	{
		public void TestCodeColumn()
		{
			var columnInfo = ColumnsBag.CodeColumn.CreateGridColumnInfo();
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", AutoCopyDocumentsSelectionLine.Schema.CSI_Code, columnInfo.ColumnName);
				AssertEquals("Width", 80, columnInfo.Width);
			});
		}

		public void TestSubTypeColumn()
		{
			var columnInfo = ColumnsBag.SubTypeColumn.CreateGridColumnInfo();
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", AutoCopyDocumentsSelectionLine.Schema.CSI_SubType, columnInfo.ColumnName);
				AssertEquals("Width", 80, columnInfo.Width);
			});
		}

		public void TestTypeColumn()
		{
			var columnInfo = ColumnsBag.TypeColumn.CreateGridColumnInfo();
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", AutoCopyDocumentsSelectionLine.Schema.CSI_Type, columnInfo.ColumnName);
				AssertEquals("Width", 80, columnInfo.Width);
			});
		}

		public void TestReferenceNumberColumn()
		{
			var columnInfo = ColumnsBag.ReferenceNumberColumn.CreateGridColumnInfo();
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", AutoCopyDocumentsSelectionLine.Schema.CSI_ReferenceNumber, columnInfo.ColumnName);
				AssertEquals("Width", 120, columnInfo.Width);
			});
		}

		public void TestReferenceNumber2Column()
		{
			var columnInfo = ColumnsBag.ReferenceNumber2Column.CreateGridColumnInfo();
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", AutoCopyDocumentsSelectionLine.Schema.CSI_ReferenceNumber2, columnInfo.ColumnName);
				AssertEquals("Width", 120, columnInfo.Width);
			});
		}

		public void TestDescriptionColumn()
		{
			var columnInfo = ColumnsBag.DescriptionColumn.CreateGridColumnInfo();
			CombineAssertions(() =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", AutoCopyDocumentsSelectionLine.Schema.CSI_Description, columnInfo.ColumnName);
				AssertEquals("Width", 120, columnInfo.Width);
			});
		}

		public void TestIsSelectedColumn()
		{
			var columnInfo = ColumnsBag.IsSelectedColumn.CreateGridColumnInfo();
			CombineAssertions(() =>
			{
				AssertType<ZCheckBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", AutoCopyDocumentsSelectionLine.Schema.IsSelected, columnInfo.ColumnName);
				AssertEquals("Width", 40, columnInfo.Width);
			});
		}

		CopyDocumentsLineGridColumnBag ColumnsBag => CopyDocumentsLineGridColumnBag.Instance;
	}
}
