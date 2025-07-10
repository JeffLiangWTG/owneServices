using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class PackageGridControlBagTest : TestCase
	{
		public void TestCSI_SubTypeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.SequenceNumberTextBox);

			var columnInfo = ColumnsBag.SequenceNumberTextBox.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_SequenceNumber", columnInfo.ColumnName);
			});
		}

		public void TestPackageTypeDropEdit()
		{
			AssertNotNull(ColumnsBag.PackageTypeDropEdit);

			var columnInfo = ColumnsBag.PackageTypeDropEdit.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_UnitType", columnInfo.ColumnName);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestNumberOfPackagesCalcEdit()
		{
			AssertNotNull(ColumnsBag.NumberOfPackagesCalcEdit);

			var columnInfo = ColumnsBag.NumberOfPackagesCalcEdit.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_UnitCount", columnInfo.ColumnName);
			});
		}

		public void TestMarksAndNumbersTextBox()
		{
			AssertNotNull(ColumnsBag.MarksAndNumbersTextBox);

			var columnInfo = ColumnsBag.MarksAndNumbersTextBox.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_MarksAndNumbers", columnInfo.ColumnName);
			});
		}

		public void TestPackageIDTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.PackageIDTextBoxColumn);
				var columnInfo = ColumnsBag.PackageIDTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_PackageID", columnInfo.ColumnName);
			});
		}

		public void TestBrandTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.BrandTextBoxColumn);
				var columnInfo = ColumnsBag.BrandTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_Brand", columnInfo.ColumnName);
			});
		}

		public void TestModelTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.ModelTextBoxColumn);
				var columnInfo = ColumnsBag.ModelTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_Model", columnInfo.ColumnName);
			});
		}

		PackageGridControlBag ColumnsBag => PackageGridControlBag.Instance;
	}
}
