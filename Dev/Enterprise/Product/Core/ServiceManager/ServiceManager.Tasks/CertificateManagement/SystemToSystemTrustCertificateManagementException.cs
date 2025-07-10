using System;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	[Serializable]
	class SystemToSystemTrustCertificateManagementException : Exception
	{
		public SystemToSystemTrustCertificateManagementException(string message)
			: base(message) { }

		public SystemToSystemTrustCertificateManagementException(string message, Exception innerException)
			: base(message, innerException) { }
#if NETFRAMEWORK
		protected SystemToSystemTrustCertificateManagementException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
#endif
	}
}
