using System;

namespace Enterprise.BufferManagement.Business
{
	[Serializable]
	class UserNotFoundException : Exception
	{
		public UserNotFoundException(string message)
			: base(message)
		{ }

#if NETFRAMEWORK
		protected UserNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
