using System;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	[Serializable]
	public class RemotePrintingException : Exception
	{
		public RemotePrintingException(string message)
			: base(message)
		{
		}

		protected RemotePrintingException(string messageOverride, Exception innerException)
			: base(messageOverride, innerException)
		{
		}

#if NETFRAMEWORK
		protected RemotePrintingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
