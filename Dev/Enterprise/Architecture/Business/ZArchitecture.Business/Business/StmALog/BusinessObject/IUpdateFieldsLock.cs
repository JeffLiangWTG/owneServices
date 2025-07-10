using System;

namespace Enterprise.ZArchitecture.Business
{
	public interface IUpdateFieldsLock
	{
		IDisposable LockForUpdatingKeyFields();
	}
}
