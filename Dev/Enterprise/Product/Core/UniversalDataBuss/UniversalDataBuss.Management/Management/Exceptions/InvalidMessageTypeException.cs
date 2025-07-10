using System;

namespace Enterprise.UniversalDataBuss.Management
{
	/// <summary>
	/// Thrown to indicate an EDIMessage subtype does not match any known valid subtypes
	/// </summary>
	[Serializable]
	public class InvalidMessageTypeException : Exception
	{
		public InvalidMessageTypeException() : base() { }

		public InvalidMessageTypeException(string message) : base(message) { }

		public InvalidMessageTypeException(string message, Exception innerException) : base(message, innerException) { }

#if NETFRAMEWORK
		InvalidMessageTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
