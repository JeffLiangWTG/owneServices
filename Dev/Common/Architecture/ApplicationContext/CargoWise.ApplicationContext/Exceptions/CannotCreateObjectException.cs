using System;

namespace CargoWise.Application.Exceptions
{
	/// <summary>
	/// Defines an exception thrown when an object cannot be created.
	/// </summary>
	[Serializable]
	public class CannotCreateObjectException : Exception
	{
		public CannotCreateObjectException()
		{
		}

		public CannotCreateObjectException(string message) : base(message)
		{
		}

		public CannotCreateObjectException(string message, Exception rootCause) : base(message, rootCause)
		{
		}

#if NETFRAMEWORK
		protected CannotCreateObjectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
