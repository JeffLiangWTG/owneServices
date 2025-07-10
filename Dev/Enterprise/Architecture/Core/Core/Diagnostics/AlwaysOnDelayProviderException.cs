using System;

namespace CargoWise.Data.SqlServer
{
	[Serializable]
	public class AlwaysOnDelayProviderException : Exception
	{
		public AlwaysOnDelayProviderException()
		{
		}

		public AlwaysOnDelayProviderException(string message)
			: base(message)
		{
		}

		public AlwaysOnDelayProviderException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected AlwaysOnDelayProviderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
