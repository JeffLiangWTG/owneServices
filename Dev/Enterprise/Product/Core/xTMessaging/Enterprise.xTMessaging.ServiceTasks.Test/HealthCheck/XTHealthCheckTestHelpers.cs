using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared.Test;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.xTMessaging.Tests.ServiceTasks.HealthCheck
{
	class XTHealthCheckTestHelpers : TestCase
	{
		public static GlbStaff CreateStaff(string staffInfo, BusinessObjectFactory factory)
		{
			var result = factory.New<GlbStaff>();
			result.GS_Code = staffInfo;
			result.GS_LoginName = staffInfo;
			result.GS_IsSystemAccount = false;
			result.GS_EmailAddress = staffInfo + "@" + staffInfo + ".com";
			factory.Save();
			return result;
		}

		public static void AssertNoErrorEmail()
		{
			var email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
			if (email != null)
			{
				Fail(string.Format("No error email expected.{0}{0}Email body contains:{0}{0}{1}{0}{0}", System.Environment.NewLine, email.Body));
			}
			else
			{
				Assert(true);
			}
		}

		public static void AssertErrorEmails(params string[] containsMessages)
		{
			AssertEquals("No error email found", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
			Env.OutgoingMailManager.EmailsCreated.Remove(email);
			AssertNotNull("Unexpected null email", email);
			foreach (var containsMessage in containsMessages)
			{
				Assert(string.Format("Wrong error email found.{0}{0}Could not find text:{0}{0}{1}{0}{0}In email body:{0}{0}{2}{0}{0}", System.Environment.NewLine, containsMessage, email.Body), email.Body.Contains(containsMessage));
			}
		}

		public static EDIInterchange CreateInterchangeForHealthCheckTests(ZGuid branchPk, string from, string to, string receiveTransmit, BusinessObjectFactory factory, bool onlyCreateInterchange)
		{
			var interchange = TestUtils.CreateInterchangeForXT(factory);

			interchange.EI_GB = branchPk;
			interchange.EI_From = from;
			interchange.EI_To = to;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_ReceiveTransmit = receiveTransmit;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Error;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;

			if (!onlyCreateInterchange)
			{
				var message = interchange.ContainedMessages.AddNew();
				message.EM_ReceiveTransmit = receiveTransmit;
				message = interchange.ContainedMessages.AddNew();
				message.EM_ReceiveTransmit = receiveTransmit;
			}

			return interchange;
		}

		public static void CleanErrorReporter()
		{
			ErrorReporter.Clear();
		}
	}

	static class XTNotifierNotificationsExtensions
	{
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
			else if (value.Length == 0)
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
