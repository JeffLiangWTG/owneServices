using System;
using System.Net;
using System.Web;
using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Common;
using Enterprise.ZArchitecture.Web.Shared.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GlobalBase.Tests
{
	class BaseWebDbEnvironmentTest : TestCase
	{
		public void TestConnectionPooling()
		{
			var pooling = new BaseWebDbEnvironment().ConnectionPooling;
			CombineAssertions(() =>
			{
				Assert(pooling.IsPooling);
				AssertEquals(0, pooling.LoadBalanceTimeout);
				AssertEquals(0, pooling.MinPoolSize);
				AssertEquals(1000, pooling.MaxPoolSize);
			});
		}

		public void TestDbConnectionGuiPlugin()
		{
			var connectionGuiPlugin = new BaseWebDbEnvironment().ConnectionGuiPlugin;
			CombineAssertions(() =>
			{
				AssertType<WebDbConnectionGuiPlugin>(connectionGuiPlugin);

				var webDbConnectionGuiPlugin = (WebDbConnectionGuiPlugin)connectionGuiPlugin;
				AssertNull(webDbConnectionGuiPlugin.NewConnectingSplashFormManager());
				Assert(webDbConnectionGuiPlugin.GetUserConfirmation($"{Guid.NewGuid()}", $"{Guid.NewGuid()}"));
			});
		}

		[ExpectNoExceptions]
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestHandleDatabaseUpgradedException()
		{
			using (TestHttpContextHelper.DisposableAppDomain(appDomain => { }, () =>
			{
				TestDatabaseUpgradeException(new DatabaseUpgradedException());
			}))
			{ }
		}

		[ExpectNoExceptions]
		[TestRequiresAdministrativePrivileges("Admin privilege is required to edit the Registry")]
		public void TestHandleDatabaseUpgradeInProgressException()
		{
			using (TestHttpContextHelper.DisposableAppDomain(appDomain => { }, () =>
			{
				TestDatabaseUpgradeException(new DatabaseUpgradeInProgressException());
			}))
			{ }
		}

		static void TestDatabaseUpgradeException(DatabaseUpgradeException upgradeException)
		{
			// Arrange
			using (TestHttpContextHelper.DisposableHttpContext(out _, out var httpResponse))
			using (TestHttpContextHelper.DisposableSession(out var sessionStateMock))
			{
				AssertNotNull(HttpContext.Current);
				AssertNotNull(HttpContext.Current.Session);

				var connectionGuiPlugin = new BaseWebDbEnvironment().ConnectionGuiPlugin;

				// Act
				connectionGuiPlugin.HandleDatabaseUpgradeException(upgradeException);
				var response = new ResponseWithContent(httpResponse);

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertEquals("The Web Application you attempted to access is currently being upgraded. Please try again shortly.", response.Content);
					sessionStateMock.Verify(x => x.Abandon(), Times.Once);
				});
			}
		}
	}
}
