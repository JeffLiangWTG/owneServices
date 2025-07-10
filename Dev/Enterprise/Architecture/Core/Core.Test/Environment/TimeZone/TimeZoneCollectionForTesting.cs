using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneCollectionForTesting : TimeZoneCollection
	{
		public IDictionary<string, ITimeZone> TimeZones_Exposed
		{
			get { return timeZones; }
		}

		public UtcDateTimeCache UtcAndServerZoneWrapperForTest
		{
			get => utcAndServerZoneWrapper;
			set => utcAndServerZoneWrapper = value;
		}
	}
}
