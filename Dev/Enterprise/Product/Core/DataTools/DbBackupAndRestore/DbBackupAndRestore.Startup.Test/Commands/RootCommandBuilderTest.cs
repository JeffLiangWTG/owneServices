using System.CommandLine.Parsing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class RootCommandBuilderTest : TestCase
{
	public void TestNewCommandsShouldRunNewCli()
	{
		VerifyCommandLineExecutionType(new[] { "restore" }, false);
		VerifyCommandLineExecutionType(new[] { "backup" }, false);
		VerifyCommandLineExecutionType(new[] { "drop" }, false);
		VerifyCommandLineExecutionType(new[] { "-Database", "Test" }, true);
	}

	void VerifyCommandLineExecutionType(string[] args, bool shouldParseLegacyCli)
	{
		// Arrange
		var builderMock = new Mock<RootCommandBuilder>();

		builderMock.Setup(b => b.Create(It.IsAny<string[]>())).CallBase();
		builderMock.Setup(b => b.ParseLegacyCLI(It.IsAny<string[]>())).Verifiable();

		// Act
		var commandLineBuilder = builderMock.Object.Create(args);
		var parser = commandLineBuilder.Build();
		var result = parser.InvokeAsync(args).Result;

		// Assert (command should exit with exit code 1 for new CLI due to missing parameters)
		AssertEquals(shouldParseLegacyCli ? 0 : 1, result);
		builderMock.Verify(b => b.ParseLegacyCLI(It.IsAny<string[]>()), shouldParseLegacyCli ? Times.Once : Times.Never);
	}
}
