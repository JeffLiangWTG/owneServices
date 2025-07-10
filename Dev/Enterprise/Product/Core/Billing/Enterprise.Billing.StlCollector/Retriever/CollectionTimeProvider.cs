using System;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class CollectionTimeProvider
	{
		DateTime? maximumSafeEndDateTimeExclusive;

		public DateTime MaximumSafeEndDateTimeExclusive
		{
			get
			{
				if (!maximumSafeEndDateTimeExclusive.HasValue)
				{
					var utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
					var utcOneHourAgo = utcNow.AddHours(-1); // should not collect from current to avoid missing late arriving data
					maximumSafeEndDateTimeExclusive = new DateTime(utcOneHourAgo.Year, utcOneHourAgo.Month, utcOneHourAgo.Day, utcOneHourAgo.Hour, 0, 0);
				}
				return maximumSafeEndDateTimeExclusive.Value;
			}
		}

		public DateTime GetDefaultStartTime(IStlItem script)
		{
			switch (script.StlGrain)
			{
				case StlDataGrain.Transactional:
				case StlDataGrain.Daily:
				{
						return MaximumSafeEndDateTimeExclusive.AddDays(-7);
				}
				case StlDataGrain.MonthlyAllowHistoricalData:
				case StlDataGrain.Snapshot:
				{
						return MaximumSafeEndDateTimeExclusive.AddMonths(-3);
				}
				case StlDataGrain.MonthlyCurrentDataOnly:
				{
						return MaximumSafeEndDateTimeExclusive.AddMonths(-1);
				}
				default:
				{
						throw new ArgumentOutOfRangeException(script.StlGrain.ToString());
				}
			}
		}
	}
}
