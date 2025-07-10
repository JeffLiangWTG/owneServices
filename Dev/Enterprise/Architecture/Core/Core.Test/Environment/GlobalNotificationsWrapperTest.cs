using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class GlobalNotificationsWrapperTest : TestCaseWithFactory
	{
		public void TestIsINotifications()
		{
			Assert("GlobalNotificationsWrapper implements INotifications.", GlobalNotificationsWrapper.Instance is INotifications);
		}

		public void TestNotifications_Error()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var notify = (INotifications)GlobalNotificationsWrapper.Instance;
			notify.AddError("This is error.");

			Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Notification message is correct.", "This is error.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestNotifications_Info()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var notify = (INotifications)GlobalNotificationsWrapper.Instance;
			notify.Add(NotificationType.Information, "This is info.");

			Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("Notification message is correct.", "This is info.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestNotifications_Warning()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var notify = (INotifications)GlobalNotificationsWrapper.Instance;
			notify.AddWarning("This is warning.");

			Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertEquals("Notification message is correct.", "This is warning.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
