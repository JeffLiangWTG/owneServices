using System.CommandLine;
using System.Linq;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class BackupCommandBuilderTest : TestCase
{
	public void TestCommandShouldContainRequiredOptions()
	{
		// Arrange
		var builder = new BackupCommandBuilder();

		// Act
		var backupConsole = new BackupDbConsole();
		var command = builder.Create(backupConsole);

		// Assert
		AssertNotNull(command);
		AssertNotNull(command.Options);
		Assert(command.Options.Any(o => o.Name == "Database" && o.IsRequired));
		Assert(command.Options.Any(o => o.Name == "Server" && o.IsRequired));
		Assert(command.Options.Any(o => o.Name == "BackupFolder" && o.IsRequired));
	}

	public void TestCommandShouldContainOptionalOptions()
	{
		// Arrange
		var builder = new BackupCommandBuilder();

		// Act
		var backupConsole = new BackupDbConsole();
		var command = builder.Create(backupConsole);

		// Assert
		AssertNotNull(command);
		AssertNotNull(command.Options);
		Assert(command.Options.Any(o => o.Name == "AuditBackupFolder"));
		Assert(command.Options.Any(o => o.Name == "EDWBackupFolder"));
		Assert(command.Options.Any(o => o.Name == "BackupOperationalDatabases"));
		Assert(command.Options.Any(o => o.Name == "BackupReferenceDatabases"));
		Assert(command.Options.Any(o => o.Name == "BackupBiDatabases"));
	}

	public void TestBackupCommandShouldRun()
	{
		// Arrange
		Defaults.ResetDefaultInstance();
		var builder = new BackupCommandBuilder();
		var backupConsoleMock = new Mock<BackupDbConsole>();

		backupConsoleMock.Setup(m => m.Start()).Verifiable();

		var command = builder.Create(backupConsoleMock.Object);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "backup", "--Database", "TestDB", "--Server", "TestServer", "--BackupFolder", "C:\\Backups\\", "--AuditBackupFolder", "C:\\Backups\\", "--EDWBackupFolder", "C:\\Backups\\", "--BackupOperationalDatabases", "true", "--BackupReferenceDatabases", "true", "--BackupBiDatabases", "true" });
		result.Wait();

		// Assert
		AssertEquals(0, result.Result);
		backupConsoleMock.Verify(m => m.Start(), Times.Once);

		AssertEquals(Defaults.Instance.ServerName, "TestServer");
		AssertEquals(Defaults.Instance.DatabaseName, "TestDB");
		AssertEquals(Defaults.Instance.BackupFolder, "C:\\Backups\\");
		AssertEquals(Defaults.Instance.AuditBackupFolder, "C:\\Backups\\");
		AssertEquals(Defaults.Instance.EdwBackupFolder, "C:\\Backups\\");
		AssertEquals(Defaults.Instance.IncludeOperationalDbs, true);
		AssertEquals(Defaults.Instance.IncludeReferenceDbs, true);
		AssertEquals(Defaults.Instance.IncludeBiDbs, true);
	}

	public void TestBackupCommandShouldNotRunWithMissingRequiredOptions()
	{
		// Arrange
		Defaults.ResetDefaultInstance();
		var builder = new BackupCommandBuilder();
		var backupConsoleMock = new Mock<BackupDbConsole>();

		backupConsoleMock.Setup(m => m.Start()).Verifiable();

		var command = builder.Create(backupConsoleMock.Object);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "backup", "--Database", "TestDB", "--AuditBackupFolder", "C:\\Backups\\", "--EDWBackupFolder", "C:\\Backups\\", "--BackupOperationalDatabases", "true", "--BackupReferenceDatabases", "true", "--BackupBiDatabases", "true" });
		result.Wait();

		// Assert
		AssertEquals(1, result.Result);
		backupConsoleMock.Verify(m => m.Start(), Times.Never);
	}
}
