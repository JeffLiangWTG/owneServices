using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class UserNotificationBaseTest : TestCase
	{
		public void TestShowDeveloperErrorAlways()
		{
			DummyUserNotification notification = new DummyUserNotification();

			notification.ShowDeveloperErrorAlways("Message1", "Caption1");
			AssertEquals("LastKey", "", notification.LastKey);
			AssertEquals("LastMessage", "Caption1", notification.LastMessage);
			AssertEquals("LastException.GetType()", typeof(DeveloperNotificationException), notification.LastException.GetType());
			AssertEquals("LastException.Message", "Message1", notification.LastException.Message);

			notification.ShowDeveloperErrorAlways("Key", "Message2", "Caption2");
			AssertEquals("LastKey", "Key", notification.LastKey);
			AssertEquals("LastMessage", "Caption2", notification.LastMessage);
			AssertEquals("LastException.GetType()", typeof(DeveloperNotificationException), notification.LastException.GetType());
			AssertEquals("LastException.Message", "Message2", notification.LastException.Message);
		}

		#region class DummyUserNotification

		class DummyUserNotification : UserNotificationBase
		{
			public override void ShowDeveloperException(string key, string message, Exception e)
			{
				LastKey = key;
				LastMessage = message;
				LastException = e;
			}

			public override void ShowError(string message, string caption)
			{
			}

			public string LastKey;
			public string LastMessage;
			public Exception LastException;
		}

		#endregion
	}
}
