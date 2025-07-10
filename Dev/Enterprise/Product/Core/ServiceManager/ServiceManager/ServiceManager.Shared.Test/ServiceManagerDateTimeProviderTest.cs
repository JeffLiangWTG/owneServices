using System;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Shared.Testing
{
	[TestFixture]
	public class ServiceManagerDateTimeProviderTest
	{
		[Test]
		public void TestCurrentTime()
		{
			ServiceManagerDateTimeProvider serviceManagerDateTimeProvider = new();
			// Arrange
			var utcTime = DateTime.UtcNow;

			// Act
			var result = serviceManagerDateTimeProvider.CurrentDateTimeUtc;

			// Assert
			Assert.That(result, Is.TypeOf<DateTime>());
			Assert.That(result, Is.EqualTo(utcTime).Within(1).Seconds);
			Assert.That(result.Kind, Is.EqualTo(DateTimeKind.Utc));
		}
	}
}
