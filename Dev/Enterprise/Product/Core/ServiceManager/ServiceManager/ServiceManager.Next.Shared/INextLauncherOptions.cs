using ServiceManager.Integration.ServiceHostUtilities;

namespace CargoWise.ServiceManager.Next.Shared;

public interface INextLauncherOptions : INextSharedOptions
{
	string EnterpriseCode { get; }
	string LauncherHubEndpoint { get; }
	TimeSpan RunnerIdleLifetime { get; }
	ServiceHostProcess.ServiceType ServiceType { get; }
	string ServerCode { get; }
}
