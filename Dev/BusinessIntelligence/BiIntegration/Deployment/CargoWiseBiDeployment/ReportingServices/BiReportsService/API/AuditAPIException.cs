using System;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	[Serializable]
	public class AuditAPIException : Exception
	{
		public AuditAPIException()
		{
		}

		public AuditAPIException(string message) : base(message)
		{
		}

		public AuditAPIException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected AuditAPIException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
