using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class WorkflowFailureHandler
	{
		public static void HandleTriggerActionFailure(ProcessTaskNotification triggerAction, Exception ex)
		{
			HandleTriggerActionFailure(triggerAction, ex.Message);
		}

		public static void HandleTriggerActionFailure(ProcessTaskNotification triggerAction, NotificationBuffer notify)
		{
			HandleTriggerActionFailure(triggerAction, notify.AsString);
		}

		public static void HandleTriggerActionFailure(ProcessTaskNotification triggerAction, string errorMessage)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowFailureHandler.HandleTriggerActionFailure"))
			{
				string parentJobDetails = triggerAction.Parent.GetParentJobDetails();
				SendNotificationEmail(
					triggerAction.Parent,
					string.Concat(
						Res.GetString("cc37c2ef-bdbe-4ba9-b06e-fa01a8687693", "WorkflowManager Trigger Action Failure -") + " ",
						parentJobDetails),
					Res.GetString("0ed835f3-0054-4031-ac7c-eb626e3953ba",
						"The following trigger action failed to run:\r\nJob: {0}\r\nTrigger Event: {1}\r\nTrigger Action: {2}\r\nError Message: {3}",
						parentJobDetails, GetTriggerDescription(triggerAction.Parent), GetTriggerActionDescription(triggerAction),
						errorMessage));
			}
		}

		public static void HandleTriggerActionFailure(ProcessTaskNotification triggerAction, Exception ex, INotifications notifications)
		{
			notifications.AddError(FormattableString.Invariant($"Action [Current User={EnvProxy.Instance.CurrentUser?.FullName}, Type={triggerAction.PQ_TriggerType}, Recipient={triggerAction.PQ_Calc_TriggerParty}, Purpose={triggerAction.PQ_MessagePurpose}] runs failed, message:{ex.Message}"));
		}

		public static bool IsEmailSenderAddressInvalidException(Exception exception)
		{
			return exception.Find<EmailNotCompleteException>() != null;
		}

		public static void HandleTriggerFailure(IBaseTrigger trigger, Exception ex)
		{
			string parentJobDetails = trigger.GetParentJobDetails();
			SendNotificationEmail(trigger,
			string.Concat(Res.GetString("6b8b472d-3683-44e6-a3ea-3c4a5a126c6b", "WorkflowManager Trigger Event Failure -") + " ", parentJobDetails),
				Res.GetString("a758dd1b-70b5-4e9d-9cba-fe4fb68e47ac", "The following trigger event failed to run:\r\nJob: {0}\r\nTrigger Event: {1}\r\nError Message: {2}", parentJobDetails, GetTriggerDescription(trigger), ex.Message));
		}

		static string GetTriggerDescription(IBaseTrigger trigger)
		{
			return trigger.GetEvent()?.SE_DescMultilingual ?? trigger.Description;
		}

		static string GetTriggerActionDescription(ProcessTaskNotification triggerAction)
		{
			string description = triggerAction.Lookups.WorkflowTriggerActionTypes.GetDescriptionFromCode(triggerAction.PQ_TriggerType);
			if (triggerAction.Document != null)
			{
				description += (string.IsNullOrEmpty(description) ? "" : " - ") + triggerAction.Document.SU_MenuName;
			}
			return description;
		}

		static void SendNotificationEmail(IBaseTrigger trigger, string subject, string body)
		{
			var emailDef = new EmailDef();
			var assignedStaff = trigger.GetAssignedStaffMember();
			var assignedGroup = trigger.GetAssignedGroup();

			if (assignedStaff != null && !string.IsNullOrEmpty(assignedStaff.GS_EmailAddress))
			{
				emailDef.AddRecipientForUserCommunication(assignedStaff.GS_EmailAddress);
			}

			if (assignedGroup != null)
			{
				foreach (string email in new EmailGroupUtility().GetGroupEmailCollection(assignedGroup.PK.ToGuid(), false))
				{
					emailDef.AddRecipientForUserCommunication(email);
				}
			}

			if (emailDef.Recipients.Count == 0)
			{
				emailDef.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.Value, WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup);
			}

			emailDef.FromDisplayName = Core.Constants.ProductName + (NoResString)" Workflow Trigger Processor";
			emailDef.Subject = subject;
			emailDef.Body = body;
			Env.OutgoingMailManager.Create(trigger.Factory, emailDef);
		}
	}
}
