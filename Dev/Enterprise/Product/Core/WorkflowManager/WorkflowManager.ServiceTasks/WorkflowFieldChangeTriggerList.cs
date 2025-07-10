using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	internal class WorkflowFieldChangeTriggerList : IEnumerable<ChangeLogTriggerGrouping>
	{
		public WorkflowFieldChangeTriggerList(params IWorkflowTrigger[] triggers)
		{
			Add(null, triggers);
		}

		public void Add(StmChangeLog changeLog, params IWorkflowTrigger[] triggers)
		{
			if (changeLog != null || triggers.Length > 0)
			{
				var triggerGrouping = triggerGroupings.SingleOrDefault(t => t.ChangeLog == changeLog);
				if (triggerGrouping == null)
				{
					triggerGrouping = new ChangeLogTriggerGrouping(changeLog);
					triggerGroupings.Add(triggerGrouping);
				}
				triggerGrouping.Triggers.AddRange(triggers);
			}
		}

		readonly HashSet<ChangeLogTriggerGrouping> triggerGroupings = new HashSet<ChangeLogTriggerGrouping>();

		public void Clear()
		{
			triggerGroupings.Clear();
		}

		#region IEnumerable Members

		public IEnumerator<ChangeLogTriggerGrouping> GetEnumerator()
		{
			return triggerGroupings.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return triggerGroupings.GetEnumerator();
		}

		#endregion
	}
}
