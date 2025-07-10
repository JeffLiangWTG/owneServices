using System;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneInfoForTesting : TimeZoneInfo
	{
		public TimeZoneInfoForTesting(string zoneName, decimal utcOffsetStandard, decimal utcOffsetDst, Guid dstZonePk)
			: base(zoneName, utcOffsetStandard, utcOffsetDst, dstZonePk)
		{
		}
	}
}
