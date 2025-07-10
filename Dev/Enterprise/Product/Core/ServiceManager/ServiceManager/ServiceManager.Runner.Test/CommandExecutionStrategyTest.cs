using System;
using System.Linq;
using Enterprise.ServiceManager.Runner;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	class CommandExecutionStrategyTest
	{
		[SetUp]
		public void SetUp()
		{
			serviceTaskRunnerStrategyMock = new Mock<IServiceTaskRunnerStrategy>();
			commandExecutionStrategy = new CommandExecutionStrategy(serviceTaskRunnerStrategyMock.Object);
		}

		[Test]
		public void TestStopCommand()
		{
			// Arrange
			var commandInfo = new StopCommandInfo();

			// Act
			var result = commandExecutionStrategy.Execute(commandInfo);

			// Assert
			Assert.That(result, Is.EqualTo(ServiceTaskRunResult.Cancelled));
			serviceTaskRunnerStrategyMock.Verify(runner => runner.Run(It.IsAny<IRunCommandInfo>()), Times.Never);
		}

		[Test]
		public void TestRunCommand()
		{
			Assert.Multiple(() =>
			{
				var values = Enum.GetValues(typeof(ServiceTaskRunResult)).Cast<ServiceTaskRunResult>();
				foreach (var value in values)
				{
					Test(value);
				}
			});

			void Test(ServiceTaskRunResult executionResult)
			{
				// Arrange
				serviceTaskRunnerStrategyMock.Reset();

				var commandInfo = new DirectRunCommandInfo("assemblyName", "code", Guid.Empty);
				serviceTaskRunnerStrategyMock
					.Setup(executor => executor.Run(It.IsAny<IRunCommandInfo>()))
					.Returns(executionResult);

				// Act
				var result = commandExecutionStrategy.Execute(commandInfo);

				// Assert
				Assert.That(result, Is.EqualTo(executionResult));
				serviceTaskRunnerStrategyMock.Verify(executor => executor.Run(It.IsAny<IRunCommandInfo>()), Times.Once);
				serviceTaskRunnerStrategyMock.Verify(executor => executor.Run(commandInfo), Times.Once);
			}
		}

		[Test]
		public void TestUnexpectedCommand()
		{
			// Arrange
			var commandInfo = new UnexpectedCommandInfo();

			// Act
			// Assert
			Assert.Throws<NotImplementedException>(() => commandExecutionStrategy.Execute(commandInfo));
			serviceTaskRunnerStrategyMock.Verify(executor => executor.Run(It.IsAny<IRunCommandInfo>()), Times.Never);
		}

		[Test]
		public void TestWrongParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = new CommandExecutionStrategy(null));
				Assert.That(result.ParamName, Is.EqualTo("serviceTaskRunnerStrategy"));

				result = Assert.Throws<ArgumentNullException>(() => commandExecutionStrategy.Execute(null));
				Assert.That(result.ParamName, Is.EqualTo("commandInfo"));
			});
		}

		CommandExecutionStrategy commandExecutionStrategy;
		Mock<IServiceTaskRunnerStrategy> serviceTaskRunnerStrategyMock;

		class UnexpectedCommandInfo : ICommandInfo
		{
			public Guid Id => throw new NotImplementedException();

			public string FormatRequestToLogMessage(RunnerLogMessageStage logMessageStage, params object[] values)
			{
				throw new NotImplementedException();
			}
		}
	}
}
