using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZBlobReadException : Exception
	{
		public ZBlobReadException(string message, Exception inner) : base(message, inner) { }

#if NETFRAMEWORK
		public ZBlobReadException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
