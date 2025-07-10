using System;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SendOriginalCargoReportMessageActionNotifyTest : MessageActionAbstractTest
	{
		public void TestOnNotifyUserOfASuccessfulSend2()
		{
			const string message = "The last message was successfully sent";
			VerifyMessage(message, a => a.NotifyUserOfASuccessfulSend(message));
		}

		public void TestWarnUserAboutSomething2()
		{
			const string message = "Some batch processors which are required for messaging are not currently running";
			VerifyMessage(message, a => a.WarnUserAboutSomething(message, string.Empty));
		}

		public void TestNotifyUserOfAnInvalidOperation2()
		{
			const string message = "Invalid operation";
			VerifyMessage(message, a => a.NotifyUserOfAnInvalidOperation(message));
		}

		public void TestOnContinueWithAction()
		{
			const string message = "OnContinueWithAction";
			VerifyMessage(message, a => a.ContinueWithAction(message, string.Empty));

			var action = GetAction();

			action.SendWithMessageErrors = false;
			Assert("ContinueWithAction is false", !action.ContinueWithAction(message, string.Empty));

			action.SendWithMessageErrors = true;
			Assert("ContinueWithAction is true", action.ContinueWithAction(message, string.Empty));
		}

		protected override BaseMessageAction GetAction() => new SendOriginalCargoReportMessageActionNotify(Notifications);

		NotificationBuffer notifications;
		NotificationBuffer Notifications => notifications ?? (notifications = new NotificationBuffer());

		void VerifyMessage(string expectedMessage, Action<BaseMessageAction> performAction)
		{
			Notifications.Clear();
			Assert("NotificationBuffer doesn't contain message", !Notifications.AsString.Contains(expectedMessage));
			performAction(GetAction());
			Assert("NotificationBuffer contains message", Notifications.AsString.Contains(expectedMessage));
		}
	}
}
