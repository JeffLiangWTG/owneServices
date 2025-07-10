using System;

namespace Enterprise.Integration
{
	[Serializable]
	public sealed class CertificateManagementException : Exception
	{
		public CertificateManagementException(string message)
			: base(message)
		{
		}

		public CertificateManagementException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		CertificateManagementException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
