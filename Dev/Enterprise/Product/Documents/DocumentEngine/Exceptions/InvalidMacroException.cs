using System;
using System.Runtime.Serialization;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class InvalidMacroException : Exception
	{
#if NET
		[Obsolete]
#endif
		protected InvalidMacroException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
		public InvalidMacroException(string message) : base(message)
		{
		}
		public InvalidMacroException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
