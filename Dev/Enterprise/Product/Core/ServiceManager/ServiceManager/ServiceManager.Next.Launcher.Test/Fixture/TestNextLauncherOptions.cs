using CargoWise.ServiceManager.Next.Shared;
using Microsoft.AspNetCore.Server.HttpSys;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace CargoWise.ServiceManager.Next.Launcher.Test.Fixture;

record TestNextLauncherOptions : INextLauncherOptions
{
	public string DatabaseName { get; init; } = "TestDatabaseName";
	public string ServerName { get; init; } = "TestServerName";
	public string EnterpriseCode { get; init; } = "WTL";
	public string LauncherHubEndpoint { get; init; } = new($"test/signalR/{Guid.NewGuid()}");
	public TimeSpan RunnerIdleLifetime { get; init; } = TimeSpan.FromSeconds(1);
	public ServiceType ServiceType { get; } = ServiceType.LauncherSecurity;
	public string ServerCode { get; init; } = "SER";

	internal Uri ExpectedHubUri => new($"http://localhost:7070/cargowise/processController/{EnterpriseCode}{ServerCode}/security/{LauncherHubEndpoint}");
	internal UrlPrefix ExpectedListenerPrefix => UrlPrefix.Create($"http://+:7070/cargowise/processController/{EnterpriseCode}{ServerCode}/security");
}
