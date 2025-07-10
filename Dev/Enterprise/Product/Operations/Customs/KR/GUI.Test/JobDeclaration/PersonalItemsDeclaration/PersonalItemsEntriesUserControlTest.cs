using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class PersonalItemsEntriesUserControlTest : TestCaseWithFactory
	{
		public void TestEntryHeaderColumns()
		{
			using (var control = new PersonalItemsEntriesUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true)[0];

				Assert(grid, "FormattedEntryNumber", typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, CusEntryHeader.Schema.CH_MessageType, typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, CusEntryHeader.Schema.MessageStatusDescription, typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, CusEntryHeader.Schema.CH_Status, typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, "MessageStatusDescription", typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, CusEntryHeader.Schema.CH_EntryStatus, typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, "EntryHeaderStatusDescription", typeof(ZTextBoxColumnStyleInfo));
				Assert(grid, CusEntryHeader.Schema.CH_EntrySubmittedDate, typeof(ZDateEditColumnStyleInfo));
			}

			void Assert(ZGrid grid, ZString controlName, Type controlType)
			{
				var checkColumn = grid.GetColumnStyle(controlName);
				AssertNotNull("Column exists", checkColumn);
				AssertEquals(controlType, checkColumn.GetType());
				AssertEquals("IsReadOnly", true, checkColumn.IsReadOnly);
			}
		}
	}
}
