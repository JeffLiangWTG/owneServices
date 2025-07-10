using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	public class ApplicationTest
	{
		[Test]
		public void TestLoggingIsConfigured()
		{
			// Arrange
			Application.ConfigureApplicationServices();

			// Act & Assert
			Assert.That(Application.LoggerFactory, Is.Not.Null);
		}

		[Test]
		public void TestTracingIsConfigured()
		{
			// Arrange
			Application.ConfigureApplicationServices();

			var logger = Application.LoggerFactory.CreateApplicationLogger(nameof(ApplicationTest));

			// Act & Assert
			Assert.DoesNotThrow(() => logger.ActivitySource.StartActivity());
		}
		
		[Test]
		public void TestShutdownApplicationServices()
		{
			// Arrange
			Application.ConfigureApplicationServices();

			// Act
			Application.ShutdownApplicationServices();

			// Assert
			Assert.That(Application.ServiceProvider, Is.Null);
		}

		[Test]
		public void TestShutdownApplicationServicesWithNoConfiguration()
		{
			// Arrange & Act & Assert
			Assert.DoesNotThrow(Application.ShutdownApplicationServices);
		}
	}
}
