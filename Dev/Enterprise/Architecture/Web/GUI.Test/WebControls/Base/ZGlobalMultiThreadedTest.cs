using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.ResourceStrings.Internal;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[UseSnapshotProtection]
	[HttpContextEnabledTest]
	sealed class ZGlobalMultiThreadedTest : TestCase
	{
		public void TestGeneralNetworkErrorsAreRedirectedToErrorPageOnBeginRequest()
		{
			// Arrange
			var generalNetWorkErrorsSqlException = SqlExceptionBuilder.CreateSqlException(18461, "A transport-level error has occurred");
			var cacheMock = new Mock<ScalarCache>();
			var scalarReturnValue = (object)DBNull.Value;
			cacheMock.Setup(x => x
				.GetValue(It.Is<DbCommand>(cmd => cmd.CommandText.Contains("DataRegGetValueNOD")), out scalarReturnValue))
				.Throws(generalNetWorkErrorsSqlException);

			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);

			using (Db.Connection.StartScalarCaching_ForTest(cacheMock.Object))
			{
				// Act
				global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
			}

			// Assert
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);

			var redirectPage = HttpContext.Current.Response.RedirectLocation;
			var queryStrings = new SecureQueryString(WebUtility.UrlDecode(redirectPage.Substring(redirectPage.IndexOf("?data=") + "?data=".Length)));

			AssertStartsWith("Should redirect to error page", "/Error.aspx", redirectPage);
			AssertEquals("Web Application Error", queryStrings["title"]);
			AssertEquals("The Web Application you attempted to access is currently unavailable", queryStrings["message"]);
		}

		[ExpectNoExceptions]
		public void TestSessionStartFromAnotherThread()
		{
			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);

			Exception exceptionFromThread = null;
			ThreadStart threadstart = new ThreadStart(delegate
			{
				try
				{
					new HttpContextEnabledTestAttribute().SetUp(this);
					global.Session_Start_ForTesting(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					exceptionFromThread = ex;
				}
			});
			var thread = new Thread(threadstart);
			thread.Start();
			thread.Join();

			if (exceptionFromThread != null)
			{
				throw exceptionFromThread;
			}
		}

		[ExpectNoExceptions]
		public void TestBeginRequestNotReportNoDbDisposableActionWhenHandleException()
		{
			var originalResourceString = ResInternal.resourceStrings.Value;
			var errorReporterMock = new Mock<IErrorReporter>();
			var task = System.Threading.Tasks.Task.Run(() =>
			{
				using (new DisposableAction(() => new HttpContextEnabledTestAttribute().TearDown(this)))
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					// Arrange
					new HttpContextEnabledTestAttribute().SetUp(this);

					var globalMock = new Mock<ZGlobalForTesting>() { CallBase = true };
					globalMock
						.Setup(x => x.ConfigurationOK)
						.Returns(true);

					globalMock
						.Protected()
						.Setup("OnCustomApplicationBeginRequest", ItExpr.IsAny<Object>(), ItExpr.IsAny<EventArgs>())
						.Throws(new UnauthorizedAccessException("Access to the path denied."));

					var global = globalMock.Object;
					TestGlobal.InitZGlobalWithHttpContext(global);

					using (new DisposableAction(() => ResInternal.resourceStrings.Value = null))
					using (new DisposableAction(() => ResInternal.resourceStrings.Value.CurrentLanguage = Res.DefaultLanguage))
					{
						ResInternal.resourceStrings.Value = originalResourceString;
						ResInternal.resourceStrings.Value.CurrentLanguage = "FR";

						//Act
						global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
					}
				}
			});
			task.Wait();

			// Assert
			errorReporterMock.Verify(x => x.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction, It.IsAny<InvalidOperationException>()), Times.Never);
		}

		public void TestBeginRequestWorksDuringUpgrade()
		{
			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);
			global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
				try
				{
					var task = new Task(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							new HttpContextEnabledTestAttribute().SetUp(this);
							AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => Db.Connection.EnsureIsOpen());
							AssertNoExceptionThrown(() => global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty));
							new HttpContextEnabledTestAttribute().TearDown(this);
						}
					});
					task.Start();
					task.Wait();
				}
				finally
				{
					adminConnection.ResetLockout();
				}
			}
		}

		public void TestRefreshRegistryItemCacheDoesNothingDuringUpgrade()
		{
			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);
			global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://one.com");
			AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
				try
				{
					var task = new Task(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							new HttpContextEnabledTestAttribute().SetUp(this);
							AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => Db.Connection.EnsureIsOpen());
							Thread.Sleep(1100);

							AssertNoExceptionThrown(() =>
							{
								global.Application_BeginRequest_ForTesting(null, EventArgs.Empty);
							});
							AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
							new HttpContextEnabledTestAttribute().TearDown(this);
						}
					});
					task.Start();
					task.Wait();
				}
				finally
				{
					adminConnection.ResetLockout();
				}
			}
		}

		public void TestUseDbConnectionInWeb()
		{
			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);
			global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://one.com");
			AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
				try
				{
					var task = new Task(() =>
					{
						new HttpContextEnabledTestAttribute().SetUp(this);
						Thread.Sleep(1100);

						AssertNoExceptionThrown(() =>
						{
							global.Application_BeginRequest_ForTesting(null, EventArgs.Empty);
						});

						AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
						new HttpContextEnabledTestAttribute().TearDown(this);
					});
					task.Start();
					task.Wait();
				}
				finally
				{
					adminConnection.ResetLockout();
				}
			}
		}
	}
}
