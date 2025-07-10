namespace Enterprise.BufferManagement.Integration
{
	public interface IJobOrWorkflowFilter
	{
		void SetJobOnly();
		void SetWorkflowOnly();
		void SetJobAndWorkflow();
	}
}
