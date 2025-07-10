using System;

namespace Enterprise.UniversalDataBuss.Management
{
	/// <summary>
	/// Thrown to indicate an EDIMessage sub-type is known but not supported in the current operation
	/// </summary>
	[Serializable]
	public class UnsupportedMessageTypeException : Exception
	{
		public UnsupportedMessageTypeException() : base() { }

		public UnsupportedMessageTypeException(string message) : base(message) { }

		public UnsupportedMessageTypeException(string message, Exception innerException) : base(message, innerException) { }

#if NETFRAMEWORK
		UnsupportedMessageTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
