using System.Collections;
using CargoWise.ServiceManager.Next.Shared.Services;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Logging.CW.Test;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

class SignalRCommandSenderTest
{
	CancellationTokenSource? cancellationTokenSource;
	Mock<IHubContext<NextHub, ITokenHubClient>>? hubContextMock;
	Mock<INextProcessRunner>? nextProcessRunnerMock;
	Mock<ILogger<SignalRCommandSender>>? loggerMock;

	[SetUp]
	public void SetUp()
	{
		cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		hubContextMock = new Mock<IHubContext<NextHub, ITokenHubClient>>(MockBehavior.Strict);
		nextProcessRunnerMock = new Mock<INextProcessRunner>(MockBehavior.Strict);
		loggerMock = new Mock<ILogger<SignalRCommandSender>>(MockBehavior.Loose);
	}

	[TearDown]
	public void TearDown()
	{
		hubContextMock?.VerifyNoOtherCalls();
		nextProcessRunnerMock?.VerifyNoOtherCalls();
	}

	[Test]
	public void AddConnection_CallPidTask()
	{
		var signalRCommandSender = CreateSignalRCommandSender();
		var connectionId = Guid.NewGuid().ToString();
		_ = CreateDummyPidRetrieveTask();
		signalRCommandSender.AddConnection(connectionId, cancellationTokenSource!.Token);
		hubContextMock!.Verify(m => m.Clients.Client(connectionId).ProcessId(cancellationTokenSource.Token), Times.Once);
	}

