using System.ComponentModel;

namespace Enterprise.Client.UPE.Business
{
	public delegate void IsRedirectedChangingEventHandler(IsRedirectedChangingEventArgs eventArgs);

	public class IsRedirectedChangingEventArgs : CancelEventArgs
	{
		public IsRedirectedChangingEventArgs(bool isRedirected)
		{
			this.IsRedirected = isRedirected;
		}

		public readonly bool IsRedirected;
	}
}
