using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemPackingDetailsGridColumnsBagTest : TestCase
	{
		public void TestPackageSequenceTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.PackageSequenceTextBoxColumn);

			var columnInfo = ColumnsBag.PackageSequenceTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "Package+CXP_Sequence", columnInfo.ColumnName);
			});
		}

		public void TestPackageQuantityCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.PackageQuantityCalEditColumn);

			var columnInfo = ColumnsBag.PackageQuantityCalEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "Package+CXP_Quantity", columnInfo.ColumnName);
				AssertNull("BindToDecimalPlaces", ((ZCalcEditColumnStyleInfo)columnInfo).BindToDecimalPlaces);
			});
		}

		public void TestPackageTypeDropEditColumn()
		{
			AssertNotNull(ColumnsBag.PackageTypeDropEditColumn);

			var columnInfo = ColumnsBag.PackageTypeDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "Package+CXP_PackageType", columnInfo.ColumnName);
			});
		}

		public void TestPackageMarksAndNumbersTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.PackageMarksAndNumbersTextBoxColumn);

			var columnInfo = ColumnsBag.PackageMarksAndNumbersTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "Package+CXP_MarksAndNumbers", columnInfo.ColumnName);
			});
		}

		public void TestContainerGuidDropEditColumn()
		{
			AssertNotNull(ColumnsBag.ContainerGuidDropEditColumn);

			var columnInfo = ColumnsBag.ContainerGuidDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZGuidDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZGuidDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CNP_CXN_Container", columnInfo.ColumnName);
				AssertEquals("ShowInDropDown", ShowInDropDownList.OnlyShowCode, ((ZGuidDropEditColumnStyleInfo)columnInfo).ShowInDropDown);
			});
		}

		public void TestPackageMarksAndNumbersStatusDropEditColumn()
		{
			AssertNotNull(ColumnsBag.PackageMarksAndNumbersStatusDropEditColumn);

			var columnInfo = ColumnsBag.PackageMarksAndNumbersStatusDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "Package+CXP_MarksAndNumbersStatus", columnInfo.ColumnName);
			});
		}

		ConsignmentItemPackingDetailsGridColumnsBag ColumnsBag => ConsignmentItemPackingDetailsGridColumnsBag.Instance;
	}
}
