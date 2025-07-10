using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Runner.Test;

public sealed class TokenClientTest
{
	CancellationTokenSource? cancellationTokenSource;
	ServiceProvider? serviceProvider;
	Mock<IHostApplicationLifetime>? hostApplicationLifetimeMock;
	Mock<ITestService>? tokenServiceMock;

	[SetUp]
	public void SetUp()
	{
		cancellationTokenSource = new CancellationTokenSource();
		hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>(MockBehavior.Strict);
		tokenServiceMock = new Mock<ITestService>(MockBehavior.Strict);

		hostApplicationLifetimeMock.Setup(x => x.ApplicationStopping).Returns(cancellationTokenSource.Token);
		tokenServiceMock.Setup(x => x.Dispose());

		var serviceCollection = new ServiceCollection();
		serviceCollection.AddScoped(sp => tokenServiceMock.Object);
		serviceCollection.AddSingleton(hostApplicationLifetimeMock.Object);
		serviceCollection.AddSingleton<TestTokenClient>();
		serviceProvider = serviceCollection.BuildServiceProvider();
	}

	void SetupTokenServiceForSuccess()
	{
		tokenServiceMock!.Setup(x => x.TestMethod(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
		tokenServiceMock.Setup(x => x.TestResultMethod(It.IsAny<CancellationToken>())).ReturnsAsync("Test Result");
	}

	void SetupTokenServiceForFailure()
	{
		tokenServiceMock!.Setup(x => x.TestMethod(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test Exception"));
		tokenServiceMock.Setup(x => x.TestResultMethod(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test Exception"));
	}

	[TearDown]
	public void TearDown()
	{
		cancellationTokenSource?.Dispose();
		serviceProvider?.Dispose();
		hostApplicationLifetimeMock?.VerifyNoOtherCalls();
		tokenServiceMock?.VerifyNoOtherCalls();
	}

	[Test]
	public async Task CallScopedService_Task_Success()
	{
		SetupTokenServiceForSuccess();
		var tokenClient = serviceProvider!.GetRequiredService<TestTokenClient>();

		await tokenClient.CallTestMethod();

		hostApplicationLifetimeMock!.Verify(x => x.ApplicationStopping, Times.Once);
		tokenServiceMock!.Verify(x => x.TestMethod(cancellationTokenSource!.Token), Times.Once);
		tokenServiceMock.Verify(x => x.Dispose(), Times.Once);
	}

	[Test]
	public void CallScopedService_Task_Failure()
	{
		SetupTokenServiceForFailure();
		var tokenClient = serviceProvider!.GetRequiredService<TestTokenClient>();

		var e = Assert.ThrowsAsync<Exception>(() => tokenClient.CallTestMethod());
		Assert.That(e?.Message, Is.EqualTo("Test Exception"));

		hostApplicationLifetimeMock!.Verify(x => x.ApplicationStopping, Times.Once);
		tokenServiceMock!.Verify(x => x.TestMethod(cancellationTokenSource!.Token), Times.Once);
		tokenServiceMock.Verify(x => x.Dispose(), Times.Once);
	}

	[Test]
	public async Task CallScopedService_TaskResult_Success()
	{
		SetupTokenServiceForSuccess();
		var tokenClient = serviceProvider!.GetRequiredService<TestTokenClient>();

		var result = await tokenClient.CallTestResultMethod();

		Assert.That(result, Is.EqualTo("Test Result"));
		hostApplicationLifetimeMock!.Verify(x => x.ApplicationStopping, Times.Once);
		tokenServiceMock!.Verify(x => x.TestResultMethod(cancellationTokenSource!.Token), Times.Once);
		tokenServiceMock.Verify(x => x.Dispose(), Times.Once);
	}

	[Test]
	public void CallScopedService_TaskResult_Failure()
	{
		SetupTokenServiceForFailure();
		var tokenClient = serviceProvider!.GetRequiredService<TestTokenClient>();

		var e = Assert.ThrowsAsync<Exception>(() => tokenClient.CallTestResultMethod());
		Assert.That(e?.Message, Is.EqualTo("Test Exception"));

		hostApplicationLifetimeMock!.Verify(x => x.ApplicationStopping, Times.Once);
		tokenServiceMock!.Verify(x => x.TestResultMethod(cancellationTokenSource!.Token), Times.Once);
		tokenServiceMock.Verify(x => x.Dispose(), Times.Once);
	}

	public interface ITestService : IDisposable
	{
		Task TestMethod(CancellationToken cancellationToken);
		Task<string> TestResultMethod(CancellationToken cancellationToken);
	}

	public sealed class TestTokenClient(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime hostApplicationLifetime) : TokenClient<ITestService>(serviceScopeFactory, hostApplicationLifetime)
	{
		public override void RegisterTokenClient(HubConnection hubConnection)
		{
			throw new NotImplementedException("Unit Test");
		}

		public Task CallTestMethod()
		{
			return CallScopedService(tokenService => tokenService.TestMethod(CancellationToken));
		}

		public Task<string> CallTestResultMethod()
		{
			return CallScopedService(tokenService => tokenService.TestResultMethod(CancellationToken));
		}
	}
}
