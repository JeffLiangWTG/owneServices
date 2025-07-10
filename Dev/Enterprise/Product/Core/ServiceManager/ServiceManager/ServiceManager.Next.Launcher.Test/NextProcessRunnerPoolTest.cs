using System.Linq.Expressions;
using CargoWise.ServiceManager.Next.Launcher.Test.Fixture;
using CargoWise.ServiceManager.Next.Shared;
using CargoWise.ServiceManager.Next.Shared.Services;
using CargoWise.SystemToSystemTrust.DataContracts;
using Moq;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

static class TestData
{
	public static IEnumerable<TestFixtureData> RunAsyncCases => new[]
	{
		new TestFixtureData(
			typeof(SignCwTokenRequest),
			typeof(SignCwTokenResponse),
			(ITokenHubClient client) => client.SignCwToken,
			new SignCwTokenRequest("audience"),
			new SignCwTokenResponse("mocked token")).SetArgDisplayNames(nameof(ITokenHubClient.SignCwToken)),
		new TestFixtureData(
			typeof(PrepareNewCertificateRequest),
			typeof(PrepareNewCertificateResponse),
			(ITokenHubClient client) => client.PrepareNewCertificate,
			new PrepareNewCertificateRequest(),
			new PrepareNewCertificateResponse("mocked csr")).SetArgDisplayNames(nameof(ITokenHubClient.PrepareNewCertificate)),
		new TestFixtureData(
			typeof(ResetAccessTokenRequest),
			typeof(ResetAccessTokenResponse),
			(ITokenHubClient client) => client.ResetAccessToken,
			new ResetAccessTokenRequest(),
			new ResetAccessTokenResponse()).SetArgDisplayNames(nameof(ITokenHubClient.ResetAccessToken)),
		new TestFixtureData(
			typeof(SetNewCertificateCredentialsRequest),
			typeof(SetNewCertificateCredentialsResponse),
			(ITokenHubClient client) => client.SetNewCertificateCredentials,
			new SetNewCertificateCredentialsRequest("opId","tId", "cId", [1, 2, 3]),
			new SetNewCertificateCredentialsResponse()).SetArgDisplayNames(nameof(ITokenHubClient.SetNewCertificateCredentials)),
		new TestFixtureData(
			typeof(SetOperationIdRequest),
			typeof(SetOperationIdResponse),
			(ITokenHubClient client) => client.SetOperationId,
			new SetOperationIdRequest("csr","operationId"),
			new SetOperationIdResponse()).SetArgDisplayNames(nameof(ITokenHubClient.SetOperationId)),
	};
}

class NextProcessRunnerPoolTestBase
{
	CancellationTokenSource? cancellationTokenSource;
	Mock<INextProcessRunnerFactory>? nextProcessRunnerFactoryMock;
	Mock<ICommandSender>? commandSenderMock;
	TestNextLauncherOptions? nextLauncherOptions;

	[SetUp]
	public void SetUp()
	{
		cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		nextProcessRunnerFactoryMock = new(MockBehavior.Strict);
		commandSenderMock = new(MockBehavior.Strict);
		nextLauncherOptions = new TestNextLauncherOptions();
	}

	[TearDown]
	public void TearDown()
	{
		cancellationTokenSource?.Cancel();
		nextProcessRunnerFactoryMock?.VerifyNoOtherCalls();
		commandSenderMock?.VerifyNoOtherCalls();
		cancellationTokenSource?.Dispose();
	}

	protected CancellationTokenSource CancellationTokenSource => cancellationTokenSource!;
	protected Mock<INextProcessRunnerFactory> NextProcessRunnerFactoryMock => nextProcessRunnerFactoryMock!;
	protected Mock<ICommandSender> CommandSenderMock => commandSenderMock!;
	protected TestNextLauncherOptions NextLauncherOptions => nextLauncherOptions!;

	protected NextProcessRunnerPool CreateNextProcessRunnerPool()
	{
		return new(nextProcessRunnerFactoryMock!.Object, commandSenderMock!.Object, nextLauncherOptions!);
	}

	protected void CheckNotYetStopped(NextProcessRunnerFixture processRunner)
	{
		Assert.That(processRunner.IsDisposed, Is.False, "Runner should not be disposed yet");
		Assert.That(processRunner.HasExited, Is.False, "Runner should not be closed yet");

		commandSenderMock!.Verify(m => m.CloseRunnerAsync(processRunner, It.IsAny<CancellationToken>()), Times.Never);
	}

