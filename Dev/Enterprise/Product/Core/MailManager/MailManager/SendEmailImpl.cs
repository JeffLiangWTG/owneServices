using System.Collections.Specialized;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = MailManager.Res;

namespace Enterprise.MailManager
{
	public class SendEmailImpl : ISendEmail
	{
		public string ToDatabaseHealthCheckNotificationGroup(string subject, string body)
		{
			EmailGroupUtility emailUntility = new EmailGroupUtility();
			StringCollection groupEmails = emailUntility.SendNotificationToDatabaseAdministrator(NotificationDataRegistry.Instance.DatabaseHealthCheckNotificationGroup.Value, false);

			if (groupEmails.Count == 0)
			{
				groupEmails = emailUntility.GetCompanyNotificationGroupEmails();
				body = Res.GetString("D670E3F0-E3BE-4F79-B323-35B2C1D0437D", "Database Health Check Notification Group does not contain any users with email addresses. Email being sent to current company Notification Group instead.\r\n{0}", body);
			}

			if (groupEmails.Count > 0)
			{
				HtmlNotificationEmailSender notificationSender = new HtmlNotificationEmailSender();
				EmailDef notificationEmail = notificationSender.CreateEmail(subject, body);
				notificationEmail.AddRecipientForUserCommunication(groupEmails);
				Env.OutgoingMailManager.CreateAndSave(notificationEmail);

				return "";
			}
			else
			{
				return Res.GetString("7E2F8A12-9472-41D6-824B-842E07080AF6", "Sending email error! Neither Database Health Check Notification Group nor current company Notification Group contains any users with email addresses. Email not sent.");
			}
		}
	}
}
