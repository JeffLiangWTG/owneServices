using Moq;
using NUnit.Framework;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.Logging.Test
{
	public class CategorizedApplicationLoggerFactoryExtensionsTest
	{
		[Test]
		public void AsCategorizedApplicationLoggerFactoryWrapsInputFactory()
		{
			// Arrange
			var applicationLoggerFactory = new Mock<IApplicationLoggerFactory>();

			// Act
			var result = applicationLoggerFactory.Object.AsCategorizedApplicationLoggerFactory();
			result.CreateCategorizedLogger(LoggerCategory.ServiceTask, "TestLogger", null);

			// Assert
			Assert.That(result, Is.InstanceOf<CategorizedApplicationLoggerFactory>());
			Assert.DoesNotThrow(() => applicationLoggerFactory.Verify(x => x.CreateApplicationLogger(It.IsAny<string>()), Times.Once));
		}

		[Test]
		public void AsCategorizedApplicationLoggerFactoryReturnsSameInstance()
		{
			// Arrange
			var applicationLoggerFactory = new Mock<IApplicationLoggerFactory>();
			var categorizedFactory = applicationLoggerFactory.Object.AsCategorizedApplicationLoggerFactory();

			// Act
			var result = categorizedFactory.AsCategorizedApplicationLoggerFactory();

			// Assert
			Assert.That(result, Is.SameAs(categorizedFactory));
		}
	}
}
