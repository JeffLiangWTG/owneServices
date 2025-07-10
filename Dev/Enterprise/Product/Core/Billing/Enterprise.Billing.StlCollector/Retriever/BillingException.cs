using System;

namespace Enterprise.Billing.StlCollector.Retriever
{
	[Serializable]
	public class BillingException : Exception
	{
		public BillingException(string message)
			: base(message)
		{
		}

		public BillingException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		protected BillingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
