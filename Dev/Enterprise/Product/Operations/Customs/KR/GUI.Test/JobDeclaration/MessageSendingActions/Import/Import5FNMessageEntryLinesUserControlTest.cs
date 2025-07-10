using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class Import5FNMessageEntryLinesUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumn()
		{
			using (var userControl = new Import5FNMessageEntryLinesUserControl())
			{
				var grid = userControl.EntryLinesGrid;

				var index = 0;
				AssertEquals(((ZCheckBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.ShouldSend));
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.EntryLineNo));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.DutyReduction));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.HSCode));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.ModelName));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.SerialNumber));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.DutyReductionCode));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.InstalmentCode));
				AssertEquals(((ZCheckBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.SpecificUse));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.PostClearanceYN));
			}
		}
	}
}
