using System;

namespace Enterprise.BufferManagement.Business
{
	[Serializable]
	public class ReleaseGateDataIsStaleException : Exception
	{
		public ReleaseGateDataIsStaleException()
		{
		}

		public ReleaseGateDataIsStaleException(string message)
			: base(message)
		{
		}

		public ReleaseGateDataIsStaleException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ReleaseGateDataIsStaleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
