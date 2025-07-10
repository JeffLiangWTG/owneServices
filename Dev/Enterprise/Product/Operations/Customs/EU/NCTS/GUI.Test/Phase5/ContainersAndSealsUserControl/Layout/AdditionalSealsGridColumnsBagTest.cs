using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class AdditionalSealsGridColumnsBagTest : TestCase
	{
		public void TestSequenceNumberTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.SequenceNumberTextBoxColumn);
				var columnInfo = ColumnsBag.SequenceNumberTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BK_SequenceNumber", columnInfo.ColumnName);
			});
		}

		public void TestSealNumberTextBoxColumn()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(ColumnsBag.SealNumberTextBoxColumn);
				var columnInfo = ColumnsBag.SealNumberTextBoxColumn.CreateGridColumnInfo();
				AssertNotNull(columnInfo);

				AssertType<ZTextBoxColumnStyleInfo>(columnInfo);
				AssertEquals("ColumnName", "BK_SealNumber", columnInfo.ColumnName);
			});
		}

		AdditionalSealsGridColumnsBag ColumnsBag => AdditionalSealsGridColumnsBag.Instance;
	}
}
