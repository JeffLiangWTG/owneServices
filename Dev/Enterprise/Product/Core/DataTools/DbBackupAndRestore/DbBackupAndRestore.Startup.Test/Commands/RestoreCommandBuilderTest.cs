using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public class RestoreCommandBuilderTest : TestCase
{
	public void TestCommandShouldContainRequiredOptions()
	{
		// Arrange
		var builder = new RestoreCommandBuilder();

		// Act
		var restorer = new RestoreDbConsole();
		var command = builder.Create(restorer);

		// Assert
		AssertNotNull(command);
		AssertNotNull(command.Options);
		Assert(command.Options.Any(o => o.Name == "Database" && o.IsRequired));
		Assert(command.Options.Any(o => o.Name == "Server" && o.IsRequired));
		Assert(command.Options.Any(o => o.Name == "BackupPath" && o.IsRequired));
		Assert(command.Options.Any(o => o.Name == "RestoreType" && o.IsRequired));
	}

	public void TestCommandShouldContainOptionalOptions()
	{
		// Arrange
		var builder = new RestoreCommandBuilder();

		var restoreManagerMock = new Mock<IDbRestoreManager>();

		restoreManagerMock.Setup(m => m.RefreshExtendedProperties(It.IsAny<string>())).Verifiable();
		restoreManagerMock.Setup(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new DbFileInfoCollection());
		var applyForAllFiles = true;
		restoreManagerMock.Setup(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles)).Returns(new DbFileInfoCollection());

		restoreManagerMock
			.Setup(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>()))
			.Verifiable();

		var restorer = new RestoreDbConsole(restoreManagerMock.Object);

		// Act
		var command = builder.Create(restorer);

		// Assert
		AssertNotNull(command);
		AssertNotNull(command.Options);
		Assert(command.Options.Any(o => o.Name == "DataFilePath"));
		Assert(command.Options.Any(o => o.Name == "LogFilePath"));
		Assert(command.Options.Any(o => o.Name == "RestoreDifferentialBackup"));
		Assert(command.Options.Any(o => o.Name == "RestoreTransactionLogBackup"));
		Assert(command.Options.Any(o => o.Name == "ExcludeAuditDb"));
		Assert(command.Options.Any(o => o.Name == "AuditServer"));
		Assert(command.Options.Any(o => o.Name == "AuditBackup"));
		Assert(command.Options.Any(o => o.Name == "AuditDataFilePath"));
		Assert(command.Options.Any(o => o.Name == "AuditLogFilePath"));
		Assert(command.Options.Any(o => o.Name == "EDWServer"));
		Assert(command.Options.Any(o => o.Name == "EDWBackup"));
		Assert(command.Options.Any(o => o.Name == "EDWDataFilePath"));
		Assert(command.Options.Any(o => o.Name == "EDWLogFilePath"));
	}

	public void TestRestoreCommandShouldRun()
	{
		// Arrange
		Defaults.ResetDefaultInstance();
		var builder = new RestoreCommandBuilder();
		var restoreManagerMock = new Mock<IDbRestoreManager>();

		restoreManagerMock.Setup(m => m.RefreshExtendedProperties(It.IsAny<string>())).Verifiable();
		restoreManagerMock.Setup(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new DbFileInfoCollection());
		var applyForAllFiles = true;
		restoreManagerMock.Setup(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles)).Returns(new DbFileInfoCollection());
		restoreManagerMock.Setup(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		restoreManagerMock.Setup(m => m.GetAvailabilityGroupName(It.IsAny<string>(), It.IsAny<string>())).Returns("AG");
		restoreManagerMock.Setup(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>())).Verifiable();
		restoreManagerMock.Setup(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		restoreManagerMock.Setup(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>())).Returns(DatabaseStatus.Online);

		var restorer = new RestoreDbConsole(restoreManagerMock.Object);
		var command = builder.Create(restorer);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "restore", "--Database", "TestDB", "--Server", "TestServer", "--BackupPath", "C:\\Backups\\TestDB.bak", "--RestoreType", "RestoreWithRecovery" });

		result.Wait();

		// Assert
		AssertEquals(0, result.Result);
		restoreManagerMock.Verify(m => m.RefreshExtendedProperties(It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles), Times.Once);
		restoreManagerMock.Verify(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>()), Times.Once);
		restoreManagerMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.GetAvailabilityGroupName(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
		restoreManagerMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

		AssertEquals(Defaults.Instance.ServerName, "TestServer");
		AssertEquals(Defaults.Instance.DatabaseName, "TestDB");
		AssertEquals(Defaults.Instance.BackupFileName, "C:\\Backups\\TestDB.bak");
		AssertEquals(Defaults.Instance.RestoreOption, DbRestoreOption.RestoreWithRecovery);
		AssertEquals(Defaults.Instance.AddDbToAvailabilityGroup, true);
		AssertEquals(Defaults.Instance.AvailabilityGroup, "AG");
	}

	public void TestRestoreCommandShouldNotRunWithMissingRequiredOptions()
	{
		// Arrange
		var builder = new RestoreCommandBuilder();
		var restoreManagerMock = new Mock<IDbRestoreManager>();

		restoreManagerMock.Setup(m => m.RefreshExtendedProperties(It.IsAny<string>())).Verifiable();
		restoreManagerMock.Setup(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new DbFileInfoCollection());
		var applyForAllFiles = true;
		restoreManagerMock.Setup(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles)).Returns(new DbFileInfoCollection());
		restoreManagerMock.Setup(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		restoreManagerMock.Setup(m => m.GetAvailabilityGroupName(It.IsAny<string>(), It.IsAny<string>())).Returns("AG");
		restoreManagerMock.Setup(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>())).Verifiable();
		restoreManagerMock.Setup(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		restoreManagerMock.Setup(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>())).Returns(DatabaseStatus.Online);

		var restorer = new RestoreDbConsole(restoreManagerMock.Object);
		var command = builder.Create(restorer);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "restore", "--BackupPath", "C:\\Backups\\TestDB.bak", "--RestoreType", "RestoreWithRecovery" });

		result.Wait();

		// Assert
		AssertEquals(1, result.Result);
		restoreManagerMock.Verify(m => m.RefreshExtendedProperties(It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles), Times.Never);
		restoreManagerMock.Verify(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>()), Times.Never);
		restoreManagerMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.GetAvailabilityGroupName(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	public void TestRestoreOnAvailabilityGroupShouldNotRunRestoreWithNoRecovery()
	{
		// Arrange
		var builder = new RestoreCommandBuilder();
		var restoreManagerMock = new Mock<IDbRestoreManager>();

		restoreManagerMock.Setup(m => m.RefreshExtendedProperties(It.IsAny<string>())).Verifiable();
		restoreManagerMock.Setup(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new DbFileInfoCollection());
		var applyForAllFiles = true;
		restoreManagerMock.Setup(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles)).Returns(new DbFileInfoCollection());
		restoreManagerMock.Setup(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		restoreManagerMock.Setup(m => m.GetAvailabilityGroupName(It.IsAny<string>(), It.IsAny<string>())).Returns("AG");
		restoreManagerMock.Setup(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>())).Verifiable();
		restoreManagerMock.Setup(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		restoreManagerMock.Setup(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>())).Returns(DatabaseStatus.Online);

		var restorer = new RestoreDbConsole(restoreManagerMock.Object);
		var command = builder.Create(restorer);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "restore", "--Database", "TestDB", "--Server", "TestServer", "--BackupPath", "C:\\Backups\\TestDB.bak", "--RestoreType", "RestoreWithNoRecovery" });

		result.Wait();

		// Assert
		AssertEquals(-1, result.Result);
		restoreManagerMock.Verify(m => m.RefreshExtendedProperties(It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles), Times.Never);
		restoreManagerMock.Verify(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>()), Times.Never);
		restoreManagerMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.GetAvailabilityGroupName(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
	}

	public void TestRestoreCommandShouldRunNoAvailabilityGroup()
	{
		// Arrange
		Defaults.ResetDefaultInstance();
		var builder = new RestoreCommandBuilder();
		var restoreManagerMock = new Mock<IDbRestoreManager>();

		restoreManagerMock.Setup(m => m.RefreshExtendedProperties(It.IsAny<string>())).Verifiable();
		restoreManagerMock.Setup(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new DbFileInfoCollection());
		var applyForAllFiles = true;
		restoreManagerMock.Setup(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles)).Returns(new DbFileInfoCollection());
		restoreManagerMock.Setup(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
		restoreManagerMock.Setup(m => m.GetAvailabilityGroupsList(It.IsAny<string>())).Returns(new List<string>());
		restoreManagerMock.Setup(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>())).Verifiable();
		restoreManagerMock.Setup(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		restoreManagerMock.Setup(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>())).Returns(DatabaseStatus.Online);

		var restorer = new RestoreDbConsole(restoreManagerMock.Object);
		var command = builder.Create(restorer);
		var rootCommand = new RootCommand() { command };

		// Act
		var result = rootCommand.InvokeAsync(new[] { "restore", "--Database", "TestDB", "--Server", "TestServer", "--BackupPath", "C:\\Backups\\TestDB.bak", "--RestoreType", "RestoreWithRecovery" });

		result.Wait();

		// Assert
		AssertEquals(0, result.Result);
		restoreManagerMock.Verify(m => m.RefreshExtendedProperties(It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.GetBackupDbFileInfoCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.ApplyExtendedProperties(It.IsAny<DbFileInfoCollection>(), out applyForAllFiles), Times.Once);
		restoreManagerMock.Verify(m => m.RestoreDatabases(It.IsAny<DbServerConfiguration>(), It.IsAny<AuditDbServerConfiguration>(), It.IsAny<EdwDbServerConfiguration>(), It.IsAny<DbRestoreSettings>()), Times.Once);
		restoreManagerMock.Verify(m => m.IsPrimaryReplicaOnAvailabilityGroup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		restoreManagerMock.Verify(m => m.GetDatabaseStatus(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.GetAvailabilityGroupName(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		restoreManagerMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

		AssertEquals(Defaults.Instance.ServerName, "TestServer");
		AssertEquals(Defaults.Instance.DatabaseName, "TestDB");
		AssertEquals(Defaults.Instance.BackupFileName, "C:\\Backups\\TestDB.bak");
		AssertEquals(Defaults.Instance.RestoreOption, DbRestoreOption.RestoreWithRecovery);
		AssertEquals(Defaults.Instance.AddDbToAvailabilityGroup, false);
		AssertEquals(Defaults.Instance.AvailabilityGroup, null);
	}
}
