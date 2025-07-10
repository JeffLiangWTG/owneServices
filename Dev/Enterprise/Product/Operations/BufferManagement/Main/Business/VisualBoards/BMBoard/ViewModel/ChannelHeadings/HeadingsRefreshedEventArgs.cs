using System;

namespace Enterprise.BufferManagement.Business
{
	public class HeadingsRefreshedEventArgs : EventArgs
	{
		public HeadingsRefreshedEventArgs(ChannelHeadingViewModelSet viewModels)
		{
			ViewModels = viewModels;
		}

		public ChannelHeadingViewModelSet ViewModels { get; }
	}
}
