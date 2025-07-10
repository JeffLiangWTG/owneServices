using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.ZArchitecture.GUI.WebLauncher
{
	[Serializable]
	public class WebUrlValidationException : Exception
	{
		public WebUrlValidationException(string message) : base(message)
		{
		}

		public WebUrlValidationException(string message, Exception ex) : base(message, ex)
		{
		}
		
#if NETFRAMEWORK
		protected WebUrlValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
