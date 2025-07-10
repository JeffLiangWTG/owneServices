using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.BufferManagement.GUI
{
	[Serializable]
	public class UnidentifiedFilterControlException : Exception
	{
		public UnidentifiedFilterControlException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public UnidentifiedFilterControlException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
