using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleButtonGridWithBlankDetachMessageTest : TestCase
	{
		public void TestDetachMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var grid = new ZModuleButtonGridWithBlankDetachMessage())
			{
				UnitTestUserNotification.Instance.AddOKAnswer();
				grid.ConfirmDetach(grid.DetachMessage.Caption);
				AssertEquals("Are you sure you want to detach the selected records? New records will be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
