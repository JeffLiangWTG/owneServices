using CargoWise.ServiceManager.Next.Runner.Test.Fixture;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Runner.Test;

class SignalRBackgroundServiceTest
{
	CancellationTokenSource? cancellationTokenSource;
	Mock<IHostApplicationLifetime>? hostApplicationLifetimeMock;
	Mock<IServiceScopeFactory>? serviceScopeFactoryMock;
	IHost? host;
	TestNextRunnerOptions? testNextRunnerOptions;

	[SetUp]
	public void Setup()
	{
		cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>(MockBehavior.Strict);
		hostApplicationLifetimeMock.Setup(m => m.StopApplication());
		hostApplicationLifetimeMock.Setup(m => m.ApplicationStopping).Returns(cancellationTokenSource.Token);
		serviceScopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
		serviceScopeFactoryMock.Setup(m => m.CreateScope().Dispose());
		var hubId = Guid.NewGuid().ToString();
		testNextRunnerOptions = new TestNextRunnerOptions(hubId);
		host = TestHub.CreateHost(hubId);
	}

	[TearDown]
	public void TearDown()
	{
		hostApplicationLifetimeMock!.VerifyNoOtherCalls();
		serviceScopeFactoryMock!.VerifyNoOtherCalls();
		cancellationTokenSource?.Dispose();
		host?.Dispose();
	}

	[Test]
	public async Task ExitApplication_UnhandledException_WhenConnectionFailsAtStartup()
	{
		using var signalRBackgroundService = CreateSignalRBackgroundService();
		var connectionStarted = false;
		signalRBackgroundService.OnConnectionStarted += _ =>
		{
			connectionStarted = true;
			return Task.CompletedTask;
		};
		await signalRBackgroundService.StartAsync(cancellationTokenSource!.Token);
		if (signalRBackgroundService.ExecuteTask is { } executeTask)
		{
			await executeTask;
		}
		Assert.Multiple(() =>
		{
			Assert.That(signalRBackgroundService.ExitCode, Is.EqualTo(RunnerExitCode.ServiceTaskUnhandledException));
			Assert.That(connectionStarted, Is.False);
			hostApplicationLifetimeMock!.Verify(m => m.StopApplication(), Times.Once);
		});
	}

	[Test]
	public async Task ExitApplication_NoIssues_WhenTokenCancelled()
	{
		await host!.StartAsync();
		using var signalRBackgroundService = CreateSignalRBackgroundService();
		var connectionStarted = false;
		signalRBackgroundService.OnConnectionStarted += _ =>
		{
			connectionStarted = true;
			cancellationTokenSource!.Cancel();
			return Task.CompletedTask;
		};
		await signalRBackgroundService.StartAsync(cancellationTokenSource!.Token);
		if (signalRBackgroundService.ExecuteTask is { } executeTask)
		{
			await executeTask;
		}
		Assert.Multiple(() =>
		{
			Assert.That(signalRBackgroundService.ExitCode, Is.EqualTo(RunnerExitCode.NoIssues));
			Assert.That(connectionStarted, Is.True);
			hostApplicationLifetimeMock!.Verify(m => m.StopApplication(), Times.Never);
		});
	}

	[Test]
	public async Task ExitApplication_NoIssues_WhenServiceClosed()
	{
		await host!.StartAsync();
		using var signalRBackgroundService = CreateSignalRBackgroundService();
		var connectionStarted = false;
		var cancellationToken = cancellationTokenSource!.Token;
		signalRBackgroundService.OnConnectionStarted += async _ =>
		{
			connectionStarted = true;
			await signalRBackgroundService.StopAsync(cancellationToken);
		};
		await signalRBackgroundService.StartAsync(cancellationToken);
		if (signalRBackgroundService.ExecuteTask is { } executeTask)
		{
			await executeTask;
		}
		Assert.Multiple(() =>
		{
			Assert.That(signalRBackgroundService.ExitCode, Is.EqualTo(RunnerExitCode.NoIssues));
			Assert.That(connectionStarted, Is.True);
			Assert.That(cancellationToken.IsCancellationRequested, Is.False);
			hostApplicationLifetimeMock!.Verify(m => m.StopApplication(), Times.Never);
		});
	}

