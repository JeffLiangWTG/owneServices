using System;

namespace Enterprise.Messaging.Business
{
	[Serializable]
	public class ConcurrencyConflictException : ApplicationException
	{
		public ConcurrencyConflictException()
		{
		}

		public ConcurrencyConflictException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ConcurrencyConflictException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
