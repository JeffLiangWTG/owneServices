using System;

namespace Enterprise.Accounting.Web.Exceptions
{
	[Serializable]
	public class AccountingWebException : Exception
	{
		public AccountingWebException(string message)
			: base(message)
		{
		}

		protected AccountingWebException(string messageOverride, Exception innerException)
			: base(messageOverride, innerException)
		{
		}

#if NETFRAMEWORK
		protected AccountingWebException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
