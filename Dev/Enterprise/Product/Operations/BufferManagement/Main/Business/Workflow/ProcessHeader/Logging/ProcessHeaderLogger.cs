using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	static class ProcessHeaderLogger
	{
		internal static void AddStartabilityEventIfNeeded(ProcessHeader processHeader)
		{
			if (BMSRegistryProvider.IsBufferManagementEnabled)
			{
				WorkflowStartabilityStrategy.AddStartabilityEvent(processHeader);
			}
		}

		internal static void AddJobStatusChangeEvent(ProcessHeader processHeader, bool isOpen)
		{
			var statusChangeEvent = GetEvent(isOpen);
			var lastStatusLog = GetLastStatusChangeLog(processHeader);
			var needsLog = lastStatusLog == null || lastStatusLog.SL_SE_NKEvent != statusChangeEvent.Code;

			if (needsLog)
			{
				processHeader.Logs.AddNew(statusChangeEvent);
			}
		}

		internal static void AddApprovedEvent(ProcessHeader processHeader, bool isApproved)
		{
			var approvedEvent = isApproved ? Events.Approved : Events.Unapproved;
			processHeader.Logs.AddNew(approvedEvent);
		}

		internal static void AddFetchHints(ProcessHeader processHeader, bool isOpen)
		{
			var statusChangeEvent = GetEvent(isOpen);
			if (ObjectFactory.Get<ITriggerProvider>().TryGetQueryForAllTriggersIncludingThoseOnParentObjects(processHeader, statusChangeEvent.Code, out ZQuery queryForFiringWorkflow))
			{
				processHeader.Factory.AddFetchHint(ProcessTasksSchema.Instance, queryForFiringWorkflow);
			}
			processHeader.Factory.AddFetchHint(StmALogSchema.Instance, GetLastStatusChangeLogQuery(processHeader));
		}

		static Event GetEvent(bool isWorkflowOpen) => isWorkflowOpen ? Events.JobOpen : Events.JobClose;

		static ZQuery GetLastStatusChangeLogQuery(ProcessHeader workflow)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, workflow.PK) { FetchOnlyFromLocalCache = !workflow.IsInDatabase };
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, RelevantEventTypes);

			return query;
		}

		static StmALog GetLastStatusChangeLog(ProcessHeader workflow)
		{
			var allLogs = workflow.Factory.Load<StmALog>(GetLastStatusChangeLogQuery(workflow));

			return allLogs.MaxBySafe(x => x.SL_EventTimeOffset.ToUtcDateTime());
		}

		static string[] RelevantEventTypes => new[] { Events.JobOpenCode, Events.JobCloseCode };
	}
}
