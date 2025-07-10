using CargoWise.Data;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing
{
	sealed class LoggerNLogConfigurationFactoryTest : TestCase
	{
		public void TestFactoryReturnsSimpleConfigurationIfRunUnderNonMainDatabase()
		{
			// Arrange
			var factory = new LoggerNLogConfigurationFactory();

			// Act
			var config = factory.GetConfiguration("AAA");

			// Assert
			Assert(config is LoggerNLogSimpleConfiguration);
		}

		public void TestFactoryReturnsRegistryConfigurationIfRunUnderMainDatabase()
		{
			// Arrange
			var factory = new LoggerNLogConfigurationFactory();

			// Act
			var config = factory.GetConfiguration(Db.DatabaseName);

			// Assert
			Assert(config is LoggerNLogRegistryConfiguration);
		}
	}
}

