using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core
{
	[ExceptionVisibility(ExceptionVisibility.User)]
	[Serializable]
	public class WebSiteUrlNotSetException : Exception
	{
		public WebSiteUrlNotSetException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected WebSiteUrlNotSetException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
