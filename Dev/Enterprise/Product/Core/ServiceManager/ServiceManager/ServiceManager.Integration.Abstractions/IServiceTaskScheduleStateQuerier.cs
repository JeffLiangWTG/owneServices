namespace ServiceManager.Integration.Abstractions
{
	public interface IServiceTaskScheduleStateQuerier
	{
		bool TryGetServiceTaskScheduleState(string serviceTaskCode, out string scheduleState);
	}
}
