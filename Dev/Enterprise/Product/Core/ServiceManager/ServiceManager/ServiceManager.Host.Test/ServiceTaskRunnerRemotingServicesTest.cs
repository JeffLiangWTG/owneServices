using System;
using System.Linq;
using CargoWise.Async;
using Enterprise.ServiceManager.Shared;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class ServiceTaskRunnerRemotingServicesTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCreate()
		{
			var exitWasClean = false;
			Server server = null;
			try
			{
				server = new Server(new[] { new ChannelOption("grpc.keepalive_permit_without_calls", 1), })
				{
					Services = { ServiceRunner.BindService(new GrpcServer((bool b) => exitWasClean = b)) },
					Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) },
				};
				server.Start();
				var port = server.Ports.Single().BoundPort;
				var remotingServices = new ProcessRunnerRemotingServices(Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>());
				var provider = remotingServices.CreateRunnerCommandQueueProxy(port);
				NUnit.Framework.Assert.That(provider, Is.Not.EqualTo(default(IRunnerCommandQueueProvider)));
				NUnit.Framework.Assert.That(provider.GetType(), Is.EqualTo(typeof(RunnerCommandQueueProvider)));
			}
			finally
			{
				server?.KillAsync().Wait(TimeSpan.FromSeconds(5));
				try
				{
					AsyncHelper.WaitAllActiveTasksForTest();
				}
				catch (Exception ex)
					when (ex is not TimeoutException)
				{
					// Ignore errors from shutdown of grpc, we're not doing it cleanly for a test
				}
			}
		}

		public void TestCreateWithNullPort()
		{
			var remotingServices = new ProcessRunnerRemotingServices(Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>());
			var result = AssertExceptionThrown<ArgumentNullException>(() => remotingServices.CreateRunnerCommandQueueProxy(null));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("grpcPort: Grpc Server not initialized"));
		}
	}
}
