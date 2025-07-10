using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class EntryMessageUserControlTest : TestCaseWithFactory
	{
		public void TestNewImportMessageUserControl()
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
