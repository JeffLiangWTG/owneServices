using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
#if NETFRAMEWORK
using System.Net;
#elif NET
using System.IO;
using System.Net.Sockets;
#endif
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	public class HttpListenerTaskIntegrationTest : TestCase
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Http Listener requires Administrative Privileges")]
		public void TestHttpListenerTaskDisposeGracefullyWithMemoryCacheDuringLongRunningGetQueueStatusQuery()
		{
			// Arrange
			using var signalToCancel = new ManualResetEventSlim();

			using var cancellationTokenProvider = new CancellationTokenSourceWrapper();
			var queuesProviderMock = new Mock<IHostedServiceQueuesProvider>();
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();

			var services = new ServiceCollection()
				.AddRegistrations(new[] { Db.ServerName, Db.DatabaseName })
				.RemoveAll<IServiceManagerTask>()
				.RemoveAll<IQueueStatusProviderFactory>()
				.AddTransient(provider =>
				{
					var queueStatusProviderFactory = provider.GetRequiredService<IQueueStatusProviderFactory>();
					var requestQueue = provider.GetRequiredService<IRequestQueueConsumable>();
					var hostServiceStatusProvider = provider.GetRequiredService<IHostServiceStatusProvider>();
					var jsonConverter = provider.GetRequiredService<IJsonConverter>();

					return CreateMockRequestQueueProcessor(queueStatusProviderFactory, requestQueue, hostServiceStatusProvider, jsonConverter);
				})
				.AddTransient(provider =>
				{
					var memoryCache = provider.GetRequiredService<IMemoryCache>();

					queuesProviderMock
						.Setup(m => m.Queues)
						.Returns(CreateMockQueues(memoryCache, signalToCancel));

					var queueStatusProvider = new QueueStatusProvider(Mock.Of<ITaskStatusProvider>(), memoryCache, ObjectFactory.Get<IApplicationSchemaResolver>(), cancellationTokenProvider, errorReporterProxyMock.Object, queuesProviderMock.Object);

					var queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
					queueStatusProviderFactoryMock
						.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
						.Returns(queueStatusProvider);

					return queueStatusProviderFactoryMock.Object;
				})
				.AddTransient<IServiceManagerTask>(provider =>
				{
					var requestQueue = provider.GetRequiredService<IRequestQueueProduceable>();
					return new HttpListenerTask(Mock.Of<IHostLogger>(), new HttpListenerWrapperFactory(), requestQueue, new HttpListenerExceptionHandler(), cancellationTokenProvider, errorReporterProxyMock.Object);
				});

			// Act
			var httpListenerTaskRun = Task.Run(() =>
			{
				using var provider = services.BuildServiceProvider();
				using (ObjectFactory.Substitute(queuesProviderMock.Object))
				{
					var serviceManagerApplication = provider.GetRequiredService<IServiceManagerApplication>();
					serviceManagerApplication.Run(cancellationTokenProvider.Token);
				}
			});

			// Act
			var httpRequestTaskRun = Task.Run(async () =>
			{
				using var client = new HttpClient();
				var url = ServiceManagerHelper.GetQueueStatusUri(null);
				var shouldRetry = false;

				do
				{
					try
					{
						await client.GetAsync(url);
						shouldRetry = false;
					}
#if NETFRAMEWORK
					catch (HttpRequestException e) when (e.InnerException is WebException { Status: WebExceptionStatus.ConnectFailure })
#elif NET
					catch (HttpRequestException e) when (e.InnerException is SocketException || e.InnerException is IOException)
#endif
					{
						if (!signalToCancel.IsSet)
						{
							shouldRetry = true; // Handle race condition for Http Listener not yet to startup.
							await Task.Delay(TimeSpan.FromMilliseconds(500));
						}
						else
						{
							shouldRetry = false; // This is when the socket hangup because HttpListenerTask is disposed prior pending tasks are completed.
						}
					}
				} while (shouldRetry);
			});

			// Act
			var cancellationTask = Task.Run(() =>
			{
				signalToCancel.Wait(); // Waiting for a request in the middle of the long-running query
				cancellationTokenProvider.Cancel();
			});

			cancellationTask.Wait();
			Task.WaitAll(httpListenerTaskRun, httpRequestTaskRun);
			AsyncHelper.WaitAllActiveTasksForTest(); // Wait for uncontrolled disposal background tasks to complete. This giving a chance to throw lost reference background tasks error if any.
		}

		IServiceManagerTask CreateMockRequestQueueProcessor(IQueueStatusProviderFactory queueStatusProviderFactory, IRequestQueueConsumable requestQueue, IHostServiceStatusProvider hostServiceStatusProvider, IJsonConverter jsonConverter)
		{
			var responseStringBuilder = new ResponseStringBuilder(Db.ServerName, Db.DatabaseName, Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), queueStatusProviderFactory, hostServiceStatusProvider, jsonConverter, Mock.Of<IServiceHostRequestProvider>());
			var requestProcessor = new RequestProcessor(responseStringBuilder, Mock.Of<IHostLogger>(), Mock.Of<IActionQueue>());
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor);

			var requestQueueProcessor = new RequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 20));
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

			return requestQueueProcessor;
		}

		IEnumerable<IHostedServiceQueue> CreateMockQueues(IMemoryCache memoryCache, ManualResetEventSlim signalDuringLongRunningQueueQuery)
		{
			return Enumerable.Range(1, 100).Select(queueIndex =>
			{
				var queueMock = new Mock<IHostedServiceQueue>();
				queueMock.Setup(q => q.Name).Returns($"Test Queue Name {queueIndex}");
				queueMock.Setup(q => q.ServiceTaskCode).Returns($"Test ServiceTaskCode {queueIndex}");
				queueMock
					.Setup(q => q.QueueResult)
					.Returns(() => new QueueResult(GetQueueResult(queueIndex), TimeSpan.Zero));

				return queueMock.Object;
			});

			int GetQueueResult(int queueIndex)
			{
				if (queueIndex == 1)
				{
					signalDuringLongRunningQueueQuery.Set();
				}

				// Mocking a long-running query and making use of MemoryCache should not throw a ObjectDisposedException
				memoryCache.AddOrGetExisting("Dummy Test Key", () => 1, TimeSpan.FromMinutes(1));
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				return 1;
			}
		}
	}
}
