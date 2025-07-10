using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	public class ServiceManagerApplicationTest : TestCase
	{
		[UseSnapshotProtection]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestStmServiceHostExitsAfterInitialization()
		{
			// Arrange
			string hostName = ServiceManagerHelper.GetHostName();
			var queryForHostName = new ZQuery(StmServiceHostSchema.SH_HostName, hostName);
			var factory = new BusinessObjectFactory();
			using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			var httpListenerInitialiser = new Mock<IHttpRequestProcessorInitialiser>();
			var exception = new ApplicationException("BreakDown");
			httpListenerInitialiser
				.Setup(x => x.ConfigureHttpRequestProcessor(It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>(), It.IsAny<IActionQueue>()))
				.Callback(() =>
				{
					cancellationTokenSource.Cancel();
					throw exception;
				});
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

			var services = new ServiceCollection()
				.AddRegistrations(new[] { Db.ServerName, Db.DatabaseName })
				.AddTransient<Controller>()
				.RemoveAll<IServiceManagerTask>()
				.AddTransient(_ => httpListenerInitialiser.Object)
				.AddTransient<IServiceManagerTask>(provider => provider.GetRequiredService<Controller>())
				.AddSingleton(errorReporterProxyMock.Object);

			using (var provider = services.BuildServiceProvider())
			using (ClearUserContext())
			{
				var serviceManagerApplication = provider.GetRequiredService<IServiceManagerApplication>();

				// Action
				serviceManagerApplication.Run(cancellationTokenSource.Token);

				// Assert
				errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.IsAny<string>(), exception), Times.Once);
				var host = factory.Load<StmServiceHost>(queryForHostName).Single();
				CombineAssertions(() =>
				{
					Assert(host.SH_IsActive);
					AssertEquals(hostName, host.SH_HostName);
				});
			}

			IDisposable ClearUserContext()
			{
				var userContext = EnvProxy.Instance.CurrentUserContext;
				EnvProxy.Instance.ClearUserContext();

				return new DisposableAction(() =>
				{
					EnvProxy.Instance.SetUserContext(userContext);
				});
			}
		}
	}
}