	[Test]
	public async Task ExitApplication_RunnerFailure_WhenSignalRServerIsStopped()
	{
		await host!.StartAsync();
		using var signalRBackgroundService = CreateSignalRBackgroundService();
		var connectionStarted = false;
		var cancellationToken = cancellationTokenSource!.Token;
		signalRBackgroundService.OnConnectionStarted += async hubConnection =>
		{
			connectionStarted = true;
			// call disconnect to simulate a server disconnection
			await hubConnection.InvokeAsync(nameof(TestHub.Disconnect), cancellationToken);
		};
		await signalRBackgroundService.StartAsync(cancellationToken);
		if (signalRBackgroundService.ExecuteTask is { } executeTask)
		{
			await executeTask;
		}
		Assert.Multiple(() =>
		{
			Assert.That(signalRBackgroundService.ExitCode, Is.EqualTo(RunnerExitCode.RunnerFailure));
			Assert.That(connectionStarted, Is.True);
			hostApplicationLifetimeMock!.Verify(m => m.StopApplication(), Times.Once);
		});
	}

	[Test]
	public async Task ExitApplication_NoIssues_WhenSignalRServerSendClose()
	{
		await host!.StartAsync();
		using var signalRBackgroundService = CreateSignalRBackgroundService();
		var connectionStarted = false;
		var cancellationToken = cancellationTokenSource!.Token;
		signalRBackgroundService.OnConnectionStarted += async hubConnection =>
		{
			connectionStarted = true;
			// call CloseMe for server to call Close
			await hubConnection.InvokeAsync(nameof(TestHub.CloseMe), cancellationToken);
		};
		await signalRBackgroundService.StartAsync(cancellationToken);
		if (signalRBackgroundService.ExecuteTask is { } executeTask)
		{
			await executeTask;
		}
		Assert.Multiple(() =>
		{
			Assert.That(signalRBackgroundService.ExitCode, Is.EqualTo(RunnerExitCode.NoIssues));
			Assert.That(connectionStarted, Is.True);
			hostApplicationLifetimeMock!.Verify(m => m.StopApplication(), Times.Once);
		});
	}

