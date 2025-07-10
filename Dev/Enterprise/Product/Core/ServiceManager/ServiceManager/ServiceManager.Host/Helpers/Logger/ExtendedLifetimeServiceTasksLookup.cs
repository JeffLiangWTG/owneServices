using System.Collections.Generic;

namespace Enterprise.ServiceManager.Host
{
	interface IExtendedLifetimeServiceTasksLookup
	{
		int GetNumberOfLogFilesToPreserve(string code);
	}

	class ExtendedLifetimeServiceTasksLookup : IExtendedLifetimeServiceTasksLookup
	{
		int NumberOfLogFilesToPreserve => 7;

		Dictionary<string, int> extendedLifetimeServiceTasks;

		Dictionary<string, int> ExtendedLifetimeServiceTasks
		{
			get
			{
				if (extendedLifetimeServiceTasks == null)
				{
					extendedLifetimeServiceTasks = new Dictionary<string, int> { { "PFC", 28 }, { "PML", 60 } };    // Performance Statistics Collector, Payroll Metric Sync
				}
				return extendedLifetimeServiceTasks;
			}
		}

		public int GetNumberOfLogFilesToPreserve(string code)
		{
			if (ExtendedLifetimeServiceTasks.ContainsKey(code))
			{
				return ExtendedLifetimeServiceTasks[code];
			}
			return NumberOfLogFilesToPreserve;
		}
	}
}
