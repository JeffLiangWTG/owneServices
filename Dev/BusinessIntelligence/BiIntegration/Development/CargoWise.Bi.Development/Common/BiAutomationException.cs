using System;

namespace CargoWise.Bi.Development.Common
{
	[Serializable]
	public class BiAutomationException : Exception
	{
		public BiAutomationException(string message)
			: base(message)
		{
		}

		public BiAutomationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected BiAutomationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
