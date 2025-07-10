using System;
using CargoWise.EntityFramework;

namespace Enterprise.Messaging.Business
{
	[Serializable]
	public class MessageProcessException : ApplicationException
	{
		public MessageProcessException()
		{
		}

		public MessageProcessException(string message, Exception innerException, BusinessObject originator)
			: this(message, innerException)
		{
			this.originator = originator;
		}

		public MessageProcessException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public MessageProcessException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected MessageProcessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public BusinessObject Originator
		{
			get { return originator; }
		}
		readonly BusinessObject originator;
	}
}
