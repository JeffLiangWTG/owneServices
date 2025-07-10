using System;
using System.Text.Json;

namespace Enterprise.ZArchitecture.Core
{
	[Serializable]
	public class RegistryJsonException : JsonException
	{
		public string RegistryName { get; }

		public string RegistryCaption { get; }

		public RegistryJsonException(string message, string registryName, string registryCaption) : base(message)
		{
			RegistryName = registryName;
			RegistryCaption = registryCaption;
		}

#if NETFRAMEWORK
		protected RegistryJsonException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
