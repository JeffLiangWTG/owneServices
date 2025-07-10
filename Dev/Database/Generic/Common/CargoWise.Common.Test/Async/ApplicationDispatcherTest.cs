using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Moq;
using NUnit.Framework;

namespace CargoWise.Common.Testing.Async
{
	internal class ApplicationDispatcherTest : TestCase
	{
		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestRecordThreadException()
		{
			TestRecordThreadException(new InvalidCastException("Test123"));
			TestRecordThreadException(new InvalidOperationException("Test567"));
			TestRecordThreadException(new OutOfMemoryException("Test789"));
			TestRecordThreadException(null);

			void TestRecordThreadException(Exception expectedException)
			{
				// Arrange
				var errorReporterMock = new Mock<IErrorReporter>();
				var dbEnvMock = new Mock<BaseDbEnvironment>();
				dbEnvMock
					.SetupGet(x => x.ConnectionGuiPlugin)
					.Returns(Mock.Of<IDbConnectionGuiPlugin>());
				dbEnvMock
					.SetupGet(x => x.ConnectionPooling)
					.Returns(Mock.Of<IConnectionPooling>());

				using var adminConnection = Db.NewAdminConnection();
				var schemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection);
				using var schemaRollbackAction = new DisposableAction(() => DbRegistry.DatabaseMajorSchemaVersion.SaveValue(schemaVersion, adminConnection));

				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(schemaVersion + 1, adminConnection);
				adminConnection.ExecuteNonQuery("kill " + Db.Connection.SPID);

				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;

				using (DbEnv.SetTemporaryDbEnvironment(dbEnvMock.Object))
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					// Act
					using var dispose = ApplicationDispatcher.RecordThreadException(expectedException);
					Db.Connection.EnsureIsOpen();
				}

				// Assert
				errorReporterMock.Verify(
					x =>
						x.Report(
							It.IsAny<string>(),
							string.Empty,
							It.Is<Exception>(y => y.InnerException == expectedException)),
					Times.Once);
			}
		}
	}
}
