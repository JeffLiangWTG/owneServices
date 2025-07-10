using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class AUCOLSEntryCreationMessageUserControlTest : TestCaseWithFactory
	{
		public void TestAUCOLSEntryCreationMessage()
		{
			using (var userControl = new AUCOLSEntryCreationMessageUserControl())
			{
				var expectedMessageText = "A COLS Entry could not be found.\r\nPlease change to another tab, then click back to this tab to create a COLS Entry.";
				AssertEquals("COLS entry creation message text when control is loaded", expectedMessageText, userControl.MessagingLabel.Text);
			}
		}
	}
}
