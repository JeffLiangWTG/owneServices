using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class SealsGridColumnsBagTest : TestCase
	{
		public void TestSealNumberTextBoxColumn()
		{
			AssertNotNull(ColumnsBag.SealNumberTextBoxColumn);

			var columnInfo = ColumnsBag.SealNumberTextBoxColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BK_SealNumber", columnInfo.ColumnName);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			});
		}

		public void TestSequenceNumberCalEditColumn()
		{
			AssertNotNull(ColumnsBag.SequenceNumberCalcEditColumn);

			var columnInfo = ColumnsBag.SequenceNumberCalcEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZCalcEditColumnStyleInfo Configuration", () =>
			{
				AssertType<ZCalcEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BK_SequenceNumber", columnInfo.ColumnName);
				AssertNull("BindToDecimalPlaces", ((ZCalcEditColumnStyleInfo)columnInfo).BindToDecimalPlaces);
			});
		}

		public void TestUnloadingStatusDropEditColumn()
		{
			AssertNotNull(ColumnsBag.UnloadingStatusDropEditColumn);

			var columnInfo = ColumnsBag.UnloadingStatusDropEditColumn.CreateGridColumnInfo();
			CombineAssertions("ZTextBoxColumnStyleInfo Configuration", () =>
			{
				AssertType<ZDropEditColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BK_UnloadingState", columnInfo.ColumnName);
			});
		}

		SealsGridColumnsBag ColumnsBag => SealsGridColumnsBag.Instance;
	}
}
