using System;

namespace Enterprise.ZArchitecture.Business
{
	public interface IUpdateFieldsLockReachAround
	{
		IDisposable LockForUpdatingKeyFields(bool shouldNotifyOnRelease);
	}
}
