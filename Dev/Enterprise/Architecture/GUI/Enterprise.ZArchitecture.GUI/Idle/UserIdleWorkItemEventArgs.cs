using System;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class UserIdleWorkItemEventArgs : EventArgs
	{
		public UserIdleWorkItemEventArgs(IUserIdleWorkItem workItem)
		{
			this.WorkItem = workItem;
		}

		public IUserIdleWorkItem WorkItem { get; private set; }
	}
}
