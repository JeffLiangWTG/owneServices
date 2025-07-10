namespace AppDomainWrappers.Net8
{
	public class LockFileStateChangedEventArgs : EventArgs
	{
		public LockFileState State { get; set; }
		public CancellationTokenSource CancellationTokenSource { get; set; }
	}
}
