using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public abstract class SupportIncidentEvent : IncidentEvent
	{
		protected SupportIncidentEvent(SupportIncident incident)
			: base(incident)
		{
		}

		protected override bool ApplyWorkflowTemplate()
		{
			bool result = false;
			var supportIncident = incident as SupportIncident;
			using (supportIncident.SuspendCalculateStatusAndDisposition())
			{
				result = base.ApplyWorkflowTemplate();
			}

			if (result)
			{
				//Delete unsaved event trigger logs because unsaved event tasks are deleted
				supportIncident.Logs.RemoveAddedLogs(log => log.SL_SE_NKEvent == Events.StatusChangeCode && log.SL_Reference.StartsWith(EventTriggerLogPrefix, StringComparison.Ordinal));
				supportIncident.Logs.AddNew(Events.StatusChange, EventTriggerLogPrefix + Code);
			}

			return result;
		}

		protected override ProcessTaskTemplate[] FindMatchedTemplates()
		{
			ProcessTaskTemplate[] matchedTemplates;
			incident.IsMatchingEventTemplates = true;
			try
			{
				matchedTemplates = base.FindMatchedTemplates();
			}
			finally
			{
				incident.IsMatchingEventTemplates = false;
			}
			return matchedTemplates;
		}

		internal static StmALog GetLastEventLog(SupportIncident incident)
		{
			return incident.Logs.Find(f => f.SL_SE_NKEvent == Events.StatusChangeCode &&
												f.SL_Reference.StartsWith(SupportIncidentEvent.EventTriggerLogPrefix) &&
												f.SL_Reference != (SupportIncidentEvent.EventTriggerLogPrefix + IncidentEventFactory.Codes.ERequestReopen) &&
												f.SL_Reference != (SupportIncidentEvent.EventTriggerLogPrefix + IncidentEventFactory.Codes.CargoWiseReopen) &&
												f.SL_Reference != (SupportIncidentEvent.EventTriggerLogPrefix + IncidentEventFactory.Codes.ChangeCriticality))
										.OrderByDescending(log => log.SL_PostedTimeUtc.IsEmpty ? log.SL_EventTime.ToUniversalBranchTime() : log.SL_PostedTimeUtc)
										.FirstOrDefault();
		}

		public static bool TriggerLastEvent(SupportIncident incident)
		{
			var result = false;
			var lastEventTriggerLog = GetLastEventLog(incident);

			if (lastEventTriggerLog != null)
			{
				ZString lastEventCode = lastEventTriggerLog.SL_Reference.SubstringSafe(SupportIncidentEvent.EventTriggerLogPrefix.Length, 3);
				IncidentEventFactory.TriggerEvent(lastEventCode, incident);
				result = true;
			}

			return result;
		}

		const string EventTriggerLogPrefix = "Event - ";
	}
}

