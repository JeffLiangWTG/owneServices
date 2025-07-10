using System;

namespace CargoWise.IO
{
	[Serializable]
	public class PdhException : Exception
	{
		public PdhException(string message = null, Exception inner = null)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		public PdhException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
