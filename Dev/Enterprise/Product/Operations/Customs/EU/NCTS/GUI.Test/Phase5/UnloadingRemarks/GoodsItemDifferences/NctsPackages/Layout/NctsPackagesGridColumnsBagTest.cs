using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class NctsPackagesGridColumnsBagTest : TestCase
	{
		public void TestSequenceNumberCalcEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.SequenceNumberCalcEditColumn);
				var columnInfo = ColumnsBag.SequenceNumberCalcEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_SequenceNumber", columnInfo.ColumnName);
				AssertEquals("ReadOnly", true, columnInfo.IsReadOnly);
			});
		}

		public void TestTypeOfDifferenceDropEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.TypeOfDifferenceDropEditColumn);
				var columnInfo = ColumnsBag.TypeOfDifferenceDropEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_TypeOfDifference", columnInfo.ColumnName);
			});
		}

		public void TestUnitCountCalcEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.UnitCountCalcEditColumn);
				var columnInfo = ColumnsBag.UnitCountCalcEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_UnitCount", columnInfo.ColumnName);
			});
		}

		public void TestUnitTypeDropEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.UnitTypeDropEditColumn);
				var columnInfo = ColumnsBag.UnitTypeDropEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_UnitType", columnInfo.ColumnName);
			});
		}

		public void TestMarksAndNumbersTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.MarksAndNumbersTextBoxColumn);
				var columnInfo = ColumnsBag.MarksAndNumbersTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

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

		public void TestGrossWeightCalcEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.GrossWeightCalcEditColumn);
				var columnInfo = ColumnsBag.GrossWeightCalcEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_GrossWeight", columnInfo.ColumnName);

				var grossWeightCalcEdit = (ZCalcEditColumnStyleInfo)columnInfo;
				AssertEquals("Decimals", 5, grossWeightCalcEdit.Decimals);
			});
		}

		public void TestGrossWeightUQDropEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.GrossWeightUQDropEditColumn);
				var columnInfo = ColumnsBag.GrossWeightUQDropEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "B5_GrossWeightUQ", columnInfo.ColumnName);
			});
		}

		NctsPackagesGridColumnsBag ColumnsBag => NctsPackagesGridColumnsBag.Instance;
	}
}
