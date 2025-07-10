using System;

namespace Enterprise.ZArchitecture.Environment
{
	[Serializable]
	public class EmailSendFailedException : OdysseyException
	{
		public EmailSendFailedException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected EmailSendFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
