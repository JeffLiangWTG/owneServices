using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.DataProviders;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DataProviderException : Exception
	{
		public DataProviderException(string message)
			: base(message)
		{
		}

		internal DataProviderException(string message, List<MethodInfoChainLink> chainLinks)
			: base(message)
		{
			ChainLinks = chainLinks;
		}

#if NETFRAMEWORK
		protected DataProviderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		internal List<MethodInfoChainLink> ChainLinks { get; }
	}
}
