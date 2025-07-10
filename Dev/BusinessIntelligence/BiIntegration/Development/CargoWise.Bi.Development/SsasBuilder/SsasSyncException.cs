using System;
using CargoWise.Bi.Development.Common;

namespace CargoWise.Bi.Development.SsasBuilder
{
	[Serializable]
	public class SsasSyncException : BiAutomationException
	{
		public SsasSyncException(string message)
			: base(message)
		{
		}
		public SsasSyncException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected SsasSyncException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
