using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class TaskOrderableSnapshot : ITaskOrderable
	{
		public TaskOrderableSnapshot(ICardContent content, ProcessTask task, PropertyCache cache)
		{
			IsCurrent = content.IsCurrent;
			Status = task.P9_Status;
			Sequence = task.P9_Sequence;
			TaskID = task.P9_TaskID;

			var workflow = content.GetWorkflow(task.Factory);
			WorkflowOrderable = new WorkflowOrderableSnapshot(workflow, cache);
		}

		public bool IsCurrent { get; private set; }
		public string Status { get; private set; }
		public int Sequence { get; private set; }
		public string TaskID { get; private set; }

		public IWorkflowOrderable WorkflowOrderable { get; private set; }

		public ZDecimal Nudge
		{
			get { return WorkflowOrderable.EffectiveNudge; }
		}

		public ZDateTime ReleaseDate
		{
			get { return WorkflowOrderable.ReleaseDateTime; }
		}
	}
}
