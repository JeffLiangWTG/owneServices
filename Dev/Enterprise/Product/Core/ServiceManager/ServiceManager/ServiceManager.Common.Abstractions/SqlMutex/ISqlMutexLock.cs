using System;

namespace ServiceManager.Common.Abstractions
{
	public interface ISqlMutexLock : IDisposable
	{
		event EventHandler<SqlMutexLockEventArgs> OnLockLost;
		string? ReturnMessage { get; }
	}
}
