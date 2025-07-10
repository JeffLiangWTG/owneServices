using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks
{
	static class EHubNotifierNotificationsExtensions
	{
		public static void AssertNotificationExists(this INotifications notifications, string notificationMessage, int count = 1)
		{
			var notificationBuffer = GetNotificationBuffer(notifications);
			Assertion.Assert(
				string.Format("{0}{0}Count of notification:{0}{0}{1}{0}{0}In notifications:{0}{0}{2}{0}{0}is not equal to {3}", System.Environment.NewLine, notificationMessage, notificationBuffer.AsString, count),
				notificationBuffer.Events.IsCountEqualTo(count, e => e.Message.StartsWith(notificationMessage)));
		}

		public static void AssertNotificationDoesNotExists(this INotifications notifications, string notificationMessage)
		{
			var notificationBuffer = GetNotificationBuffer(notifications);
			Assertion.Assert(
				string.Format("{0}{0}Found notification:{0}{0}{1}{0}{0}In notifications:{0}{0}{2}{0}{0}", System.Environment.NewLine, notificationMessage, notificationBuffer.AsString),
				notificationBuffer.Events.All(e => e.Message != notificationMessage));
		}

		public static void AssertNotificationContains(this INotifications notifications, string notificationMessage)
		{
			var notificationBuffer = GetNotificationBuffer(notifications);
			Assertion.Assert(
				string.Format("{0}{0}Could not find notification:{0}{0}{1}{0}{0}In notifications:{0}{0}{2}{0}{0}", System.Environment.NewLine, notificationMessage, notificationBuffer.AsString),
				notificationBuffer.Events.Any(e => e.Message.Contains(notificationMessage)));
		}

		public static void AssertNotificationContains(this INotifications notifications, string notificationMessage, ErrorType expectedErrorType)
		{
			var notificationBuffer = GetNotificationBuffer(notifications);
			INotification notification = notificationBuffer.Events.FirstOrDefault(e => e.Message.Contains(notificationMessage));
			Assertion.AssertNotNull(
				string.Format("{0}{0}Could not find notification:{0}{0}{1}{0}{0}In notifications:{0}{0}{2}{0}{0}", System.Environment.NewLine, notificationMessage, notificationBuffer.AsString),
				notification);
			Assertion.Assert("Wrong Notification Type", ZNotificationsExtensions.NotificationTypeName(notification.Type, CargoWise.ResourceStrings.Grammar.PluralState.NonPlural).Contains(expectedErrorType.Name));
		}

		public static void AssertEmptyNotification(this INotifications notifications)
		{
			var notificationBuffer = GetNotificationBuffer(notifications);
			Assertion.Assert(
				string.Format("{0}{0}Expected empty notification but found:{0}{0}{1}{0}{0}", System.Environment.NewLine,  notificationBuffer.AsString),
				notificationBuffer.Events.All(x => !string.IsNullOrEmpty(x.Message.Trim())));
		}

		public static void AssertNotificationContainsMessages(this INotifications notifications, params string[] notificationMessages)
		{
			var notificationBuffer = GetNotificationBuffer(notifications);
			var assert = notificationMessages.All(x => notificationBuffer.Events.Any(y => y.Message.Contains(x)));

			if (!assert)
			{
				var report = string.Join("\r\n", notificationMessages.Select(x => notificationBuffer.Events.Any(y => y.Message == x) ? HtmlFormatGoodValue(x) : HtmlFormatBadValue(x)));
				var message = HtmlFormatter.Html("Could not find notifications:");
				message += "<br/><br/>" + report
					+ "<br/>In notifcations:<br/><br/>" + HtmlFormatGoodValue(notificationBuffer.AsString) + "<br/>";
				AssertionWithHtml.HtmlFail(message);
			}
			else
			{
				Assertion.Assert(true);
			}
		}

		public static NotificationBuffer GetNotificationBuffer(this INotifications notifications)
		{
			return (NotificationBuffer)notifications;
		}

		static string HtmlFormatGoodValue(string value)
		{
			return HtmlFormatValue(value, 230, 245, 230);
		}

		static string HtmlFormatBadValue(string value)
		{
			return HtmlFormatValue(value, 255, 230, 230);
		}

		static string HtmlFormatValue(string value, int red, int green, int blue)
		{
			string result;

			if (value == null)
			{
				result = "null<br>";
			}
			else if (value.Length > 0)
			{
				result = "empty string<br>";
			}
			else
			{
				result =
					"<code>" +
					"<div style='background-color: rgb(" + red + "," + green + "," + blue + ")'>" +
					HtmlFormatter.Html(value) +
					"</div>" +
					"</code>";
			}

			return result;
		}
	}
}
