using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderAndChildrenProcessTaskCollectionView : ProcessHeaderProcessTaskCollectionView
	{
		public ProcessHeaderAndChildrenProcessTaskCollectionView(ProcessHeader processHeader, bool allowCompletionStatements = false)
			: base(processHeader, allowCompletionStatements)
		{
		}

		protected override bool BelongsToParent(ProcessTask task)
		{
			return base.BelongsToParent(task) || IsTaskInDescendentWorkflow(task);
		}

		protected virtual ProcessHeader GetProcessHeader(ProcessTask task)
		{
			return task.GetProcessHeader();
		}

		bool IsTaskInDescendentWorkflow(ProcessTask task)
		{
			var workflow = GetProcessHeader(task);
			return workflow != null && !workflow.IsDeleted && workflow.GetWorkflowParents().Any(parent => parent == ProcessHeader);
		}
	}
}
