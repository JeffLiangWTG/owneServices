using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Model = CargoWise.PAVE.Common.Model;

namespace Enterprise.BufferManagement.Service
{
	class ModelToTaskOrderable : ITaskOrderable
	{
		public ModelToTaskOrderable(Model.Task task, Model.Workflow workflow)
		{
			TaskID = task.ID;
			IsCurrent = task.IsStartable ?? false;
			Status = task.Status;
			Sequence = task.Sequence;
			Nudge = workflow?.Nudge ?? 0;
			ReleaseDate = workflow?.ReleaseDateTimeUTC ?? ZDateTime.Empty;
		}

		public bool IsCurrent { get; }

		public string Status { get; }

		public ZDecimal Nudge { get; }

		public ZDateTime ReleaseDate { get; }

		public int Sequence { get; }

		public string TaskID { get; }

		public IWorkflowOrderable WorkflowOrderable => null;
	}
}
