namespace Enterprise.BufferManagement.Integration
{
	public interface IJobOrWorkflowOptimisable
	{
		bool ShouldOptimiseQueryForWorkflowOnly { get; set; }
	}
}
