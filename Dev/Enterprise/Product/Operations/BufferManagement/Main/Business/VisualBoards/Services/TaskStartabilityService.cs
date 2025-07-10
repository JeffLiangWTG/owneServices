using System;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class TaskStartabilityService : NonResettableService
	{
		public TaskStartabilityService(IsStartableMap map)
		{
			this.map = map;
		}

		readonly IsStartableMap map;

		public bool IsStartable(IProcessTask task, Func<IProcessTask, bool> calculateIsStartableFunc)
		{
			return map.IsStartable(task, calculateIsStartableFunc);
		}
	}
}