	[Test]
	public void AddConnection_WhenPidTaskFails_HandleException()
	{
		var signalRCommandSender = CreateSignalRCommandSender();
		var connectionId = Guid.NewGuid().ToString();
		var exception = new Exception("Exception for unit tests");
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId).ProcessId(It.IsAny<CancellationToken>()))
			.ThrowsAsync(exception);
		signalRCommandSender.AddConnection(connectionId, cancellationTokenSource!.Token);
		hubContextMock!.Verify(m => m.Clients.Client(connectionId).ProcessId(cancellationTokenSource.Token), Times.Once);
		loggerMock!.VerifyLogException((AggregateException ex) => ex.InnerException == exception, Times.Once);
	}

	[TestCaseSource(typeof(SignalRCommandTestData), nameof(SignalRCommandTestData.TestCases))]
	public void SendRequestAsync_WhenStartFails_Throws(ISignalRCommandTestData testData)
	{
		var invalidOperationException = new InvalidOperationException("Exception for unit tests");
		nextProcessRunnerMock!.Setup(m => m.RunnerCode).Returns("token");
		nextProcessRunnerMock
			.Setup(m => m.StartAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(invalidOperationException);
		var e = Assert.ThrowsAsync<InvalidOperationException>(() => testData.RunRequestAsync(this, CreateSignalRCommandSender()));
		Assert.That(e, Is.EqualTo(invalidOperationException));
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.StartAsync(cancellationTokenSource!.Token), Times.Once);
	}

	[TestCaseSource(typeof(SignalRCommandTestData), nameof(SignalRCommandTestData.TestCases))]
	public void SendRequestAsync_WhenTokenExpires_Throws(ISignalRCommandTestData testData)
	{
		var commandSender = CreateSignalRCommandSender();
		var loopCount = 0;
		var maxLoopBeforeCancel = 10;
		nextProcessRunnerMock!.Setup(m => m.HasExited).Callback(() => ++loopCount).Returns(false);
		nextProcessRunnerMock!.Setup(m => m.RunnerCode).Returns("token");
		nextProcessRunnerMock
			.Setup(m => m.StartAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(123);
		var requestTask = Task.Run(() => testData.RunRequestAsync(this, commandSender));
		while (!cancellationTokenSource!.IsCancellationRequested && (loopCount < maxLoopBeforeCancel))
		{
			Assert.That(requestTask.IsCompleted, Is.False);
		}
		cancellationTokenSource.Cancel();
		var e = Assert.ThrowsAsync<OperationCanceledException>(async () => await requestTask);
		Assert.That(e?.CancellationToken, Is.EqualTo(cancellationTokenSource.Token));

		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeast(maxLoopBeforeCancel));
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.StartAsync(cancellationTokenSource!.Token), Times.Once);
	}

	[TestCaseSource(typeof(SignalRCommandTestData), nameof(SignalRCommandTestData.TestCases))]
	public async Task SendRequestAsync_WhenConnected_SendRequestAndReturnResponse(ISignalRCommandTestData testData)
	{
		var connectionId = Guid.NewGuid().ToString();
		var loopCount = 0;
		var maxLoopBeforeRetrievePid = 10;
		var pid = 12345;
		var pidRetrieveTask = CreateDummyPidRetrieveTask();
		var commandSender = CreateSignalRCommandSender();
		commandSender.AddConnection(connectionId, cancellationTokenSource!.Token);
		nextProcessRunnerMock!.Setup(m => m.HasExited).Callback(() => ++loopCount).Returns(false);
		nextProcessRunnerMock!.Setup(m => m.RunnerCode).Returns("token");
		nextProcessRunnerMock
			.Setup(m => m.StartAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(pid);
		SignalRCommandTestData.SetupHubContextMock(this, connectionId, exception: null);

		var requestTask = Task.Run(() => testData.RunRequestAsync(this, commandSender));
		while (!cancellationTokenSource!.IsCancellationRequested && (loopCount < maxLoopBeforeRetrievePid))
		{
			Assert.That(requestTask.IsCompleted, Is.False);
		}
		pidRetrieveTask.SetResult(pid);
		var response = await requestTask;
		Assert.That(response, Is.EqualTo(testData.GetExpectedResponse(connectionId)));

		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeast(maxLoopBeforeRetrievePid));
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.StartAsync(cancellationTokenSource!.Token), Times.Once);

		hubContextMock!.Verify(m => m.Clients.Client(connectionId).ProcessId(cancellationTokenSource.Token), Times.Once);
		testData.Verify(this, connectionId, Times.Once);
	}

	[TestCaseSource(typeof(SignalRCommandTestData), nameof(SignalRCommandTestData.TestCases))]
	public async Task SendRequestAsync_WhenConnectedToMultipleProcesses_SendRequestToProcessAndReturnResponse(ISignalRCommandTestData testData)
	{
		const string connectionId1 = nameof(connectionId1);
		const string connectionId2 = nameof(connectionId2);
		const string connectionId3 = nameof(connectionId3);
		nextProcessRunnerMock!.Setup(m => m.HasExited).Returns(false);
		nextProcessRunnerMock!.Setup(m => m.RunnerCode).Returns("token");
		nextProcessRunnerMock
			.Setup(m => m.StartAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(222);
		SignalRCommandTestData.SetupHubContextMock(this, connectionId1, exception: null);
		SignalRCommandTestData.SetupHubContextMock(this, connectionId2, exception: null);
		SignalRCommandTestData.SetupHubContextMock(this, connectionId3, exception: null);

		var commandSender = CreateSignalRCommandSender();
		RegisterConnection(commandSender, connectionId1, 111, cancellationTokenSource!.Token);
		RegisterConnection(commandSender, connectionId2, 222, cancellationTokenSource.Token);
		RegisterConnection(commandSender, connectionId3, 333, cancellationTokenSource.Token);

		var token = await testData.RunRequestAsync(this, commandSender);
		Assert.That(token, Is.EqualTo(testData.GetExpectedResponse(connectionId2)));

		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.StartAsync(cancellationTokenSource!.Token), Times.Once);

		hubContextMock!.Verify(
			m => m.Clients.Client(connectionId1).ProcessId(It.IsAny<CancellationToken>()),
			Times.Once);
		hubContextMock!.Verify(
			m => m.Clients.Client(connectionId2).ProcessId(It.IsAny<CancellationToken>()),
			Times.Once);
		hubContextMock!.Verify(
			m => m.Clients.Client(connectionId3).ProcessId(It.IsAny<CancellationToken>()),
			Times.Once);

		testData.Verify(this, connectionId1, Times.Never);
		testData.Verify(this, connectionId2, Times.Once);
		testData.Verify(this, connectionId3, Times.Never);
	}

	[TestCaseSource(typeof(SignalRCommandTestData), nameof(SignalRCommandTestData.TestCases))]
	public void SendRequestAsync_WhenConnected_SendRequestAndPropagateException(ISignalRCommandTestData testData)
	{
		var connectionId = Guid.NewGuid().ToString();
		var pid = 2345;
		var pidRetrieveTask = CreateDummyPidRetrieveTask();
		pidRetrieveTask.SetResult(pid);
		var commandSender = CreateSignalRCommandSender();
		var exception = new Exception("Exception for unit tests");
		commandSender.AddConnection(connectionId, cancellationTokenSource!.Token);
		nextProcessRunnerMock!.Setup(m => m.HasExited).Returns(false);
		nextProcessRunnerMock!.Setup(m => m.RunnerCode).Returns("token");
		nextProcessRunnerMock
			.Setup(m => m.StartAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(pid);
		SignalRCommandTestData.SetupHubContextMock(this, connectionId, exception);
		var e = Assert.ThrowsAsync<Exception>(() => testData.RunRequestAsync(this, commandSender));
		Assert.That(e, Is.EqualTo(exception));

		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.StartAsync(cancellationTokenSource!.Token), Times.Once);

		hubContextMock!.Verify(m => m.Clients.Client(connectionId).ProcessId(cancellationTokenSource.Token), Times.Once);
		testData.Verify(this, connectionId, Times.Once);
	}

	[TestCaseSource(typeof(SignalRCommandTestData), nameof(SignalRCommandTestData.TestCases))]
	public void SendRequestAsync_WhenProcessExitsEarly_Throws(ISignalRCommandTestData testData)
	{
		var pid = 23;
		var runnerCode = "token";
		nextProcessRunnerMock!.Setup(m => m.HasExited).Returns(true);
		nextProcessRunnerMock!.Setup(m => m.ProcessId).Returns(pid);
		nextProcessRunnerMock!.Setup(m => m.RunnerCode).Returns(runnerCode);
		nextProcessRunnerMock
			.Setup(m => m.StartAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(pid);

		var e = Assert.ThrowsAsync<InvalidOperationException>(() => testData.RunRequestAsync(this, CreateSignalRCommandSender()));
		Assert.That(e?.Message, Is.EqualTo($"{runnerCode} PID={pid}: Process has exited."));

		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.ProcessId, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.StartAsync(cancellationTokenSource!.Token), Times.Once);
	}

	[TestCaseSource(typeof(SignalRCommandTestData), nameof(SignalRCommandTestData.TestCases))]
	public void SendRequestAsync_AfterRemove_DoNotSendRequest(ISignalRCommandTestData testData)
	{
		var connectionId = Guid.NewGuid().ToString();
		var pid = 345;
		var pidRetrieveTask = CreateDummyPidRetrieveTask();
		var commandSender = CreateSignalRCommandSender();
		commandSender.AddConnection(connectionId, cancellationTokenSource!.Token);
		var loopCount = 0;
		var maxLoopBeforeRemove = 10;
		var maxLoopBeforeCancel = 2 * maxLoopBeforeRemove;
		nextProcessRunnerMock!.Setup(m => m.HasExited).Callback(() => ++loopCount).Returns(false);
		nextProcessRunnerMock!.Setup(m => m.RunnerCode).Returns("token");
		nextProcessRunnerMock
			.Setup(m => m.StartAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(pid);

		var requestTask = Task.Run(() => testData.RunRequestAsync(this, commandSender));
		while (!cancellationTokenSource!.IsCancellationRequested && (loopCount < maxLoopBeforeRemove))
		{
			Assert.That(requestTask.IsCompleted, Is.False);
		}
		commandSender.RemoveConnection(connectionId);
		pidRetrieveTask.SetResult(pid);
		loopCount = 0;
		while (!cancellationTokenSource!.IsCancellationRequested && (loopCount < maxLoopBeforeCancel))
		{
			Assert.That(requestTask.IsCompleted, Is.False);
		}
		cancellationTokenSource.Cancel();
		var e = Assert.ThrowsAsync<OperationCanceledException>(async () => await requestTask);
		Assert.That(e?.CancellationToken, Is.EqualTo(cancellationTokenSource.Token));

		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeast(maxLoopBeforeCancel));
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.StartAsync(cancellationTokenSource!.Token), Times.Once);

		hubContextMock!.Verify(m => m.Clients.Client(connectionId).ProcessId(cancellationTokenSource.Token), Times.Once);
	}

	[Test]
	public async Task CloseRunnerAsync_NoProcess_Returns()
	{
		var pid = TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
		nextProcessRunnerMock!.Setup(m => m.HasExited).Returns(false);
		nextProcessRunnerMock.Setup(m => m.ProcessId).Returns(pid);
		nextProcessRunnerMock.Setup(m => m.RunnerCode).Returns("token");

		var commandSender = CreateSignalRCommandSender();
		await commandSender.CloseRunnerAsync(nextProcessRunnerMock.Object, cancellationTokenSource!.Token);

		nextProcessRunnerMock.Verify(m => m.ProcessId, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.HasExited, Times.Never);
	}

	[Test]
	public async Task CloseRunnerAsync_ProcessAlreadyExited_Returns()
	{
		var connectionId = Guid.NewGuid().ToString();
		var pid = TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
		nextProcessRunnerMock!.Setup(m => m.HasExited).Returns(true);
		nextProcessRunnerMock.Setup(m => m.ProcessId).Returns(pid);
		nextProcessRunnerMock.Setup(m => m.RunnerCode).Returns("token");

		var commandSender = CreateSignalRCommandSender();
		RegisterConnection(commandSender, connectionId, pid, cancellationTokenSource!.Token);

		await commandSender.CloseRunnerAsync(nextProcessRunnerMock.Object, cancellationTokenSource!.Token);

		nextProcessRunnerMock.Verify(m => m.ProcessId, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.HasExited, Times.Once);
	}

	[Test]
	public async Task CloseRunnerAsync_WhenConnected_SendMessageAndWaitForProcessStop()
	{
		var connectionId = Guid.NewGuid().ToString();
		var pid = TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
		var loopCount = 0;
		var maxLoopBeforeProcessStop = 10;
		nextProcessRunnerMock!.Setup(m => m.HasExited).Callback(() => ++loopCount).Returns(() => loopCount >= maxLoopBeforeProcessStop);
		nextProcessRunnerMock.Setup(m => m.ProcessId).Returns(pid);
		nextProcessRunnerMock.Setup(m => m.RunnerCode).Returns("token");
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId).Close(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		var commandSender = CreateSignalRCommandSender();
		RegisterConnection(commandSender, connectionId, pid, cancellationTokenSource!.Token);
		var closeTask = commandSender.CloseRunnerAsync(nextProcessRunnerMock.Object, cancellationTokenSource!.Token);
		while (!cancellationTokenSource!.IsCancellationRequested && (loopCount < maxLoopBeforeProcessStop))
		{
			Assert.That(closeTask.IsCompleted, Is.False);
		}
		await closeTask;

		nextProcessRunnerMock.Verify(m => m.ProcessId, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeast(maxLoopBeforeProcessStop));
		hubContextMock!.Verify(m => m.Clients.Client(connectionId).Close(cancellationTokenSource.Token), Times.Once);
	}

	[Test]
	public void CloseRunnerAsync_WhenConnected_SendMessageAndWaitForCancellation()
	{
		var connectionId = Guid.NewGuid().ToString();
		var pid = TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
		var loopCount = 0;
		var maxLoopBeforeCancel = 10;
		nextProcessRunnerMock!.Setup(m => m.HasExited).Callback(() => ++loopCount).Returns(false);
		nextProcessRunnerMock.Setup(m => m.ProcessId).Returns(pid);
		nextProcessRunnerMock.Setup(m => m.RunnerCode).Returns("token");
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId).Close(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		var commandSender = CreateSignalRCommandSender();
		RegisterConnection(commandSender, connectionId, pid, cancellationTokenSource!.Token);
		var closeTask = Task.Run(() => commandSender.CloseRunnerAsync(nextProcessRunnerMock.Object, cancellationTokenSource!.Token));
		while (!cancellationTokenSource!.IsCancellationRequested && (loopCount < maxLoopBeforeCancel))
		{
			Assert.That(closeTask.IsCompleted, Is.False);
		}
		cancellationTokenSource.Cancel();
		var e = Assert.ThrowsAsync<OperationCanceledException>(async () => await closeTask);
		Assert.That(e?.CancellationToken, Is.EqualTo(cancellationTokenSource.Token));

		nextProcessRunnerMock.Verify(m => m.ProcessId, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.HasExited, Times.AtLeast(maxLoopBeforeCancel));
		hubContextMock!.Verify(m => m.Clients.Client(connectionId).Close(cancellationTokenSource.Token), Times.Once);
	}

	[Test]
	public void CloseRunnerAsync_WhenConnected_SendMessageAndPropagateException()
	{
		var connectionId = Guid.NewGuid().ToString();
		var pid = TestContext.CurrentContext.Random.NextUShort(1, ushort.MaxValue);
		var exception = new Exception("SignalR failure for unit test");

		nextProcessRunnerMock!.Setup(m => m.HasExited).Returns(false);
		nextProcessRunnerMock.Setup(m => m.ProcessId).Returns(pid);
		nextProcessRunnerMock.Setup(m => m.RunnerCode).Returns("token");
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId).Close(It.IsAny<CancellationToken>()))
			.ThrowsAsync(exception);

		var commandSender = CreateSignalRCommandSender();
		RegisterConnection(commandSender, connectionId, pid, cancellationTokenSource!.Token);
		var e = Assert.ThrowsAsync<Exception>(() => commandSender.CloseRunnerAsync(nextProcessRunnerMock.Object, cancellationTokenSource!.Token));
		Assert.That(e, Is.EqualTo(exception));

		nextProcessRunnerMock.Verify(m => m.ProcessId, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.HasExited, Times.Once);
		hubContextMock!.Verify(m => m.Clients.Client(connectionId).Close(cancellationTokenSource!.Token), Times.Once);
	}

	[Test]
	public async Task CloseRunnerAsync_WhenConnectedToMultipleProcesses_SendRequestToProcess()
	{
		const string connectionId1 = nameof(connectionId1);
		const string connectionId2 = nameof(connectionId2);
		const string connectionId3 = nameof(connectionId3);
		nextProcessRunnerMock!.SetupSequence(m => m.HasExited).Returns(false).Returns(true);
		nextProcessRunnerMock.Setup(m => m.ProcessId).Returns(2222);
		nextProcessRunnerMock.Setup(m => m.RunnerCode).Returns("token");
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId1).Close(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId2).Close(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId3).Close(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		var commandSender = CreateSignalRCommandSender();
		RegisterConnection(commandSender, connectionId1, 1111, cancellationTokenSource!.Token);
		RegisterConnection(commandSender, connectionId2, 2222, cancellationTokenSource.Token);
		RegisterConnection(commandSender, connectionId3, 3333, cancellationTokenSource.Token);

		await commandSender.CloseRunnerAsync(nextProcessRunnerMock.Object, cancellationTokenSource!.Token);
		nextProcessRunnerMock.Verify(m => m.ProcessId, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.RunnerCode, Times.AtLeastOnce);
		nextProcessRunnerMock.Verify(m => m.HasExited, Times.Exactly(2));
		hubContextMock!.Verify(m => m.Clients.Client(connectionId2).Close(cancellationTokenSource!.Token), Times.Once);
	}

	void RegisterConnection(ICommandSender commandSender, string connectionId, int pid, CancellationToken cancellationToken)
	{
		hubContextMock!
			.Setup(m => m.Clients.Client(connectionId).ProcessId(It.IsAny<CancellationToken>()))
			.ReturnsAsync(pid);
		commandSender.AddConnection(connectionId, cancellationToken);
		hubContextMock.Verify(m => m.Clients.Client(connectionId).ProcessId(cancellationToken), Times.Once);
	}

	TaskCompletionSource<int> CreateDummyPidRetrieveTask()
	{
		var retrievePidTask = new TaskCompletionSource<int>();
		hubContextMock!
			.Setup(m => m.Clients.Client(It.IsAny<string>()).ProcessId(It.IsAny<CancellationToken>()))
			.Returns(retrievePidTask.Task);
		return retrievePidTask;
	}

	SignalRCommandSender CreateSignalRCommandSender()
	{
		return new SignalRCommandSender(loggerMock!.Object, hubContextMock!.Object);
	}

	public interface ISignalRCommandTestData
	{
		object GetExpectedResponse(string connectionId);
		Task<object> RunRequestAsync(SignalRCommandSenderTest test, ICommandSender commandSender);
		void SetupHubContextMock(SignalRCommandSenderTest test, string connectionId, Exception? exception);
		void Verify(SignalRCommandSenderTest test, string connectionId, Func<Times> times);
	}

	class SignCwTokenSignalRCommandTestData : ISignalRCommandTestData
	{
		readonly string audience = Guid.NewGuid().ToString();

		public object GetExpectedResponse(string connectionId) => new SignCwTokenResponse(token: $"token for {connectionId} and SignCwTokenRequest {{ Audience = {audience} }}");

		public void SetupHubContextMock(SignalRCommandSenderTest test, string connectionId, Exception? exception)
		{
			test.hubContextMock!
				.Setup(m => m.Clients.Client(connectionId).SignCwToken(It.IsAny<SignCwTokenRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((SignCwTokenRequest req, CancellationToken ct) => exception is null ? new SignCwTokenResponse(token: $"token for {connectionId} and {req}") : throw exception);
		}

		public async Task<object> RunRequestAsync(SignalRCommandSenderTest test, ICommandSender commandSender)
		{
			var signCwTokenRequest = new SignCwTokenRequest(audience);
			return await commandSender.SendRequestAsync<SignCwTokenRequest, SignCwTokenResponse>(test.nextProcessRunnerMock!.Object, client => client.SignCwToken, signCwTokenRequest, test.cancellationTokenSource!.Token);
		}

		public void Verify(SignalRCommandSenderTest test, string connectionId, Func<Times> times)
		{
			test.hubContextMock!.Verify(
				m => m.Clients.Client(connectionId).SignCwToken(
					It.Is<SignCwTokenRequest>(r => r.Audience == audience),
					test.cancellationTokenSource!.Token),
				times);
		}
	}

	class PrepareNewCertificateSignalRCommandTestData : ISignalRCommandTestData
	{
		public object GetExpectedResponse(string connectionId) => new PrepareNewCertificateResponse($"mock response for {connectionId} and PrepareNewCertificateRequest {{ }}");

		public void SetupHubContextMock(SignalRCommandSenderTest test, string connectionId, Exception? exception)
		{
			test.hubContextMock!
				.Setup(m => m.Clients.Client(connectionId).PrepareNewCertificate(It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((PrepareNewCertificateRequest req, CancellationToken ct) => exception is null ? new PrepareNewCertificateResponse(csr: $"mock response for {connectionId} and {req}") : throw exception);
		}

		public async Task<object> RunRequestAsync(SignalRCommandSenderTest test, ICommandSender commandSender)
		{
			var request = new PrepareNewCertificateRequest();
			return await commandSender.SendRequestAsync<PrepareNewCertificateRequest, PrepareNewCertificateResponse>(test.nextProcessRunnerMock!.Object, client => client.PrepareNewCertificate, request, test.cancellationTokenSource!.Token);
		}

		public void Verify(SignalRCommandSenderTest test, string connectionId, Func<Times> times)
		{
			test.hubContextMock!.Verify(
				m => m.Clients.Client(connectionId).PrepareNewCertificate(
					new PrepareNewCertificateRequest(),
					test.cancellationTokenSource!.Token),
				times);
		}
	}

	class ResetAccessTokenSignalRCommandTestData : ISignalRCommandTestData
	{
		public object GetExpectedResponse(string connectionId) => new ResetAccessTokenResponse();

		public void SetupHubContextMock(SignalRCommandSenderTest test, string connectionId, Exception? exception)
		{
			test.hubContextMock!
				.Setup(m => m.Clients.Client(connectionId).ResetAccessToken(It.IsAny<ResetAccessTokenRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((ResetAccessTokenRequest req, CancellationToken ct) => exception is null ? new() : throw exception);
		}

		public async Task<object> RunRequestAsync(SignalRCommandSenderTest test, ICommandSender commandSender)
		{
			var request = new ResetAccessTokenRequest();
			return await commandSender.SendRequestAsync<ResetAccessTokenRequest, ResetAccessTokenResponse>(test.nextProcessRunnerMock!.Object, client => client.ResetAccessToken, request, test.cancellationTokenSource!.Token);
		}

		public void Verify(SignalRCommandSenderTest test, string connectionId, Func<Times> times)
		{
			test.hubContextMock!.Verify(
				m => m.Clients.Client(connectionId).ResetAccessToken(
					new ResetAccessTokenRequest(),
					test.cancellationTokenSource!.Token),
				times);
		}
	}

	class SetNewCertificateCredentialsSignalRCommandTestData : ISignalRCommandTestData
	{
		readonly string operationId = Guid.NewGuid().ToString();
		readonly string tenantId = Guid.NewGuid().ToString();
		readonly string clientId = Guid.NewGuid().ToString();
		readonly byte[] certificate = Guid.NewGuid().ToByteArray();

		public object GetExpectedResponse(string connectionId) => new SetNewCertificateCredentialsResponse();

		public void SetupHubContextMock(SignalRCommandSenderTest test, string connectionId, Exception? exception)
		{
			test.hubContextMock!
				.Setup(m => m.Clients.Client(connectionId).SetNewCertificateCredentials(It.IsAny<SetNewCertificateCredentialsRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((SetNewCertificateCredentialsRequest req, CancellationToken ct) => exception is null ? new() : throw exception);
		}

		public async Task<object> RunRequestAsync(SignalRCommandSenderTest test, ICommandSender commandSender)
		{
			var request = new SetNewCertificateCredentialsRequest(operationId, tenantId, clientId, certificate);
			return await commandSender.SendRequestAsync<SetNewCertificateCredentialsRequest, SetNewCertificateCredentialsResponse>(test.nextProcessRunnerMock!.Object, client => client.SetNewCertificateCredentials, request, test.cancellationTokenSource!.Token);
		}

		public void Verify(SignalRCommandSenderTest test, string connectionId, Func<Times> times)
		{
			var expectedRequest = new SetNewCertificateCredentialsRequest(operationId, tenantId, clientId, certificate);
			test.hubContextMock!.Verify(m => m.Clients.Client(connectionId).SetNewCertificateCredentials(expectedRequest, test.cancellationTokenSource!.Token), times);
		}
	}

	class SetOperationIdSignalRCommandTestData : ISignalRCommandTestData
	{
		readonly string csr = Guid.NewGuid().ToString();
		readonly string operationId = Guid.NewGuid().ToString();

		public object GetExpectedResponse(string connectionId) => new SetOperationIdResponse();

		public void SetupHubContextMock(SignalRCommandSenderTest test, string connectionId, Exception? exception)
		{
			test.hubContextMock!
				.Setup(m => m.Clients.Client(connectionId).SetOperationId(It.IsAny<SetOperationIdRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((SetOperationIdRequest req, CancellationToken ct) => exception is null ? new() : throw exception);
		}

		public async Task<object> RunRequestAsync(SignalRCommandSenderTest test, ICommandSender commandSender)
		{
			var request = new SetOperationIdRequest(csr, operationId);
			return await commandSender.SendRequestAsync<SetOperationIdRequest, SetOperationIdResponse>(test.nextProcessRunnerMock!.Object, client => client.SetOperationId, request, test.cancellationTokenSource!.Token);
		}

		public void Verify(SignalRCommandSenderTest test, string connectionId, Func<Times> times)
		{
			test.hubContextMock!.Verify(
				m => m.Clients.Client(connectionId).SetOperationId(
					new SetOperationIdRequest(csr, operationId),
					test.cancellationTokenSource!.Token),
				times);
		}
	}

	public class SignalRCommandTestData
	{
		static readonly List<ISignalRCommandTestData> TestData =
		[
			new SignCwTokenSignalRCommandTestData(),
			new PrepareNewCertificateSignalRCommandTestData(),
			new ResetAccessTokenSignalRCommandTestData(),
			new SetNewCertificateCredentialsSignalRCommandTestData(),
			new SetOperationIdSignalRCommandTestData(),
		];

		public static IEnumerable TestCases => TestData
			.Select(testCase => new TestCaseData(testCase).SetName($"{{m}}({testCase.GetType().Name})"));

		public static void SetupHubContextMock(SignalRCommandSenderTest test, string connectionId, Exception? exception)
		{
			foreach (var testCase in TestData)
			{
				testCase.SetupHubContextMock(test, connectionId, exception);
			}
		}
	}
}
