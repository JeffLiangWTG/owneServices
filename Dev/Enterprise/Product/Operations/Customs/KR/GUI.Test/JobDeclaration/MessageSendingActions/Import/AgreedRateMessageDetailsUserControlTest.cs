using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class AgreedRateMessageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumn()
		{
			using (var userControl = new AgreedRateMessageDetailsUserControl())
			{
				var grid = userControl.DetailsGrid;

				var index = 0;
				AssertEquals(((ZCalcEditColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.EntryLineNo));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.FormattedHSCode));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.HSDescription));
				AssertEquals(((ZTextBoxColumnStyleInfo)grid.ColumnStyles[index++]).ColumnName, nameof(MessageSendingEntryLineObject.ModelName));
			}
		}
	}
}
