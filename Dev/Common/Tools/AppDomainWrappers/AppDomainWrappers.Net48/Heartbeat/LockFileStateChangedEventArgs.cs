using System;
using System.Threading;

namespace AppDomainWrappers.Net48
{
	public class LockFileStateChangedEventArgs : EventArgs
	{
		public LockFileState State { get; set; }
		public CancellationTokenSource CancellationTokenSource { get; set; }
	}
}
