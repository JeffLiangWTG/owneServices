using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentClientReopenEvent : SupportIncidentCargoWiseReopenEvent
	{
		public SupportIncidentClientReopenEvent(SupportIncident incident)
			: base(incident)
		{
		}

		public override ZString Code
		{
			get { return IncidentEventFactory.Codes.ERequestReopen; }
		}

		protected override bool ApplyWorkflowTemplate()
		{
			bool result = base.ApplyWorkflowTemplate();
			if (!result)
			{
				CloneLastClosedOrCancelledTask();
			}

			return result;
		}

		public ProcessTask CloneLastClosedOrCancelledTask()
		{
			var tasks = incident.WorkflowItems.Tasks.Cast<ProcessTask>();
			if (!tasks.Any(task => task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled))
			{
				var supportIncident = (SupportIncident)incident;
				var lastClosedOrCancelledTask = supportIncident.GetLastClosedOrCancelledTask();

				if (lastClosedOrCancelledTask != null)
				{
					using (supportIncident.SuspendCalculateStatusAndDisposition())
					{
						var excludedProperties = new BusinessObjectCloneArgs(new[] { ProcessTasksSchema.P9_ActualDate.Name, ProcessTasksSchema.P9_ActualDuration.Name });
						var clonedTask = lastClosedOrCancelledTask.Clone(excludedProperties) as SupportIncidentProcessTask;
						incident.WorkflowItems.SetDefaultsForNewTask(clonedTask, false);
						incident.WorkflowItems.Add(clonedTask);
						clonedTask.P9_Sequence = tasks.Max(task => task.P9_Sequence) + 1;
						SetCalculatedTaskStatus(clonedTask);
						return clonedTask;
					}
				}
			}

			return null;
		}

		protected override void SetAssignee()
		{
			if (incident.WorkflowItems.Tasks.Count > 0)
			{
				base.SetAssignee();
			}
			else
			{
				var supportIncident = incident as SupportIncident;
				if (supportIncident.ProductAreaAssignedStaff != null)
				{
					supportIncident.AssignToStaff(supportIncident.ProductAreaAssignedStaff, "");
				}
			}
		}
	}
}

