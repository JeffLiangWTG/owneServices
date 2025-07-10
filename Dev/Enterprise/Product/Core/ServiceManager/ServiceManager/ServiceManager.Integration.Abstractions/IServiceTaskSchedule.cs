using Enterprise.Integration.ServiceManager;

namespace ServiceManager.Integration.Abstractions
{
	public interface IServiceTaskSchedule : IStmScheduleTask
	{
		string ConfigString { get; set; }
		string GetBranchCountryCode();
		bool IsNudgeable { get; }
	}
}
