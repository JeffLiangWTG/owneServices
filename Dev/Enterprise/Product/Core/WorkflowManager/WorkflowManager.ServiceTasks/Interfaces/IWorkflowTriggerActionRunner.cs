namespace Enterprise.WorkflowManager.ServiceTasks
{
	public interface IWorkflowTriggerActionRunner
	{
		void Run(IWorkflowTriggerAction action);
	}
}
