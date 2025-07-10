using CargoWise.ServiceManager.Next.Shared;

namespace CargoWise.ServiceManager.Next.Runner.Test.Fixture;

record TestNextRunnerOptions : INextRunnerOptions
{
	public TestNextRunnerOptions(string hubId)
	{
		LauncherHub = new Uri($"http://localhost/signalR/{hubId}");
	}

	public Uri LauncherHub { get; init; }
	public string DatabaseName { get; init; } = "TestDatabaseName";
	public string ServerName { get; init; } = "TestServerName";
}
