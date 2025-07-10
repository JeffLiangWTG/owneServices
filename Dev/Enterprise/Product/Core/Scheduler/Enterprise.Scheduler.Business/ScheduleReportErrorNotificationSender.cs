using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Scheduler.Business
{
	public class ScheduleReportErrorNotificationSender
	{
		public ScheduleReportErrorNotificationSender(StmScheduleTask task, INotifications notifications)
		{
			Task = task;
			Notifications = notifications;
		}

		StmScheduleTask Task { get; }

		INotifications Notifications { get; }

		string Subject { get; set; }

		string Body { get; set; }

		Func<string[]> DefaultAction { get; set; }

		ScheduledReportErrorNotificationType NotificationType { get; set; }

		string[] ConstantRecipients { get; set; }

		bool ShouldAddErrorIntoNotifications { get; set; }

		public void SendErrorNotification(string subject, string body, Func<string[]> defaultAction, bool shouldAddErrorIntoNotifications = false, ScheduledReportErrorNotificationType notificationType = ScheduledReportErrorNotificationType.Error, string[] constantRecipients = null)
		{
			DefaultAction = defaultAction;
			Subject = subject;
			Body = body;
			ShouldAddErrorIntoNotifications = shouldAddErrorIntoNotifications;
			NotificationType = notificationType;
			ConstantRecipients = constantRecipients;

			var notificationOption = SystemDataRegistry.Instance.SRRErrorNotificationOptions.Value;
			switch (notificationOption)
			{
				case Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL:
					SendErrorNotificationToStaffRole();
					break;

				case Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP:
					SendErrorNotificationToGroupRole();
					break;

				default:
					SendErrorNotificationToDefaultRecipients();
					break;
			}
		}

		void SendErrorNotificationToDefaultRecipients()
		{
			var recipients = DefaultAction.Invoke();
			AddErrorNotificationUserMessages(recipients);
		}

		void SendErrorNotificationToStaffRole()
		{
			var staffRoles = SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.Value
				.OfType<CodeDescriptionBool>()
				.Where(r => r.Bool)
				.Select(r => r.Code)
				.ToArray();

			if (staffRoles.Length > 0)
			{
				var recipientsWithEmailAddress = Task.Recipients
					.OfType<StmScheduleTaskRecipient>()
					.SelectMany(recipient => recipient.Header?.StaffAssignments?.Where(assignment => assignment != null && !string.IsNullOrEmpty(assignment.O8_Role) && staffRoles.Contains(assignment.O8_Role)) ?? new List<OrgStaffAssignments>()).Where(assignment => assignment != null)
					.Select(assignment => assignment.PersonResponsible).Where(staff => staff != null && !staff.GS_IsSystemAccount && staff.GS_EmailAddress.IsValid)
					.Select(staff => staff.GS_EmailAddress.ToString()).Where(address => !string.IsNullOrEmpty(address))
					.Distinct()
					.ToArray();

				if (recipientsWithEmailAddress.Length > 0)
				{
					SendErrorNotification(recipientsWithEmailAddress);
					return;
				}
			}

			SendErrorNotificationToGroupRole();
		}

		void SendErrorNotificationToGroupRole()
		{
			var groupPk = SystemDataRegistry.Instance.SRRErrorNotificationGroups.Value;
			if (groupPk != Guid.Empty)
			{
				var recipientsWithEmailAddress = new EmailGroupUtility().GetGroupEmailCollection(groupPk, false).Cast<string>().ToArray();
				if (recipientsWithEmailAddress.Length > 0)
				{
					SendErrorNotification(recipientsWithEmailAddress);
					return;
				}
			}

			SendErrorNotificationToDefaultRecipients();
		}

		void SendErrorNotification(string[] recipients)
		{
			var mail = new EmailDef();
			mail.ContentType = EmailContentTypes.HTML;
			mail.Subject = Subject;
			mail.Body = Body;
			if (ConstantRecipients != null)
			{
				recipients = recipients.Union(ConstantRecipients).Distinct().ToArray();
			}
			mail.AddRecipientForUserCommunication(recipients);

			if (NotificationType == ScheduledReportErrorNotificationType.Error)
			{
				EnvProxy.Instance.OutgoingMailManager.CreateAndSave(mail, Task.Factory);
			}
			else
			{
				EnvProxy.Instance.OutgoingMailManager.Create(Task.Factory, mail);
			}

			AddErrorNotificationUserMessages(recipients);
		}

		void AddErrorNotificationUserMessages(string[] emails)
		{
			if (emails?.Length > 0 && Notifications != null)
			{
				var notificationUserMessage = Res.GetString("085EEF05-7679-4B3B-8335-A0EF91BBDE29", @"Error notification email was sent to the following email address.
{0}", string.Join(", ", emails));
				var notificationMessage = ShouldAddErrorIntoNotifications ? Body + System.Environment.NewLine + notificationUserMessage : notificationUserMessage;
				Notifications.Add(CargoWise.ComponentModel.NotificationType.Information, notificationMessage);
			}
		}
	}

	public enum ScheduledReportErrorNotificationType
	{
		Error,
		Canceled
	}
}
