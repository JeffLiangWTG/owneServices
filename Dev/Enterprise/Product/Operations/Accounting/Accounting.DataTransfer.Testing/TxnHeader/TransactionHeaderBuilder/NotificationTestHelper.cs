using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	// This class serves as a helper class rather than a test class. See usage in DirectReceiptPaymentDataAdapterTest class for example.
	public class NotificationTestHelper : TestCase
	{
		public void AssertNotificationsContainsErrorMessage(NotificationBuffer notify, ZString expectedMessage)
		{
			ZString notifications = new ZString(notify.AsString);
			ZString assertionMessage = "Notifications should contain the following error. " + System.Environment.NewLine + System.Environment.NewLine;
			assertionMessage += expectedMessage + System.Environment.NewLine + "----------------------------" + System.Environment.NewLine;
			assertionMessage += System.Environment.NewLine + "Notifications are as follows:" + System.Environment.NewLine + System.Environment.NewLine + notifications;

			AssertEquals(assertionMessage, true, notifications.Contains(expectedMessage));
		}

		public void AssertNotificationsContainsErrorMessageOnce(NotificationBuffer notify, ZString expectedMessage)
		{
			ZString notifications = new ZString(notify.AsString);
			ZString assertionMessage = "Notifications should contain the following error once. " + System.Environment.NewLine + System.Environment.NewLine;
			assertionMessage += expectedMessage + System.Environment.NewLine + "----------------------------" + System.Environment.NewLine;
			assertionMessage += System.Environment.NewLine + "Notifications are as follows:" + System.Environment.NewLine + System.Environment.NewLine + notifications;

			AssertEquals(assertionMessage, 1, notify.Events.Count(x => ((NotificationSubscriberType)(x.Type)).Name == nameof(NotificationTypes.Error) && x.Message.Contains(expectedMessage)));
		}

		public void AssertNotificationsDoesNotContainErrorMessage(NotificationBuffer notify, ZString expectedMessage)
		{
			ZString notifications = new ZString(notify.AsString);
			ZString assertionMessage = "Notifications should NOT contain the following error. " + System.Environment.NewLine + System.Environment.NewLine;
			assertionMessage += expectedMessage + System.Environment.NewLine + "----------------------------" + System.Environment.NewLine;
			assertionMessage += System.Environment.NewLine + "Notifications are as follows:" + System.Environment.NewLine + System.Environment.NewLine + notifications;

			AssertEquals(assertionMessage, true, !notifications.Contains(expectedMessage));
		}

		public void AssertNotificationsContainsNoErrors(NotificationBuffer notify)
		{
			Assert("NotificationBuffer should have no Errors", !notify.ContainsNotificationType(CargoWise.ComponentModel.NotificationType.Error));
		}
	}
}
