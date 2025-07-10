using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ContainersOrEquipmentsGridColumnsBagTest : TestCase
	{
		public void TestContainerNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.ContainerNumberTextBoxColumn);

			var columnInfo = ColumnsBag.ContainerNumberTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CXN_ContainerNumber", columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestIsEquipmentCheckBoxColumn()
		{
			AssertNotNull(ColumnsBag.IsEquipmentCheckBoxColumn);

			var columnInfo = ColumnsBag.IsEquipmentCheckBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZCheckBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCheckBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CXN_IsEquipment", columnInfo.ColumnName);
			});
		}

		public void TestSequenceCalEditColumn()
		{
			AssertNotNull(ColumnsBag.SequenceCalcEditColumn);

			var columnInfo = ColumnsBag.SequenceCalcEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CXN_Sequence", columnInfo.ColumnName);
			});
		}

		public void TestStatusDropEditColumn()
		{
			AssertNotNull(ColumnsBag.StatusDropEditColumn);

			var columnInfo = ColumnsBag.StatusDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZDropEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CXN_Status", columnInfo.ColumnName);
			});
		}

		public void TestSealCountCalcEditColumn()
		{
			AssertNotNull(ColumnsBag.SealCountCalcEditColumn);

			var columnInfo = ColumnsBag.SealCountCalcEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "CXN_SealCount", columnInfo.ColumnName);
			});
		}

		ContainersOrEquipmentsGridColumnsBag ColumnsBag => ContainersOrEquipmentsGridColumnsBag.Instance;
	}
}
