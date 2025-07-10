using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class EntryMessageUserControlTest : TestCaseWithFactory
	{
		public void TestNewMessageUserControl()
		{
			using (var control = new EntryMessageUserControl())
			{
				var messageUserControl = control.NewMessageUserControl;
				AssertType<MessageUserControl>(messageUserControl);
				messageUserControl.Dispose();
			}
		}
	}
}
