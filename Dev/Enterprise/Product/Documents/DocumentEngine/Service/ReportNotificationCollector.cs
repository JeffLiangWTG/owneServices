using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Scheduler.Business;

namespace Enterprise.DocumentEngine
{
	public class ReportNotificationCollector : ZNotificationCollector
	{
		public ReportNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage) : base(business, includeChildren, includeNotificationTypeInMessage)
		{
		}

		public ReportNotificationCollector(IBusiness business, ZBool includeChildren, ZBool includeNotificationTypeInMessage, PropertyDescriptionType descriptionToInclude) : base(business, includeChildren, includeNotificationTypeInMessage, descriptionToInclude)
		{
		}

		protected override INotification GetNotification(BusinessObject bizObj, INotification notification, string replacedMessage)
		{
			var name = bizObj.GetType().Name;

			if (bizObj is FilterField filterField)
			{
				name = filterField.DisplayName ?? "";
				replacedMessage = $"[{name}] {replacedMessage}";
			}
			else if (bizObj is ReportScheduleTask || bizObj is ReportScheduleTaskRecipient || bizObj is StmScheduleTaskRecurrence)
			{
				var identifier = string.Empty;
				if (bizObj is IServiceDtoMapper mapper && !string.IsNullOrEmpty(mapper.Identifier))
				{
					identifier = mapper.Identifier + ".";
				}

				if (notification is PropertyNotification propertyNotification)
				{
					PropertiesNotificationMapping.TryGetValue($"{bizObj.GetType().Name}.{propertyNotification.PropertyName}", out var propertyName);
					if (string.IsNullOrEmpty(propertyName))
					{
						propertyName = $"{name}.{propertyNotification.PropertyName}";
					}
					replacedMessage = $"[{identifier}{propertyName}] {notification.Message}";
				}
			}
			else if (bizObj is StmScheduleTaskCopyRecipient copyRecipient)
			{
				var recipient = copyRecipient.Factory.Load<ReportScheduleTaskRecipient>(copyRecipient.SCR_S6);
				if (recipient is IServiceDtoMapper recipientMapper && !string.IsNullOrEmpty(recipientMapper.Identifier))
				{
					var recipientIdentifier = recipientMapper.Identifier;
					var propertyName = string.Empty;
					switch (copyRecipient.SCR_RecipientType)
					{
						case Core.Constants.CopyRecipientType.EmailToRecipient:
							propertyName = $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.ToFaxOrEmail)}";
							break;
						case Core.Constants.CopyRecipientType.CarbonCopyRecipient:
							propertyName = $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.CarbonCopyRecipients)}";
							break;
						case Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient:
							propertyName = $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.BlindCarbonCopyRecipients)}";
							break;
						default:
							break;
					}
					replacedMessage = $"[{recipientIdentifier}.{propertyName}] {notification.Message}";
				}
			}
			else
			{
				name = bizObj.HumanReadableName;
				replacedMessage = $"[{name}] {replacedMessage}";
			}

			return base.GetNotification(bizObj, notification, replacedMessage);
		}

		readonly Dictionary<string, string> PropertiesNotificationMapping = new ()
		{
			{ $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleTaskRecipient.S6_EmailToRecipientsAsString)}", $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.ToFaxOrEmail)}" },
			{ $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleTaskRecipient.S6_EmailFromAddress)}", $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.EmailFromAddress)}" },
			{ $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleTaskRecipient.S6_FaxOverride)}", $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.ToFaxOrEmail)}" },
			{ $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleTaskRecipient.S6_CarbonCopyRecipientsAsString)}", $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.CarbonCopyRecipients)}" },
			{ $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleTaskRecipient.S6_BlindCarbonCopyRecipientsAsString)}", $"{nameof(ReportScheduleTaskRecipient)}.{nameof(ReportScheduleRecipientData.BlindCarbonCopyRecipients)}" },
			{ $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTask.S5_ScheduleDescription)}", $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTaskData.ScheduleDescription)}" },
			{ $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTask.S5_NextScheduledPrintRunTimeUtc)}", $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTaskData.NextRunTimeLocal)}" },
			{ $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTask.UserFK)}", $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTaskData.UserFk)}" },
			{ $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTask.S5_TaskPeriodCount)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.TaskPeriodCount)}" },
			{ $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTask.S5_StartDate)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.StartDateLocal)}" },
			{ $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTask.S5_EndDate)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.EndDateLocal)}" },
			{ $"{nameof(ReportScheduleTask)}.{nameof(ReportScheduleTask.S5_DayList)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
			{ $"{nameof(StmScheduleTaskRecurrence)}.{nameof(StmScheduleTaskRecurrence.Sunday)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
			{ $"{nameof(StmScheduleTaskRecurrence)}.{nameof(StmScheduleTaskRecurrence.Monday)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
			{ $"{nameof(StmScheduleTaskRecurrence)}.{nameof(StmScheduleTaskRecurrence.Tuesday)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
			{ $"{nameof(StmScheduleTaskRecurrence)}.{nameof(StmScheduleTaskRecurrence.Wednesday)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
			{ $"{nameof(StmScheduleTaskRecurrence)}.{nameof(StmScheduleTaskRecurrence.Thursday)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
			{ $"{nameof(StmScheduleTaskRecurrence)}.{nameof(StmScheduleTaskRecurrence.Friday)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
			{ $"{nameof(StmScheduleTaskRecurrence)}.{nameof(StmScheduleTaskRecurrence.Saturday)}", $"{nameof(StmScheduleTaskRecurrence)}.{nameof(ScheduleRecurrenceData.WeekDayList)}" },
		};
	}
}
