using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class UserNotificationServiceTest : TestCaseWithFactory
	{
		public void TestQueryUserResponse_Message()
		{
			var service = new UserNotificationService();

			service.QueryUserResponse("message", "caption", 1, 100);

			AssertEquals("message text", "message", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("caption text", "caption", UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		public void TestQueryUserResponse_List()
		{
			var formShown = false;

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f => formShown = f is UserDropDownConfirmationDialog);

			var list = new CodeDescriptionPairList();
			list.AddPair("aaa", "aaa desc");
			list.AddPair("bbb", "bbb desc");
			list.AddPair("ccc", "ccc desc");

			var service = new UserNotificationService();
			service.QueryUserResponse("message", "caption", list);

			Assert("UserDropDownConfirmationDialog has been shown", formShown);
		}
	}
}