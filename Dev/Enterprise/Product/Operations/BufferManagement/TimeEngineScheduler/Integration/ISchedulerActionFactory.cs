namespace Enterprise.TimeEngineScheduler.Integration
{
	public interface ISchedulerActionFactory
	{
		ISchedulerAction GetSchedulerAction(string code);
	}
}
