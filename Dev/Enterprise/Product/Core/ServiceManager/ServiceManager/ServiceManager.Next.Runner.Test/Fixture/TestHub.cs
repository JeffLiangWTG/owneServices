using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CargoWise.ServiceManager.Next.Runner.Test.Fixture;

// Hub for mock server
internal sealed class TestHub : Hub
{
	readonly ILogger logger;
	public static readonly ConcurrentDictionary<string, int> ProcessIdByConnectionId = new ();
	public static readonly ConcurrentDictionary<string, Exception> ExceptionByConnectionId = new ();

	// create a temporary host running a test SignalR server
	public static IHost CreateHost(string hubId)
	{
		var hostBuilder = new HostBuilder();
		hostBuilder.ConfigureWebHost(webHostBuilder =>
		{
			webHostBuilder.UseTestServer();
			webHostBuilder.ConfigureServices(services =>
			{
				services.AddSignalR();
				services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
			});
			webHostBuilder.Configure(appBuilder =>
			{
				appBuilder.UseRouting();
				appBuilder.UseEndpoints(endpoints => endpoints.MapHub<TestHub>($"signalR/{hubId}"));
			});
		});
		return hostBuilder.Build();
	}

	public TestHub(ILogger<TestHub> logger)
	{
		this.logger = logger;
	}

	public override async Task OnConnectedAsync()
	{
		await base.OnConnectedAsync().ConfigureAwait(false);
		logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		logger.LogDebug(exception, "Client disconnected: {ConnectionId}", Context.ConnectionId);
		await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
	}

	public void Disconnect()
	{
		logger.LogDebug("Client asked for disconnection: {ConnectionId}", Context.ConnectionId);
		Context.Abort();
	}

	public async Task CloseMe()
	{
		logger.LogDebug("Client asked for clean closure: {ConnectionId}", Context.ConnectionId);
		await Clients.Caller.InvokeAsync<bool>("Close", Context.ConnectionAborted);
	}

	public async Task CallProcessId()
	{
		var connectionId = Context.ConnectionId;
		logger.LogDebug("Client asked for processId: {ConnectionId}", connectionId);
		try
		{
			var result = await Clients.Caller.InvokeAsync<int>("ProcessId", Context.ConnectionAborted);
			ProcessIdByConnectionId[connectionId] = result;
		}
		catch (Exception e)
		{
			logger.LogDebug(e, "Error while calling client");
			ExceptionByConnectionId[connectionId] = e;
		}
	}
}
