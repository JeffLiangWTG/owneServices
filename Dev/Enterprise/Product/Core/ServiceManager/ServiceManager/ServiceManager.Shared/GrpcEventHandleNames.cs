using System;
using System.Globalization;

namespace Enterprise.ServiceManager.Shared
{
	public class GrpcEventHandleNames
	{
		public GrpcEventHandleNames(string? baseEventHandle = null)
		{
			BaseEventWaitHandleName = baseEventHandle ?? Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
			HostEventWaitHandleName = $"{BaseEventWaitHandleName}_Host";
			RunnerEventWaitHandleName = $"{BaseEventWaitHandleName}_Runner";
		}

		public string BaseEventWaitHandleName { get; }
		public string HostEventWaitHandleName { get; }
		public string RunnerEventWaitHandleName { get; }
	}
}
