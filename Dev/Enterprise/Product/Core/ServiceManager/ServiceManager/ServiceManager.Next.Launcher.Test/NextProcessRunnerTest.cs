using System.Diagnostics;
using CargoWise.ServiceManager.Next.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

class NextProcessRunnerTest
{
	bool hasExited;
	bool processStart = true;
	ICollection<ProcessStartInfo>? capturedProcessStartInfoCollection;
	Mock<IProcess>? processMock;
	Mock<IProcessFactory>? mockProcessFactory;
	TestNextRunnerOptions? testNextRunnerOptions;

	[SetUp]
	public void Setup()
	{
		capturedProcessStartInfoCollection = new List<ProcessStartInfo>();
		processMock = new Mock<IProcess>(MockBehavior.Strict);
		processMock.Setup(m => m.Start()).Returns(() => processStart);
		processMock.Setup(m => m.Id).Returns(1234);
		processMock.Setup(m => m.StartInfo).Returns(capturedProcessStartInfoCollection.First);
		processMock.Setup(m => m.HasExited).Returns(() => hasExited);
		processMock.Setup(m => m.Dispose());
		processMock.Setup(m => m.BeginErrorReadLine());
		processMock.Setup(m => m.BeginOutputReadLine());
		processMock.Setup(m => m.Kill());
		processMock.SetupAdd(m => m.ErrorDataReceived += It.IsAny<DataReceivedEventHandler>());
		processMock.SetupAdd(m => m.OutputDataReceived += It.IsAny<DataReceivedEventHandler>());
		mockProcessFactory = new Mock<IProcessFactory>(MockBehavior.Strict);
		mockProcessFactory
			.Setup(m => m.Create(Capture.In(capturedProcessStartInfoCollection), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
			.Returns(processMock.Object);
		testNextRunnerOptions = new TestNextRunnerOptions();
	}

	[TearDown]
	public void TearDown()
	{
		mockProcessFactory?.VerifyNoOtherCalls();
		processMock?.VerifyNoOtherCalls();
	}

	[Test]
	public void Constructor_Does_Not_Start_Process()
	{
		var nextProcessRunner = CreateNextProcessRunner("token");
		mockProcessFactory?.VerifyNoOtherCalls();

		Assert.That(nextProcessRunner.ProcessId, Is.EqualTo(0));
		Assert.That(nextProcessRunner.HasExited, Is.False);
		mockProcessFactory?.VerifyNoOtherCalls();

		nextProcessRunner.Dispose();
		mockProcessFactory?.VerifyNoOtherCalls();
	}

	[Test]
	public void StartAsync_WhenInvalidRunnerCode_Throws()
	{
		using var nextProcessRunner = CreateNextProcessRunner("invalid");
		var e = Assert.ThrowsAsync<NotImplementedException>(() => nextProcessRunner.StartAsync(CancellationToken.None));
		Assert.That(e?.Message, Is.EqualTo("Invalid runner code: invalid."));
	}

	[Test]
	public async Task StartAsync_WhenProcessNotTerminated_Starts()
	{
		var nextProcessRunner = CreateNextProcessRunner("token");
		var result = await nextProcessRunner.StartAsync(CancellationToken.None);

		Assert.Multiple(() =>
		{
			mockProcessFactory!.Verify(
				m => m.Create(
					It.IsAny<ProcessStartInfo>(),
					true,
					ProcessPriorityClass.Normal),
				Times.Once);
			Assert.That(result, Is.EqualTo(1234));
			Assert.That(nextProcessRunner.ProcessId, Is.EqualTo(1234));
			Assert.That(nextProcessRunner.HasExited, Is.False);
			Assert.That(capturedProcessStartInfoCollection, Is.Not.Empty);
			var capturedProcessStartInfo = capturedProcessStartInfoCollection!.First();
			Assert.That(capturedProcessStartInfo.FileName, Is.EqualTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CargoWise.ServiceManager.Next.Runner.exe")));
			Assert.That(capturedProcessStartInfo.WorkingDirectory, Is.EqualTo(AppDomain.CurrentDomain.BaseDirectory));
			Assert.That(capturedProcessStartInfo.RedirectStandardOutput, Is.True);
			Assert.That(capturedProcessStartInfo.RedirectStandardError, Is.True);
			Assert.That(capturedProcessStartInfo.UseShellExecute, Is.False);
			Assert.That(capturedProcessStartInfo.Arguments, Is.EqualTo(testNextRunnerOptions!.ExpectedArguments));
			Assert.That(capturedProcessStartInfoCollection, Is.EquivalentTo(new[] { capturedProcessStartInfo }));
			processMock!.Verify(m => m.Start(), Times.Once);
			processMock.Verify(m => m.Id, Times.AtLeastOnce);
			processMock.Verify(m => m.StartInfo, Times.AtLeastOnce);
			processMock.Verify(m => m.HasExited, Times.AtLeastOnce);
			processMock.Verify(m => m.BeginErrorReadLine(), Times.Once);
			processMock.Verify(m => m.BeginOutputReadLine(), Times.Once);
			processMock.VerifyAdd(m => m.ErrorDataReceived += It.IsAny<DataReceivedEventHandler>(), Times.Once);
			processMock.VerifyAdd(m => m.OutputDataReceived += It.IsAny<DataReceivedEventHandler>(), Times.Once);

			processMock.VerifyNoOtherCalls();
			mockProcessFactory.VerifyNoOtherCalls();
		});

		nextProcessRunner.Dispose();
		processMock!.Verify(m => m.Kill(), Times.Once);
		processMock.VerifyRemove(m => m.ErrorDataReceived -= It.IsAny<DataReceivedEventHandler>(), Times.Once);
		processMock.VerifyRemove(m => m.OutputDataReceived -= It.IsAny<DataReceivedEventHandler>(), Times.Once);
		processMock.Verify(m => m.Id, Times.AtLeastOnce);
		processMock.Verify(m => m.StartInfo, Times.AtLeastOnce);
		processMock.Verify(m => m.HasExited, Times.AtLeastOnce);
		processMock.Verify(m => m.Dispose(), Times.Once);
		processMock.VerifyNoOtherCalls();
		mockProcessFactory!.VerifyNoOtherCalls();
	}

	[Test]
	public void StartAsync_WhenStartFails_Throws()
	{
		processStart = false;
		var nextProcessRunner = CreateNextProcessRunner("token");
		var e = Assert.ThrowsAsync<InvalidOperationException>(() => nextProcessRunner.StartAsync(CancellationToken.None));
		Assert.Multiple(() =>
		{
			Assert.That(e?.Message, Does.StartWith("token PID=0: Already running"));
			mockProcessFactory!.Verify(
				m => m.Create(It.IsAny<ProcessStartInfo>(), true, ProcessPriorityClass.Normal),
				Times.Once);
			processMock!.Verify(m => m.Start(), Times.Once);
			processMock.Verify(m => m.StartInfo, Times.AtLeastOnce);
			processMock.VerifyAdd(m => m.ErrorDataReceived += It.IsAny<DataReceivedEventHandler>(), Times.Once);
			processMock.VerifyAdd(m => m.OutputDataReceived += It.IsAny<DataReceivedEventHandler>(), Times.Once);
			processMock.VerifyNoOtherCalls();
			mockProcessFactory.VerifyNoOtherCalls();
		});
		nextProcessRunner.Dispose();
		processMock!.VerifyNoOtherCalls();
		mockProcessFactory!.VerifyNoOtherCalls();
	}

	[Test]
	public async Task StartAsync_WhenProcessTerminated_Throws()
	{
		hasExited = false;
		var nextProcessRunner = CreateNextProcessRunner("token");
		// starts the process once
		var pid = await nextProcessRunner.StartAsync(CancellationToken.None);
		mockProcessFactory!.Verify(
			m => m.Create(It.IsAny<ProcessStartInfo>(), true, ProcessPriorityClass.Normal),
			Times.Once);
		processMock!.Invocations.Clear();

		hasExited = true;
		var e = Assert.ThrowsAsync<InvalidOperationException>(() => nextProcessRunner.StartAsync(CancellationToken.None));
		Assert.Multiple(() =>
		{
			Assert.That(e?.Message, Is.EqualTo($"token PID={pid}: Process has exited."));
			mockProcessFactory!.Verify(
				m => m.Create(It.IsAny<ProcessStartInfo>(), true, ProcessPriorityClass.Normal),
				Times.Once);
			processMock.Verify(m => m.Id, Times.AtLeastOnce);
			processMock.Verify(m => m.HasExited, Times.AtLeastOnce);
			processMock.VerifyNoOtherCalls();
			mockProcessFactory.VerifyNoOtherCalls();
		});
		nextProcessRunner.Dispose();
		processMock!.Verify(m => m.Kill(), Times.Never);
		processMock.VerifyRemove(m => m.ErrorDataReceived -= It.IsAny<DataReceivedEventHandler>(), Times.Once);
		processMock.VerifyRemove(m => m.OutputDataReceived -= It.IsAny<DataReceivedEventHandler>(), Times.Once);
		processMock.Verify(m => m.Id, Times.AtLeastOnce);
		processMock.Verify(m => m.HasExited, Times.AtLeastOnce);
		processMock.Verify(m => m.Dispose(), Times.Once);
		processMock.VerifyNoOtherCalls();
		mockProcessFactory!.VerifyNoOtherCalls();
	}

	[Test]
	public async Task StartAsync_WhenAlreadyStartedReturns_ProcessId()
	{
		var expectedPid = 1234;
		var nextProcessRunner = CreateNextProcessRunner("token");

		// starts the process once
		var pid1 = await nextProcessRunner.StartAsync(CancellationToken.None);
		mockProcessFactory!.Verify(
			m => m.Create(It.IsAny<ProcessStartInfo>(), true, ProcessPriorityClass.Normal),
			Times.Once);
		processMock!.Invocations.Clear();
		Assert.That(pid1, Is.EqualTo(expectedPid));

		var pid2 = await nextProcessRunner.StartAsync(CancellationToken.None);
		Assert.That(pid2, Is.EqualTo(expectedPid));

		processMock.Verify(m => m.Id, Times.AtLeastOnce);
		processMock.Verify(m => m.HasExited, Times.AtLeastOnce);
		processMock.VerifyNoOtherCalls();

		nextProcessRunner.Dispose();
		processMock!.Verify(m => m.Kill(), Times.Once);
		processMock.VerifyRemove(m => m.ErrorDataReceived -= It.IsAny<DataReceivedEventHandler>(), Times.Once);
		processMock.VerifyRemove(m => m.OutputDataReceived -= It.IsAny<DataReceivedEventHandler>(), Times.Once);
		processMock.Verify(m => m.Id, Times.AtLeastOnce);
		processMock.Verify(m => m.StartInfo, Times.AtLeastOnce);
		processMock.Verify(m => m.HasExited, Times.AtLeastOnce);
		processMock.Verify(m => m.Dispose(), Times.Once);
		processMock.VerifyNoOtherCalls();
		mockProcessFactory!.VerifyNoOtherCalls();
	}

	NextProcessRunner CreateNextProcessRunner(string runnerCode) => new (new NullLogger<NextProcessRunner>(), mockProcessFactory!.Object, testNextRunnerOptions!, runnerCode);

	class TestNextRunnerOptions : INextRunnerOptions
	{
		readonly Guid id = Guid.NewGuid();
		public string DatabaseName => $"Db_{id}";
		public string ServerName => $"Server_{id}";
		public Uri LauncherHub => new Uri($"http://localhost:1234/signalR/{id}");

		public string ExpectedArguments => $"-LauncherHub:{LauncherHub} {ServerName} {DatabaseName}";
	}
}
