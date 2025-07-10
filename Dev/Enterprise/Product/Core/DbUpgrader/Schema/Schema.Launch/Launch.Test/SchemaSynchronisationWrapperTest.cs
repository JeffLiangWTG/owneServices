using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	[UseSnapshotProtection]
	sealed class SchemaSynchronisationWrapperTest : TestCase
	{
		public void TestRunThrowsException()
		{
			// Arrange
			const string message = ":(";

			using (AdoTestUtils.CreateDbDropExistingDisposable(DatabaseName, Db.DatabaseName))
			{
				var exception = new InvalidOperationException(message);
				upgradeManagerMock.Setup(manager => manager.StartTask(It.IsAny<string>())).Throws(exception);

				var schemaSynchronisationWrapper = new SchemaSynchronisationWrapper(upgradeManagerMock.Object, DatabaseName, Db.Connection);

				// Act
				// Assert
				var result = AssertExceptionThrown<Exception>(() => schemaSynchronisationWrapper.Run());
				AssertContains("Failed to synchronise database", result.Message);
				AssertContains(DatabaseName, result.Message);
				AssertEquals(exception, result.InnerException);
				upgradeManagerMock.Verify(manager => manager.ShowTaskError(It.IsAny<string>()), Times.Once);
				upgradeManagerMock.Verify(manager => manager.ShowTaskError(message), Times.Once);
			}
		}

		public void TestRunThrowsSqlExceptionCouldNotObtainExclusiveLock()
		{
			// Arrange
			const string message = "Could not obtain exclusive lock on";

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, DatabaseName, Db.DatabaseName))
			{
				var exception = AdoTestUtils.GetSqlException(1807, message, connection);
				upgradeManagerMock.Setup(manager => manager.StartTask(It.IsAny<string>())).Throws(exception);

				var schemaSynchronisationWrapper = new SchemaSynchronisationWrapper(upgradeManagerMock.Object, DatabaseName, connection);

				// Act
				// Assert
				var result = AssertExceptionThrown<Exception>(() => schemaSynchronisationWrapper.Run());
				AssertContains("Failed to synchronise database", result.Message);
				AssertContains(DatabaseName, result.Message);
				AssertEquals(exception, result.InnerException);
				AssertContains("Current queries", result.Message);
				upgradeManagerMock.Verify(manager => manager.ShowTaskError(It.IsAny<string>()), Times.Once);
				upgradeManagerMock.Verify(manager => manager.ShowTaskError(message), Times.Once);
			}
		}

		public void TestRunThrowsSqlException()
		{
			// Arrange
			const string message = ":(";

			using (AdoTestUtils.CreateDbDropExistingDisposable(DatabaseName, Db.DatabaseName))
			{
				var exception = AdoTestUtils.GetSqlException(1234, message, Db.Connection);
				upgradeManagerMock.Setup(manager => manager.StartTask(It.IsAny<string>())).Throws(exception);

				var schemaSynchronisationWrapper = new SchemaSynchronisationWrapper(upgradeManagerMock.Object, DatabaseName, Db.Connection);

				// Act
				// Assert
				var result = AssertExceptionThrown<Exception>(() => schemaSynchronisationWrapper.Run());
				AssertContains("Failed to synchronise database", result.Message);
				AssertContains(DatabaseName, result.Message);
				AssertEquals(exception, result.InnerException);
				AssertNotContains("Current queries", result.Message);
				upgradeManagerMock.Verify(manager => manager.ShowTaskError(It.IsAny<string>()), Times.Once);
				upgradeManagerMock.Verify(manager => manager.ShowTaskError(message), Times.Once);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			upgradeManagerMock = new Mock<IUpgradeManager>();
		}

		const string DatabaseName = nameof(SchemaSynchronisationWrapperTest);
		const string TemplateDbName = "SchemaSynchronisationWrapperTest_TemplateDb";
		Mock<IUpgradeManager> upgradeManagerMock;
	}
}
