using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	[Serializable]
	public class XsdCreationException : Exception
	{
		public XsdCreationException(string message)
			: base(message)
		{
		}

		public XsdCreationException(string message, Exception ex) : base(message, ex)
		{
		}

#if NETFRAMEWORK
		protected XsdCreationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
