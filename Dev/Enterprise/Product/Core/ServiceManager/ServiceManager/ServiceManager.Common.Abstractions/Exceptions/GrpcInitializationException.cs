using System;

namespace ServiceManager.Common.Abstractions
{
	[Serializable]
	public abstract class GrpcInitializationException : Exception
	{
		protected GrpcInitializationException(Type type)
			: this($"Grpc cannot be created for [{type}].")
		{
		}

		protected GrpcInitializationException() : this("Grpc cannot be created.")
		{
		}

		protected GrpcInitializationException(string message) : base(message)
		{
		}

		protected GrpcInitializationException(string message, Exception ex) : base(message, ex)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected GrpcInitializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
