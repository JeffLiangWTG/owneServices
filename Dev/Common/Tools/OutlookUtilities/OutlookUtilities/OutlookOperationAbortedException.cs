using System;

namespace Enterprise.Interop.OutlookIntegration
{
	[Serializable]
	public class OutlookOperationAbortedException : OutlookException
	{
		internal OutlookOperationAbortedException(string message, Exception nested) : base(message, nested)
		{
		}

#if NETFRAMEWORK
		protected OutlookOperationAbortedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
