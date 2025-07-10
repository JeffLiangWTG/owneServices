using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class LoggerConfigurationCorruptedExceptionTest : EnvironmentCorruptedException<LoggerConfigurationCorruptedException>
	{
		[Test]
		public void TestTaskCodeAndMessage()
		{
			// Arrange
			var message = "Hello world";
			const string taskCode = "xxx";
			var serviceTaskConfigMock = new Mock<IHostedServiceAttribute>();
			serviceTaskConfigMock.Setup(x => x.Code).Returns(taskCode);
			serviceTaskConfigMock.Setup(x => x.TypeName).Returns("typeName1");
			serviceTaskConfigMock.Setup(x => x.TypeAssemblyName).Returns("assemblyName1");

			string expectedMessage = $@"The service task 'xxx - typeName1, assemblyName1' has corrupted the logging configuration. Ensure that the logging configuration is not altered during the run and that added rules and targets are removed at the end of the service task's run.{System.Environment.NewLine}Hello world";

			// Act
			var exception = new LoggerConfigurationCorruptedException(serviceTaskConfigMock.Object, message);

			// Assert
			Assert.That(exception.Message, Is.EqualTo(expectedMessage));
			Assert.That(exception.Message, Contains.Substring(taskCode));
		}
	}
}
