using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class ChangeLogTriggerGrouping : IGrouping<StmChangeLog, IWorkflowTrigger>
	{
		public ChangeLogTriggerGrouping(StmChangeLog changeLog)
		{
			ChangeLog = changeLog;
			Triggers = new List<IWorkflowTrigger>();
		}

		public StmChangeLog ChangeLog { get; }
		public List<IWorkflowTrigger> Triggers { get; }

		public StmChangeLog Key
		{
			get
			{
				return ChangeLog;
			}
		}

		public IEnumerator<IWorkflowTrigger> GetEnumerator()
		{
			return Triggers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Triggers.GetEnumerator();
		}

		public bool Equals(ChangeLogTriggerGrouping other)
		{
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.ChangeLog, ChangeLog);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof(ChangeLogTriggerGrouping))
			{
				return false;
			}

			return Equals((ChangeLogTriggerGrouping)obj);
		}

		public override int GetHashCode()
		{
			return (ChangeLog != null ? ChangeLog.GetHashCode() : 0);
		}
	}
}
