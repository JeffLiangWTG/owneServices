using System;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Shared.Testing
{
	public class ServiceTaskLogFiltersTest
	{
		[Test]
		public void TestSetFilters()
		{
			Assert.Multiple(() =>
			{
				AssertFilters(new ServiceTaskLogFilters(), null, null, null, null, null, null);
				AssertFilters(new ServiceTaskLogFilters { ServiceTaskCode = "    abc", HostName = "WTg.CoM   " }, "ABC", null, "wtg.com", null, null, null);
				AssertFilters(
					new ServiceTaskLogFilters
					{
						ServiceTaskCode = "PRC",
						HostName = "eye.wtg.com",
						Severity = LogType.Information,
						ProcessId = "123",
						FromDateTimeUtc = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
						ToDateTimeUtc = new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc)
					},
					"PRC",
					LogType.Information,
					"eye.wtg.com",
					"123",
					new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc));
			});
		}

		void AssertFilters(ServiceTaskLogFilters filters,
			string? expectedServiceTaskCode,
			LogType? expectedSeverity,
			string? expectedHostName,
			string? expectedProcessId,
			DateTime? expectedFromDateTimeUtc,
			DateTime? expectedToDateTimeUtc)
		{
			Assert.That(filters.ServiceTaskCode, Is.EqualTo(expectedServiceTaskCode));
			Assert.That(filters.Severity, Is.EqualTo(expectedSeverity));
			Assert.That(filters.HostName, Is.EqualTo(expectedHostName));
			Assert.That(filters.ProcessId, Is.EqualTo(expectedProcessId));
			Assert.That(filters.FromDateTimeUtc, Is.EqualTo(expectedFromDateTimeUtc));
			Assert.That(filters.ToDateTimeUtc, Is.EqualTo(expectedToDateTimeUtc));
		}
	}
}
