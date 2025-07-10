using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemsGridColumnsBagTest	: TestCase
	{
		public void TestLineNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.LineNumberTextBoxColumn);

			var columnInfo = ColumnsBag.LineNumberTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CCI_LineNumber", columnInfo.ColumnName);
			});
		}

		public void TestGrossMassCalEditColumn()
		{
			AssertNotNull(ColumnsBag.GrossMassCalEditColumn);

			var columnInfo = ColumnsBag.GrossMassCalEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CCI_GrossMass", columnInfo.ColumnName);
			});
		}

		public void TestNetMassCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.NetMassCalcEditColumn);

			var columnInfo = ColumnsBag.NetMassCalcEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CCI_NetMass", columnInfo.ColumnName);
			});
		}

		public void TestUniqueConsignmentReferenceTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.UniqueConsignmentReferenceTextBoxColumn);

			var columnInfo = ColumnsBag.UniqueConsignmentReferenceTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CCI_UniqueConsignmentReference", columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestUniqueConsignmentReferenceStatusDropEditColumn()
		{
			AssertNotNull(ColumnsBag.UniqueConsignmentReferenceStatusDropEditColumn);

			var columnInfo = ColumnsBag.UniqueConsignmentReferenceStatusDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CCI_UniqueConsignmentReferenceStatus", columnInfo.ColumnName);
			});
		}

		public void TestDiscrepancyStatusDropEditColumn()
		{
			AssertNotNull(ColumnsBag.DiscrepancyStatusDropEditColumn);

			var columnInfo = ColumnsBag.DiscrepancyStatusDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CCI_DiscrepancyStatus", columnInfo.ColumnName);
			});
		}

		ConsignmentItemsGridColumnsBag ColumnsBag => ConsignmentItemsGridColumnsBag.Instance;
	}
}
