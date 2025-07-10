using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class CompletionStatementProcessTaskCollectionView : ProcessHeaderAndChildrenProcessTaskCollectionView
	{
		public CompletionStatementProcessTaskCollectionView(ProcessHeader workflow)
			: base(workflow, allowCompletionStatements: true)
		{
			completionStatementTaskType = ProcessTask.GetCompletionStatementTaskType(workflow.Parent.WorkflowType);
		}

		readonly string completionStatementTaskType;

		protected override bool IsTaskPartOfCollection(ProcessTask task)
		{
			return base.IsTaskPartOfCollection(task) && task.IsCompletionStatement;
		}

		protected override void AddNewTaskCore(ProcessTask task)
		{
			using (task.GetValidationSuspender())
			{
				base.AddNewTaskCore(task);
				ProcessTaskCompletionStatementCollectionView.SetDefaultsOnNewCompletionStatement(this, task, completionStatementTaskType);
			}
		}
	}
}
