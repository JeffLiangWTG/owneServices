using System;

namespace CargoWise.Application
{
	/// <summary>
	/// Defines an exception thrown during a reflection operation.
	/// </summary>
	[Serializable]
	public class ReflectionException : Exception
	{
		public ReflectionException()
		{
		}
		public ReflectionException(string message) : base(message)
		{
		}
		public ReflectionException(string message, Exception rootCause) : base(message, rootCause)
		{
		}

#if NETFRAMEWORK
		protected ReflectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
