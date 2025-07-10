using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Core
{
	public interface IHotKeyMonitor
	{
		bool Enabled { get; set; }

		void ProcessCmdKey(object sender, string keyName, HashSet<string> registeredKeys, string processorName, bool processed);

		void ProcessExceptions(string controlName, string keyName, string callStacks);

		string GetEnvironmentInfo();

		SortedDictionary<string, string> GetAllSpans();

		SortedDictionary<string, string> GetFilteredSpans();

		void ClearAll();

		string GetLatestRecordTime();
	}
}
