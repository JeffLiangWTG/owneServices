using CargoWise.Setup.Services;
using Moq;

namespace CargoWise.Setup.Test.Helpers;

internal class TestProcessHelper
{
	public static IProcess MockProcess(bool present, int exitCode = -1)
	{
		var procMock = new Mock<IProcess>();
		procMock.SetupGet(x => x.Present).Returns(present);
		procMock.SetupGet(x => x.ExitCode).Returns(exitCode);
		return procMock.Object;
	}
}
