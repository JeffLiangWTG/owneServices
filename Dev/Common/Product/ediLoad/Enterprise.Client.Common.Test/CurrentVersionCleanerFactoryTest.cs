using System;
using Enterprise.Upgrades;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.Common.Testing
{
	class CurrentVersionCleanerFactoryTest : TestCase
	{
		public void TestWrongParamsCall()
		{
			AssertExceptionThrown<ArgumentException>(() => new CurrentVersionCleanerFactory().GetCurrentVersionCleaner(null));
		}

		public void TestGetCurrentVersionCleaner()
		{
			// Arrange
			var factory = new CurrentVersionCleanerFactory();

			// Act
			var cleaner = factory.GetCurrentVersionCleaner(new Mock<ICurrentVersionCleanerConfig>().Object);

			// Assert
			AssertEquals(true, typeof(ICurrentVersionCleaner).IsAssignableFrom(cleaner.GetType()));
		}
	}
}
