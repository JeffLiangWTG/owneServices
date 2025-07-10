using System;

namespace CargoWise.Loader.Common.Exceptions
{
	[Serializable]
	class ConfigurationNotInitializedException : Exception
	{
		public ConfigurationNotInitializedException()
		{
		}

		public ConfigurationNotInitializedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ConfigurationNotInitializedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
