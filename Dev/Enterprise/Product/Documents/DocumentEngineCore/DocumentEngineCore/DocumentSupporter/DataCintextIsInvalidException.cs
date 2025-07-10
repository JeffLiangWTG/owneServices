using System;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	[Serializable]
	public class DataContextIsInvalidException : ArgumentException
	{
		public DataContextIsInvalidException(string message = null, Exception inner = null)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected DataContextIsInvalidException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
