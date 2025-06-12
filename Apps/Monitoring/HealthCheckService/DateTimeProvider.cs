using System;

namespace XH.XT.Monitoring.HealthCheckService
{
	public interface IDateTimeProvider
	{
		DateTime UtcNow { get; }
	}

	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => DateTime.UtcNow;
	}
}
