using System.Diagnostics;

namespace CargoWise.Loader.Common
{
	public interface IEventLogProxy
	{
		void WriteEntry(string source, string message, EventLogEntryType type);
	}
}
