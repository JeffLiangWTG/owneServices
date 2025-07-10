using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Core
{
	public interface ILowPriorityProcessPauser
	{
		TimeSpan Wait(ILogger logger = null, TimeSpan? maxWaitTime = null, Action callbackOnDelay = null);
	}
}
