using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Core.Environment
{
	sealed class UserNotificationTest : TestCase
	{
		public void TestIsInteractive()
		{
			AssertEquals("Is Interactive", true, Notification.IsInteractive);
		}

		public void TestQueryDefaultValue()
		{
			var
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 0);
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 1);
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 1, 1);
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 0, 5);
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 1, 5);
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 5, 5);
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 6, 5);
			answerIsAlwaysBlank = Notification.QueryDefaultValue("123", "Check this", "Please check this", 10, true, "Send");
			Assert("This test ran without blowing up... but it did not create any popups (they are suppressed during tests) so it's a bit pointless really", true);
		}

		public void TestFromINotificationGoesThrough()
		{
			var notification = new Notification(CargoWise.ComponentModel.NotificationType.Information, "message");
			Notification.Show(notification);
			AssertEquals("message", UnitTestUserNotification.Instance.LastMessage.Text);

			notification = new Notification(CargoWise.ComponentModel.NotificationType.Warning, "warning");
			Notification.Show(notification);
			AssertEquals("warning", UnitTestUserNotification.Instance.LastMessage.Text);

			notification = new Notification(CargoWise.ComponentModel.NotificationType.Error, "error");
			Notification.Show(notification);
			AssertEquals("error", UnitTestUserNotification.Instance.LastMessage.Text);

			Notification.Show("Please Accept or Deny the Amendment Request", "", ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Information, "Accept", "Deny");
			AssertEquals("Please Accept or Deny the Amendment Request", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowDeveloperErrorAlways()
		{
			try
			{
				var expectedMessage = "Developer error message";
				Notification.ShowDeveloperErrorAlways(expectedMessage, "Caption");
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

				var e = ExceptionReporterTestListener.Instance[0];
				AssertEquals(typeof(DeveloperNotificationException), e.GetType());
				Assert("Exception message should start with message", e.Message.StartsWith(expectedMessage));
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowDeveloperErrorOnce()
		{
			try
			{
				Notification.ShowDeveloperErrorOnce("TestKey", "Test Message", "Caption");
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

				Notification.ShowDeveloperErrorOnce("TestKey", "Different Test Message", "Different Caption");
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

				Notification.ShowDeveloperErrorOnce("DifferentTestKey", "Test Message", "Caption");
				AssertEquals(2, ExceptionReporterTestListener.Instance.Count);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowDeveloperException()
		{
			try
			{
				Exception expected = new ArgumentException("Developer error message");
				Notification.ShowDeveloperException(expected.Message, expected);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(expected, ExceptionReporterTestListener.Instance[0]);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowDeveloperExceptionOnce()
		{
			try
			{
				var ex = new Exception("test exception");
				Notification.ShowDeveloperExceptionOnce("TestKey", "Test Message", ex);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

				Notification.ShowDeveloperExceptionOnce("TestKey", "Different Test Message", ex);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

				Notification.ShowDeveloperExceptionOnce("DifferentTestKey", "Test Message", ex);
				AssertEquals(2, ExceptionReporterTestListener.Instance.Count);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowDeveloperExceptionWithMessage()
		{
			try
			{
				Exception expected = new ArgumentException("Developer error message");
				Notification.ShowDeveloperException(expected.Message, expected);
				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(expected, ExceptionReporterTestListener.Instance[0]);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShowError_MultiThreading()
		{
			var lastMessageReported = string.Empty;
			Exception threadException = null;

			var thread = new Thread(() =>
			{
				try
				{
					ErrorReporter.Clear();

					Notification.ShowError("bla");

					lastMessageReported = ErrorReporter.LastMessageReported;
					ErrorReporter.Clear();
				}
				catch (Exception ex)
				{
					threadException = ex;
				}
			});

			thread.Start();
			thread.Join();

			AssertNull(threadException);
			AssertEquals(string.Empty, lastMessageReported);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Notification = new UserNotification();
		}

		UserNotification Notification;

		#endregion
	}
}
