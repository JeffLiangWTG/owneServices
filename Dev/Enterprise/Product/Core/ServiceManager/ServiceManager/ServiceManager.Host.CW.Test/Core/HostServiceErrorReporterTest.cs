using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class HostServiceErrorReporterTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			loggerMock = new Mock<IHostLogger>();
			exceptionHandlerMock = new Mock<IExceptionHandler>();
		}

		public void TestWrongConstructorParams()
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => _ = new HostServiceErrorReporter(loggerMock.Object, exceptionHandlerMock.Object));

				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HostServiceErrorReporter(null, exceptionHandlerMock.Object));
				AssertEquals(result.ParamName, "hostLogger");

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HostServiceErrorReporter(loggerMock.Object, null));
				AssertEquals(result.ParamName, "exceptionHandler");
			});
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestReport_ConcurrencyException_WhenDatabaseUpgraded_DbConnectionIsHealthy()
		{
			// We want non-developer reporting logic
			var user = new UserForTest();
			user.IsDeveloperLogin = false;
			var userContext = new Environment.Testing.UserContextTest.UserContextForTest(user, Env.CurrentCompany);

			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock
				.SetupGet(x => x.Key)
				.Callback(() =>
				{
					// run sql to trigger DatabaseUpgradeException
					Db.Connection.ExecuteScalar("SELECT @@servername");
				});

			// All this is to accurately setup the same environment the HostServiceErrorReporter operates under in production.
			var dbEnvMock = new DbEnvironmentWithMockGuiPluginForTest();
			var errorReporter = new HostServiceErrorReporter(Mock.Of<IHostLogger>(), new HostExceptionHandler());
			ExceptionReporter.DisableExposed();
			using (new DisposableAction(() => ExceptionReporter.DisableExposed()))
			using (Env.SetTemporaryUserContext(userContext))
			using (Globals.SetIsUnitTestingProductionFunctionality())
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporter))
			using (ObjectFactory.Substitute(versionMock.Object))
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (DbEnv.SetTemporaryDbEnvironment(dbEnvMock))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				errorReporter.Enable();
				Db.Connection.CloseConnection();

				var dummyBizO = new BusinessObjectFactory().New<DummyBusinessObject>();
				var sqlException = SqlExceptionBuilder.CreateSqlException(1, "SQL error");
				var concurrencyException = new ZDataConcurrencyException(sqlException, ((INeedRow)dummyBizO).Row, Db.Connection);
				ErrorReporter.ReportOnce(concurrencyException.Message, concurrencyException);

				AssertEquals("ErrorReporter.TotalErrorCount", ErrorReporter.TotalErrorCount, 1);
				dbEnvMock.MockPlugin.Verify(x => x.HandleDbConcurrencyException(It.IsAny<Exception>()), Times.Once);
				dbEnvMock.MockPlugin.Verify(x => x.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()), Times.Once);
				AssertEquals("DatabaseUpgradedExceptionHasBeenThrown", Db.Connection.DatabaseUpgradedExceptionHasBeenThrown, false);
				AssertEquals("Connection.State", Db.Connection.State, System.Data.ConnectionState.Closed);
			}
		}

		class DbEnvironmentWithMockGuiPluginForTest : BaseDbEnvironment
		{
			public Mock<IDbConnectionGuiPlugin> MockPlugin = new Mock<IDbConnectionGuiPlugin>();
			public override IDbConnectionGuiPlugin ConnectionGuiPlugin => MockPlugin.Object;
		}

		Mock<IHostLogger> loggerMock;
		Mock<IExceptionHandler> exceptionHandlerMock;
	}
}
