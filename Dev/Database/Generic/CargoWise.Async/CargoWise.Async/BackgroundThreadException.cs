using System;

namespace CargoWise.Async
{
	[Serializable]
	public class BackgroundThreadException : Exception
	{
		public BackgroundThreadException()
		{
		}

		public BackgroundThreadException(string message)
			: base(message)
		{
		}

		public BackgroundThreadException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected BackgroundThreadException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
