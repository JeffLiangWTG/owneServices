using System;

using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Data.Mutex
{
	[Serializable]
	public class MutexNotLockedException : ZException
	{
		internal MutexNotLockedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MutexNotLockedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
