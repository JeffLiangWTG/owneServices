using System;

namespace CargoWise.Application
{
	/// <summary>
	/// Defines an exception thrown when an object type cannot be loaded.
	/// </summary>
	[Serializable]
	public class CannotLoadObjectTypeException : ReflectionException
	{
		public CannotLoadObjectTypeException()
		{
		}
		public CannotLoadObjectTypeException(string message) : base(message)
		{
		}
		public CannotLoadObjectTypeException(string message, Exception rootCause) : base(message, rootCause)
		{
		}

#if NETFRAMEWORK
		protected CannotLoadObjectTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
