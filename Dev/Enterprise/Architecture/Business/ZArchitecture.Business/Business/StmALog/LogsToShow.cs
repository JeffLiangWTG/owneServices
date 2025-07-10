using System;

namespace Enterprise.ZArchitecture.Business
{
	[Flags]
	public enum LogsToShow
	{
		All = ChangeLogs | Operations,
		ChangeLogs = 1,
		Operations = 2,
	}
}
