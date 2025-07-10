using System;

namespace Enterprise.eHubMessaging.Business
{
	[Serializable]
	public class XPathException : SystemException
	{
#if NETFRAMEWORK
		protected XPathException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public XPathException(string message)
			: base(message)
		{
		}
	}

	[Serializable]
	public class XPathReaderException : XPathException
	{
#if NETFRAMEWORK
		protected XPathReaderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public XPathReaderException(string message)
			: base(message)
		{
		}
	}
}
