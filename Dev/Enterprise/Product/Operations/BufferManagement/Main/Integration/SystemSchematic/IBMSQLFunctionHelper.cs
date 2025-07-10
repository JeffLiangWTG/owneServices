namespace Enterprise.BufferManagement.Integration
{
	public interface IBMSQLFunctionHelper
	{
		string GetCurrentTasks { get; }
		string GetCurrentTasksInWorkflows { get; }
	}
}
