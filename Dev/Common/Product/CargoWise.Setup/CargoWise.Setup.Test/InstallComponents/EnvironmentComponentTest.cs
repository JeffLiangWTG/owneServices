using CargoWise.Setup.InstallComponents;
using CargoWise.Setup.Services;
using CargoWise.Setup.Test.Helpers;
using Enterprise.Upgrades;
using Enterprise.Upgrades.Installers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.Setup.Test.InstallComponents;

class EnvironmentComponentTest : InstallComponentTestBase
{
	[SetUp]
	protected void SetUp()
	{
		eventSourceCreatorMock = new Mock<IEventSourceCreator>();
		browserCompatibilityConfiguratorMock = new Mock<IBrowserCompatibilityConfigurator>();
		urlRegistrationMock = new Mock<IEdiUrlRegistration>();
		clientMsiInstallerMock = new Mock<ICargoWiseStartInstaller>();
		fontInstallerMock = new Mock<IFontInstaller>();
		pathProviderMock = new Mock<IInstallPathProvider>();
		loggerMock = new Mock<ILogger<EnvironmentComponent>>();

		component = new EnvironmentComponent(
			eventSourceCreatorMock.Object,
			browserCompatibilityConfiguratorMock.Object,
			urlRegistrationMock.Object,
			clientMsiInstallerMock.Object,
			fontInstallerMock.Object,
			pathProviderMock.Object,
			loggerMock.Object);
	}

	Mock<IEventSourceCreator> eventSourceCreatorMock;
	Mock<IBrowserCompatibilityConfigurator> browserCompatibilityConfiguratorMock;
	Mock<IEdiUrlRegistration> urlRegistrationMock;
	Mock<ICargoWiseStartInstaller> clientMsiInstallerMock;
	Mock<IFontInstaller> fontInstallerMock;
	Mock<IInstallPathProvider> pathProviderMock;
	Mock<ILogger<EnvironmentComponent>> loggerMock;
	EnvironmentComponent component;

	protected override IInstallationComponent CreateComponentForIdempotencyTest()
	{
		var registry = new WindowsRegistryProxy();
		var eventSourceCreator = new EventSourceCreator(Mock.Of<ILogger<EventSourceCreator>>());
		var browserCompatibilityConfigurator = new BrowserCompatibilityConfigurator(registry,Mock.Of<ILogger<BrowserCompatibilityConfigurator>>());
		var urlRegistration = new EdiUrlRegistration(registry, Mock.Of<ILogger<EdiUrlRegistration>>());
		var fontInstaller = new FontInstaller();
		var cargoWiseStartInstaller = new CargoWiseStartInstaller(registry, new ProcessRunnerProxy(), new InstallPathProvider(), Mock.Of<ICargoWiseStartExeUpdater>(), Mock.Of<ILogger<CargoWiseStartInstaller>>());
		var pathProvider = new InstallPathProvider();
		return new EnvironmentComponent(eventSourceCreator, browserCompatibilityConfigurator, urlRegistration, cargoWiseStartInstaller, fontInstaller, pathProvider, Mock.Of<ILogger<EnvironmentComponent>>());
	}

	[Test]
	public void TestInstallCallsServices()
	{
		// Arrange
		var fakeInstallPath = "C:\\installPath\\version";
		pathProviderMock.Setup(x => x.GetInstallPath()).Returns(fakeInstallPath);
		var config = new PartialConfigurationModel().WithDefaults();

		// Act
		component.Install(config, CancellationToken.None);

		// Assert
		eventSourceCreatorMock.Verify(x => x.CreateEventSource(), Times.Once);
		browserCompatibilityConfiguratorMock.Verify(x => x.ConfigureCompatibilityMode(), Times.Once);
		urlRegistrationMock.Verify(x => x.RegisterUrlHandlers(), Times.Once);
		clientMsiInstallerMock.Verify(x => x.Install(), Times.Once);
		fontInstallerMock.Verify(x => x.InstallAllFonts(fakeInstallPath), Times.Once);
	}

