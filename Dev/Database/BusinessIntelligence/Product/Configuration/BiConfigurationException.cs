using System;

namespace CargoWise.Bi.Configuration
{
	[Serializable]
	public class BiConfigurationException : Exception
	{
		public BiConfigurationException(string message)
			: base(message)
		{
		}

		public BiConfigurationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected BiConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
