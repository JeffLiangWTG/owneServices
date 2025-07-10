using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public record ScheduleWithInfo(ServiceTaskInfo Info, IServiceTask Task)
	{
		public ServiceTaskInfo Info { get; } = Info;
		public IServiceTask Task { get; } = Task;
	}
}
