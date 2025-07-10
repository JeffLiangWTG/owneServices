using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class StartupCheckAvailableDiskSpaceTest : AbstractApplicationStartupTaskTest<StartupCheckAvailableDiskSpace>
	{
		public void TestCheckAvailableDiskSpaceWhenDiskSpaceIsOK()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var systemInformationMock = new Mock<ZSystemInformation> { CallBase = true };
			systemInformationMock
				.Protected()
				.Setup<ulong>("AvailableFreeSpaceCore")
				.Returns(1000000);

			using (ZSystemInformation.SetInstanceForTesting(systemInformationMock.Object))
			{
				bool result = new StartupCheckAvailableDiskSpace().Execute(new ApplicationArguments(System.Array.Empty<string>()));
				Assert("Task should return true", result);
				Assert("No error message when disk space is more than required", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestCheckAvailableDiskSpaceWhenDiskSpaceIsTooLow()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var systemInformationMock = new Mock<ZSystemInformation> { CallBase = true };
			systemInformationMock
				.Protected()
				.Setup<ulong>("AvailableFreeSpaceCore")
				.Returns(1);

			using (ZSystemInformation.SetInstanceForTesting(systemInformationMock.Object))
			{
				bool result = new StartupCheckAvailableDiskSpace().Execute(new ApplicationArguments(System.Array.Empty<string>()));
				Assert("Task should return true", result);
				Assert("Display error message when disk space is less than required", UnitTestUserNotification.Instance.LastMessage.Contains("Your available disk space of"));
			}
		}

		[ExpectNoExceptions]
		public void TestCheckAvailableDiskSpaceWhenDiskSpaceIsTooLowAndThenIsOk()
		{
			// Arrange
			var systemInformationMock = new Mock<ZSystemInformation> { CallBase = true };

			var diskFreeSpaceInMb = (ulong)ZSystemInformation.MinimumRequirements.DiskFreeSpaceInMB / 2;
			systemInformationMock
				.Protected()
				.Setup<ulong>("AvailableFreeSpaceCore")
				.Returns(diskFreeSpaceInMb)
				.Callback(() =>
				{
					systemInformationMock
						.Protected()
						.Setup<ulong>("AvailableFreeSpaceCore")
						.Returns(ZSystemInformation.MinimumRequirements.DiskFreeSpaceInMB * 3);
				});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (ZSystemInformation.SetInstanceForTesting(systemInformationMock.Object))
			{
				// Act
				bool result = new StartupCheckAvailableDiskSpace().Execute(new ApplicationArguments(System.Array.Empty<string>()));

				// Assert
				AssertContains(diskFreeSpaceInMb.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override int DefaultErrorExitCode => ExitCodes.StartupCheckAvailableDiskSpaceError;
	}
}
