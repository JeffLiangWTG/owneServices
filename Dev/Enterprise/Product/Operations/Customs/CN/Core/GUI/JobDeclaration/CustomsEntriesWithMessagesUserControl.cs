using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CN.GUI
{
	internal class CustomsEntriesWithMessagesUserControl : EntriesWithMessagesOnDeclarationUserControl
	{
		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new CustomsEntriesAndEntryLinesUserControl();
		}
	}
}
