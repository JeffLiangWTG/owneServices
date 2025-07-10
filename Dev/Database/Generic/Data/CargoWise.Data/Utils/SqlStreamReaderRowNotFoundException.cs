using System;

namespace CargoWise.Data
{
	[Serializable]
	public class SqlStreamReaderRowNotFoundException : Exception
	{
		public SqlStreamReaderRowNotFoundException(string message) : base(message) { }

#if NETFRAMEWORK
		public SqlStreamReaderRowNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
