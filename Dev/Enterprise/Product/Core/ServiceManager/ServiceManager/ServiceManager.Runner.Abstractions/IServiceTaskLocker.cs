using System;

namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskLocker
	{
		bool TryAcquireLock(string code, out IDisposable disposableLock);
	}

	public interface ISqlApplicationLocker : IServiceTaskLocker { }

	public interface ISqlMutexLocker : IServiceTaskLocker { }
}
