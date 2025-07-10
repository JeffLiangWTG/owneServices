using System;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host.Core.Http
{
	class RequestProcessorFactoryTest : TestCase
	{
		public void TestWrongParamsCall()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RequestProcessorFactory(null, CreateMockQueueStatusProviderFactory(), Mock.Of<IHostServiceStatusProvider>(), Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RequestProcessorFactory(Mock.Of<IHostLogger>(), null, Mock.Of<IHostServiceStatusProvider>(), Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("queueStatusProviderFactory"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RequestProcessorFactory(Mock.Of<IHostLogger>(), CreateMockQueueStatusProviderFactory(), null, Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostServiceStatusProvider"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RequestProcessorFactory(Mock.Of<IHostLogger>(), CreateMockQueueStatusProviderFactory(), Mock.Of<IHostServiceStatusProvider>(), null, Mock.Of<IServiceHostRequestProvider>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("jsonConverter"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RequestProcessorFactory(Mock.Of<IHostLogger>(), CreateMockQueueStatusProviderFactory(), Mock.Of<IHostServiceStatusProvider>(), Mock.Of<IJsonConverter>(), null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceHostRequestProvider"));

				var factory = new RequestProcessorFactory(Mock.Of<IHostLogger>(), CreateMockQueueStatusProviderFactory(), Mock.Of<IHostServiceStatusProvider>(), Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>());

				result = AssertExceptionThrown<ArgumentNullException>(() => factory.CreateRequestProcessor(null, Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("actionQueue"));

				result = AssertExceptionThrown<ArgumentNullException>(() => factory.CreateRequestProcessor(Mock.Of<IActionQueue>(), null, Mock.Of<ITaskStatusProvider>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("scheduler"));

				result = AssertExceptionThrown<ArgumentNullException>(() => factory.CreateRequestProcessor(Mock.Of<IActionQueue>(), Mock.Of<ITaskScheduler>(), null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("statusProvider"));
			});
		}

		[ExpectNoExceptions]
		public void TestFactoryReturnsValue()
		{
			var f = new RequestProcessorFactory(Mock.Of<IHostLogger>(), CreateMockQueueStatusProviderFactory(), Mock.Of<IHostServiceStatusProvider>(), Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>());
			var result = f.CreateRequestProcessor(Mock.Of<IActionQueue>(), Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>());
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(IRequestProcessor)));
		}

		static IQueueStatusProviderFactory CreateMockQueueStatusProviderFactory()
		{
			var queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(Mock.Of<IQueueStatusProvider>());

			return queueStatusProviderFactoryMock.Object;
		}
	}
}
