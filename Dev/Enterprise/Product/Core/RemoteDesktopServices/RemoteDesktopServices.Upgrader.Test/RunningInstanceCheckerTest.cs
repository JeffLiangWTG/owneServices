using System.Threading;
using CargoWise.Loader.Common;
using CargoWise.RemoteDesktopServices.Upgrader.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RemoteDesktopServices.Upgrader.Test
{
	sealed class RunningInstanceCheckerTest : TestCase
	{
		public void TestInstallExcludingDependencies_WhenPreviousInstallationNotFinish_ReturnsError()
		{
			// Arrange
			var services = new Mock<IServiceContainer>();
			services
				.SetupGet(s => s.MessageBox)
				.Returns(Mock.Of<IMessageBoxProxy>());
			var config = new Configuration()
			{
				Services = services.Object,
			};
			var installation = new Installation(config);
			var installationResult = new InstallationResultCollection();
			Mutex mutex = new Mutex(true, "Global\\CargoWiseOneRemoteDesktopServicesUpgrader");
			InstallationResult result = null;

			// Act
			var runningInstanceCheckerForTestFirst = new RunningInstanceCheckerForTest(installation, mutex);
			runningInstanceCheckerForTestFirst.InstallExcludingDependenciesForTest();
			var thread = new Thread(() =>
			{
				var runningInstanceCheckerForTestSecond = new RunningInstanceCheckerForTest(installation, mutex);
				result = runningInstanceCheckerForTestSecond.InstallExcludingDependenciesForTest();
			});
			thread.Start();
			thread.Join();
			mutex.ReleaseMutex();

			// Assert
			AssertEquals(InstallationResultStatus.Error, result.Status);
		}
	}

	public class RunningInstanceCheckerForTest : RunningInstanceChecker
	{
		public RunningInstanceCheckerForTest(Installation installation, Mutex mutex)
			: base(installation, mutex, "CargoWise.RemoteDesktopServices.Upgrader")
		{ }

		protected override bool NeedsToInstallCore()
		{
			return base.NeedsToInstallCore();
		}

		public InstallationResult InstallExcludingDependenciesForTest()
		{
			return base.InstallExcludingDependencies();
		}
	}
}
