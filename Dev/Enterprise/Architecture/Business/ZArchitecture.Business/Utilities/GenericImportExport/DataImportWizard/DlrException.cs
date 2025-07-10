using System;

namespace Enterprise.ZArchitecture.DataMapping
{
	[Serializable]
	public class DlrException : Exception
	{
		public DlrException(string message)
			: base(message)
		{
		}

		public DlrException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		protected DlrException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
