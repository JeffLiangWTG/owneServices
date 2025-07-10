using System;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ILockMechanism
	{
		bool TryGetLock(string key, out IDisposable appLock);
	}
}
