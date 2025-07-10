using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.Logging.Test
{
	public class CategorizedApplicationLoggerFactoryTest
	{
		[Test]
		public void TestConstructorArgumentNullExceptions()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => _ = new CategorizedApplicationLoggerFactory(null));
			Assert.That(ex!.ParamName, Is.EqualTo("loggerFactory"));
		}

		[Test]
		public void CreateCategorizedLoggerWrapsLoggerFactoryMethod()
		{
			// Act
			var logger = loggerFactory!.CreateCategorizedLogger(LoggerCategory.ServiceTask, loggerFactory.GetType().Name, []);

			// Assert
			applicationLoggerFactory!.Verify(x => x.CreateApplicationLogger(It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void AddProviderWrapsLoggerFactoryMethod()
		{
			// Arrange
			var provider = new Mock<ILoggerProvider>();

			// Act
			loggerFactory!.AddProvider(provider.Object);

			// Assert
			applicationLoggerFactory!.Verify(x => x.AddProvider(It.IsAny<ILoggerProvider>()), Times.Once);
		}

		[Test]
		public void CreateApplicationLoggerWrapsLoggerFactoryMethod()
		{
			// Act
			var logger = loggerFactory!.CreateApplicationLogger(loggerFactory.GetType().Name);

			// Assert
			applicationLoggerFactory!.Verify(x => x.CreateApplicationLogger(It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void CreateLoggerWrapsLoggerFactoryMethod()
		{
			// Act
			var logger = loggerFactory!.CreateLogger(loggerFactory.GetType().Name);

			// Assert
			applicationLoggerFactory!.Verify(x => x.CreateLogger(It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void DisposeWrapsLoggerFactoryMethod()
		{
			// Act
			loggerFactory!.Dispose();

			// Assert
			applicationLoggerFactory!.Verify(x => x.Dispose(), Times.Once);
		}

		[SetUp]
		public void Setup()
		{
			applicationLoggerFactory = new Mock<IApplicationLoggerFactory>();
			applicationLogger = new Mock<IApplicationLogger>();
			applicationLoggerFactory.Setup(x => x.CreateApplicationLogger(It.IsAny<string>()))
				.Returns(applicationLogger.Object);

			loggerFactory = new CategorizedApplicationLoggerFactory(applicationLoggerFactory.Object);
		}

		CategorizedApplicationLoggerFactory? loggerFactory;
		Mock<IApplicationLoggerFactory>? applicationLoggerFactory;
		Mock<IApplicationLogger>? applicationLogger;
	}
}
