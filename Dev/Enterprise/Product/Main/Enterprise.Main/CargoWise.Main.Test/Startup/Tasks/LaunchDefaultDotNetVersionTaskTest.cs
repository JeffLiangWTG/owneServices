using CargoWise.Main.Startup.DotNetVersionSwitch;
using Enterprise.Startup;
using Moq;
using NUnit.Framework;
using static CargoWise.Main.Test.ApplicationArgumentsTestHelper;

namespace CargoWise.Main.Test.Startup.Tasks
{
	class LaunchDefaultDotNetVersionTaskTest : TestCase
	{
		public void TestExecuteShouldLaunchVersionIfCurrentVersionNotRunning()
		{
			var mockDotNetVersionManager = new Mock<IDotNetVersionManager>();
			mockDotNetVersionManager.Setup(m => m.IsVersionCurrentRunning())
				.Returns(false);

			var mockSwitchManager = new Mock<IDotNetVersionSwitchManager>();
			mockSwitchManager.Setup(m => m.GetDefaultNetVersionItem())
				.Returns(mockDotNetVersionManager.Object);

			var task = new LaunchDefaultDotNetVersionTask(mockSwitchManager.Object);
			using (TemporaryApplicationArguments([]))
			{
				AssertNoExceptionThrown(task.Execute);
			}

			mockSwitchManager.Verify(m => m.GetDefaultNetVersionItem(), Times.Once);

			mockDotNetVersionManager.Verify(m => m.IsVersionCurrentRunning(), Times.Once);
			mockDotNetVersionManager.Verify(m => m.LaunchCurrentVersion(), Times.Once);
		}

		public void TestExecuteShouldNotLaunchVersionIfCurrentVersionIsRunning()
		{
			var mockDotNetVersionManager = new Mock<IDotNetVersionManager>();
			mockDotNetVersionManager.Setup(m => m.IsVersionCurrentRunning())
				.Returns(true);

			var mockSwitchManager = new Mock<IDotNetVersionSwitchManager>();
			mockSwitchManager.Setup(m => m.GetDefaultNetVersionItem())
				.Returns(mockDotNetVersionManager.Object);

			var task = new LaunchDefaultDotNetVersionTask(mockSwitchManager.Object);
			using (TemporaryApplicationArguments([]))
			{
				AssertNoExceptionThrown(task.Execute);
			}

			mockSwitchManager.Verify(m => m.GetDefaultNetVersionItem(), Times.Once);

			mockDotNetVersionManager.Verify(m => m.IsVersionCurrentRunning(), Times.Once);
			mockDotNetVersionManager.Verify(m => m.LaunchCurrentVersion(), Times.Never);
		}

		public void TestShouldExecuteIsFalseWhenSkipDotNetVersionSwitch()
		{
			var task = new LaunchDefaultDotNetVersionTask();
			using (TemporaryApplicationArguments(["-SkipDotNetVersionSwitch"]))
			{
				Assert(!task.ShouldExecute());
			}
		}

		public void TestShouldExecuteIsFalseWhenNotEnforcedNetCoreVersionAndDisableNetVersionSwitch()
		{
			var mockSwitchManager = new Mock<IDotNetVersionSwitchManager>();
			mockSwitchManager.Setup(m => m.EnforcedNetCoreVersionForUser()).Returns(false);
			mockSwitchManager.Setup(m => m.IsNetVersionSwitchEnabled()).Returns(false);

			var task = new LaunchDefaultDotNetVersionTask(mockSwitchManager.Object);
			using (TemporaryApplicationArguments([]))
			{
				Assert(!task.ShouldExecute());
			}
		}

		public void TestShouldExecuteIsTrueWhenEnforcedNetCoreVersion()
		{
			var mockSwitchManager = new Mock<IDotNetVersionSwitchManager>();
			mockSwitchManager.Setup(m => m.EnforcedNetCoreVersionForUser()).Returns(true);
			mockSwitchManager.Setup(m => m.IsNetVersionSwitchEnabled()).Returns(false);

			var task = new LaunchDefaultDotNetVersionTask(mockSwitchManager.Object);
			using (TemporaryApplicationArguments([]))
			{
				Assert(task.ShouldExecute());
			}
		}

		public void TestShouldExecuteIsTrueWhenEnableNetVersionSwitch()
		{
			var mockSwitchManager = new Mock<IDotNetVersionSwitchManager>();
			mockSwitchManager.Setup(m => m.EnforcedNetCoreVersionForUser()).Returns(true);
			mockSwitchManager.Setup(m => m.IsNetVersionSwitchEnabled()).Returns(false);

			var task = new LaunchDefaultDotNetVersionTask(mockSwitchManager.Object);
			using (TemporaryApplicationArguments([]))
			{
				Assert(task.ShouldExecute());
			}
		}
	}
}
