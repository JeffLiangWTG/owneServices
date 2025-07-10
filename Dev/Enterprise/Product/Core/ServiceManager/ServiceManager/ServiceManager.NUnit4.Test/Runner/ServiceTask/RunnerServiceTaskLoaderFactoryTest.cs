using System;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Shared.Abstractions;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace CargoWise.ServiceManager.Runner.Test.ServiceTask;

class RunnerServiceTaskLoaderFactoryTest
{
	[Test]
	public void TestWrongConstructorParamsCall()
	{
		var exception = Assert.Throws<ArgumentNullException>(() => new RunnerServiceTaskLoaderFactory(null!));
		Assert.That(exception?.ParamName, Is.EqualTo("serviceProvider"));
	}

	[Test]
	public void TestCreateSchedulerServiceTaskLoader()
	{
		// Arrange
		using var mock1 = ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>());
		using var mock2 = ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>());
		using var mock3 = ObjectFactory.Substitute(Mock.Of<IServiceTaskScheduleStatusProvider>());
		using var serviceProvider = CreateServices().BuildServiceProvider();

		var factory = new RunnerServiceTaskLoaderFactory(serviceProvider);

		// Act
		var loader = factory.CreateServiceTaskLoader();

		// Assert
		Assert.That(loader, Is.Not.Null);
		Assert.That(loader, Is.InstanceOf<NativeServiceTaskLoader>());
	}

	IServiceCollection CreateServices() =>
		CompositionRoot
			.AddRegistrations(new ServiceCollection(), ApplicationLoggingTestHelper.MockLoggerFactory());
}
