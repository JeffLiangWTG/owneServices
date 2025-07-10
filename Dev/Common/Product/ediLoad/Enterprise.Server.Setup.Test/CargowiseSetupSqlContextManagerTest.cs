using System;
using System.Collections.Generic;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	public class CargowiseSetupSqlContextManagerTest : TestCase
	{
		CargowiseSetupSqlContextManager contextManager;
		readonly Mock<ISqlConnectionProvider> connectionProviderMock = new Mock<ISqlConnectionProvider>();
		protected override void SetUp()
		{
			base.SetUp();
			Application.ConfigureApplicationServices(
				services =>
				{
					services.RemoveAll<ISqlConnectionProvider>();
#pragma warning disable IDE0001 // Simplify Names
					services.AddSingleton<ISqlConnectionProvider>((sp) => connectionProviderMock.Object);
#pragma warning restore IDE0001 // Simplify Names
				}
				);
			contextManager = (CargowiseSetupSqlContextManager)Application.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();
		}

		public void TestContextManager_OpenConnetion_UsesSAAccountFirst()
		{
			using var dummyConnection = TestConnectionProvider.OpenNewConnection("master");
			connectionProviderMock.Setup(x => x.OpenNewSqlConnection<SysAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Returns(dummyConnection).Verifiable(Times.Once);

			contextManager.OpenConnection(".");

			Assert(contextManager.CurrentContext is not null);
			Assert(contextManager.Connection == dummyConnection);
			connectionProviderMock.VerifyAll();
		}

		public void TestContextManager_OpenConnetion_FallbackToOdysseyAdmin()
		{
			using var dummyConnection = TestConnectionProvider.OpenNewConnection("master");
			connectionProviderMock.Setup(x => x.OpenNewSqlConnection<SysAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Throws(new Exception("Login Failed")).Verifiable(Times.Once);
			connectionProviderMock.Setup(x => x.OpenNewSqlConnection<OdysseyAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Returns(dummyConnection).Verifiable(Times.Once);

			contextManager.OpenConnection(".");

			Assert(contextManager.CurrentContext is not null);
			Assert(contextManager.Connection == dummyConnection);
			connectionProviderMock.VerifyAll();
		}

		public void TestContextManager_OpenConnetion_FallBackToIntegratedSecurity()
		{
			connectionProviderMock.Setup(x => x.OpenNewSqlConnection<SysAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Throws(new Exception("Login Failed")).Verifiable(Times.Once);
			connectionProviderMock.Setup(x => x.OpenNewSqlConnection<OdysseyAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Throws(new Exception("Login Failed")).Verifiable(Times.Once);

			contextManager.OpenConnection(".");

			Assert(contextManager.CurrentContext is not null);
#pragma warning disable CW1065 // Encrypt SQL Connection Rule
			var connectionString = new SqlConnectionStringBuilder() { ConnectionString = contextManager.Connection.ConnectionString };
#pragma warning restore CW1065 // Encrypt SQL Connection Rule
			Assert(connectionString.IntegratedSecurity = true);
			connectionProviderMock.VerifyAll();
		}

		delegate void customConnectionCallback(string serverName, Action<SqlConnectionStringBuilder> build, ref List<Exception> exceptions);
		public void TestContextManager_OpenConnetion_FallbackToEmptyPassword()
		{
			connectionProviderMock.Setup(x => x.OpenNewSqlConnection<SysAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Throws(new Exception("Login Failed")).Verifiable(Times.Once);
			connectionProviderMock.Setup(x => x.OpenNewSqlConnection<OdysseyAdminCredentials>(It.IsAny<IProtectedDataService>(), It.IsAny<Action<SqlConnectionStringBuilder>>()))
				.Throws(new Exception("Login Failed")).Verifiable(Times.Once);

			using var dummyConnection = TestConnectionProvider.OpenNewConnection("master");
			var contextManagerMock = new Mock<CargowiseSetupSqlContextManager>(new object[] { Application.ServiceProvider.GetRequiredService<ISqlConnectionProvider>(), Application.ServiceProvider.GetRequiredService<IProtectedDataServiceFactory>() });
			var exceptions = new List<Exception>();
			var firstCall = true;
			var customConnectionString = string.Empty;

			contextManagerMock.Setup(x => x.AttemptOpenCustomConnection(It.IsAny<string>(), It.IsAny<Action<SqlConnectionStringBuilder>>(), ref It.Ref<List<Exception>>.IsAny))
				.Callback(new customConnectionCallback((string server, Action<SqlConnectionStringBuilder> build, ref List<Exception> exceptions) =>
				{
					if (firstCall)
					{
						exceptions.Add(new Exception("Login Failed"));
						firstCall = false;
					}
					else
					{
#pragma warning disable CW1065 // Encrypt SQL Connection Rule
						var builder = new SqlConnectionStringBuilder();
#pragma warning restore CW1065 // Encrypt SQL Connection Rule
						build(builder);
						customConnectionString = builder.ConnectionString;
					}
				}));

			var message = AssertExceptionThrown<InvalidOperationException>(() => contextManagerMock.Object.OpenConnection(".")).Message;
			AssertContains("Unable to open a SysAdmin connection to the specified server.", message);
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(customConnectionString);
			AssertEquals(builder.UserID, "sa");
			AssertEquals(builder.Password, string.Empty);

			connectionProviderMock.VerifyAll();
			contextManagerMock.VerifyAll();
		}
	}
}
