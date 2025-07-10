using System;

namespace Enterprise.ComplianceRisk.Business
{
	[Serializable]
	public class ComplianceCheckException : Exception
	{
		public ComplianceCheckException()
		{
		}

		public ComplianceCheckException(string message) : base(message)
		{
		}

		public ComplianceCheckException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ComplianceCheckException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
