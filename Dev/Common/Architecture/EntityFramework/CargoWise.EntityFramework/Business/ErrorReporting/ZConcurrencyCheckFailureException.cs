using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZConcurrencyCheckFailureException : ZCannotSaveException
	{
		public ZConcurrencyCheckFailureException(string message, string heading, bool shouldReprocess)
			: base(message, heading, shouldReprocess)
		{
		}

#if NETFRAMEWORK
		protected ZConcurrencyCheckFailureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
