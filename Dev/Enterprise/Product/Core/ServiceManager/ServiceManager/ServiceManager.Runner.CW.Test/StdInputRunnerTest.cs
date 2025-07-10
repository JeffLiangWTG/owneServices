using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared;
using Enterprise.ServiceManager.Shared.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace CargoWise.ServiceManager.Runner.Test
{
	public class StdInputRunnerTest : TestCase
	{
		TextWriter originalConsoleWriter;
		TextReader originalConsoleReader;
		IDisposable listener;

		protected override void SetUp()
		{
			base.SetUp();
			listener = ApplicationLoggingTestHelper.Listen();
			originalConsoleWriter = Console.Out;
			originalConsoleReader = Console.In;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Console.SetOut(originalConsoleWriter);
			Console.SetIn(originalConsoleReader);
			listener.Dispose();
		}

		[UseSnapshotProtection]
		public void TestCommandWithRun()
		{
			var commands = new[] { "run TS1", "exit" };
			var expectedOutputs = new[] { "Initialising", "Running", "Complete" };
			AssertTestCommands(commands, expectedOutputs);
		}

		[UseSnapshotProtection]
		public void TestCommandWithRun_IrregularInput()
		{
			var commands = new[] { "rUn TS1", "exit" };
			var expectedOutputs = new[] { "Initialising", "Running", "Complete" };
			AssertTestCommands(commands, expectedOutputs);
		}

		[UseSnapshotProtection]
		public void TestCommandWithoutRun()
		{
			string[] commands = { "TS1", "stop" };
			string[] expectedOutputs = { "Initialising", "Running", "Complete" };
			AssertTestCommands(commands, expectedOutputs);
		}

		[UseSnapshotProtection]
		public void TestCommandWithOptions()
		{
			string[] commands = { "run -code:TS1", "stop" };
			string[] expectedOutputs = { "Initialising", "Running", "Complete" };
			AssertTestCommands(commands, expectedOutputs);
		}

		public void TestCommandWithInvalidCode()
		{
			string[] commands = { "run -code:!@$", "stop" };
			string[] expectedOutputs = { "Cannot determine assembly name from code '!@$'" };
			AssertTestCommands(commands, expectedOutputs);
		}

		[UseSnapshotProtection]
		public void TestHostedServiceAttributeCodeRuns()
		{
			var commands = new[] { "run -assemblyName:ZClientKNA.ServiceTasks -code:ZK1", "exit" };
			var expectedOutputs = new[] { "Initialising", "Running", "Complete" };
			AssertTestCommands(commands, expectedOutputs);

			commands = new[] { "run -assemblyName:ZClientSWT.ServiceTasks -code:ZS1", "exit" };
			AssertTestCommands(commands, expectedOutputs);
		}

		[UseSnapshotProtection]
		public void TestClientHostedServiceAttributeCodeRuns()
		{
			var commands = new[] { "TS1", "exit" };
			var expectedOutputs = new[] { "Initialising", "Running", "Complete" };
			AssertTestCommands(commands, expectedOutputs);

			commands = new[] { "run -code:TS1", "exit" };
			AssertTestCommands(commands, expectedOutputs);
		}

		void AssertTestCommands(string[] commands, string[] expectedOutputs)
		{
			using (var stringreader = new StringReader(string.Join(Environment.NewLine, commands)))
			using (var stringWriter = new StringWriter())
			{
				Console.SetIn(stringreader);
				Console.SetOut(stringWriter);

				var configProvider = new HostedServiceAttributeProvider();
				var configClientProvider = ObjectFactory.Get<IClientHostedServiceAttributeProvider>();
				var serviceTaskHandlerFactory = new Mock<IServiceTaskHandlerFactory>();
				serviceTaskHandlerFactory
					.Setup(f => f.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>()))
					.Returns(Mock.Of<IDisposableServiceTaskHandler>());
				var initializer = new ServiceTaskHandlerInitializer(configProvider, serviceTaskHandlerFactory.Object, Mock.Of<IResourceManagement>(), Mock.Of<IRunnerLogger>(), ApplicationLoggingTestHelper.MockLoggerFactory());

				var serviceTaskLoaderFactoryMock = new Mock<IServiceTaskLoaderFactory>();
				serviceTaskLoaderFactoryMock
					.Setup(x => x.CreateServiceTaskLoader())
					.Returns(
						new NativeServiceTaskLoader(
							configProvider,
							Mock.Of<IServiceTaskScheduleStatusProvider>(),
							Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(),
							Mock.Of<IDateTimeProvider>()));

				var transactionAdapterFactoryMock = new Mock<ITransactionAdapterFactory>();
				using var nativeServiceTaskTransactionAdapter = new NativeServiceTaskTransactionAdapter(
								configProvider,
								Mock.Of<IServiceTaskScheduleStatusProvider>(),
								Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(),
								Mock.Of<IDateTimeProvider>());
				transactionAdapterFactoryMock
					.Setup(x => x.CreateTransactionAdapter())
					.Returns(nativeServiceTaskTransactionAdapter);

				var scheduleManager = new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider()), new ServiceTaskRequirementsChecker());
				var runner = new StdInputRunner(
				new Mock<IRunnerLogger>().Object, initializer, scheduleManager, configProvider, configClientProvider);
				var applicationExceptionHandler =
					new ApplicationExceptionHandler(new Mock<IErrorReporterProxy>().Object);
				runner.Run(false, applicationExceptionHandler);

				var outputs = stringWriter.ToString();

				foreach (var expectedOutput in expectedOutputs)
				{
					AssertContains(expectedOutput, outputs);
				}
			}
		}

		public void TestRunCommandWithPreExistingSchedules()
		{
			// Arrange
			var commands = new[] { "run TS1", "exit" };
			var expectedOutputs = new[] { "Initialising", "Running", "Complete" };
			using (var stringreader = new StringReader(string.Join(Environment.NewLine, commands)))
			using (var stringWriter = new StringWriter())
			{
				Console.SetIn(stringreader);
				Console.SetOut(stringWriter);

				var configProvider = new HostedServiceAttributeProvider();
				var configClientProvider = ObjectFactory.Get<IClientHostedServiceAttributeProvider>();
				var initializerMock = new Mock<IServiceTaskHandlerInitializer>();
				initializerMock
					.Setup(i => i.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(Mock.Of<IDisposableServiceTaskHandler>());
				var scheduleManagerMock = new Mock<IServiceTaskScheduleManager>();
				scheduleManagerMock
					.Setup(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
					.Returns(new[] { Mock.Of<IServiceTask>(t => t.Code == "TS1"), Mock.Of<IServiceTask>(t => t.Code == "TS2") });
				var runner = new StdInputRunner(
					new Mock<IRunnerLogger>().Object, initializerMock.Object, scheduleManagerMock.Object, configProvider, configClientProvider);
				var applicationExceptionHandler =
					new ApplicationExceptionHandler(new Mock<IErrorReporterProxy>().Object);

				// Act
				runner.Run(false, applicationExceptionHandler);

				// Assert
				var outputs = stringWriter.ToString();
				foreach (var expectedOutput in expectedOutputs)
				{
					AssertContains(expectedOutput, outputs);
				}
			}
		}

		public class DatabaseAccessTest : TestCase
		{
			TextWriter originalConsoleWriter;
			TextReader originalConsoleReader;

			protected override void SetUp()
			{
				base.SetUp();
				originalConsoleWriter = Console.Out;
				originalConsoleReader = Console.In;
			}

			protected override void TearDown()
			{
				base.TearDown();
				Console.SetOut(originalConsoleWriter);
				Console.SetIn(originalConsoleReader);
			}

			public void TestDbAccessFromRunInternalMustFailAsShouldBeProvidedByTaskRunner()
			{
				// Arrange
				var commands = new[] { "run TS1", "exit" };
				var hostedServiceConfigProviderMock = new Mock<IHostedServiceAttributeProvider>(MockBehavior.Strict);
				var clientHostedServiceConfigProviderMock = new Mock<IClientHostedServiceAttributeProvider>(MockBehavior.Strict);
				var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
				hostedServiceConfigMock
					.SetupGet(config => config.TypeAssemblyName)
					.Returns("assembly");

				clientHostedServiceConfigProviderMock
					.Setup(provider => provider.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(() =>
					{
						using (var connection = Db.Connection)
						{
							connection.ExecuteScalar("SELECT @@servername");
						}
						return hostedServiceConfigMock.Object;
					});
				var runnerExposed = new StdInputRunnerExposed(new Mock<IRunnerLogger>(MockBehavior.Strict).Object, new Mock<IServiceTaskHandlerInitializer>(MockBehavior.Strict).Object, new Mock<IServiceTaskScheduleManager>(MockBehavior.Strict).Object, hostedServiceConfigProviderMock.Object, clientHostedServiceConfigProviderMock.Object);

				var errorReporterMock = new Mock<IErrorReporter>();

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				using (var stringreader = new StringReader(string.Join(Environment.NewLine, commands)))
				using (var stringWriter = new StringWriter())
				{
					Console.SetIn(stringreader);
					Console.SetOut(stringWriter);

					// Act
					var thread = new Thread(() => runnerExposed.RunInternalExposed(true));
					thread.Start();
					thread.Join();
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					clientHostedServiceConfigProviderMock.Verify(provider => provider.GetClientHostedServiceAttribute(It.IsAny<string>()), Times.Exactly(1));
					hostedServiceConfigProviderMock.Verify(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
					errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
					errorReporterMock.Verify(reporter => reporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction, It.IsAny<InvalidOperationException>()), Times.Once);
				});
			}

			public void TestDbAccessFromRunIsAllowed()
			{
				// Arrange
				var commands = new[] { "run TS1", "exit" };
				var hostedServiceConfigProviderMock = new Mock<IHostedServiceAttributeProvider>(MockBehavior.Strict);
				var clientHostedServiceConfigProviderMock = new Mock<IClientHostedServiceAttributeProvider>(MockBehavior.Strict);
				var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
				hostedServiceConfigMock
					.SetupGet(config => config.TypeAssemblyName)
					.Returns("assembly");

				clientHostedServiceConfigProviderMock
					.Setup(provider => provider.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(() =>
					{
						Db.Connection.ExecuteScalar("SELECT @@servername");
						return hostedServiceConfigMock.Object;
					});
				var runner = new StdInputRunner(new Mock<IRunnerLogger>(MockBehavior.Strict).Object, new Mock<IServiceTaskHandlerInitializer>(MockBehavior.Strict).Object, new Mock<IServiceTaskScheduleManager>(MockBehavior.Strict).Object, hostedServiceConfigProviderMock.Object, clientHostedServiceConfigProviderMock.Object);

				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				var applicationExceptionHandler =
					new ApplicationExceptionHandler(errorReporterProxyMock.Object);

				using (var stringreader = new StringReader(string.Join(Environment.NewLine, commands)))
				using (var stringWriter = new StringWriter())
				{
					Console.SetIn(stringreader);
					Console.SetOut(stringWriter);

					// Act
					runner.Run(true, applicationExceptionHandler);
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					clientHostedServiceConfigProviderMock.Verify(provider => provider.GetClientHostedServiceAttribute(It.IsAny<string>()), Times.Exactly(1));
					hostedServiceConfigProviderMock.Verify(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
					errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				});
			}

			public void TestGetConfigStringFromSchedule()
			{
				// Arrange
				var commands = new[] { "run UPG", "exit" };
				var mockRepo = new MockRepository(MockBehavior.Strict);
				var hostedServiceConfigProviderMock = mockRepo.Create<IHostedServiceAttributeProvider>();
				var clientHostedServiceConfigProviderMock = mockRepo.Create<IClientHostedServiceAttributeProvider>();
				var hostedServiceConfigMock = mockRepo.Create<IHostedServiceAttribute>();
				var serviceTaskMock = mockRepo.Create<IServiceTask>();
				serviceTaskMock
					.Setup(s => s.Code)
					.Returns("UPG");
				var serviceTaskScheduleInitializerMock = mockRepo.Create<IServiceTaskScheduleManager>();
				var serviceTaskHandlerMock = mockRepo.Create<IDisposableServiceTaskHandler>();
				var serviceTaskInitializerMock = mockRepo.Create<IServiceTaskHandlerInitializer>();

				hostedServiceConfigMock.SetupGet(config => config.TypeAssemblyName).Returns("assembly");
				hostedServiceConfigMock.SetupGet(x => x.Type).Returns(typeof(Mock<IServiceTaskHandler>));
				clientHostedServiceConfigProviderMock.Setup(provider => provider.GetClientHostedServiceAttribute(It.IsAny<string>())).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigProviderMock.Setup(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>())).Returns(hostedServiceConfigMock.Object);
				serviceTaskMock.SetupGet(x => x.ConfigString).Returns("My_configString_from_schedule").Verifiable("ConfigString was not read from the configured task schedule.");
				serviceTaskScheduleInitializerMock.Setup(x => x.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>())).Returns(new [] { serviceTaskMock.Object });
				serviceTaskInitializerMock.Setup(x => x.CreateServiceTaskHandler("assembly", "UPG", "My_configString_from_schedule")).Returns(serviceTaskHandlerMock.Object).Verifiable("CreateServiceTask was not called, or the correct arguments was not provided.");
				serviceTaskHandlerMock.Setup(x => x.Run(It.IsAny<CancellationToken>())).Verifiable("Service task did not run.");
				var runner = new StdInputRunner(new Mock<IRunnerLogger>(MockBehavior.Strict).Object, serviceTaskInitializerMock.Object, serviceTaskScheduleInitializerMock.Object, hostedServiceConfigProviderMock.Object, clientHostedServiceConfigProviderMock.Object);
				var applicationExceptionHandler =
					new ApplicationExceptionHandler(new Mock<IErrorReporterProxy>().Object);
				using (var stringreader = new StringReader(string.Join(Environment.NewLine, commands)))
				using (var stringWriter = new StringWriter())
				{
					Console.SetIn(stringreader);
					Console.SetOut(stringWriter);

					// Act
					runner.Run(true, applicationExceptionHandler);
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					mockRepo.VerifyAll();
				});
			}

			public void TestGetConfigStringFromSchedule_OverrideInCommandLine()
			{
				// Arrange
				var commands = new[] { "run UPG -configString:Y,Y,Overrided_Config_String", "exit" };
				var mockRepo = new MockRepository(MockBehavior.Strict);
				var hostedServiceConfigProviderMock = mockRepo.Create<IHostedServiceAttributeProvider>();
				var clientHostedServiceConfigProviderMock = mockRepo.Create<IClientHostedServiceAttributeProvider>();
				var hostedServiceConfigMock = mockRepo.Create<IHostedServiceAttribute>();
				var serviceTaskMock = mockRepo.Create<IServiceTask>();
				serviceTaskMock
					.Setup(s => s.Code)
					.Returns("UPG");
				var serviceTaskScheduleInitializerMock = mockRepo.Create<IServiceTaskScheduleManager>();
				var serviceTaskHandlerMock = mockRepo.Create<IDisposableServiceTaskHandler>();
				var serviceTaskInitializerMock = mockRepo.Create<IServiceTaskHandlerInitializer>();

				hostedServiceConfigMock.SetupGet(config => config.TypeAssemblyName).Returns("assembly");
				hostedServiceConfigMock.SetupGet(x => x.Type).Returns(typeof(Mock<IServiceTaskHandler>));
				clientHostedServiceConfigProviderMock.Setup(provider => provider.GetClientHostedServiceAttribute(It.IsAny<string>())).Returns(hostedServiceConfigMock.Object);
				hostedServiceConfigProviderMock.Setup(provider => provider.GetHostedServiceAttribute(It.IsAny<string>(), It.IsAny<string>())).Returns(hostedServiceConfigMock.Object);
				serviceTaskScheduleInitializerMock.Setup(x => x.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>())).Returns(new[] { serviceTaskMock.Object });
				serviceTaskInitializerMock.Setup(x => x.CreateServiceTaskHandler("assembly", "UPG", "Y,Y,Overrided_Config_String")).Returns(serviceTaskHandlerMock.Object).Verifiable("CreateServiceTask was not called, or the correct arguments was not provided.");
				serviceTaskHandlerMock.Setup(x => x.Run(It.IsAny<CancellationToken>())).Verifiable("Service task did not run.");
				var runner = new StdInputRunner(new Mock<IRunnerLogger>(MockBehavior.Strict).Object, serviceTaskInitializerMock.Object, serviceTaskScheduleInitializerMock.Object, hostedServiceConfigProviderMock.Object, clientHostedServiceConfigProviderMock.Object);
				var applicationExceptionHandler =
					new ApplicationExceptionHandler(new Mock<IErrorReporterProxy>().Object);
				using (var stringreader = new StringReader(string.Join(Environment.NewLine, commands)))
				using (var stringWriter = new StringWriter())
				{
					Console.SetIn(stringreader);
					Console.SetOut(stringWriter);

					// Act
					runner.Run(true, applicationExceptionHandler);
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					mockRepo.VerifyAll();
				});
			}

			class StdInputRunnerExposed : StdInputRunner
			{
				public StdInputRunnerExposed(IRunnerLogger runnerLogger, IServiceTaskHandlerInitializer serviceTaskHandlerInitializer, IServiceTaskScheduleManager serviceTaskScheduleInitializer, IHostedServiceAttributeProvider hostedServiceAttributeProvider, IClientHostedServiceAttributeProvider clientHostedServiceAttributeProvider)
					: base(runnerLogger, serviceTaskHandlerInitializer, serviceTaskScheduleInitializer, hostedServiceAttributeProvider, clientHostedServiceAttributeProvider)
				{
				}

				public RunnerExitCode RunInternalExposed(bool singleRun)
				{
					return base.RunInternal(singleRun);
				}
			}
		}
	}
}
