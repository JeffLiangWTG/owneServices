using System;
using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Assemblies.Testing
{
	class AssembliesUpgraderTransactionalTest : TransactionedTestCase
	{
		public void TestUpgradeRequiredIfVersionChanged()
		{
			// Arrange
			var upgrader = new AssembliesUpgrader(upgradeManager, Db.Connection, new VersionLabel(0, 0));

			// Act
			var result = upgrader.IsUpgradeRequired;

			// Assert
			AssertEquals(true, result);
		}

		public void TestUpgradeNotRequiredIfVersionIsTheSame()
		{
			// Arrange
			var upgrader = new AssembliesUpgrader(upgradeManager, Db.Connection, SqlClrAssembliesVersion.Application);

			// Act
			var result = upgrader.IsUpgradeRequired;

			// Assert
			AssertEquals(false, result);
		}

		public void TestUpgradeRequiredIfDatabaseUpgraderWantsIt()
		{
			// Arrange
			var upgrader = new AssembliesUpgrader(upgradeManager, Db.Connection, SqlClrAssembliesVersion.Application, databaseAssembliesUpgrader1, databaseAssembliesUpgrader2);
			databaseAssembliesUpgrader2.ChangeableLatestVersion = new VersionLabel(-1, -1);

			// Act
			var result = upgrader.IsUpgradeRequired;

			// Assert
			AssertEquals(true, result);
		}

		public void TestUpgradeNotRequired()
		{
			// Arrange
			var upgrader = new AssembliesUpgrader(upgradeManager, Db.Connection, SqlClrAssembliesVersion.Application, databaseAssembliesUpgrader1, databaseAssembliesUpgrader2);

			// Act
			var result = upgrader.IsUpgradeRequired;

			// Assert
			AssertEquals(false, result);
			AssertEquals(0, databaseAssembliesUpgrader1.RunUpdateCount);
			AssertEquals(0, databaseAssembliesUpgrader2.RunUpdateCount);
		}

		public void TestDoUpgradeRunsRequestedDatabaseUpgraders()
		{
			// Arrange
			var upgrader = new AssembliesUpgrader(upgradeManager, Db.Connection, SqlClrAssembliesVersion.Application, databaseAssembliesUpgrader1, databaseAssembliesUpgrader2);
			databaseAssembliesUpgrader2.ChangeableLatestVersion = new VersionLabel(-1, -1);

			// Act
			upgrader.RunUpgrade();

			// Assert
			AssertEquals(0, databaseAssembliesUpgrader1.RunUpdateCount);
			AssertEquals(1, databaseAssembliesUpgrader2.RunUpdateCount);
		}

		public void TestDoUpgradeRunsAllDatabaseUpgraders()
		{
			// Arrange
			var upgrader = new AssembliesUpgrader(upgradeManager, Db.Connection, SqlClrAssembliesVersion.Application, databaseAssembliesUpgrader1, databaseAssembliesUpgrader2);
			databaseAssembliesUpgrader1.ChangeableLatestVersion = new VersionLabel(-1, -1);
			databaseAssembliesUpgrader2.ChangeableLatestVersion = new VersionLabel(-1, -1);

			// Act
			upgrader.RunUpgrade();

			// Assert
			AssertEquals(1, databaseAssembliesUpgrader1.RunUpdateCount);
			AssertEquals(1, databaseAssembliesUpgrader2.RunUpdateCount);
		}

		public void TestDoUpgradeUpdatesVersion()
		{
			// Arrange
			DbRegistry.DatabaseMajorClrAssembliesVersion.SaveValue(-12, Db.Connection);
			DbRegistry.DatabaseMinorClrAssembliesVersion.SaveValue(-13, Db.Connection);
			var upgrader = new AssembliesUpgrader(upgradeManager, Db.Connection, new VersionLabel(-1, -1));
			upgrader.RunUpgrade();

			// Act
			var result = new
			{
				Major = DbRegistry.DatabaseMajorClrAssembliesVersion.LoadValue(Db.Connection),
				Minor = DbRegistry.DatabaseMinorClrAssembliesVersion.LoadValue(Db.Connection),
			};

			// Assert
			AssertEquals(SqlClrAssembliesVersion.Application.Major, result.Major);
			AssertEquals(SqlClrAssembliesVersion.Application.Minor, result.Minor);
		}

		protected override void SetUp()
		{
			base.SetUp();
			upgradeManager = new DummyUpgradeManager();
			databaseAssembliesUpgrader1 = new DatabaseAssembliesUpgraderStub(upgradeManager, Db.Connection, SqlClrAssembliesVersion.Application, Array.Empty<SqlAssemblyClrObjectInfo>());
			databaseAssembliesUpgrader2 = new DatabaseAssembliesUpgraderStub(upgradeManager, Db.Connection, SqlClrAssembliesVersion.Application, Array.Empty<SqlAssemblyClrObjectInfo>());
		}

		DatabaseAssembliesUpgraderStub databaseAssembliesUpgrader1;
		DatabaseAssembliesUpgraderStub databaseAssembliesUpgrader2;
		DummyUpgradeManager upgradeManager;

		class DatabaseAssembliesUpgraderStub : DatabaseAssembliesUpgrader
		{
			public DatabaseAssembliesUpgraderStub(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade, params SqlAssemblyClrObjectInfo[] registeredClrObjects)
				: base(manager, upgConnection, versionBeforeUpgrade, registeredClrObjects)
			{
			}

			public override void RunUpgrade()
			{
				RunUpdateCount++;
			}

			protected override VersionLabel LatestVersion => ChangeableLatestVersion;

			public VersionLabel ChangeableLatestVersion { get; set; } = SqlClrAssembliesVersion.Application;

			public int RunUpdateCount { get; set; }
		}
	}
}
