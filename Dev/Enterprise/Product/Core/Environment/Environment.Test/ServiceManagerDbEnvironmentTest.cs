using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	class ServiceManagerDbEnvironmentPooledTest : TestCase
	{
		public void TestPoolingEnabled()
		{
			// Arrange
			var serviceControllerDbEnvironment = new ServiceManagerDbEnvironmentPooled();

			// Act
			var connectionPooling = serviceControllerDbEnvironment.ConnectionPooling;

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(nameof(connectionPooling.IsPooling), true, connectionPooling.IsPooling);
				AssertEquals(nameof(connectionPooling.MaxPoolSize), 1000, connectionPooling.MaxPoolSize);
				AssertEquals(nameof(connectionPooling.MinPoolSize), 0, connectionPooling.MinPoolSize);
			});
		}
	}

	class ServiceManagerDbEnvironmentUnpooledTest : TestCase
	{
		public void TestPoolingDisabled()
		{
			// Arrange
			var serviceControllerDbEnvironment = new ServiceManagerDbEnvironmentUnpooled();

			// Act
			var noConnectionPooling = serviceControllerDbEnvironment.ConnectionPooling;

			// Assert
			AssertEquals(nameof(noConnectionPooling.IsPooling), false, noConnectionPooling.IsPooling);
		}
	}
}