	[Test]
	public async Task SignCwToken_SignCwTokenAsync_WhenSignalRServerSendSignCwToken()
	{
		var tokenGeneratorServiceMock = new Mock<ITokenGeneratorService>(MockBehavior.Strict); 
		tokenGeneratorServiceMock
			.Setup(m => m.SignCwTokenAsync(It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((SignCwTokenRequest request, CancellationToken c) => new SignCwTokenResponse($"test token for {request}"));
		var audience = Guid.NewGuid().ToString();
		var request = new SignCwTokenRequest(audience);
		var (response, exception) = await StartServiceAndSendRequest<SignCwTokenRequest, SignCwTokenResponse, ITokenGeneratorService>(
			"SignCwToken",
			request,
			tokenGeneratorServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.EqualTo(new SignCwTokenResponse(token: $"test token for SignCwTokenRequest {{ Audience = {audience} }}")));
			Assert.That(exception, Is.Null);
			tokenGeneratorServiceMock.Verify(m => m.SignCwTokenAsync(request, It.IsAny<CancellationToken>()), Times.Once);
			tokenGeneratorServiceMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task SignCwToken_ReportException_WhenSignalRServerSendSignCwToken()
	{
		var tokenGeneratorServiceMock = new Mock<ITokenGeneratorService>(MockBehavior.Strict);
		tokenGeneratorServiceMock
			.Setup(m => m.SignCwTokenAsync(It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Dummy failure for unit tests"));
		var audience = Guid.NewGuid().ToString();
		var request = new SignCwTokenRequest(audience);
		var (response, exception) = await StartServiceAndSendRequest<SignCwTokenRequest, SignCwTokenResponse, ITokenGeneratorService>(
			"SignCwToken",
			request,
			tokenGeneratorServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.Null);
			Assert.That(exception, Is.TypeOf<HubException>().With.Message.EqualTo("Dummy failure for unit tests"));
			tokenGeneratorServiceMock.Verify(m => m.SignCwTokenAsync(request, It.IsAny<CancellationToken>()), Times.Once);
			tokenGeneratorServiceMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task PrepareNewCertificateRequest_PrepareNewCertificateRequest_WhenSignalRServerSendPrepareNewCertificateRequest()
	{
		var capturedRequests = new List<PrepareNewCertificateRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.PrepareNewCertificateAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PrepareNewCertificateResponse("Test CSR"));
		var request = new PrepareNewCertificateRequest();
		var (response, exception) = await StartServiceAndSendRequest<PrepareNewCertificateRequest, PrepareNewCertificateResponse, ITokenConfigWriterService>(
			"PrepareNewCertificate",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.EqualTo(new PrepareNewCertificateResponse("Test CSR")));
			Assert.That(exception, Is.Null);
			Assert.That(capturedRequests.Select(Serialize), Is.EquivalentTo(new[] { "{}", }));
			tokenConfigWriterServiceMock.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()),
				Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task PrepareNewCertificateRequest_ReportException_WhenSignalRServerSendPrepareNewCertificateRequest()
	{
		var capturedRequests = new List<PrepareNewCertificateRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.PrepareNewCertificateAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Dummy failure for unit tests"));
		var request = new PrepareNewCertificateRequest();
		var (response, exception) = await StartServiceAndSendRequest<PrepareNewCertificateRequest, PrepareNewCertificateResponse, ITokenConfigWriterService>(
			"PrepareNewCertificate",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.Null);
			Assert.That(exception, Is.TypeOf<HubException>().With.Message.EqualTo("Dummy failure for unit tests"));
			Assert.That(capturedRequests.Select(Serialize), Is.EquivalentTo(new[] { "{}", }));
			tokenConfigWriterServiceMock.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()),
				Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task ResetAccessTokenRequest_ResetAccessToken_WhenSignalRServerSendResetAccessTokenRequest()
	{
		var capturedRequests = new List<ResetAccessTokenRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.ResetAccessTokenAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ResetAccessTokenResponse());
		var request = new ResetAccessTokenRequest();
		var (response, exception) = await StartServiceAndSendRequest<ResetAccessTokenRequest, ResetAccessTokenResponse, ITokenConfigWriterService>(
			"ResetAccessToken",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(Serialize(response), Is.EqualTo("{}"));
			Assert.That(exception, Is.Null);
			Assert.That(capturedRequests.Select(Serialize), Is.EquivalentTo(new[] { "{}", }));
			tokenConfigWriterServiceMock.Verify(
				m => m.ResetAccessTokenAsync(It.IsAny<ResetAccessTokenRequest>(), It.IsAny<CancellationToken>()),
				Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task ResetAccessTokenRequest_ReportException_WhenSignalRServerSendResetAccessTokenRequest()
	{
		var capturedRequests = new List<ResetAccessTokenRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.ResetAccessTokenAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Dummy failure for unit tests"));
		var request = new ResetAccessTokenRequest();
		var (response, exception) = await StartServiceAndSendRequest<ResetAccessTokenRequest, ResetAccessTokenResponse, ITokenConfigWriterService>(
			"ResetAccessToken",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.Null);
			Assert.That(exception, Is.TypeOf<HubException>().With.Message.EqualTo("Dummy failure for unit tests"));
			Assert.That(capturedRequests.Select(Serialize), Is.EquivalentTo(new[] { "{}", }));
			tokenConfigWriterServiceMock.Verify(
				m => m.ResetAccessTokenAsync(It.IsAny<ResetAccessTokenRequest>(), It.IsAny<CancellationToken>()),
				Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task SetOperationIdRequest_SetOperationId_WhenSignalRServerSendSetOperationIdRequest()
	{
		var capturedRequests = new List<SetOperationIdRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.SetOperationIdAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new SetOperationIdResponse());
		var request = new SetOperationIdRequest("Test CSR", "Test OperationId");
		var (response, exception) = await StartServiceAndSendRequest<SetOperationIdRequest, SetOperationIdResponse, ITokenConfigWriterService>(
			"SetOperationId",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.Not.Null);
			Assert.That(exception, Is.Null);
			Assert.That(capturedRequests, Is.EquivalentTo(new[] { request, }));
			tokenConfigWriterServiceMock.Verify(
				m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), It.IsAny<CancellationToken>()),
				Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
		Assert.That(System.Text.Json.JsonSerializer.Serialize(response), Is.EqualTo("{}"));
	}

	[Test]
	public async Task SetOperationIdRequest_ReportException_WhenSignalRServerSendSetOperationIdRequest()
	{
		var capturedRequests = new List<SetOperationIdRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.SetOperationIdAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Dummy failure for unit tests"));
		var request = new SetOperationIdRequest("Test CSR", "Test OperationId");
		var (response, exception) = await StartServiceAndSendRequest<SetOperationIdRequest, SetOperationIdResponse, ITokenConfigWriterService>(
			"SetOperationId",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.Null);
			Assert.That(exception, Is.TypeOf<HubException>().With.Message.EqualTo("Dummy failure for unit tests"));
			Assert.That(capturedRequests, Is.EquivalentTo(new[] { request, }));
			tokenConfigWriterServiceMock.Verify(m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), It.IsAny<CancellationToken>()), Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
	}

	[Test]
	public async Task SetNewCertificateCredentialsRequest_SetNewCertificateCredentials_WhenSignalRServerSendSetNewCertificateCredentialsRequest()
	{
		var capturedRequests = new List<SetNewCertificateCredentialsRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.SetNewCertificateCredentialsAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new SetNewCertificateCredentialsResponse());
		var certificate = new byte[] { 1, 2, 3, 4, 5, };
		var request = new SetNewCertificateCredentialsRequest("Test OperationId", "Test TenantId", "Test ClientId", certificate);
		var (response, exception) = await StartServiceAndSendRequest<SetNewCertificateCredentialsRequest, SetNewCertificateCredentialsResponse, ITokenConfigWriterService>(
			"SetNewCertificateCredentials",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.Not.Null);
			Assert.That(exception, Is.Null);
			Assert.That(capturedRequests, Is.EquivalentTo(new[] { request, }));
			tokenConfigWriterServiceMock.Verify(m => m.SetNewCertificateCredentialsAsync(It.IsAny<SetNewCertificateCredentialsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
		Assert.That(System.Text.Json.JsonSerializer.Serialize(response), Is.EqualTo("{}"));
	}

	[Test]
	public async Task SetNewCertificateCredentialsRequest_ReportException_WhenSignalRServerSendSetNewCertificateCredentialsRequest()
	{
		var capturedRequests = new List<SetNewCertificateCredentialsRequest>();
		var tokenConfigWriterServiceMock = new Mock<ITokenConfigWriterService>(MockBehavior.Strict);
		tokenConfigWriterServiceMock
			.Setup(m => m.SetNewCertificateCredentialsAsync(Capture.In(capturedRequests), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Dummy failure for unit tests"));
		var certificate = new byte[] { 5, 8, 2, 1, };
		var request = new SetNewCertificateCredentialsRequest("Test OperationId", "Test TenantId", "Test ClientId", certificate);
		var (response, exception) = await StartServiceAndSendRequest<SetNewCertificateCredentialsRequest, SetNewCertificateCredentialsResponse, ITokenConfigWriterService>(
			"SetNewCertificateCredentials",
			request,
			tokenConfigWriterServiceMock);
		Assert.Multiple(() =>
		{
			Assert.That(response, Is.Null);
			Assert.That(exception, Is.TypeOf<HubException>().With.Message.EqualTo("Dummy failure for unit tests"));
			Assert.That(capturedRequests, Is.EquivalentTo(new[] { request, }));
			tokenConfigWriterServiceMock.Verify(m => m.SetNewCertificateCredentialsAsync(It.IsAny<SetNewCertificateCredentialsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
			tokenConfigWriterServiceMock.VerifyNoOtherCalls();
		});
	}

	async Task<(TResponse?, Exception?)> StartServiceAndSendRequest<TRequest, TResponse, TService>(string method, TRequest request, Mock<TService> mockService) where TService : class
	{
		serviceScopeFactoryMock!
			.Setup(m => m.CreateScope().ServiceProvider.GetService(typeof(TService)))
			.Returns(mockService.Object);
		await host!.StartAsync();
		using var signalRBackgroundService = CreateSignalRBackgroundService();
		var connectionStarted = false;
		var cancellationToken = cancellationTokenSource!.Token;
		Exception? exception = null;
		TResponse? response = default;
		signalRBackgroundService.OnConnectionStarted += async hubConnection =>
		{
			connectionStarted = true;
			var connectionId = hubConnection.ConnectionId;
			Assert.That(connectionId, Is.Not.Null);
			// call method on client
			(response, exception) = await SendSignalRRequest<TRequest, TResponse>(connectionId!, method, request);
			// stop the service after that to stop the test
			await signalRBackgroundService.StopAsync(cancellationToken);
		};
		await signalRBackgroundService.StartAsync(cancellationToken);
		if (signalRBackgroundService.ExecuteTask is { } executeTask)
		{
			await executeTask;
		}
		Assert.Multiple(() =>
		{
			Assert.That(signalRBackgroundService.ExitCode, Is.EqualTo(RunnerExitCode.NoIssues));
			Assert.That(connectionStarted, Is.True);
			serviceScopeFactoryMock!.Verify(m => m.CreateScope().ServiceProvider.GetService(typeof(TService)), Times.Once);
			serviceScopeFactoryMock!.Verify(m => m.CreateScope().Dispose(), Times.Once);
			hostApplicationLifetimeMock!.Verify(m => m.ApplicationStopping, Times.Once);
			hostApplicationLifetimeMock!.Verify(m => m.StopApplication(), Times.Never);
		});
		return (response, exception);
	}

	[Test]
	public async Task ProcessId_ReturnProcessId_WhenSignalRServerSendProcessId()
	{
		await host!.StartAsync();
		using var signalRBackgroundService = CreateSignalRBackgroundService();
		var connectionStarted = false;
		var cancellationToken = cancellationTokenSource!.Token;
		string? connectionId = null;
		signalRBackgroundService.OnConnectionStarted += async hubConnection =>
		{
			connectionStarted = true;
			connectionId = hubConnection.ConnectionId;
			// call CallProcessId for server to call ProcessId
			await hubConnection.InvokeAsync(nameof(TestHub.CallProcessId), cancellationToken);
			// stop the service after that to stop the test
			await signalRBackgroundService.StopAsync(cancellationToken);
		};
		await signalRBackgroundService.StartAsync(cancellationToken);
		if (signalRBackgroundService.ExecuteTask is { } executeTask)
		{
			await executeTask;
		}
		Assert.Multiple(() =>
		{
			Assert.That(signalRBackgroundService.ExitCode, Is.EqualTo(RunnerExitCode.NoIssues));
			Assert.That(connectionStarted, Is.True);
			Assert.That(TestHub.ProcessIdByConnectionId, Does.ContainKey(connectionId!).WithValue(Environment.ProcessId));
			Assert.That(TestHub.ExceptionByConnectionId, Does.Not.ContainKey(connectionId!));
			hostApplicationLifetimeMock!.Verify(m => m.ApplicationStopping, Times.Never);
			hostApplicationLifetimeMock!.Verify(m => m.StopApplication(), Times.Never);
		});
	}

	SignalRBackgroundService CreateSignalRBackgroundService()
	{
		var logger = host!.Services.GetRequiredService<ILogger<SignalRBackgroundService>>();
		var tokenGeneratorServiceClient = new TokenGeneratorServiceClient(serviceScopeFactoryMock!.Object, hostApplicationLifetimeMock!.Object);
		var tokenConfigWriterServiceClient = new TokenConfigWriterServiceClient(serviceScopeFactoryMock!.Object, hostApplicationLifetimeMock!.Object);
		return new SignalRBackgroundService(
			logger,
			hostApplicationLifetimeMock!.Object,
			testNextRunnerOptions!,
			new TestHttpClientFactory(_ => host.GetTestServer().CreateHandler()),
			serviceScopeFactoryMock!.Object,
			new ITokenClient[] { tokenGeneratorServiceClient, tokenConfigWriterServiceClient, });
	}

	async Task<(TResponse?, Exception?)> SendSignalRRequest<TRequest, TResponse>(string connectionId, string method, TRequest request)
	{
		var hubContext = host!.Services.GetRequiredService<IHubContext<TestHub>>();
		Assert.That(connectionId, Is.Not.Null);
		var hubContextClient = hubContext.Clients.Client(connectionId!);

		try
		{
			var response = await hubContextClient.InvokeAsync<TResponse>(method, request, cancellationTokenSource!.Token);
			return (response, null);
		}
		catch (Exception e)
		{
			return (default, e);
		}
	}

	// Mock factory to ensure SignalR connection is using the mock test server
	class TestHttpClientFactory : IHttpMessageHandlerFactory
	{
		readonly Func<string, HttpMessageHandler> factory;

		public TestHttpClientFactory(Func<string, HttpMessageHandler> factory)
		{
			this.factory = factory;
		}

		public HttpMessageHandler CreateHandler(string name) => factory(name);
	}

	static string? Serialize<TData>(TData? data)
	{
		return data is null ? null : System.Text.Json.JsonSerializer.Serialize(data);
	}
}
