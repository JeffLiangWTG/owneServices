using System;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class ProcessStartFinishNotifierTest : TestCase
	{
		public void TestNotificationsWithoutStartFinishTime()
		{
			CognosNotificationBufferForTest notifications = new CognosNotificationBufferForTest();
			AssertEquals("Pre-condition", "", notifications.AsString);
			using (new ProcessStartFinishNotifier(notifications, "MEH MEH"))
			{
				notifications.Notify(new InfoNotification("Progressing"));
			}

			AssertEquals(@"
Start MEH MEH
Progressing
Finish MEH MEH
".TrimStart(), notifications.AsString);
			notifications.Clear();
			using (new ProcessStartFinishNotifier(notifications, "MEH MEH", false, true))
			{
				notifications.Notify(new InfoNotification("Progressing"));
			}

			AssertEquals(@"
Start MEH MEH
Progressing
Finish MEH MEH

".TrimStart(), notifications.AsString);
		}

		[TestDate(2005, 1, 10, 23, 34, 55)]
		public void TestNotificationsWithStartFinishTime()
		{
			CognosNotificationBufferForTest notifications = new CognosNotificationBufferForTest();
			AssertEquals("Pre-condition", "", notifications.AsString);
			using (new ProcessStartFinishNotifier(notifications, "MEH MEH", true, false))
			{
				notifications.Notify(new InfoNotification("Progressing"));
				TestDateAttribute.Date = new DateTime(2005, 1, 10, 23, 34, 58);
			}

			AssertEquals(@"
Start MEH MEH - 10/01/2005 23:34:55
Progressing
Finish MEH MEH - 10/01/2005 23:34:58
".TrimStart(), notifications.AsString);
			notifications.Clear();
			using (new ProcessStartFinishNotifier(notifications, "MEH MEH", true, true))
			{
				notifications.Notify(new InfoNotification("Progressing"));
			}

			AssertEquals(@"
Start MEH MEH - 10/01/2005 23:34:58
Progressing
Finish MEH MEH - 10/01/2005 23:34:58

".TrimStart(), notifications.AsString);
		}

		public void TestNotificationsWhenThereIsErrorNotification()
		{
			CognosNotificationBufferForTest notifications = new CognosNotificationBufferForTest();
			AssertEquals("Pre-condition", "", notifications.AsString);
			using (new ProcessStartFinishNotifier(notifications, "MEH MEH"))
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "Something wrong"));
			}

			AssertEquals(@"
Start MEH MEH
Error: Something wrong
".TrimStart(), notifications.AsString);
		}
	}
}
