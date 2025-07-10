using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	sealed class SendNotificationEmailHelperTest : TestCase
	{
		public void TestDailySendNotificationEmailOnce()
		{
			SendNotificationEmailHelper.Instance.LastSendNotificationTimes_ExposedForTest.Clear();
			var emails = new List<string>();
			SendNotificationEmailHelper.Instance.SendNotificationEmail += (sender, e) => emails.Add(e.Subject + " " + e.Body);

			SendNotificationEmailHelper.DailySendNotificationEmailOnce("TestObject", "TestBody1");
			SendNotificationEmailHelper.DailySendNotificationEmailOnce("TestObject", "TestBody2");

			AssertEquals("Send the email only once", 1, emails.Count);
			AssertEquals("TestObject TestBody1", emails[0]);

			SendNotificationEmailHelper.Instance.SetLastSendNotificationTimeForTest("TestObject2", DateTime.UtcNow.AddDays(-1));
			AssertEquals("LastSendNotificationTimes should have 2 items", 2, SendNotificationEmailHelper.Instance.LastSendNotificationTimes_ExposedForTest.Count);

			SendNotificationEmailHelper.Instance.SetLastSendNotificationTimeForTest("TestObject", DateTime.UtcNow.AddDays(-1));
			SendNotificationEmailHelper.DailySendNotificationEmailOnce("TestObject", "TestBody3");
			SendNotificationEmailHelper.DailySendNotificationEmailOnce("TestObject", "TestBody4");
			AssertEquals("Email be sent again after more than one day", 2, emails.Count);
			AssertEquals("TestObject TestBody3", emails[1]);
			AssertEquals("LastSendNotificationTimes should be clean", 1, SendNotificationEmailHelper.Instance.LastSendNotificationTimes_ExposedForTest.Count);
			AssertEquals("TestObject", SendNotificationEmailHelper.Instance.LastSendNotificationTimes_ExposedForTest.First().Key);
		}

		public void TestDailySendNotificationEmailOnce_ReportError()
		{
			var errors = new List<string>();
			var emails = new List<string>();
			SendNotificationEmailHelper.Instance.SendNotificationEmail += (sender, e) => emails.Add(e.Subject + " " + e.Body);
			SendNotificationEmailHelper.Instance.ReportError += (sender, e) => errors.Add(e.Key + " " + e.Message + e.Exception);

			SendNotificationEmailHelper.DailySendNotificationEmailOnce(null, "TestBody1");
			SendNotificationEmailHelper.DailySendNotificationEmailOnce("", "TestBody2");

			AssertEquals("Report error when subject is null or empty", 2, errors.Count);
		}
	}
}