	[Test]
	public void TestEnvironmentSetupPreventsConcurrency()
	{
		// Arrange
		eventSourceCreatorMock.Setup(x => x.CreateEventSource()).Callback(AssertMutexIsHeld);
		var config = new PartialConfigurationModel().WithDefaults();

		// Act
		component.Install(config, CancellationToken.None);

		// Assert
		loggerMock.VerifyLog(LogLevel.Information, Times.Never());

		void AssertMutexIsHeld()
		{
			var couldAcquire = true;
			var task = Task.Run(() =>
			{
				using var mutex = new Mutex(false, EnvironmentComponent.MutexName);
				couldAcquire = mutex.WaitOne(TimeSpan.FromMilliseconds(10));
				if (couldAcquire)
				{
					mutex.ReleaseMutex();
				}
			});
			task.Wait();
			Assert.That(couldAcquire, Is.False);
		}
	}

	[Test]
	public void TestEnvironmentSetupMutexLogsWhenWaiting()
	{
		// Arrange
		// take global mutex and release it when the component logs anything
		var mutexReleaseEvent = new AutoResetEvent(false);
		var task = AcquireMutexWithNewTask(mutexReleaseEvent);
		loggerMock.SetupLogCallback(() => mutexReleaseEvent.Set());

		var config = new PartialConfigurationModel().WithDefaults();

		// Act
		component.Install(config, CancellationToken.None);

		// Assert
		loggerMock.VerifyLog(LogLevel.Information, "Waiting to acquire environment setup mutex", Times.Once());
		eventSourceCreatorMock.Verify(x => x.CreateEventSource(), Times.Once());
		task.Wait();
	}

	[Test]
	public void TestEnvironmentSetupHandlesMutexTimeout()
	{
		// Arrange
		var mutexReleaseEvent = new AutoResetEvent(false);
		var task = AcquireMutexWithNewTask(mutexReleaseEvent);
		var config = new PartialConfigurationModel().WithDefaults();
		component.mutexTimeout = TimeSpan.FromMilliseconds(10);

		// Act & Assert
		Assert.Throws<TimeoutException>(() => component.Install(config, CancellationToken.None));

		mutexReleaseEvent.Set();
		task.Wait();
	}

	Task AcquireMutexWithNewTask(AutoResetEvent releaseEvent)
	{
		var gotMutex = false;
		var task = Task.Run(() =>
		{
			using var mutex = new Mutex(false, EnvironmentComponent.MutexName);
			try
			{
				gotMutex = mutex.WaitOne(0);
			}
			catch (AbandonedMutexException)
			{
			}
			releaseEvent.WaitOne(10_000);
			if (gotMutex)
			{
				mutex.ReleaseMutex();
			}
		});
		Thread.Sleep(50); // give time to try mutex
		Assert.That(gotMutex, Is.True, "Precondition");
		return task;
	}

	[Test]
	public void TestEnvironmentSetupHandlesAbandonedMutex()
	{
		// Arrange
		// take global mutex and exit thread it when the component logs anything
		var mutexReleaseEvent = new AutoResetEvent(false);
		var mutexHolderThread = AcquireMutexOnNewThread(mutexReleaseEvent, shouldRelease: false);
		loggerMock.SetupLogCallback(() => mutexReleaseEvent.Set());

		var config = new PartialConfigurationModel().WithDefaults();

		// Act
		Assert.DoesNotThrow(() => component.Install(config, CancellationToken.None));

		mutexHolderThread.Join();

		Thread AcquireMutexOnNewThread(AutoResetEvent releaseEvent, bool shouldRelease = true)
		{
			var gotMutex = false;
			var thread = new Thread(() =>
			{
				try
				{
					using var mutex = new Mutex(false, EnvironmentComponent.MutexName);
					try
					{
						gotMutex = mutex.WaitOne(0);
					}
					catch (AbandonedMutexException)
					{
					}

					releaseEvent.WaitOne(10_000);
					if (gotMutex && shouldRelease)
					{
						mutex.ReleaseMutex();
					}
				}
				catch (Exception)
				{
				}
			});
			thread.Start();
			Thread.Sleep(50); // give time to try mutex
			Assert.That(gotMutex, Is.True, "Precondition");
			return thread;
		}
	}
}
