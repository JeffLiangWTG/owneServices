using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Runner;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;
using WTG.StaticAnalysis.Annotation;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace ServiceManager.Runner.CW1.Test.ServiceProviderImpl
{
	public class ServiceTaskHandlerFactoryTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestLogEntriesExistWhenUseNETCoreIsEnabled()
		{
			//Arrange
			using (SystemDataRegistry.Instance.SwitchRunnerToNetCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var assembly = Assembly.GetExecutingAssembly();

				// Act
				try
				{
					_ = serviceTaskHandlerFactory.CreateServiceTaskHandler(assembly.GetName().ToString(), "type");
				}
				catch (Exception e)
				{
					Assert(e is InvalidOperationException);
				}

				// Assert
				serviceTaskLoggerMock.Verify(proxy => proxy.Log(LogLevel.Information, $"Assembly version is {assembly.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkDisplayName}"), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestNoExtraLogEntriesWhenUseNETCoreIsDisabled()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchRunnerToNetCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var assemblyName = Assembly.GetExecutingAssembly().GetName().ToString();

				//Act
				try
				{
					_ = serviceTaskHandlerFactory.CreateServiceTaskHandler(assemblyName, "type");
				}
				catch (Exception e)
				{
					Assert(e is InvalidOperationException);
				}

				// Assert
				serviceTaskLoggerMock.Verify(proxy => proxy.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Never);
			}
		}

		public void TestWrongConstructorParams()
		{
			var serviceTaskLogger = Mock.Of<IServiceTaskLogger>();
			var registry = Mock.Of<IRunnerRegistrySettings>();
			var taskLoaderFactory = Mock.Of<IServiceTaskLoaderFactory>();
			var loggerFactory = Mock.Of<ILoggerFactory>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>($"{nameof(serviceTaskLogger)} should check null.", () => new ServiceTaskHandlerFactory(null, registry, taskLoaderFactory, loggerFactory));
				AssertExceptionThrown<ArgumentNullException>($"{nameof(registry)} should check null.", () => new ServiceTaskHandlerFactory(serviceTaskLogger, null, taskLoaderFactory, loggerFactory));
				AssertExceptionThrown<ArgumentNullException>($"{nameof(taskLoaderFactory)} should check null.", () => new ServiceTaskHandlerFactory(serviceTaskLogger, registry, null, loggerFactory));
				AssertExceptionThrown<ArgumentNullException>($"{nameof(loggerFactory)} should check null.", () => new ServiceTaskHandlerFactory(serviceTaskLogger, registry, taskLoaderFactory, null));
			});
		}

		public void TestServiceTaskNotInAssemblyThrowsException()
		{
			var assemblyName = Assembly.GetExecutingAssembly().FullName.Split(',')[0];
			var serviceTaskType = "FileNotInAssembly";

			using (SystemDataRegistry.Instance.SwitchRunnerToNetCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				try
				{
					serviceTaskHandlerFactory.CreateServiceTaskHandler(assemblyName, serviceTaskType);
				}
				catch (InvalidOperationException e)
				{
					AssertEquals($"The service task ({serviceTaskType}) was not found in the assembly: {assemblyName}", e.Message);
				}
			}
		}

		public void TestServiceTaskInAssemblyDoesNotThrowException()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyName = assembly.FullName.Split(',')[0];
			var serviceTaskType = assembly.DefinedTypes.First(type => type.FullName.Contains(nameof(TestServiceProviderImpl))).FullName;

			using (SystemDataRegistry.Instance.SwitchRunnerToNetCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNoExceptionThrown(() => serviceTaskHandlerFactory.CreateServiceTaskHandler(assemblyName, serviceTaskType));
			}
		}

		public void TestServiceObjectNotOfServiceProviderImplTypeThrowsException()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyName = assembly.FullName.Split(',')[0];
			var serviceTaskType = assembly.DefinedTypes.First(type => type.FullName.Contains(nameof(ServiceTaskHandlerFactoryTest))).FullName;

			using (SystemDataRegistry.Instance.SwitchRunnerToNetCore.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				try
				{
					serviceTaskHandlerFactory.CreateServiceTaskHandler(assemblyName, serviceTaskType);
				}
				catch (InvalidOperationException e)
				{
					AssertEquals($"{serviceTaskType} is not of type ServiceProviderImpl.", e.Message);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			serviceTaskLoggerMock = new Mock<IServiceTaskLogger>();
			runnerRegistrySettings = new DirectRunnerRegistrySettings();
			taskLoaderFactoryMock = new Mock<IServiceTaskLoaderFactory>();
			loggerFactoryMock = new Mock<ILoggerFactory>();
			serviceTaskHandlerFactory = new ServiceTaskHandlerFactory(serviceTaskLoggerMock.Object, runnerRegistrySettings, taskLoaderFactoryMock.Object, loggerFactoryMock.Object);
		}

		[CodeAlive($"Used in {nameof(TestServiceTaskInAssemblyDoesNotThrowException)}")]
		class TestServiceProviderImpl : Integration.ServiceTasks.CW.ServiceProviderImpl
		{
			public override void RunTask(CancellationToken token)
			{
				throw new NotImplementedException();
			}
		}

		IRunnerRegistrySettings runnerRegistrySettings;
		Mock<IServiceTaskLogger> serviceTaskLoggerMock;
		Mock<IServiceTaskLoaderFactory> taskLoaderFactoryMock;
		Mock<ILoggerFactory> loggerFactoryMock;
		ServiceTaskHandlerFactory serviceTaskHandlerFactory;
	}
}
