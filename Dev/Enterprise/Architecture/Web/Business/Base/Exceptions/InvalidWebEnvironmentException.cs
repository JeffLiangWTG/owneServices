using System;

namespace Enterprise.ZArchitecture.Web.Business
{
	[Serializable]
	public sealed class InvalidWebEnvironmentException : Exception
	{
		public InvalidWebEnvironmentException()
		{
		}

		public InvalidWebEnvironmentException(string message) : base(message)
		{
		}

		public InvalidWebEnvironmentException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		InvalidWebEnvironmentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
