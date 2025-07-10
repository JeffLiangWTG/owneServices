using System;

namespace ServiceManager.Common.Abstractions
{
	public interface IJobObject : IDisposable
	{
		bool AddProcess(IntPtr intPtr);
	}
}
