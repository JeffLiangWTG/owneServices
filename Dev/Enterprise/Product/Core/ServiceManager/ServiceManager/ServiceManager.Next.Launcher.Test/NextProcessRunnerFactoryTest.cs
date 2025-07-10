using CargoWise.ServiceManager.Next.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Launcher.Test;

class NextProcessRunnerFactoryTest
{
	[Test]
	public void Constructor_DoesNotCallServiceProvider()
	{
		var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
		var processRunnerPool = new NextProcessRunnerFactory(serviceProviderMock.Object);
		Assert.That(processRunnerPool, Is.Not.Null);
		serviceProviderMock.VerifyNoOtherCalls();
	}

	[Test]
	public void Create_CreateNextProcessRunner([Values] bool callOnce)
	{
		var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
		var processFactoryMock = new Mock<IProcessFactory>(MockBehavior.Strict);
		serviceProviderMock
			.Setup(m => m.GetService(typeof(IServiceProviderIsService)))
			.Returns(null!);
		serviceProviderMock
			.Setup(m => m.GetService(typeof(ILogger<NextProcessRunner>)))
			.Returns(new NullLogger<NextProcessRunner>());
		serviceProviderMock
			.Setup(m => m.GetService(typeof(IProcessFactory)))
			.Returns(processFactoryMock.Object);
		var nextRunnerOptionsMock = new Mock<INextRunnerOptions>();

		var processRunnerPool = new NextProcessRunnerFactory(serviceProviderMock.Object);
		using var processRunner = processRunnerPool.Create("token", nextRunnerOptionsMock.Object);
		Assert.Multiple(() =>
		{
			Assert.That(processRunner, Is.TypeOf<NextProcessRunner>());
			Assert.That(processRunner.RunnerCode, Is.EqualTo("token"));

			serviceProviderMock.Verify(m => m.GetService(typeof(IServiceProviderIsService)), Times.Once);
			serviceProviderMock.Verify(m => m.GetService(typeof(ILogger<NextProcessRunner>)), Times.Once);
			serviceProviderMock.Verify(m => m.GetService(typeof(IProcessFactory)), Times.Once);
			serviceProviderMock.VerifyNoOtherCalls();
			processFactoryMock.VerifyNoOtherCalls();
			nextRunnerOptionsMock.VerifyNoOtherCalls();
		});

		if (callOnce)
		{
			return;
		}

		serviceProviderMock.Invocations.Clear();
		using var otherProcessRunner = processRunnerPool.Create("token", nextRunnerOptionsMock.Object);
		Assert.Multiple(() =>
		{
			Assert.That(otherProcessRunner, Is.TypeOf<NextProcessRunner>());
			Assert.That(otherProcessRunner.RunnerCode, Is.EqualTo("token"));
			Assert.That(otherProcessRunner, Is.Not.SameAs(processRunner));

			serviceProviderMock.Verify(m => m.GetService(typeof(IServiceProviderIsService)), Times.Once);
			serviceProviderMock.Verify(m => m.GetService(typeof(ILogger<NextProcessRunner>)), Times.Once);
			serviceProviderMock.Verify(m => m.GetService(typeof(IProcessFactory)), Times.Once);
			serviceProviderMock.VerifyNoOtherCalls();
			processFactoryMock.VerifyNoOtherCalls();
			nextRunnerOptionsMock.VerifyNoOtherCalls();
		});
	}
}
