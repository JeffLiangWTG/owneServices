using System;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	[Serializable]
	public class AnalyticsAPIException : Exception
	{
		public AnalyticsAPIException()
		{
		}

		public AnalyticsAPIException(string message) : base(message)
		{
		}

		public AnalyticsAPIException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected AnalyticsAPIException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
