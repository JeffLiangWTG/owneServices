using System;

namespace Enterprise.Dat.Implementation
{
	[Serializable]
	public class LoginFailedException : Exception
	{
		public LoginFailedException()
		{
		}

		public LoginFailedException(string message)
			: base(message)
		{
		}

		public LoginFailedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected LoginFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
