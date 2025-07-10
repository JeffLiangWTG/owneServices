using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class EmailStatusNotifier : IStatusNotifier
	{
		readonly EmailGroupUtility emailUtility = new EmailGroupUtility();

		public void Notify(string title, string message, IDictionary<string, string> attachments)
		{
			Argument.NotNullOrEmpty(title, nameof(title));
			Argument.NotNullOrEmpty(message, nameof(message));

			if (EDIDataRegistry.Instance.AvsMonitoringNotificationGroup == Guid.Empty)
			{
				throw new InvalidOperationException("Could not find a valid notification group.");
			}

			var emails = emailUtility
				.GetGroupEmailCollection(EDIDataRegistry.Instance.AvsMonitoringNotificationGroup, false)?
				.Cast<string>()
				.Where(email => !string.IsNullOrEmpty(email))
				.ToArray() ?? Array.Empty<string>();

			if (!emails.Any())
			{
				throw new InvalidOperationException("Could not find any valid email from notification group.");
			}

			var notificationEmail = new EmailDef();
			notificationEmail.AddRecipientForSystemCommunication(emails);
			notificationEmail.Subject = title;
			notificationEmail.Body = message;

			if (attachments?.Any() == true)
			{
				attachments
					.Select(attachment => new AttachmentDef(attachment.Key, Encoding.UTF8.GetBytes(attachment.Value)))
					.ToList()
					.ForEach(def => notificationEmail.Attachments.Add(def));
			}

			Env.OutgoingMailManager.CreateAndSave(notificationEmail);
		}
	}
}
