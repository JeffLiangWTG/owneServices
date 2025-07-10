using System;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IUserIdleWorkItem : IDisposable
	{
		int StartDelay { get; }
	}
}
