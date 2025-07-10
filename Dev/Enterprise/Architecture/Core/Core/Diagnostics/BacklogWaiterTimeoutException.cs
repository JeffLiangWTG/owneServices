using System;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	[Serializable]
	public class BacklogWaiterTimeoutException : Exception
	{
		public BacklogWaiterTimeoutException()
		{
		}

		public BacklogWaiterTimeoutException(string message)
			: base(message)
		{
		}

		public BacklogWaiterTimeoutException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected BacklogWaiterTimeoutException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
