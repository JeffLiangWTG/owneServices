using Enterprise.ServiceManager.Runner.Extensions;
using Moq;
using NUnit.Framework;
using WTG.ApplicationLogging.Abstractions;

namespace ServiceManager.Runner.CW.Test
{
	public class ApplicationLoggingExtensionsTest
	{
		[Test]
		public void CreateRunnerLoggerReturnsApplicationLogger()
		{
			// Arrange
			var loggerFactoryMock = new Mock<IApplicationLoggerFactory>();
			loggerFactoryMock
				.Setup(factory => factory.CreateApplicationLogger("ServiceManagerRunner"))
				.Returns(Mock.Of<IApplicationLogger>());

			// Act
			var logger = loggerFactoryMock.Object.CreateRunnerLogger();

			// Assert
			Assert.That(logger, Is.InstanceOf<IApplicationLogger>());
		}
	}
}