	protected void WaitAndCheckGracefulStop(NextProcessRunnerFixture processRunner, CancellationToken cancellationToken)
	{
		Assert.That(SpinWait.SpinUntil(() => processRunner.IsDisposed, TimeSpan.FromSeconds(5)), Is.True, "Runner should be disposed by now");
		CheckDisposed(processRunner, expectedGracefulStop: true, cancellationToken);
	}

	protected void CheckDisposed(NextProcessRunnerFixture processRunner, bool expectedGracefulStop, CancellationToken cancellationToken)
	{
		Assert.That(processRunner.IsDisposed, Is.True, "Runner should be disposed");
		Assert.That(processRunner.HasExited, Is.EqualTo(expectedGracefulStop), $"Runner should be {(expectedGracefulStop ? "gracefully closed" : "disposed")}");

		commandSenderMock!.Verify(m => m.CloseRunnerAsync(processRunner, It.Is(IsNotSameToken(cancellationToken))), expectedGracefulStop ? Times.Once : Times.Never);
	}

	protected static Expression<Func<CancellationToken, bool>> IsNotSameToken(CancellationToken cancellationToken)
	{
		return c => c.CanBeCanceled && !c.IsCancellationRequested && c != cancellationToken;
	}

	protected class NextProcessRunnerFixture : INextProcessRunner
	{
		int? processId;

		public NextProcessRunnerFixture(string runnerCode)
		{
			RunnerCode = runnerCode;
		}

		public int ProcessId => processId ?? 0;
		public bool IsDisposed { get; private set; }
		public bool HasExited { get; set; }
		public string RunnerCode { get; }
		public void Dispose() { IsDisposed = true; }
		public Task<int> StartAsync(CancellationToken cancellationToken)
		{
			processId ??= TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
			return Task.FromResult(processId.Value);
		}
	}
}

[TestFixtureSource(typeof(TestData), nameof(TestData.RunAsyncCases))]
class NextProcessRunnerPoolTest<TRequest, TResponse> : NextProcessRunnerPoolTestBase
{
	readonly TokenHandlerDelegate<TRequest, TResponse> handler;
	readonly TRequest request;
	readonly TResponse response;

	public NextProcessRunnerPoolTest(Func<ITokenHubClient, Func<TRequest, CancellationToken, Task<TResponse>>> handler, TRequest request, TResponse response)
	{
		this.handler = new TokenHandlerDelegate<TRequest, TResponse>(handler);
		this.request = request;
		this.response = response;
	}

	[Test]
	public async Task RunAsync_CreateRunnerAndCallRunRequestAsync_ReturnValue()
	{
		var capturedNextRunnerOptionsCollection = new List<INextRunnerOptions>();
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner = new NextProcessRunnerFixture("token");
		NextProcessRunnerFactoryMock
			.Setup(m => m.Create(It.IsAny<string>(), Capture.In(capturedNextRunnerOptionsCollection)))
			.Returns(nextProcessRunner);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<TRequest, TResponse>>(), It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(response);
		CommandSenderMock
			.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback(() => nextProcessRunner.HasExited = true)
			.Returns(Task.CompletedTask);

		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var result = await processRunnerPool.RunAsync("token", handler, request, cancellationToken);

		var capturedNextRunnerOptions = capturedNextRunnerOptionsCollection.LastOrDefault();
		Assert.Multiple(() =>
		{
			Assert.That(result, Is.EqualTo(response));
			Assert.That(capturedNextRunnerOptionsCollection, Is.EquivalentTo(new[] { capturedNextRunnerOptions }));
			Assert.That(capturedNextRunnerOptions?.DatabaseName, Is.EqualTo(NextLauncherOptions.DatabaseName));
			Assert.That(capturedNextRunnerOptions?.ServerName, Is.EqualTo(NextLauncherOptions.ServerName));
			Assert.That(capturedNextRunnerOptions?.LauncherHub, Is.EqualTo(NextLauncherOptions.ExpectedHubUri));
			CheckNotYetStopped(nextProcessRunner);
			NextProcessRunnerFactoryMock.Verify(m => m.Create("token", It.IsAny<INextRunnerOptions>()), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner, handler, request, cancellationToken), Times.Once);
		});
		WaitAndCheckGracefulStop(nextProcessRunner, cancellationToken);
	}

	[Test]
	public async Task RunAsync_CreateRunnerAndCallRunRequestAsync_PropagateProcessException()
	{
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner = new NextProcessRunnerFixture("token");
		NextProcessRunnerFactoryMock
			.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<TRequest, TResponse>>(), It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new NotImplementedException("Dummy error for unit test"));
		CommandSenderMock
			.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback(() => nextProcessRunner.HasExited = true)
			.Returns(Task.CompletedTask);

		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var e = Assert.ThrowsAsync<NotImplementedException>(() => processRunnerPool.RunAsync("token", handler, request, cancellationToken));

		Assert.Multiple(() =>
		{
			Assert.That(e?.Message, Is.EqualTo("Dummy error for unit test"));
			CheckNotYetStopped(nextProcessRunner);
			NextProcessRunnerFactoryMock.Verify(m => m.Create("token", It.IsAny<INextRunnerOptions>()), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner, handler, request, cancellationToken), Times.Once);
		});

		WaitAndCheckGracefulStop(nextProcessRunner, cancellationToken);
	}

	[Test]
	public async Task RunAsync_CreateRunnerAndCallRunRequestAsync_PropagateCreateException()
	{
		var cancellationToken = CancellationTokenSource.Token;
		var notImplementedException = new NotImplementedException("Dummy error for unit test");

		NextProcessRunnerFactoryMock
			.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<INextRunnerOptions>()))
			.Throws(notImplementedException);
		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var e = Assert.ThrowsAsync<NotImplementedException>(() => processRunnerPool.RunAsync("token", handler, request, cancellationToken));
		Assert.That(e, Is.EqualTo(notImplementedException));

		NextProcessRunnerFactoryMock.Verify(m => m.Create("token", It.IsAny<INextRunnerOptions>()), Times.Once);
	}
}

