using System;
using System.Diagnostics;
using System.Threading;

namespace CargoWise.Async
{
	public class ThreadLog
	{
		internal ThreadLog(bool takeOwnership, bool useFullTrace = true, (string filePath, string memberName, int lineNumber) callerInfo = default)
		{
			if (takeOwnership)
			{
				ThreadID = Thread.CurrentThread.ManagedThreadId;
				ThreadName = Thread.CurrentThread.Name;
			}
			else
			{
				ThreadID = null;
				ThreadName = null;
			}

			if (useFullTrace)
			{
				ThreadStackTrace = new StackTrace(1);
			}
			else
			{
				this.callerInfo = callerInfo;
			}
		}
		readonly (string filePath, string memberName, int lineNumber) callerInfo;
		public int? ThreadID { get; private set; }
		public string ThreadName { get; private set; }
		public StackTrace ThreadStackTrace { get; private set; }

		public string LightTraceInfo => FormattableString.Invariant($"Full stack disabled, file:{callerInfo.filePath}, member:{callerInfo.memberName}, line:{callerInfo.lineNumber}"); // Error message text
	}
}
