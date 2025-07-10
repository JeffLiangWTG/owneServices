#if DEBUG

namespace Enterprise.Integration
{
	public interface IWorkflowServiceTaskTestHelper
	{
		string RunLogWalker();
		string RunFieldChangeTriggerProcessor();
	}
}

#endif
