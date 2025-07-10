using System;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery
{
	[Serializable]
	public class KnownErrorException : ApplicationException
	{
		public KnownErrorException(string message) : base(message) { }
		public KnownErrorException(string message, Exception innerException) : base(message, innerException) { }
#if NETFRAMEWORK
		protected KnownErrorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
