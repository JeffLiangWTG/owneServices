using System;

namespace CargoWise.Application
{
	/// <summary>
	/// Defines a generic exception for the object factory.
	/// </summary>
	[Serializable]
	public class ObjectFactoryException : Exception
	{
		public ObjectFactoryException()
		{
		}
		public ObjectFactoryException(string message) : base(message)
		{
		}
		public ObjectFactoryException(string message, Exception rootCause) : base(message, rootCause)
		{
		}

#if NETFRAMEWORK
		protected ObjectFactoryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
