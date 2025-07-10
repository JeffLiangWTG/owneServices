using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	[Serializable]
	public class InvalidFilterConfigurationException : Exception
	{
		public MultilingualString FilterMultilingualDescription { get; }
		public string ModuleName { get; }

		public InvalidFilterConfigurationException(string message, MultilingualString multilingualDescription, string moduleName)
			: base(message)
		{
			FilterMultilingualDescription = multilingualDescription;
			ModuleName = moduleName;
		}

#if NETFRAMEWORK
		protected InvalidFilterConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
