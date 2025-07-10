#if DEBUG
using System;
using Moq;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	class NotificationsDecoratorTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NotificationsDecorator(null));
		}

		public void TestAdd()
		{
			var notificationMock = new Mock<INotifications>();
			notificationMock.Object.AddError("Something we don't care about");

			var decoratedNotifications = new NotificationsDecorator(notificationMock.Object);

			AssertEquals(false, decoratedNotifications.ReportedFatalError);

			decoratedNotifications.AddWarning("Warning Message");

			AssertEquals(false, decoratedNotifications.ReportedFatalError);
			notificationMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Message == "Warning Message")));

			decoratedNotifications.AddError("Error Message");

			AssertEquals(true, decoratedNotifications.ReportedFatalError);
			notificationMock.Verify(n => n.Add(It.Is<INotification>(notif => notif.Message == "Error Message")));

			var message = @"Warning Message
Error Message";
			AssertEquals(message, decoratedNotifications.MessagesAsString);
		}
	}
}
#endif
