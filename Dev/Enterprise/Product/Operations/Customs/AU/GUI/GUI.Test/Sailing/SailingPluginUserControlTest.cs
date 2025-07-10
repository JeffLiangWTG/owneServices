using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Sailing.GUI.Testing
{
	sealed class SailingPluginUserControlTest : TestCaseWithFactory
	{
		public void TestActualArrivalMessageUserControlBinding()
		{
			using (var control = new SailingPluginUserControl())
			{
				AssertEquals("MessagesGrid.BindTo", "Destinations.Messages", control.actualArrivalMessageUserControl.MessagesGrid.BindTo);
				AssertEquals("MessageTextTextBox.BindTo", "Destinations.Messages.EM_FormattedMessageText", control.actualArrivalMessageUserControl.MessageTextTextBox.BindTo);
			}
		}
	}
}
