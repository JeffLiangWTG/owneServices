using System;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace ServiceManager.NUnit4.Test.Runner;

class ServiceTaskRunnerWithNextRunTimeCheckFactoryTest
{
	[Test]
	public void TestWrongConstructorParamsCall()
	{
		var exception = Assert.Throws<ArgumentNullException>(() => new ServiceTaskRunnerWithNextRunTimeCheckFactory(null));
		Assert.That(exception?.ParamName, Is.EqualTo("serviceProvider"));
	}

	[Test]
	public void TestCreateNativeServiceTaskRunnerWithNextRunTimeCheck()
	{
		// Arrange
		using var mock1 = ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>());
		using var mock2 = ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>());
		using var mock3 = ObjectFactory.Substitute(Mock.Of<IServiceTaskScheduleStatusProvider>());
		using var serviceProvider = CreateServices().BuildServiceProvider();

		var factory = new ServiceTaskRunnerWithNextRunTimeCheckFactory(serviceProvider);

		// Act
		var runner = factory.CreateRunner();

		// Assert
		Assert.That(runner, Is.Not.Null);
		Assert.That(runner, Is.InstanceOf<NativeServiceTaskRunnerWithNextRunTimeCheck>());
	}

	IServiceCollection CreateServices() =>
		CompositionRoot
			.AddRegistrations(new ServiceCollection(), ApplicationLoggingTestHelper.MockLoggerFactory())
			.AddSingleton(Mock.Of<IRunnerLogger>());
}

