using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	class SqlSecurityManagerCancellationTokenTests
	{
		[Test]
		public void TestBuildSecurityLogsAndThrowsOperationCanceledExceptionIfTokenIsAlreadyCancelled()
		{
			// Arrange
			var source = new CancellationTokenSource(60000);
			var token = source.Token;
			source.Cancel();
			var loggerMock = new Mock<ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			// Act
			// Assert
			Assert.That(() => sqlSecurityManager.BuildSecurity(adminConnection, token), Throws.InstanceOf<OperationCanceledException>());
			loggerMock.Verify(
				l => l.Log(LogType.Error,
				It.Is<string>(m =>
				m.StartsWith("Cancellation was requested"))));
		}

		[Test]
		public void TestBuildSecurityForAllDatabasesLogsAndThrowsOperationCanceledExceptionIfTokenIsAlreadyCancelled()
		{
			// Arrange
			var source = new CancellationTokenSource(180000);
			var token = source.Token;
			source.Cancel();
			var loggerMock = new Mock<ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);

			// Act
			// Assert
			Assert.That(() => sqlSecurityManager.BuildSecurityForAllDatabases(adminConnection, token), Throws.InstanceOf<OperationCanceledException>());

			loggerMock.Verify(
				l => l.Log(LogType.Error,
				It.Is<string>(m =>
				m.StartsWith("Cancellation was requested"))));
		}

		[Test]
		public void TestBuildSecurityLogsAndThrowsOperationCanceledExceptionIfTokenIsCancelledAfterSecurityBuildStarted()
		{
			// Arrange
			var source = new CancellationTokenSource(180000);
			var token = source.Token;
			var loggerMock = new Mock<ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);
			var isOperationCancelledExceptionThrown = false;

			using (var taskStartedWaitHandle = new ManualResetEvent(false))
			{
				var task = Task.Run(() =>
				{
					try
					{
						using (var connection = Db.NewAdminConnection())
						{
							taskStartedWaitHandle.Set();

							sqlSecurityManager.BuildSecurity(connection, token);
						}
					}
					catch (OperationCanceledException)
					{
						isOperationCancelledExceptionThrown = true;
					}
				});

				// Act
				taskStartedWaitHandle.WaitOne();
				source.Cancel();
				task.Wait(300000);

				// Assert
				Assert.That(isOperationCancelledExceptionThrown);
				loggerMock.Verify(
					l => l.Log(LogType.Error,
					It.Is<string>(m =>
					m.StartsWith("Cancellation was requested"))));
			}
		}

		[Test]
		public void TestBuildSecurityForAllDatabasesLogsAndThrowsOperationCanceledExceptionIfTokenIsCancelledAfterSecurityBuildStarted()
		{
			// Arrange
			var source = new CancellationTokenSource(180000);
			var token = source.Token;
			var loggerMock = new Mock<ILogger>();
			var sqlSecurityManager = new SqlSecurityManager(loggerMock.Object, Helper.MainDatabaseNameOutsideTestCase);
			var isOperationCancelledExceptionThrown = false;

			using (var taskStartedWaitHandle = new ManualResetEvent(false))
			{
				var task = Task.Run(() =>
				{
					try
					{
						using (var connection = Db.NewAdminConnection())
						{
							taskStartedWaitHandle.Set();
							sqlSecurityManager.BuildSecurityForAllDatabases(connection, token);
						}
					}
					catch (OperationCanceledException)
					{
						isOperationCancelledExceptionThrown = true;
					}
				});

				// Act
				taskStartedWaitHandle.WaitOne();
				source.Cancel();
				task.Wait(300000);

				// Assert
				Assert.That(isOperationCancelledExceptionThrown);
				loggerMock.Verify(
					l => l.Log(LogType.Error,
					It.Is<string>(m =>
					m.StartsWith("Cancellation was requested"))));
			}
		}

		#region Implementation

		[SetUp]
		public void SetUp()
		{
			adminConnection = Db.NewAdminConnection();
			Helper.DropExtraDatabases(adminConnection);
			Helper.EnsureExtraDatabasesWithSchemas(adminConnection);
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);

			foreach (var databaseName in Helper.Databases())
			{
				using (((ICurrentDbControl)adminConnection).UseDatabase(databaseName))
				{
					Helper.DropDatabasePrincipals(adminConnection);
				}
			}
		}

		[TearDown]
		public void TearDown()
		{
			Helper.DropServerTestEntities(adminConnection, Db.DatabaseName);

			Helper.DropExtraDatabases(adminConnection);

			if (!adminConnection.IsDbWriteable(Db.DatabaseName))
			{
				adminConnection.AlterDbWriteableState(Db.DatabaseName, writeable: true);
			}

			adminConnection?.Dispose();
		}

		AdminConnection adminConnection;

		#endregion Implementation
	}
}
