using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers
{
	[Serializable]
	public class KafkaFlushTimeoutException : Exception
	{
		public KafkaFlushTimeoutException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected KafkaFlushTimeoutException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
