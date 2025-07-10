using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class BaseRemoveCertificateHandler : BaseApplicationHandler
	{
		protected override void HandleCore(EdiIdentityApplication application)
		{
			var certificates = application.Certificates
				.Where(c => c.ICE_IsActive &&
							(c.ICE_IsCertificateRevoked ||
							c.ICE_CertificateExpiryDate < ZDateTime.UtcNow ||
							application.NeedRollbackAzureApplication()))
				.ToArray();

			var thumbprints = certificates
				.Where(c => !c.ICE_CertificateThumbprint.IsEmpty)
				.Select(c => c.ICE_CertificateThumbprint.ToString())
				.ToArray();

			RemoveCertificates(application, thumbprints);

			foreach (var certificate in certificates)
			{
				if (certificate.ICE_CertificateExpiryDate > ZDateTime.UtcNow || certificate.ICE_CertificateThumbprint.IsEmpty)
				{
					certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
				}
				certificate.ICE_IsActive = false;
			}
			application.Factory.Save();
		}

		public override bool Applicable(EdiIdentityApplication application)
		{
			return application.NeedRemoveCertificates();
		}

		protected virtual void RemoveCertificates(EdiIdentityApplication application, string[] thumbprints)
		{
		}
	}
}
