using System;

namespace Enterprise.Interop.OutlookIntegration
{
	[Serializable]
	public class OutlookDialogBoxOpenException : OutlookOperationAbortedException
	{
		public OutlookDialogBoxOpenException(string message, Exception nested)
			: base(message, nested)
		{
		}

#if NETFRAMEWORK
		protected OutlookDialogBoxOpenException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
