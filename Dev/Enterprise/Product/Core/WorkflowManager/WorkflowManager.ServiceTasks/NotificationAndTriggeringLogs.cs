using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	internal class NotificationAndTriggeringLogs
	{
		public NotificationAndTriggeringLogs(ProcessTaskNotification notification)
			: this(notification, null)
		{
		}

		public NotificationAndTriggeringLogs(ProcessTaskNotification notification, StmChangeLog changeLog)
		{
			this.notification = notification;
			if (changeLog != null)
			{
				triggeringChangeLogs.Add(changeLog);
			}
		}

		readonly ProcessTaskNotification notification;
		readonly List<StmChangeLog> triggeringChangeLogs = new List<StmChangeLog>();

		public ProcessTaskNotification Notification
		{
			get { return notification; }
		}

		public StmChangeLog[] Logs
		{
			get { return triggeringChangeLogs.ToArray(); }
		}

		public void AddChangeLog(StmChangeLog changeLog)
		{
			if (changeLog != null && !triggeringChangeLogs.Contains(changeLog))
			{
				triggeringChangeLogs.Add(changeLog);
			}
		}

		#region Matching

		public bool MatchesNotification(ProcessTaskNotification anotherNotification)
		{
			return (anotherNotification != null)
				&& IsWorkflowParentEqual(anotherNotification)
				&& AreAllColumnsEqual(anotherNotification)
				&& AreParentColumnsEqualIfRequired(anotherNotification);
		}

		bool IsWorkflowParentEqual(ProcessTaskNotification anotherNotification)
		{
			return (notification.Parent == null && anotherNotification.Parent == null)
				|| ((notification.Parent != null && anotherNotification.Parent != null) && (notification.Parent.ParentID == anotherNotification.Parent.ParentID));
		}

		bool AreAllColumnsEqual(ProcessTaskNotification anotherNotification)
		{
			foreach (SchemaColumn schemaColumn in AllColumnsExceptPKs)
			{
				IZType value1 = (IZType)notification[schemaColumn];
				IZType value2 = (IZType)anotherNotification[schemaColumn];
				if (!value1.Equals(value2))
				{
					return false;
				}
			}

			if (WorkflowTriggerActionTypeConstants.IsSetField(notification.PQ_TriggerType)
				&& (notification.PQ_FieldName != anotherNotification.PQ_FieldName || notification.PQ_FieldValue != anotherNotification.PQ_FieldValue))
			{
				return false;
			}

			return true;
		}

		bool AreParentColumnsEqualIfRequired(ProcessTaskNotification anotherNotification)
		{
			switch (notification.PQ_TriggerType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML:
				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc:
					return
						(notification.Parent == null || anotherNotification.Parent == null)
						|| (notification.Parent != null && anotherNotification.Parent != null) && notification.Parent.TriggerEventCode.Equals(anotherNotification.Parent.TriggerEventCode);
				default:
					return true;
			}
		}

		SchemaColumn[] AllColumnsExceptPKs
		{
			get
			{
				if (allColumnsExceptPKs == null)
				{
					SchemaColumn[] columnsToExclude = new SchemaColumn[] { ProcessTaskNotificationSchema.PK, ProcessTaskNotificationSchema.PQ_P9 };
					allColumnsExceptPKs = ProcessTaskNotificationSchema.All.Cast<SchemaColumn>().Except(columnsToExclude).ToArray();
				}
				return allColumnsExceptPKs;
			}
		}
		SchemaColumn[] allColumnsExceptPKs;

		#endregion
	}
}
