using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ArchiveManager.Business.Schedule;
using Moq;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Schedule
{
	public class NotificationsArchiveLoggerTest : TestCase
	{
		public void TestLogAndReportErrorCallsErrorReporterFirstThenLogsSubsequentErrors()
		{
			var notificationsMock = new Mock<INotifications>();
			var logger = new NotificationsArchiveLogger(notificationsMock.Object);

			ErrorReporter.Clear();
			notificationsMock
				.Setup(m => m.Add(It.IsAny<Notification>()))
				.Callback((INotification n) =>
				{
					AssertEquals("Notification type", NotificationType.Error, n.Type);
					AssertStartsWith("Error message starts with", "ABC|something broke", n.Message);
					AssertEndsWith("Error message ends with", "System.Exception: Exception of type 'System.Exception' was thrown.", n.Message);
				})
				.Verifiable(Times.Exactly(2), "Error should be logged for every subsequent call after the first as ErrorReporter already logs once");

			logger.LogAndReportError("TestAMError", "ABC", "something broke", new Exception());
			logger.LogAndReportError("TestAMError", "ABC", "something broke", new Exception());
			logger.LogAndReportError("TestAMError", "ABC", "something broke", new Exception());

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("ArchiveManager.TestAMError", ErrorReporter.LastKeyReported);
			AssertEquals("ABC|something broke", ErrorReporter.LastMessageReported);

			notificationsMock.VerifyAll();
			ErrorReporter.Clear();
		}
	}
}
