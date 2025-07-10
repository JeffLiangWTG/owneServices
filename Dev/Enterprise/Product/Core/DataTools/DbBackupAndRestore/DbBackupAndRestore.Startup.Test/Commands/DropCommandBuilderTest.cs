using System.CommandLine;
using System.Linq;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class DropCommandBuilderTest : TestCase
{
	public void TestCommandShouldContainRequiredOptions()
	{
		// Arrange
		var builder = new DropCommandBuilder();

		// Act
		var maintenanceConsole = new MaintenanceDbConsole();
		var command = builder.Create(maintenanceConsole);

		// Assert
		AssertNotNull(command);
		AssertNotNull(command.Options);
		Assert(command.Options.Any(o => o.Name == "Database" && o.IsRequired));
		Assert(command.Options.Any(o => o.Name == "Server" && o.IsRequired));
	}

	public void TestCommandShouldContainOptionalOptions()
	{
		// Arrange
		var builder = new DropCommandBuilder();

		// Act
		var maintenanceConsole = new MaintenanceDbConsole();
		var command = builder.Create(maintenanceConsole);

		// Assert
		AssertNotNull(command);
		AssertNotNull(command.Options);
		Assert(command.Options.Any(o => o.Name == "DropOperationalDatabases"));
		Assert(command.Options.Any(o => o.Name == "DropReferenceDatabases"));
		Assert(command.Options.Any(o => o.Name == "DropBIDatabases"));
	}

	public void TestDropCommandShouldRun()
	{
		// Arrange
		Defaults.ResetDefaultInstance();
		var builder = new DropCommandBuilder();
		var maintenanceConsoleMock = new Mock<MaintenanceDbConsole>();

		maintenanceConsoleMock.Setup(m => m.Start()).Verifiable();

		var command = builder.Create(maintenanceConsoleMock.Object);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "drop", "--Database", "TestDB", "--Server", "TestServer", "--DropOperationalDatabases", "true", "--DropReferenceDatabases", "true", "--DropBIDatabases", "true" });
		result.Wait();

		// Assert
		AssertEquals(0, result.Result);
		maintenanceConsoleMock.Verify(m => m.Start(), Times.Once);

		AssertEquals(Defaults.Instance.ServerName, "TestServer");
		AssertEquals(Defaults.Instance.DatabaseName, "TestDB");
		AssertEquals(Defaults.Instance.IncludeOperationalDbs, true);
		AssertEquals(Defaults.Instance.IncludeBiDbs, true);
		AssertEquals(Defaults.Instance.IncludeReferenceDbs, true);
	}

	public void TestDropCommandShouldNotRunWithMissingRequiredOptions()
	{
		// Arrange
		Defaults.ResetDefaultInstance();
		var builder = new DropCommandBuilder();
		var maintenanceConsoleMock = new Mock<MaintenanceDbConsole>();

		maintenanceConsoleMock.Setup(m => m.Start()).Verifiable();

		var command = builder.Create(maintenanceConsoleMock.Object);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "drop", "--DropOperationalDatabases", "true", "--DropReferenceDatabases", "true", "--DropBIDatabases", "true" });
		result.Wait();

		// Assert
		AssertEquals(1, result.Result);
		maintenanceConsoleMock.Verify(m => m.Start(), Times.Never);
	}
}
