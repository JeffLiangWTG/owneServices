using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class NaiveTaskOrderable : ITaskOrderable
	{
		public NaiveTaskOrderable(ProcessTask processTask, BMBoardSectionViewModel viewModel)
			: this(processTask, viewModel?.ShowJobCards, viewModel?.Cache)
		{
		}

		public NaiveTaskOrderable(ProcessTask processTask, bool? showJobCards, PropertyCache cache)
		{
			Argument.NotNull(showJobCards, nameof(showJobCards));
			Argument.NotNull(cache, nameof(cache));

			Task = processTask;

			Status = Task.P9_Status;
			Sequence = Task.P9_Sequence;
			TaskID = Task.P9_TaskID;

			if (cache == null)
			{
				IsCurrent = Task.IsStartable();
				Nudge = Task.GetEffectiveNudge(showJobCards ?? false);
			}
			else
			{
				IsCurrent = Task.IsStartable(cache);
				Nudge = cache.GetCachedValue(Task.P9_FH_ProcessHeader, BMBoardSectionViewModel.CacheConstants.EffectiveNudge, () => Task.GetEffectiveNudge(showJobCards ?? false));
			}
		}

		ProcessTask Task { get; }
		public bool IsCurrent { get; }
		public string Status { get; }
		public ZDecimal Nudge { get; }
		public int Sequence { get; }
		public string TaskID { get; }
		public ZDateTime ReleaseDate => WorkflowOrderable.ReleaseDateTime;
		public IWorkflowOrderable WorkflowOrderable => Task.GetProcessHeader();
	}
}
