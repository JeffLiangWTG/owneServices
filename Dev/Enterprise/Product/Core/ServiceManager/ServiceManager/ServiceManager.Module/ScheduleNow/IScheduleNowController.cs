using System;
using System.Collections.Generic;

namespace Enterprise.ServiceManager.Module.ScheduleNow
{
	public interface IScheduleNowController
	{
		void ScheduleNow(IEnumerable<string> taskCodes, Action<string> callbackAction, bool forceRestart);
	}
}
