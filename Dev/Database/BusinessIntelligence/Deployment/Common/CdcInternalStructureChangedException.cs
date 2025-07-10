using System;

namespace Enterprise.ChangeDataCapture.Common
{
	[Serializable]
	public class CdcInternalStructureChangedException : Exception
	{
		public CdcInternalStructureChangedException(string message)
			: base(message)
		{
		}
		public CdcInternalStructureChangedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected CdcInternalStructureChangedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
