using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Shared.Abstractions;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace Enterprise.ServiceManager.Host.Testing.Core.ServiceTask;

class HostServiceTaskLoaderFactoryTest
{
	[Test]
	public void TestWrongConstructorParamsCall()
	{
		var exception = Assert.Throws<ArgumentNullException>(() => new HostServiceTaskLoaderFactory(null));
		Assert.That(exception?.ParamName, Is.EqualTo("serviceProvider"));
	}

	[Test]
	public void TestCreateNativeServiceTaskLoaderWhenFlagIsTrue()
	{
		// Arrange
		using var mock1 = ObjectFactory.Substitute(Mock.Of<IClientHostedServiceAttributeProvider>());
		using var mock2 = ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>());
		using var mock3 = ObjectFactory.Substitute(Mock.Of<IServiceTaskScheduleStatusProvider>());
		using var serviceProvider = new ServiceCollection()
			.AddRegistrations([])
			.BuildServiceProvider();

		var factory = new HostServiceTaskLoaderFactory(serviceProvider);

		// Act
		var loader = factory.CreateServiceTaskLoader();

		// Assert
		Assert.That(loader, Is.Not.Null);
		Assert.That(loader, Is.InstanceOf<NativeServiceTaskLoader>());
	}
}
