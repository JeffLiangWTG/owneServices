using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Loader.Common
{
	public sealed class EventLogProxy : IEventLogProxy
	{
		public EventLogProxy()
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1069:Do Not Use EventLog.WriteEntry", Justification = "No reference to ZArchitecture")]
		public void WriteEntry(string source, string message, EventLogEntryType type)
		{
			EventLog.WriteEntry(source, message, type);
		}
	}
}
