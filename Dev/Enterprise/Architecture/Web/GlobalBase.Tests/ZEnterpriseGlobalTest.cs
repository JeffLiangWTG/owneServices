using System;
using System.Net;
using System.Threading;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Common;
using Enterprise.ZArchitecture.Web.Shared.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GlobalBase.Tests
{
	[HttpContextEnabledTest]
	class ZEnterpriseGlobalTest : TestCase
	{
		[ExpectNoExceptions]
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestInitialiseWeb()
		{
			using (TestHttpContextHelper.DisposableAppDomain(appDomain => { }, () =>
			{
				var cancellationTokenSource = new CancellationTokenSource();
				var appStartedEvent = new ManualResetEventSlim(false);
				var enterpriseGlobal = new ZEnterpriseGlobal();
				var webThread = new Thread(() =>
				{
					using (enterpriseGlobal.StartApplicationDisposable())
					{
						appStartedEvent.Set();

						while (!cancellationTokenSource.IsCancellationRequested)
						{
							Thread.Sleep(100);
						}
					}
				});

				try
				{
					webThread.Start();
					appStartedEvent.Wait(cancellationTokenSource.Token);

					CombineAssertions("ZEnterpriseGlobal is initialised with correct environment", () =>
					{
						AssertNotNull(DbEnv.Instance);
						Assert(DbEnv.Instance.GetType().IsAssignableFrom(typeof(BaseWebDbEnvironment)));
						Assert(ErrorReporter.Instance.GetType().IsAssignableFrom(typeof(BaseWebExceptionReporter)));
						AssertEquals(true, enterpriseGlobal.EnableErrorReport_Exposed);
						Assert(enterpriseGlobal.WebExceptionReporter_Exposed is BaseWebExceptionReporter);
						Assert(enterpriseGlobal.WebEnvProvider_Exposed is WebEnvProvider);
						Assert(enterpriseGlobal.WebEnvProvider_Exposed.Instance is WebEnvironment);
						Assert(enterpriseGlobal.TopLevelExceptionHandler_Exposed is TopLevelWebExceptionHandler);

						var environment = enterpriseGlobal.WebEnvProvider_Exposed.Instance as IEnvironment;
						AssertNotNull(environment);
						Assert(environment.SemaphoreProvider is WebSemaphoreProvider);

						var semaphoreProvider = environment.SemaphoreProvider;
						AssertNotNull(semaphoreProvider);
					});
				}
				finally
				{
					cancellationTokenSource.Cancel();
					webThread.Join();
				}
			})) { }
		}

		public void TestHandleUnhandledSQLException_MaintainDbConnectionWithDisposableAction()
		{
			// Arrange
			ErrorReporter.Clear();
			var sqlException = SqlExceptionBuilder.CreateSqlException(2, 0, 11, Db.ServerName, "Error Number 2 : A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)", "", 0);
			var httpContext = HttpContext.Current;
			var thread = new Thread(() =>
			{
				var zEnterpriseGlobal = new ZEnterpriseGlobalForTesting();
				HttpContext.Current = httpContext;
				HttpContext.Current.AddError(sqlException);

				// Act
				zEnterpriseGlobal.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			});
			thread.Start();
			thread.Join();

			// Assert
			AssertExceptionWasHandled();
			AssertNotContains("Should NOT have reported accessing DbConnection without disposable Action", ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction, ErrorReporter.LastMessageReported);
		}

		void AssertExceptionWasHandled()
		{
			CombineAssertions(() =>
			{
				AssertNotEquals((int)HttpStatusCode.InternalServerError, HttpContext.Current.Response.StatusCode);
				AssertNotEquals($"{HttpStatusCode.InternalServerError}", HttpContext.Current.Response.StatusDescription);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
				AssertEquals(0, ExceptionReporter.Instance.TotalReportCount);
			});
		}

		public void TestBeginRequest_DbUpgradeAlreadyThrown()
		{
			// Arrange
			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock.Object))
			using (var dbEnv = new DbEnvironmentWithMockGuiPlugin())
			using (var disposable = new DisposableAction(
				() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
				() =>
				{
					Db.ConnectionOverrideForTest.Dispose();
					Db.ConnectionOverrideForTest = null;
				}))
			{
				// Cause a DatabaseUpgradedException
				Exception firstException = null;
				try
				{
					((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection();
				}
				catch (Exception ex)
				{
					firstException = ex;
				}

				var global = new ZEnterpriseGlobal();

				// Act
				// Assert
				AssertExceptionThrown<DatabaseUpgradedException>(() => global.Application_BeginRequest_Exposed(null, EventArgs.Empty));
				AssertType<DatabaseUpgradedException>(firstException);

				AssertType<DatabaseUpgradedException>(firstException);
			}
		}

		public void TestIsDatabaseUpgradedFlagIsClearedOnBeginRequest()
		{
			// Arrange
			using var disposableAction = new DisposableAction(() =>
			{
				((IDbUpgradeSupport)Db.Instance).SetDatabaseHasBeenUpgraded(-1);
			}, Db.ResetDatabaseUpgraded_ForTest);

			AssertEquals(true, Db.IsDatabaseUpgraded);

			// Act
			// Assert
			var global = new ZEnterpriseGlobal();
			AssertNoExceptionThrown(() => global.Application_BeginRequest_Exposed(null, EventArgs.Empty));

			AssertEquals(false, Db.IsDatabaseUpgraded);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ZEnterpriseGlobal.CloseAndReopenConnectionIntervalForTest.Value = TimeSpan.Zero;
		}

		protected override void TearDown()
		{
			ZEnterpriseGlobal.CloseAndReopenConnectionIntervalForTest.ResetValue();
			base.TearDown();
		}
	}

	public class DbEnvironmentWithMockGuiPlugin : BaseDbEnvironment, IDisposable
	{
		public DbEnvironmentWithMockGuiPlugin()
		{
			existingEnv = DbEnv.Instance;
			DbEnv.SetDbEnvironment(this);
		}

		public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;
		readonly IDbConnectionGuiPlugin connectionGuiPlugin = Mock.Of<IDbConnectionGuiPlugin>();

		public void Dispose() => DbEnv.SetDbEnvironment(existingEnv);
		readonly IDbEnvironment existingEnv;
	}

	public class ZEnterpriseGlobalForTesting : ZEnterpriseGlobal
	{
		public void Application_Error_ForTesting(object sender, EventArgs e) => Application_Error(sender, e);
	}
}
