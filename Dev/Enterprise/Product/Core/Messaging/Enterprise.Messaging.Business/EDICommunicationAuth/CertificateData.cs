using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Messaging.Business.EDICommunicationAuthInbound
{
	public class CertificateData : NonPersistentBusinessObject
	{
		public ZString CommonName { get; set; }
		public ZString SerialNumber { get; set; }
		public ZString Issuer { get; set; }
		public ZString ValidFrom { get; set; }
		public ZString ValidTo { get; set; }
		public ZString CertificatePem { get; set; }
	}
}
