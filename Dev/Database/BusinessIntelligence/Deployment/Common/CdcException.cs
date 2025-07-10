using System;

namespace Enterprise.ChangeDataCapture.Common
{
	[Serializable]
	public class CdcException : Exception
	{
		public CdcException(string message)
			: base(message)
		{
		}
		public CdcException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected CdcException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