class NextProcessRunnerPoolTest : NextProcessRunnerPoolTestBase
{
	[Test]
	public async Task Constructor_DoesNotCallFactory()
	{
		await using var nextProcessRunnerPool = CreateNextProcessRunnerPool();
		Assert.That(nextProcessRunnerPool, Is.Not.Null);
	}

	[Test]
	public async Task RunAsync_ReuseSameRunner_WhenSameCodeAndCommandComplete()
	{
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner = new NextProcessRunnerFixture("token");
		NextProcessRunnerFactoryMock
			.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse>>(), It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((INextProcessRunner runner, TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> _, SignCwTokenRequest request, CancellationToken _) => new SignCwTokenResponse($"[{runner.RunnerCode}] mocked response for {request.Audience}"));
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<PrepareNewCertificateRequest, PrepareNewCertificateResponse>>(), It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((INextProcessRunner runner, TokenHandlerDelegate<PrepareNewCertificateRequest, PrepareNewCertificateResponse> _, PrepareNewCertificateRequest request, CancellationToken _) => new PrepareNewCertificateResponse($"[{runner.RunnerCode}] mocked response"));
		CommandSenderMock
			.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback(() => nextProcessRunner.HasExited = true)
			.Returns(Task.CompletedTask);

		TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> signCwTokenHandler = client => client.SignCwToken;
		TokenHandlerDelegate<PrepareNewCertificateRequest, PrepareNewCertificateResponse> prepareTokenHandler = client => client.PrepareNewCertificate;
		var signCwTokenRequest = new SignCwTokenRequest("aud");
		var prepareNewCertificateRequest = new PrepareNewCertificateRequest();

		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var result1 = await processRunnerPool.RunAsync("token", signCwTokenHandler, signCwTokenRequest, cancellationToken);
		var result2 = await processRunnerPool.RunAsync("token", prepareTokenHandler, prepareNewCertificateRequest, cancellationToken);

		Assert.Multiple(() =>
		{
			Assert.That(result1.Token, Is.EqualTo("[token] mocked response for aud"));
			Assert.That(result2.Csr, Is.EqualTo("[token] mocked response"));
			CheckNotYetStopped(nextProcessRunner);
			NextProcessRunnerFactoryMock.Verify(m => m.Create("token", It.IsAny<INextRunnerOptions>()), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner, signCwTokenHandler, signCwTokenRequest, cancellationToken), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner, prepareTokenHandler, prepareNewCertificateRequest, cancellationToken), Times.Once);
		});

		WaitAndCheckGracefulStop(nextProcessRunner, cancellationToken);
	}

	[Test]
	public async Task RunAsync_ReuseSameRunner_WhenSameCodeAndCommandThrows()
	{
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner = new NextProcessRunnerFixture("token");
		NextProcessRunnerFactoryMock
			.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse>>(), It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((INextProcessRunner runner, TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> _, SignCwTokenRequest request, CancellationToken _) => throw new NotImplementedException($"[{runner.RunnerCode}] Dummy error for {request.Audience}"));
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<PrepareNewCertificateRequest, PrepareNewCertificateResponse>>(), It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((INextProcessRunner runner, TokenHandlerDelegate<PrepareNewCertificateRequest, PrepareNewCertificateResponse> _, PrepareNewCertificateRequest request, CancellationToken _) => throw new NotImplementedException($"[{runner.RunnerCode}] Dummy error"));
		CommandSenderMock
			.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback(() => nextProcessRunner.HasExited = true)
			.Returns(Task.CompletedTask);

		TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> signCwTokenHandler = client => client.SignCwToken;
		TokenHandlerDelegate<PrepareNewCertificateRequest, PrepareNewCertificateResponse> prepareTokenHandler = client => client.PrepareNewCertificate;
		var signCwTokenRequest = new SignCwTokenRequest("aud");
		var prepareNewCertificateRequest = new PrepareNewCertificateRequest();

		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var e1 = Assert.ThrowsAsync<NotImplementedException>(() => processRunnerPool.RunAsync("token", signCwTokenHandler, signCwTokenRequest, cancellationToken));
		var e2 = Assert.ThrowsAsync<NotImplementedException>(() => processRunnerPool.RunAsync("token", prepareTokenHandler, prepareNewCertificateRequest, cancellationToken));

		Assert.Multiple(() =>
		{
			Assert.That(e1?.Message, Is.EqualTo("[token] Dummy error for aud"));
			Assert.That(e2?.Message, Is.EqualTo("[token] Dummy error"));
			CheckNotYetStopped(nextProcessRunner);
			NextProcessRunnerFactoryMock.Verify(m => m.Create("token", It.IsAny<INextRunnerOptions>()), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner, signCwTokenHandler, signCwTokenRequest, cancellationToken), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner, prepareTokenHandler, prepareNewCertificateRequest, cancellationToken), Times.Once);
		});

		WaitAndCheckGracefulStop(nextProcessRunner, cancellationToken);
	}

	[Test]
	public async Task RunAsync_UseDifferentRunner_WhenDifferentCode()
	{
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner1 = new NextProcessRunnerFixture("token1");
		using var nextProcessRunner2 = new NextProcessRunnerFixture("token2");
		NextProcessRunnerFactoryMock
			.Setup(m => m.Create(nextProcessRunner1.RunnerCode, It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner1);
		NextProcessRunnerFactoryMock
			.Setup(m => m.Create(nextProcessRunner2.RunnerCode, It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner2);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse>>(), It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((INextProcessRunner runner, TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> _, SignCwTokenRequest request, CancellationToken _) => new SignCwTokenResponse($"[{runner.RunnerCode}] mocked response for {request.Audience}"));
		CommandSenderMock
			.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback((INextProcessRunner nextProcessRunner, CancellationToken _) => ((NextProcessRunnerFixture)nextProcessRunner).HasExited = true)
			.Returns(Task.CompletedTask);

		TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> signCwTokenHandler = client => client.SignCwToken;
		var signCwTokenRequest1 = new SignCwTokenRequest("audience1");
		var signCwTokenRequest2 = new SignCwTokenRequest("audience2");

		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var result1 = await processRunnerPool.RunAsync("token1", signCwTokenHandler, signCwTokenRequest1, cancellationToken);
		var result2 = await processRunnerPool.RunAsync("token2", signCwTokenHandler, signCwTokenRequest2, cancellationToken);

		Assert.Multiple(() =>
		{
			Assert.That(result1.Token, Is.EqualTo("[token1] mocked response for audience1"));
			Assert.That(result2.Token, Is.EqualTo("[token2] mocked response for audience2"));
			CheckNotYetStopped(nextProcessRunner1);
			CheckNotYetStopped(nextProcessRunner2);
			NextProcessRunnerFactoryMock.Verify(m => m.Create("token1", It.IsAny<INextRunnerOptions>()), Times.Once);
			NextProcessRunnerFactoryMock.Verify(m => m.Create("token2", It.IsAny<INextRunnerOptions>()), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner1, signCwTokenHandler, signCwTokenRequest1, cancellationToken), Times.Once);
			CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner2, signCwTokenHandler, signCwTokenRequest2, cancellationToken), Times.Once);
		});

		WaitAndCheckGracefulStop(nextProcessRunner1, cancellationToken);
		WaitAndCheckGracefulStop(nextProcessRunner2, cancellationToken);
	}

	[Test]
	public async Task RunAsync_UseDifferentRunner_WhenSameCodeAndFirstCommandNotComplete()
	{
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner1 = new NextProcessRunnerFixture("token");
		using var nextProcessRunner2 = new NextProcessRunnerFixture("token");
		HashSet<INextProcessRunner> calledRunners = new();
		NextProcessRunnerFactoryMock!
			.SetupSequence(m => m.Create(It.IsAny<string>(), It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner1)
			.Returns(nextProcessRunner2);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse>>(), It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.Returns( async (INextProcessRunner runner, TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> _, SignCwTokenRequest request, CancellationToken token) =>
			{
				calledRunners.Add(runner);
				if (runner == nextProcessRunner1)
				{
					await Task.Delay(NextLauncherOptions.RunnerIdleLifetime.Add(TimeSpan.FromMilliseconds(500)), token);
				}
				return new SignCwTokenResponse($"[{runner.RunnerCode}] mocked response for {request.Audience}");
			});
		CommandSenderMock.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback((INextProcessRunner nextProcessRunner, CancellationToken _) => ((NextProcessRunnerFixture)nextProcessRunner).HasExited = true)
			.Returns(Task.CompletedTask);

		TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> signCwTokenHandler = client => client.SignCwToken;
		var signCwTokenRequest1 = new SignCwTokenRequest("audience1");
		var signCwTokenRequest2 = new SignCwTokenRequest("audience2");

		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var task1 = processRunnerPool.RunAsync("token", signCwTokenHandler, signCwTokenRequest1, cancellationToken);
		Assert.That(SpinWait.SpinUntil(() => calledRunners.Contains(nextProcessRunner1), TimeSpan.FromSeconds(5)), Is.True, "Runner1 should be called");
		Assert.That(task1.IsCompleted, Is.False, $"Task1 should not be completed before second call is sent, but is {task1.Status}");
		var response2 = await processRunnerPool.RunAsync("token", signCwTokenHandler, signCwTokenRequest2, cancellationToken);
		Assert.That(task1.IsCompleted, Is.False, $"Task1 should not be completed after second call is done, but is {task1.Status}");
		CheckNotYetStopped(nextProcessRunner1);
		CheckNotYetStopped(nextProcessRunner2);
		NextProcessRunnerFactoryMock.Verify(m => m.Create("token", It.IsAny<INextRunnerOptions>()), Times.Exactly(2));
		CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner1, signCwTokenHandler, signCwTokenRequest1, cancellationToken), Times.Once);
		CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner2, signCwTokenHandler, signCwTokenRequest2, cancellationToken), Times.Once);

		var response1 = await task1;
		Assert.Multiple(() =>
		{
			Assert.That(response1.Token, Is.EqualTo("[token] mocked response for audience1"));
			Assert.That(response2.Token, Is.EqualTo("[token] mocked response for audience2"));
			CheckNotYetStopped(nextProcessRunner1);
		});

		WaitAndCheckGracefulStop(nextProcessRunner2, cancellationToken);
		WaitAndCheckGracefulStop(nextProcessRunner1, cancellationToken);
	}

	[Test]
	public async Task RunAsync_UseDifferentRunner_WhenSameCodeAndFirstRunnerStops()
	{
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner1 = new NextProcessRunnerFixture("token");
		using var nextProcessRunner2 = new NextProcessRunnerFixture("token");
		HashSet<INextProcessRunner> calledRunners = new();
		NextProcessRunnerFactoryMock
			.SetupSequence(m => m.Create(It.IsAny<string>(), It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner1)
			.Returns(nextProcessRunner2);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse>>(), It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((INextProcessRunner runner, TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> _, SignCwTokenRequest request, CancellationToken _) => new SignCwTokenResponse($"[{runner.RunnerCode}] mocked response for {request.Audience}"));
		CommandSenderMock.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback((INextProcessRunner nextProcessRunner, CancellationToken _) => ((NextProcessRunnerFixture)nextProcessRunner).HasExited = true)
			.Returns(Task.CompletedTask);

		TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> signCwTokenHandler = client => client.SignCwToken;
		var signCwTokenRequest1 = new SignCwTokenRequest("audience1");
		var signCwTokenRequest2 = new SignCwTokenRequest("audience2");

		await using var processRunnerPool = CreateNextProcessRunnerPool();
		var response1 = await processRunnerPool.RunAsync("token", signCwTokenHandler, signCwTokenRequest1, cancellationToken);
		CheckNotYetStopped(nextProcessRunner1);

		// Force the first runner to stop, simulating that the process has been killed
		nextProcessRunner1.HasExited = true;

		var response2 = await processRunnerPool.RunAsync("token", signCwTokenHandler, signCwTokenRequest2, cancellationToken);
		CheckNotYetStopped(nextProcessRunner2);
		NextProcessRunnerFactoryMock.Verify(m => m.Create("token", It.IsAny<INextRunnerOptions>()), Times.Exactly(2));
		CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner1, signCwTokenHandler, signCwTokenRequest1, cancellationToken), Times.Once);
		CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner2, signCwTokenHandler, signCwTokenRequest2, cancellationToken), Times.Once);

		Assert.Multiple(() =>
		{
			Assert.That(response1.Token, Is.EqualTo("[token] mocked response for audience1"));
			Assert.That(response2.Token, Is.EqualTo("[token] mocked response for audience2"));
			CheckNotYetStopped(nextProcessRunner2);
		});

		WaitAndCheckGracefulStop(nextProcessRunner2, cancellationToken);
	}

	[Test]
	public async Task Dispose_DisposeAllRunners()
	{
		var cancellationToken = CancellationTokenSource.Token;
		using var nextProcessRunner1 = new NextProcessRunnerFixture("token1");
		using var nextProcessRunner2 = new NextProcessRunnerFixture("token2");
		NextProcessRunnerFactoryMock
			.SetupSequence(m => m.Create(It.IsAny<string>(), It.IsAny<INextRunnerOptions>()))
			.Returns(nextProcessRunner1)
			.Returns(nextProcessRunner2);
		CommandSenderMock
			.Setup(m => m.SendRequestAsync(It.IsAny<INextProcessRunner>(), It.IsAny<TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse>>(), It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((INextProcessRunner runner, TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> _, SignCwTokenRequest request, CancellationToken _) => new SignCwTokenResponse($"[{runner.RunnerCode}] mocked response for {request.Audience}"));
		CommandSenderMock
			.Setup(m => m.CloseRunnerAsync(It.IsAny<INextProcessRunner>(), It.IsAny<CancellationToken>()))
			.Callback((INextProcessRunner nextProcessRunner, CancellationToken _) => ((NextProcessRunnerFixture)nextProcessRunner).HasExited = true)
			.Returns(Task.CompletedTask);

		TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> signCwTokenHandler = client => client.SignCwToken;
		var signCwTokenRequest1 = new SignCwTokenRequest("audience1");
		var signCwTokenRequest2 = new SignCwTokenRequest("audience2");

		await using (var processRunnerPool = CreateNextProcessRunnerPool())
		{
			var response1 = await processRunnerPool.RunAsync("token1", signCwTokenHandler, signCwTokenRequest1, cancellationToken);
			var response2 = await processRunnerPool.RunAsync("token2", signCwTokenHandler, signCwTokenRequest2, cancellationToken);

			Assert.Multiple(() =>
			{
				Assert.That(response1.Token, Is.EqualTo("[token1] mocked response for audience1"));
				Assert.That(response2.Token, Is.EqualTo("[token2] mocked response for audience2"));
				CheckNotYetStopped(nextProcessRunner1);
				CheckNotYetStopped(nextProcessRunner2);
				NextProcessRunnerFactoryMock.Verify(m => m.Create("token1", It.IsAny<INextRunnerOptions>()),
					Times.Once);
				NextProcessRunnerFactoryMock.Verify(m => m.Create("token2", It.IsAny<INextRunnerOptions>()),
					Times.Once);
				CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner1, signCwTokenHandler, signCwTokenRequest1, cancellationToken), Times.Once);
				CommandSenderMock.Verify(m => m.SendRequestAsync(nextProcessRunner2, signCwTokenHandler, signCwTokenRequest2, cancellationToken), Times.Once);
			});
		}

		CheckDisposed(nextProcessRunner1, expectedGracefulStop: false, cancellationToken);
		CheckDisposed(nextProcessRunner2, expectedGracefulStop: false, cancellationToken);
	}
}
