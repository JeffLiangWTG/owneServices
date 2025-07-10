using System.Globalization;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	static class ExtensionMethods
	{
		internal static IWorkflowTrigger LoadTrigger(this IQueuedLog log)
		{
			switch (log.SJ_ParentTableCode)
			{
				case ProcessTasksSchema.Constants.Prefix:
					return log.Factory.Load<ProcessTask>(log.SJ_ParentID);

				case ProcessJobTriggerLinkSchema.Constants.Prefix:
					return log.Factory.Load<IProcessJobTriggerLink>(log.SJ_ParentID);

				default:
					return null;
			}
		}

		internal static string GetRecordID(this IBaseTrigger trigger)
		{
			var processTask = trigger as ProcessTask;

			if (processTask != null)
			{
				return processTask.P9_TaskID;
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"[Seq: {0}, Event: {1}, Field: {2}]", trigger.Sequence, trigger.TriggerEventCode, trigger.TriggerFieldName);
			}
		}
	}
}
