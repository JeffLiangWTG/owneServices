using System;
using System.Diagnostics;
using System.Threading;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	class ServiceTaskInitializerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			hostedServiceConfigProviderMock = new Mock<IHostedServiceAttributeProvider>();
			serviceTaskFactoryMock = new Mock<IServiceTaskHandlerFactory>();
			runnerLoggerMock = new Mock<IRunnerLogger>();
			resourceManagementMock = new Mock<IResourceManagement>();
			serviceTaskHandlerInitializer = new ServiceTaskHandlerInitializer(hostedServiceConfigProviderMock.Object, serviceTaskFactoryMock.Object, resourceManagementMock.Object, runnerLoggerMock.Object, ApplicationLoggingTestHelper.MockLoggerFactory());
		}

		Mock<IHostedServiceAttributeProvider> hostedServiceConfigProviderMock;
		Mock<IRunnerLogger> runnerLoggerMock;
		Mock<IServiceTaskHandlerFactory> serviceTaskFactoryMock;
		Mock<IResourceManagement> resourceManagementMock;
		ServiceTaskHandlerInitializer serviceTaskHandlerInitializer;

		public class MiscellaneousTest : ServiceTaskInitializerTest
		{
			public void TestWrongParamsCall()
			{
				CombineAssertions(() =>
				{
					var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskHandlerInitializer(null, serviceTaskFactoryMock.Object, resourceManagementMock.Object, runnerLoggerMock.Object, Mock.Of<IApplicationLoggerFactory>()));
					AssertEquals("hostedServiceAttributeProvider", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskHandlerInitializer(hostedServiceConfigProviderMock.Object, null, resourceManagementMock.Object, runnerLoggerMock.Object, Mock.Of<IApplicationLoggerFactory>()));
					AssertEquals("serviceTaskHandlerFactory", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskHandlerInitializer(hostedServiceConfigProviderMock.Object, serviceTaskFactoryMock.Object, null, runnerLoggerMock.Object, Mock.Of<IApplicationLoggerFactory>()));
					AssertEquals("resourceManagement", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskHandlerInitializer(hostedServiceConfigProviderMock.Object, serviceTaskFactoryMock.Object, resourceManagementMock.Object, null, Mock.Of<IApplicationLoggerFactory>()));
					AssertEquals("runnerLogger", result.ParamName);

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ServiceTaskHandlerInitializer(hostedServiceConfigProviderMock.Object, serviceTaskFactoryMock.Object, resourceManagementMock.Object, runnerLoggerMock.Object, null));
					AssertEquals("loggerFactory", result.ParamName);
				});
			}
		}

		public class CreateServiceTaskTest : ServiceTaskInitializerTest
		{
			public void TestCreateAddsActivityForInitialize()
			{
				// Arrange
				var serviceTaskMock = new Mock<IServiceTaskHandler>();
				hostedServiceConfigProviderMock
					.Setup(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>()))
					.Returns(new Mock<IHostedServiceAttribute>().Object);
				serviceTaskFactoryMock
					.Setup(factory => factory.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>()))
					.Returns(serviceTaskMock.Object);

				Activity activity = null;
				using var listener = ApplicationLoggingTestHelper.ListenFor("ServiceManagerRunner", "ServiceTaskHandlerInitializer.Initialize", o => activity = o);

				// Act
				_ = serviceTaskHandlerInitializer.CreateServiceTaskHandler(string.Empty, string.Empty, string.Empty);

				// Assert
				AssertNotNull(activity);
				AssertCollectionContains(activity.Tags, o => o.Key == "ServiceTaskCode");
				AssertCollectionContains(activity.Tags, o => o.Key == "ServiceTaskAssembly");
			}

			public void TestCreatesAndReturnsTheSameServiceTaskWrappedInDisposable()
			{
				// Arrange
				using var listener = ApplicationLoggingTestHelper.Listen();

				var serviceTaskMock = new Mock<IServiceTaskHandler>();
				hostedServiceConfigProviderMock
					.Setup(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>()))
					.Returns(new Mock<IHostedServiceAttribute>().Object);
				serviceTaskFactoryMock
					.Setup(factory => factory.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>()))
					.Returns(serviceTaskMock.Object);

				// Act
				var handler = serviceTaskHandlerInitializer.CreateServiceTaskHandler(string.Empty, string.Empty, string.Empty);
				var cancellationToken = new CancellationToken();
				handler.Run(cancellationToken);

				// Assert
				AssertType(typeof(DisposableServiceTaskHandler), handler);
				serviceTaskMock.Verify(h => h.Run(cancellationToken), Times.Once);
				serviceTaskFactoryMock.Verify(factory => factory.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestCreatesTaskWithRightParams()
			{
				CombineAssertions(() =>
				{
					Test(string.Empty, string.Empty, string.Empty);
					Test("assembly", "code", "typeName");
				});

				void Test(string assemblyName, string code, string typeName)
				{
					// Arrange
					using var listener = ApplicationLoggingTestHelper.Listen();

					serviceTaskFactoryMock.Reset();
					hostedServiceConfigProviderMock.Reset();

					var serviceTaskMock = new Mock<IDisposableServiceTaskHandler>();
					var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
					hostedServiceConfigMock
						.SetupGet(config => config.TypeName)
						.Returns(typeName);
					hostedServiceConfigProviderMock
						.Setup(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>()))
						.Returns(hostedServiceConfigMock.Object);
					serviceTaskFactoryMock
						.Setup(factory => factory.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>()))
						.Returns(serviceTaskMock.Object);

					// Act
					_ = serviceTaskHandlerInitializer.CreateServiceTaskHandler(assemblyName, code);

					// Assert
					serviceTaskFactoryMock.Verify(factory => factory.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
					serviceTaskFactoryMock.Verify(factory => factory.CreateServiceTaskHandler(assemblyName, typeName), Times.Once);
					hostedServiceConfigMock.VerifyGet(config => config.TypeName, Times.Once);
					hostedServiceConfigProviderMock.Verify(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
					hostedServiceConfigProviderMock.Verify(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), code), Times.Once);
				}
			}

			[ExpectNoExceptions]
			public void TestCallsInitializeRunningEnvironmentWithRightParams()
			{
				CombineAssertions(() =>
				{
					Test(string.Empty);
					Test("attribute string");
				});

				void Test(string configString)
				{
					// Arrange
					using var listener = ApplicationLoggingTestHelper.Listen();

					serviceTaskFactoryMock.Reset();
					hostedServiceConfigProviderMock.Reset();

					var serviceTaskMock = new Mock<IDisposableServiceTaskHandler>();
					var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
					hostedServiceConfigProviderMock
						.Setup(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>()))
						.Returns(hostedServiceConfigMock.Object);
					serviceTaskFactoryMock
						.Setup(factory => factory.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>()))
						.Returns(serviceTaskMock.Object);

					// Act
					_ = serviceTaskHandlerInitializer.CreateServiceTaskHandler(string.Empty, string.Empty, configString);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						serviceTaskMock.Verify(task => task.InitializeRunningEnvironment(It.IsAny<IHostedServiceAttribute>(), It.IsAny<string>(), It.IsAny<ILogger>()), Times.Once);
						serviceTaskMock.Verify(task => task.InitializeRunningEnvironment(
								hostedServiceConfigMock.Object,
								configString,
								runnerLoggerMock.Object),
							Times.Once);
					});
				}
			}
		}
	}
}
