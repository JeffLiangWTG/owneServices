using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.ServiceManager.Host.Testing.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class ServiceManagerHostTest : TestCase
	{
		public void TestWrongConstructorParams()
		{
			var exception = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerHost(null, Mock.Of<IEventLogger>(), serviceErrorHandler, Mock.Of<IHostStartupCommandResolver>(), Mock.Of<IDbConnectionSetup>(), Mock.Of<IHostRegistry>()));
			AssertEquals("hostOptions", exception.ParamName);

			exception = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerHost(hostOptions.Object, null, serviceErrorHandler, Mock.Of<IHostStartupCommandResolver>(), Mock.Of<IDbConnectionSetup>(), Mock.Of<IHostRegistry>()));
			AssertEquals("eventLogger", exception.ParamName);

			exception = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerHost(hostOptions.Object, Mock.Of<IEventLogger>(), null, Mock.Of<IHostStartupCommandResolver>(), Mock.Of<IDbConnectionSetup>(), Mock.Of<IHostRegistry>()));
			AssertEquals("exceptionReporter", exception.ParamName);

			exception = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerHost(hostOptions.Object, Mock.Of<IEventLogger>(), serviceErrorHandler, null, Mock.Of<IDbConnectionSetup>(), Mock.Of<IHostRegistry>()));
			AssertEquals("commandResolver", exception.ParamName);

			exception = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerHost(hostOptions.Object, Mock.Of<IEventLogger>(), serviceErrorHandler, Mock.Of<IHostStartupCommandResolver>(), null, Mock.Of<IHostRegistry>()));
			AssertEquals("dbConnectionSetup", exception.ParamName);

			exception = AssertExceptionThrown<ArgumentNullException>(() => new ServiceManagerHost(hostOptions.Object, Mock.Of<IEventLogger>(), serviceErrorHandler, Mock.Of<IHostStartupCommandResolver>(), Mock.Of<IDbConnectionSetup>(), null));
			AssertEquals("hostRegistry", exception.ParamName);
		}

		[ExpectNoExceptions]
		public void TestRunWithExceptionGetsLogged()
		{
			// Arrange
			hostStartupCommandResolver.Setup(o => o.Resolve()).Throws<Exception>();

			// Act
			var result = CreateServiceManagerHost().Run();

			// Assert
			eventLogger.Verify(o => o.Log(LogLevel.Error, It.IsAny<string>(), It.IsAny<Exception>()), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestServiceManagerHostCallsDbConnectionSetup()
		{
			AppDomainHelper.RunInAnotherAppDomain(
				nameof(TestServiceManagerHostCallsDbConnectionSetup),
				CallBackForTestControllerServiceCallsDbConnectionEventHandler);
		}

		public void TestServiceManagerHostCallsDbConnectionSetupEvenIfApplicationLoginsAreMissing()
		{
			var loginName = $"{Db.DatabaseName}_RestrictedWriterLogin";
			var sqlCommandToGetLoginName = $"SELECT TOP 1 name FROM sys.server_principals WHERE name = '{loginName}'";
			Db.Connection.Dispose();

			using (var adminConnection = Db.NewAdminConnection())
			{
				using (var command = adminConnection
					.Command($"IF EXISTS({sqlCommandToGetLoginName}) DROP LOGIN [{loginName}]"))
				{
					command.ExecuteNonQuery();
					try
					{
						command.CommandText = sqlCommandToGetLoginName;

						AssertEquals($"Login '{loginName}' should have been deleted.", null, command.ExecuteScalar());
						AppDomainHelper.RunInAnotherAppDomain(
							nameof(TestServiceManagerHostCallsDbConnectionSetupEvenIfApplicationLoginsAreMissing),
							CallBackForTestControllerServiceCallsDbConnectionEventHandler);
					}
					finally
					{
						// restore RestrictedWriterLogin so test terminates successfully
						((IDbLoginRepair)adminConnection).EnsureRestrictedWriterDbLogin();
					}
				}
			}
		}

		#region Implementation

		[SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending Migration")]   // WI00669071 - Do not use System.AppDomain.
		static void CallBackForTestControllerServiceCallsDbConnectionEventHandler()
		{
			// Arrange
			var hostStartupCommandResolverMock = new Mock<IHostStartupCommandResolver>();
			hostStartupCommandResolverMock.Setup(o => o.Resolve()).Returns(Mock.Of<IHostStartupCommand>());

			var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
			var databaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");

			var dbConnectionSetupErrorHandlerMock = new Mock<IDbConnectionSetup>();
			dbConnectionSetupErrorHandlerMock
				.Setup(h => h.TryConnectAndHandleErrors())
				.Callback(() =>
				{
					hostStartupCommandResolverMock.Verify(o => o.Resolve(), Times.Never());
				});

			var serviceManagerHost = new ServiceCollection()
				.AddRegistrations(new[] { serverName, databaseName, })
				.RemoveAll<IDbConnectionSetup>()
				.AddTransient(_ => dbConnectionSetupErrorHandlerMock.Object)
				.RemoveAll<IHostStartupCommandResolver>()
				.AddTransient(_ => hostStartupCommandResolverMock.Object)
				.BuildServiceProvider()
				.GetRequiredService<ServiceManagerHost>();

			// Act
			serviceManagerHost.Run();

			// Assert
			dbConnectionSetupErrorHandlerMock.Verify(h => h.TryConnectAndHandleErrors(), "Should be called before startup command for logging");
		}

		#endregion Implementation

		protected override void SetUp()
		{
			base.SetUp();

			hostOptions = new Mock<IServiceManagerHostOptions>();
			eventLogger = new Mock<IEventLogger>();
			hostStartupCommandResolver = new Mock<IHostStartupCommandResolver>();
			dbConnectionSetup = new Mock<IDbConnectionSetup>();
			hostRegistryMock = new Mock<IHostRegistry>();

			serviceErrorHandler = new HostServiceErrorReporter(Mock.Of<IHostLogger>(), Mock.Of<IExceptionHandler>());
		}

		ServiceManagerHost CreateServiceManagerHost()
		{
			return new ServiceManagerHost(hostOptions.Object, eventLogger.Object, serviceErrorHandler, hostStartupCommandResolver.Object, dbConnectionSetup.Object, hostRegistryMock.Object);
		}

		HostServiceErrorReporter serviceErrorHandler;
		Mock<IServiceManagerHostOptions> hostOptions;
		Mock<IEventLogger> eventLogger;
		Mock<IHostStartupCommandResolver> hostStartupCommandResolver;
		Mock<IDbConnectionSetup> dbConnectionSetup;
		Mock<IHostRegistry> hostRegistryMock;
	}
}
