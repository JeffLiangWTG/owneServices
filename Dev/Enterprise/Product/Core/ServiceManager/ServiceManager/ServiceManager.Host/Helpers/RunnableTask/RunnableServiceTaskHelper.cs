using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.ServiceManager.Host
{
	static class RunnableServiceTaskHelper
	{
		static readonly Overridable<string> forcefullyDisabledTasks = new Overridable<string>(string.Empty);
		static readonly Overridable<HashSet<string>> forcefullyDisabledTasksSet = new Overridable<HashSet<string>>(new HashSet<string>());
		public static bool IsTaskDisabled(string taskCode, string registryForcefullyDisabledTasks)
		{
			if (forcefullyDisabledTasks.Value != registryForcefullyDisabledTasks)
			{
				forcefullyDisabledTasks.Value = registryForcefullyDisabledTasks;
				forcefullyDisabledTasksSet.Value = new HashSet<string>(
					forcefullyDisabledTasks.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
					?? Array.Empty<string>());
			}
			return forcefullyDisabledTasksSet.Value.Contains(taskCode);
		}
	}
}
