using CargoWise.ServiceManager.Next.Runner.Test.Fixture;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Runner.Test;

abstract class BaseTokenClientTest<T> where T : class, ITokenClient
{
	protected CancellationTokenSource? cancellationTokenSource;
	protected Mock<IHostApplicationLifetime>? hostApplicationLifetimeMock;
	IHost? host;
	HubConnection? hubConnection;
	ServiceProvider? serviceProvider;

	[SetUp]
	public void SetUpBase()
	{
		var hubId = Guid.NewGuid().ToString();
		host = TestHub.CreateHost(hubId);
		cancellationTokenSource = new CancellationTokenSource();
		hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>(MockBehavior.Strict);

		hostApplicationLifetimeMock.Setup(x => x.ApplicationStopping).Returns(cancellationTokenSource.Token);

		serviceProvider = BuildServiceCollection().BuildServiceProvider();
		var nextRunnerOptions = new TestNextRunnerOptions(hubId);
		hubConnection = new HubConnectionBuilder()
			.WithUrl(
				nextRunnerOptions.LauncherHub,
				o => o.HttpMessageHandlerFactory = _ => host.GetTestServer().CreateHandler())
			.Build();
	}

	protected virtual ServiceCollection BuildServiceCollection()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(hostApplicationLifetimeMock!.Object);
		serviceCollection.AddSingleton<ITokenClient, T>();
		return serviceCollection;
	}

	[TearDown]
	public void TearDownBase()
	{
		cancellationTokenSource?.Dispose();
		serviceProvider?.Dispose();
		hostApplicationLifetimeMock?.VerifyNoOtherCalls();
		host?.Dispose();
	}

	protected async Task<TResponse> SendSignalRRequest<TRequest, TResponse>(string method, TRequest request)
	{
		var hubContext = host!.Services.GetRequiredService<IHubContext<TestHub>>();
		var connectionId = hubConnection!.ConnectionId;
		Assert.That(connectionId, Is.Not.Null);
		var hubContextClient = hubContext.Clients.Client(connectionId!);

		return await hubContextClient.InvokeAsync<TResponse>(method, request, cancellationTokenSource!.Token);
	}

	protected async Task RegisterTokenClientAndStartSignalR()
	{
		var tokenClient = serviceProvider!.GetRequiredService<ITokenClient>();
		tokenClient.RegisterTokenClient(hubConnection!);

		await host!.StartAsync();
		await hubConnection!.StartAsync();
	}

	protected static string? Serialize<TData>(TData? data)
	{
		return data is null ? null : System.Text.Json.JsonSerializer.Serialize(data);
	}
}
