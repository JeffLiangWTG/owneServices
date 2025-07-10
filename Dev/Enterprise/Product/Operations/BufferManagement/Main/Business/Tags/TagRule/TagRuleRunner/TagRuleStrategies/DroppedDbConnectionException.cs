using System;

namespace Enterprise.BufferManagement.Business
{
	[Serializable]
	public class DroppedDbConnectionException : Exception
	{
		public DroppedDbConnectionException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		public DroppedDbConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
