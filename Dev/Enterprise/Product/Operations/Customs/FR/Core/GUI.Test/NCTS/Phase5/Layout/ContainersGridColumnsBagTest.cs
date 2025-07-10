using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class ContainersGridColumnsBagTest : TestCase
	{
		public void TestTypeContainerFindBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.TypeContainerFindBoxColumn);
				var columnInfo = ColumnsBag.TypeContainerFindBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZGuidFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BC_RC", columnInfo.ColumnName);
			});
		}

		public void TestSequenceNumberTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.SequenceNumberTextBoxColumn);
				var columnInfo = ColumnsBag.SequenceNumberTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BC_SequenceNumber", columnInfo.ColumnName);
			});
		}

		public void TestModeDropEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.ModeDropEditColumn);
				var columnInfo = ColumnsBag.ModeDropEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("ColumnName", "BC_Mode", columnInfo.ColumnName);
			});
		}

		public void TestContainerNumTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.ContainerNumTextBoxColumn);
				var columnInfo = ColumnsBag.ContainerNumTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BC_ContainerNum", columnInfo.ColumnName);
			});
		}

		public void TestSeal1TextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.Seal1TextBoxColumn);
				var columnInfo = ColumnsBag.Seal1TextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "Seal1", columnInfo.ColumnName);
			});
		}

		public void TestSeal2TextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.Seal2TextBoxColumn);
				var columnInfo = ColumnsBag.Seal2TextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "Seal2", columnInfo.ColumnName);
			});
		}

		public void TestTotalSealCountCalcEditColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.TotalSealCountCalcEditColumn);
				var columnInfo = ColumnsBag.TotalSealCountCalcEditColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "TotalSealCount", columnInfo.ColumnName);
			});
		}

		ContainersGridColumnsBag ColumnsBag => ContainersGridColumnsBag.Instance;
	}
}
