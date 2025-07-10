using System;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	[Serializable]
	public class ClientLinkException : Exception
	{
		public ClientLinkException(string message) : base(message)
		{ }

		public ClientLinkException(string message, Exception ex) : base(message, ex)
		{ }

#if NETFRAMEWORK
		protected ClientLinkException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif
	}
}
