using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using WTG.StaticAnalysis.Annotation;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	static class EventLogHelper
	{
		[SuppressMessage("Enterprise", "EDI012", Justification = "The event sources are specifically targeted for loggings")]
		public static IEnumerable<EventRecord> ReadEventLogs(DateTime? dateTimeSince = null)
		{
			return
				ReadApplicationEventLogs(AspDotNet40EventSource, dateTimeSince)
					.Concat(ReadApplicationEventLogs(CargoWiseOne, dateTimeSince));
		}

		static IEnumerable<EventRecord> ReadApplicationEventLogs(string source, DateTime? dateTimeSince)
		{
			var dateTimeNow = DateTime.Now;
			dateTimeSince =
				LastEventReadTimeDictionary.ContainsKey(source)
					? LastEventReadTimeDictionary[source]
					: dateTimeSince ?? dateTimeNow.AddSeconds(-1);

			var eventLogs = new List<EventRecord>();
			var eventsQuery = new EventLogQuery(
				"Application",
				PathType.LogName,
				$@"
*[
	System/Provider/@Name='{source}'
	and
	System/TimeCreated/@SystemTime > '{dateTimeSince?.ToUniversalTime():o}'
	and
	System/TimeCreated/@SystemTime <= '{dateTimeNow.ToUniversalTime():o}'
]");
			using var eventLogReader = new EventLogReader(eventsQuery);
			var eventRecord = eventLogReader.ReadEvent();

			while (eventRecord != null)
			{
				eventLogs.Add(eventRecord);
				eventRecord = eventLogReader.ReadEvent();
			}

			LastEventReadTimeDictionary[source] = dateTimeNow;

			return eventLogs;
		}

		[ThreadSafe]
		static readonly Dictionary<string, DateTime> LastEventReadTimeDictionary = new ();

		[SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public const string CargoWiseOne = "CargoWise One";
		public const string AspDotNet40EventSource = "ASP.NET 4.0.30319.0";
	}
}

